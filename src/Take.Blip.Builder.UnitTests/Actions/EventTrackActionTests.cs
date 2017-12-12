using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions;
using Take.Blip.Client.Extensions.Bucket;
using Take.Blip.Client.Extensions.EventTracker;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class EventTrackActionTests : IDisposable
    {
        public EventTrackActionTests()
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
        public async Task ValidEventTrackWithIdentityShouldSucced()
        {
            var eventTrackAction = new EventTrackAction(EventTrackExtension);

            var settings = new JObject();
            settings["category"] = "categoryX";
            settings["action"] = "actionA";
            settings["identity"] = "myidentity@msging.net";
            settings["extras"] = null;

            await eventTrackAction.ExecuteAsync(Context, null, CancellationToken);
        }

    }
}
