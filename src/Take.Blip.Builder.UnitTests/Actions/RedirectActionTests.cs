using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Take.Blip.Builder.Actions.Redirect;
using Take.Blip.Builder.Actions.SetVariable;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class RedirectActionTests : ActionTestsBase
    {
        public RedirectActionTests()
        {
            RedirectManager = Substitute.For<IRedirectManager>();
            Logger = Substitute.For<ILogger>();
        }

        private RedirectAction GetTarget()
        {
            return new RedirectAction(RedirectManager, Logger);
        }

        public ILogger Logger { get; }

        public IRedirectManager RedirectManager { get; }

        [Fact]
        public async Task ExecuteShouldSuccess()
        {
            // Arrange
            var target = GetTarget();
            var redirect = new JObject();
            redirect.Add("address", "bot1");

            // Act
            await target.ExecuteAsync(Context, redirect, CancellationToken);

            // Assert
            Context.Received(1);
        }

        [Fact]
        public async Task ExecuteWithLogShouldSuccess()
        {
            // Arrange
            var target = GetTarget();
            var redirect = new JObject();
            redirect.Add("address", "bot1");
            Context.Input.Message.Metadata = new Dictionary<string, string> { { "REDIRECT_TEST_LOG", "bot1"  } };

            // Act
            await target.ExecuteAsync(Context, redirect, CancellationToken);

            // Assert
            Context.Received(1);
        }

        [Fact]
        public async Task ExecuteShouldFailure()
        {
            // Arrange
            var target = GetTarget();

            // Act
            try
            {
                await target.ExecuteAsync(Context, null, CancellationToken);
            }
            catch (Exception e) {
                Context.DidNotReceive();
            }
        }
    }
}