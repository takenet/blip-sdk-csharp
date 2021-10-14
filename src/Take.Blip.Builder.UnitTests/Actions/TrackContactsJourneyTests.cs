using System;
using System.Threading.Tasks;
using Lime.Protocol;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Shouldly;
using Take.Blip.Builder.Actions.TrackContactsJourney;
using Take.Blip.Client.Extensions.ContactsJourney;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class TrackContactsJourneyTests : ActionTestsBase
    {
        public TrackContactsJourneyTests()
        {
            ContactsJourneyExtension = Substitute.For<IContactsJourneyExtension>();
        }

        public IContactsJourneyExtension ContactsJourneyExtension { get; private set; }

        [Fact]
        public async Task ValidContactsJourneyNodeShouldSucceed()
        {
            // Arrange
            var stateId = "onboarding";
            var stateName = "Start";
            var identity = Identity.Parse("myidentity@msging.net");
            Context.UserIdentity.Returns(identity);

            var contactsJourneyAction = new TrackContactsJourneyAction(ContactsJourneyExtension);
            var settings = new JObject
            {
                ["stateId"] = stateId,
                ["stateName"] = stateName,
            };

            // Act
            await contactsJourneyAction.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await ContactsJourneyExtension.Received(1).AddAsync(
                stateId,
                stateName,
                previousStateId: null,
                previousStateName: null,
                contactIdentity: identity,
                fireAndForget: true,
                cancellationToken: CancellationToken);
        }

        [Fact]
        public async Task ValidContactsJourneyNodeWithPreviousNodeShouldSucceed()
        {
            // Arrange
            var stateId = "state1";
            var stateName = "State One";
            var previousStateId = "onboarding";
            var previousStateName = "Start";
            var identity = Identity.Parse("myidentity@msging.net");

            Context.UserIdentity.Returns(identity);

            var contactsJourneyAction = new TrackContactsJourneyAction(ContactsJourneyExtension);
            var settings = new JObject
            {
                ["stateId"] = stateId,
                ["stateName"] = stateName,
                ["previousStateId"] = previousStateId,
                ["previousStateName"] = previousStateName,
            };

            // Act
            await contactsJourneyAction.ExecuteAsync(Context, settings, CancellationToken);

            // Assert
            await ContactsJourneyExtension.Received(1).AddAsync(
                stateId,
                stateName,
                previousStateId,
                previousStateName,
                contactIdentity: identity,
                fireAndForget: true,
                cancellationToken: CancellationToken);
        }

        [Fact]
        public async Task ContactsJourneyNodeWithoutStateIdShouldFail()
        {
            // Arrange
            var stateName = "State One";
            var previousStateId = "onboarding";
            var previousStateName = "Start";
            var identity = Identity.Parse("myidentity@msging.net");

            Context.UserIdentity.Returns(identity);

            var contactsJourneyAction = new TrackContactsJourneyAction(ContactsJourneyExtension);
            var settings = new JObject
            {
                ["stateName"] = stateName,
                ["previousStateId"] = previousStateId,
                ["previousStateName"] = previousStateName,
            };

            // Assert
            var exception = await Should.ThrowAsync<ArgumentException>(
                // Act
                async () => await contactsJourneyAction.ExecuteAsync(Context, settings, CancellationToken)
            );
            exception.Message.ShouldContain("stateId");
        }

        [Fact]
        public async Task ContactsJourneyNodeWithoutStateNameShouldFail()
        {
            // Arrange
            var stateId = "state1";
            var previousStateId = "onboarding";
            var previousStateName = "Start";
            var identity = Identity.Parse("myidentity@msging.net");

            Context.UserIdentity.Returns(identity);

            var contactsJourneyAction = new TrackContactsJourneyAction(ContactsJourneyExtension);
            var settings = new JObject
            {
                ["stateId"] = stateId,
                ["previousStateId"] = previousStateId,
                ["previousStateName"] = previousStateName,
            };

            // Assert
            var exception = await Should.ThrowAsync<ArgumentException>(
                // Act
                async () => await contactsJourneyAction.ExecuteAsync(Context, settings, CancellationToken)
            );
            exception.Message.ShouldContain("stateName");
        }
    }
}
