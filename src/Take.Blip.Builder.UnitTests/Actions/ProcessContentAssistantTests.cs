using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ProcessContentAssistant;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ProcessContentAssistantTests : ActionTestsBase
    {
        private readonly IArtificialIntelligenceExtension _artificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
        private readonly ProcessContentAssistantAction _processContentAssistantAction;

        public ProcessContentAssistantTests()
        {
            _processContentAssistantAction = new ProcessContentAssistantAction(_artificialIntelligenceExtension);
        }

        [Fact]
        public async Task ValidContentAssistantRequest_ShouldSucceedAsync()
        {
            //Arrange
            var minimumIntentScore = 0.5;
            var settings = new ProcessContentAssistantSettings
            {
                Text = "Test case"
            };

            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;

            var contentAssistantResource = new AnalysisRequest
            {
                Text = settings.Text,
                Score = Context.Flow.BuilderConfiguration.MinimumIntentScore ?? default
            };

            //Act
            await _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            await _artificialIntelligenceExtension.Received(1).GetContentResultAsync(Arg.Is<AnalysisRequest>(a => a.Text == settings.Text && a.Score == minimumIntentScore), cancellationToken: CancellationToken);
        }

        [Fact]
        public async Task ValidContentAssistantRequestWithoutScore_ShouldSucceedAsync()
        {
            //Arrange
            var settings = new ProcessContentAssistantSettings
            {
                Text = "Test case"
            };
            var contentAssistantResource = new AnalysisRequest
            {
                Text = settings.Text
            };

            //Act
            await _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            await _artificialIntelligenceExtension.Received(1).GetContentResultAsync(Arg.Is<AnalysisRequest>(a => a.Text == settings.Text), cancellationToken: CancellationToken);
        }

        [Fact]
        public async Task ValidContentAssistantRequestWithoutText_ShouldFailAsync()
        {
            //Arrange
            var minimumIntentScore = 0.5;
            var settings = new ProcessContentAssistantSettings();
            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;

            //Act
            Action functionCall = () => _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            Assert.Throws<ArgumentException>(functionCall);
        }
    }
}
