using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using Take.Blip.Builder.Hosting;
using Take.Blip.Client.Extensions.HelpDesk;
using Takenet.Iris.Messaging.Resources;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class AttendanceRedirectHandlerFixture : IDisposable
    {
        public IContext Context { get; private set; }
        public IAttendanceRedirectHandler AttendanceRedirectHandler { get; private set; }
        public IHelpDeskExtension HelpDeskExtension { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public Identity FromIdentity { get; private set; }
        public Identity UserIdentity { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public AttendanceRedirectHandlerFixture()
        {
            Context = Substitute.For<IContext>();
            HelpDeskExtension = Substitute.For<IHelpDeskExtension>();
            Configuration = Substitute.For<IConfiguration>();

            AttendanceRedirectHandler = new AttendanceRedirectHandler(HelpDeskExtension, Configuration);

            FromIdentity = new Identity() { Domain = "msging.net", Name = "test" };
            UserIdentity = new Identity() { Domain = "msging.net", Name = "user" };

            CancellationToken = default;
        }

        public void Dispose()
        {
            
        }
    }

    public class AttendanceRedirectHandlerTests : IClassFixture<AttendanceRedirectHandlerFixture>
    {

        private readonly AttendanceRedirectHandlerFixture _fixture;

        public AttendanceRedirectHandlerTests(AttendanceRedirectHandlerFixture fixture)
        {
            this._fixture = fixture;
        }

        [Fact]
        public async Task RedirectToAttendanceAsync_WhenFeatureToggleIsOff_ShouldNotCallMethods()
        {
            /* Arrange */
            MockConfigurationIsAttendanceRedirectAllowed(_fixture.Configuration, false);

            /* Action */
            await _fixture.AttendanceRedirectHandler.RedirectToAttendanceAsync(
                _fixture.FromIdentity, _fixture.UserIdentity, _fixture.Context, _fixture.CancellationToken);

            /* Assert */
            await _fixture.Context.Received(0).SetVariableAsync(AttendanceRedirectHandler.HELP_DESK_HAS_TICKET, true.ToString(), _fixture.CancellationToken);
            await _fixture.HelpDeskExtension.Received(0).CreateTicketAsync(_fixture.FromIdentity, Arg.Any<Ticket>(), _fixture.CancellationToken);
            await _fixture.Context.Received(0).DeleteVariableAsync(AttendanceRedirectHandler.ATTENDANCE_REDIRECT_KEY, _fixture.CancellationToken);
        }

        [Theory]
        [InlineData("tests%40take.net@blip.ai", 1)]
        [InlineData("", 0)]
        [InlineData(" ", 0)]
        [InlineData(null, 0)]
        public async Task RedirectToAttendanceAsync_WithAttendanceRedirect_ShouldMatchNumberOfCallsAsync(string attendanceRedirect, int calls)
        {
            /* Arrange */
            _fixture.Context.ClearReceivedCalls();
            _fixture.HelpDeskExtension.ClearReceivedCalls();
            MockContextGetContextVariableAsync(_fixture.Context, AttendanceRedirectHandler.ATTENDANCE_REDIRECT_KEY, attendanceRedirect);
            MockConfigurationIsAttendanceRedirectAllowed(_fixture.Configuration, true);

            /* Action */
            await _fixture.AttendanceRedirectHandler.RedirectToAttendanceAsync(
                _fixture.FromIdentity, _fixture.UserIdentity, _fixture.Context, _fixture.CancellationToken);

            /* Assert */
            await _fixture.Context.Received(calls).SetVariableAsync(AttendanceRedirectHandler.HELP_DESK_HAS_TICKET, true.ToString(), _fixture.CancellationToken);
            await _fixture.HelpDeskExtension.Received(calls).CreateTicketAsync(_fixture.FromIdentity, Arg.Any<Ticket>(), _fixture.CancellationToken);
            await _fixture.Context.Received(calls).DeleteVariableAsync(AttendanceRedirectHandler.ATTENDANCE_REDIRECT_KEY, _fixture.CancellationToken);
        }

        private void MockConfigurationIsAttendanceRedirectAllowed(IConfiguration configuration, bool result)
        {
            configuration.IsAttendanceRedirectAllowed.Returns(result);
        }

        private void MockContextGetContextVariableAsync(IContext context, string mockString, string mockReturn)
        {
            context.GetContextVariableAsync(mockString, Arg.Any<CancellationToken>()).Returns(mockReturn);
        }
    }
}
