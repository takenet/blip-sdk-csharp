using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using NSubstitute;
using SimpleInjector;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;
using Take.Blip.Client;
using Take.Blip.Client.Extensions.Bucket;
using Xunit;
using Action = Take.Blip.Builder.Models.Action;
using Input = Take.Blip.Builder.Models.Input;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class FlowManagerTests : IDisposable
    {
        public FlowManagerTests()
        {
            BucketExtension = Substitute.For<IBucketExtension>();
            Sender = Substitute.For<ISender>();
            StorageManager = Substitute.For<IStorageManager>();
            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public IBucketExtension BucketExtension { get; set; }

        public ISender Sender { get; set; }

        public IStorageManager StorageManager { get; set; }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IFlowManager GetTarget()
        {
            var container = new Container();
            container.Options.AllowOverridingRegistrations = true;
            container.RegisterBuilder();
            container.RegisterSingleton(BucketExtension);
            container.RegisterSingleton(Sender);
            container.RegisterSingleton(StorageManager);
            return container.GetInstance<IFlowManager>();
        }

        [Fact]
        public async Task SimplePingFlowWithoutConditionsShouldSendMessage()
        {
            // Arrange
            var input = new PlainText() {Text = "Ping!"};
            var user = new Identity("user", "domain");
            var messageType = "text/plain";
            var messageContent = "Pong!";
            var flow = new Flow()
            {
                Id = Guid.NewGuid().ToString(),
                States = new[]
                {
                    new State
                    {
                        Id = "root",
                        Root = true,
                        Input = new Input(),
                        Outputs = new[]
                        {
                            new Output
                            {
                                StateId = "ping"
                            }
                        }
                    },
                    new State
                    {
                        Id = "ping",
                        InputActions = new[]
                        {
                            new Action
                            {
                                Type = "SendMessage",
                                Settings = new Dictionary<string, object>
                                {
                                    {"type", messageType},
                                    {"content", messageContent}
                                }
                            }
                        }
                    }
                }
            };
            var target = GetTarget();

            // Act
            await target.ProcessInputAsync(input, user, flow, CancellationToken);

            // Assert
            StorageManager.Received(1).SetStateIdAsync(flow.Id, user, "ping", CancellationToken);
            StorageManager.Received(1).DeleteStateIdAsync(flow.Id, user, CancellationToken);
            Sender
                .Received(1)
                .SendMessageAsync(
                    Arg.Is<Message>(m => 
                        m.Id != null
                        && m.To.ToIdentity().Equals(user)
                        && m.Type.ToString().Equals(messageType) 
                        && m.Content.ToString() == messageContent), 
                    CancellationToken);
        }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }
    }
}
