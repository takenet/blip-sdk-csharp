using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Shouldly;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Actions.TrackEvent;
using Take.Blip.Client.Extensions.EventTracker;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class TrackEventActionTests : IDisposable
    {
        public TrackEventActionTests()
        {
            EventTrackExtension = Substitute.For<IEventTrackExtension>();
            Context = Substitute.For<IContext>();

            CancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        }

        public CancellationToken CancellationToken => CancellationTokenSource.Token;

        public CancellationTokenSource CancellationTokenSource { get; set; }

        public IEventTrackExtension EventTrackExtension { get; private set; }

        public IContext Context { get; private set; }

        public void Dispose()
        {
            CancellationTokenSource.Dispose();
        }

        [Fact]
        public async Task ValidEventTrackShouldSucceed()
        {
            // Arrange
            var category = "categoryX";
            var action = "actionA";
            var identity = Identity.Parse("myidentity@msging.net");
            var extras = new Dictionary<string, string>()
            {
                {"key1", "value1"}
            };

            Context.User.Returns(identity);
            var eventTrackAction = new TrackEventAction(EventTrackExtension);
            var settings = new JObject
            {
                ["category"] = category,
                ["action"] = action,
                ["extras"] = JObject.FromObject(extras)
            };

            // Act
            await eventTrackAction.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await EventTrackExtension.Received(1).AddAsync(
                category, 
                action, 
                Arg.Is<Dictionary<string, string>>(d => extras.Keys.All(k => d.ContainsKey(k) && d[k] == extras[k])), 
                CancellationToken, 
                identity);
        }


        [Fact]
        public async Task EventTrackWithoutCategoryShouldFail()
        {
            // Arrange
            string category = null;
            var action = "actionA";
            var identity = "myidentity@msging.net";
            var extras = new Dictionary<string, string>()
            {
                {"key1", "value1"}
            };


            var eventTrackAction = new TrackEventAction(EventTrackExtension);
            var settings = new JObject
            {
                ["category"] = category,
                ["action"] = action,
                ["identity"] = identity,
                ["extras"] = JObject.FromObject(extras)
            };


            // Act
            try
            {
                await eventTrackAction.ExecuteAsync(Context, settings, CancellationToken);
                throw new Exception("The expected exception was not thrown");
            }
            catch (ArgumentException ex)
            {
                // Assert
                ex.Message.ShouldBe("The 'category' settings value is required for 'TrackEventAction' action");
                
                await EventTrackExtension.DidNotReceive().AddAsync(
                    category,
                    action,
                    Arg.Is<Dictionary<string, string>>(d => extras.Keys.All(k => d.ContainsKey(k) && d[k] == extras[k])),
                    CancellationToken,
                    identity);
            }            
        }

    }
}
