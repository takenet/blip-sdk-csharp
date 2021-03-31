using Lime.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Actions.ProcessContentAssistant;
using Take.Blip.Builder.Models;
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
                Text = "Test case",
                OutputVariable = "responseVariable",
                Score = 55
            };

            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;

            var contentAssistantResource = new AnalysisRequest
            {
                Text = settings.Text,
                Score = settings.Score.Value/100
            };

            var contentResult = new ContentResult
            {
                Combinations = new ContentCombination[]
               {
                    new ContentCombination
                    {
                        Entities = new string[] { "teste" },
                        Intent = "Teste"
                    },
               },
                Name = "Name",
                Result = new Message
                {
                    Content = "Answer"
                }
            };

            var contentResultResponse = JsonConvert.SerializeObject(new ContentAssistantActionResponse
            {
                HasCombination = true,
                Entities = contentResult.Combinations.First().Entities.ToList(),
                Intent = contentResult.Combinations.First().Intent,
                Value = contentResult.Result.Content.ToString()
            });


            //Act
            _artificialIntelligenceExtension.GetContentResultAsync(Arg.Is<AnalysisRequest>(
                ar => 
                ar.Score == contentAssistantResource.Score && 
                ar.Text == contentAssistantResource.Text), 
                Arg.Any<CancellationToken>()).Returns(contentResult);

            await _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);


            //Assert
            await Context.Received(1).SetVariableAsync(settings.OutputVariable, contentResultResponse, CancellationToken);
        }

        [Fact]
        public async Task ValidContentAssistantRequestWithoutScore_ShouldSucceedAsync()
        {
            //Arrange
            var minimumIntentScore = 0.5;
            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;
            var settings = new ProcessContentAssistantSettings
            {
                Text = "Test case",
                OutputVariable = "responseVariable"
            };
            var contentAssistantResource = new AnalysisRequest
            {
                Text = settings.Text
            };

            var contentAssistantResult = new ContentResult
            {
                Combinations = new ContentCombination[]
               {
                    new ContentCombination
                    {
                        Entities = new string[] {},
                        Intent = "Teste"
                    },
               },
                Name = "Name",
                Result = new Message
                {
                    Content = "Answer"
                }
            };

            var contentAssistantActionResponse = JsonConvert.SerializeObject(new ContentAssistantActionResponse
            {
                HasCombination = true,
                Entities = contentAssistantResult.Combinations.First().Entities.ToList(),
                Intent = contentAssistantResult.Combinations.First().Intent,
                Value = contentAssistantResult.Result.Content.ToString()
            });

            //Act
            _artificialIntelligenceExtension.GetContentResultAsync(Arg.Is<AnalysisRequest>(ar => ar.Text == contentAssistantResource.Text), CancellationToken).Returns(contentAssistantResult);
            await _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            await Context.Received(1).SetVariableAsync(settings.OutputVariable, contentAssistantActionResponse, CancellationToken);
        }

        [Fact]
        public async Task ValidContentAssistantRequestWithoutText_ShouldFailAsync()
        {
            //Arrange
            var minimumIntentScore = 0.5;
            var settings = new ProcessContentAssistantSettings();
            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;

            //Act
            System.Action functionCall = () => _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            Assert.Throws<ArgumentException>(functionCall);
        }

        [Fact]
        public async Task ValidContentAssistantRequestWithoutOutputVariable_ShouldFailAsync()
        {
            //Arrange
            var minimumIntentScore = 0.5;
            var settings = new ProcessContentAssistantSettings
            {
                Text = "Test case"
            };

            Context.Flow.BuilderConfiguration.MinimumIntentScore = minimumIntentScore;

            //Act
            System.Action functionCall = () => _processContentAssistantAction.ExecuteAsync(Context, JObject.FromObject(settings), CancellationToken);

            //Assert
            Assert.Throws<ArgumentException>(functionCall);
        }
    }
}
