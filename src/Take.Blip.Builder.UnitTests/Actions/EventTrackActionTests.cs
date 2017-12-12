using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;
using Take.Blip.Builder.Actions;
using Take.Blip.Client.Extensions.Bucket;
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
            var identity = "myidentity@msging.net";
            var extras = new Dictionary<string, string>()
            {
                {"key1", "value1"}
            };


            var eventTrackAction = new TrackEventAction(EventTrackExtension);
            var settings = new JObject();

            settings["category"] = category;
            settings["action"] = action;
            settings["identity"] = identity;
            settings["extras"] = JObject.FromObject(extras);

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
            var settings = new JObject();

            settings["category"] = category;
            settings["action"] = action;
            settings["identity"] = identity;
            settings["extras"] = JObject.FromObject(extras);

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
