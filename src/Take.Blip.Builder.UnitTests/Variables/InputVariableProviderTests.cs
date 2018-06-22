using Lime.Messaging.Contents;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using NSubstitute;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class InputVariableProviderTests : CancellationTokenTestsBase
    {

        #region Lazy Input user message

        public static string NoIntentInputText = "Test run";

        public static string IntentsAndEntitiesText = "I would like to buy a mussarela pizza";

        public static string MultipleIntentsAndEntitiesText = "I have a plane and a toy car";

        #endregion

        #region Artificial Intelligence Extension Responses

        public AnalysisResponse NoIntentResponse = new AnalysisResponse();

        public AnalysisResponse IntentsAndEntitiesResponse = new AnalysisResponse
        {
            Intentions = new IntentionResponse[]
            {
                new IntentionResponse
                {
                    Name = "Buy pizza",
                    Score = 0.94
                }
            },
            Entities = new EntityResponse[]
            {
                new EntityResponse
                {
                    Name = "Pizza",
                    Value = "Mussarela"
                }
            }
        };

        public AnalysisResponse MultipleIntentsAndEntitiesResponse = new AnalysisResponse
        {
            Intentions = new IntentionResponse[]
            {
                new IntentionResponse
                {
                    Name = "Ownership",
                    Score = 0.98
                },
                new IntentionResponse
                {
                    Name = "Drive",
                    Score = 0.46
                }
            },
            Entities = new EntityResponse[]
            {
                new EntityResponse
                {
                    Name = "Vehicle",
                    Value = "Plane"
                }
            }
        };

        #endregion

        #region Message and input objects

        public PlainText NoIntentMessage = new PlainText
        {
            Text = NoIntentInputText
        };

        public PlainText IntentsAndEntitiesMessage = new PlainText
        {
            Text = IntentsAndEntitiesText
        };

        public PlainText MultipleIntentsAndEntitiesMessage = new PlainText
        {
            Text = MultipleIntentsAndEntitiesText
        };

        public LazyInput NoIntentInput;
        public LazyInput IntentsAndEntitiesInput;
        public LazyInput MultipleIntentsAndEntitiesInput;

        #endregion

        #region Contexts

        public Context NoInputContext;
        public Context IntentsAndEntitiesContext;
        public Context MultipleIntentsAndEntitiesContext;

        #endregion

        public InputVariableProviderTests()
        {
            DocumentSerializer = new DocumentSerializer();

            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();

            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == NoIntentInputText), Arg.Any<CancellationToken>()).Returns(NoIntentResponse);
            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == IntentsAndEntitiesText), Arg.Any<CancellationToken>()).Returns(IntentsAndEntitiesResponse);
            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == MultipleIntentsAndEntitiesText), Arg.Any<CancellationToken>()).Returns(MultipleIntentsAndEntitiesResponse);

            NoIntentInput = new LazyInput(NoIntentMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            IntentsAndEntitiesInput = new LazyInput(IntentsAndEntitiesMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            MultipleIntentsAndEntitiesInput = new LazyInput(MultipleIntentsAndEntitiesMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);

            NoInputContext = new Context("me", NoIntentInput, new Builder.Models.Flow(), null, new List<InputVariableProvider>());
            IntentsAndEntitiesContext = new Context("me", IntentsAndEntitiesInput, new Builder.Models.Flow(), null, new List<InputVariableProvider>());
            MultipleIntentsAndEntitiesContext = new Context("me", MultipleIntentsAndEntitiesInput, new Builder.Models.Flow(), null, new List<InputVariableProvider>());
        }

        public InputVariableProvider GetTarget()
        {
            return new InputVariableProvider(DocumentSerializer);
        }

        public DocumentSerializer DocumentSerializer;

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension;

        [Fact]
        public async Task CheckIfOneIntentVariableIsValid()
        {
            // Arrage
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", NoInputContext, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(NoIntentResponse));
        }

        [Fact]
        public async Task CheckIfMultipleIntentsVariableIsValid()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", IntentsAndEntitiesContext, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(IntentsAndEntitiesResponse));
        }

        [Fact]
        public async Task CheckIfMultipleIntentsAndEntitiesVariableIsValid()
        {
            // Arrange
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", MultipleIntentsAndEntitiesContext, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(MultipleIntentsAndEntitiesResponse));
        }
    }
}
