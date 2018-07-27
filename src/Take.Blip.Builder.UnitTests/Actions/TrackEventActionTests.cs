using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lime.Protocol;
using Shouldly;
using Take.Blip.Builder.Actions.TrackEvent;
using Take.Blip.Client.Extensions.EventTracker;
using Xunit;
using Take.Blip.Client;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class TrackEventActionTests : ActionTestsBase
    {
        public TrackEventActionTests()
        {
            EventTrackExtension = Substitute.For<IEventTrackExtension>();
        }

        public IEventTrackExtension EventTrackExtension { get; private set; }


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
                label: null,
                value: null,
                messageId: null,
                extras: Arg.Is<Dictionary<string, string>>(d => extras.Keys.All(k => d.ContainsKey(k) && d[k] == extras[k])), 
                cancellationToken: CancellationToken, 
                contactIdentity: identity);
        }

        [Fact]
        public async Task ValidEventTrackWithAllFieldsShouldSucceed()
        {
            // Arrange
            var category = "categoryX";
            var action = "actionA";
            var label = "labelll";
            decimal value = (decimal)45.78;
            var identity = Identity.Parse("myidentity@msging.net");
            var messageId = EnvelopeId.NewId();
            var extras = new Dictionary<string, string>()
            {
                {"key1", "value1"}
            };

            Context.User.Returns(identity);
            EnvelopeReceiverContext<Message>.Create(new Message { Id = messageId });

            var eventTrackAction = new TrackEventAction(EventTrackExtension);
            var settings = new JObject
            {
                ["category"] = category,
                ["action"] = action,
                ["label"] = label,
                ["value"] = value,
                ["extras"] = JObject.FromObject(extras)
            };

            // Act
            await eventTrackAction.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await EventTrackExtension.Received(1).AddAsync(
                category,
                action,
                label: label,
                value: value,
                messageId: messageId,
                extras: Arg.Is<Dictionary<string, string>>(d => extras.Keys.All(k => d.ContainsKey(k) && d[k] == extras[k])),
                cancellationToken: CancellationToken,
                contactIdentity: identity);
        }

        [Fact]
        public async Task EventTrackWithInvalidValueShouldSucceed()
        {
            // Arrange
            var category = "categoryX";
            var action = "actionA";
            var label = "labelll";
            string value = "abcdeninr";
            var identity = Identity.Parse("myidentity@msging.net");
            var messageId = EnvelopeId.NewId();
            var extras = new Dictionary<string, string>()
            {
                {"key1", "value1"}
            };

            Context.User.Returns(identity);
            EnvelopeReceiverContext<Message>.Create(new Message { Id = messageId });

            var eventTrackAction = new TrackEventAction(EventTrackExtension);
            var settings = new JObject
            {
                ["category"] = category,
                ["action"] = action,
                ["label"] = label,
                ["value"] = value,
                ["extras"] = JObject.FromObject(extras)
            };

            // Act
            await eventTrackAction.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await EventTrackExtension.Received(1).AddAsync(
                category,
                action,
                label: label,
                value: null,
                messageId: messageId,
                extras: Arg.Is<Dictionary<string, string>>(d => extras.Keys.All(k => d.ContainsKey(k) && d[k] == extras[k])),
                cancellationToken: CancellationToken,
                contactIdentity: identity);
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
