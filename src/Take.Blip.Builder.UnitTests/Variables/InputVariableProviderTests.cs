using Lime.Messaging;
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
        public static string NoIntentInputText = "Test run";

        public static string IntentsAndEntitiesText = "I would like to buy a mussarela pizza";

        public static string MultipleIntentsAndEntitiesText = "I have a plane and a toy car";

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

        public IContext Context;

        public LazyInput LazyInput;

        public DocumentSerializer DocumentSerializer;

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension;

        public InputVariableProviderTests()
        {
            DocumentSerializer = new DocumentSerializer(new DocumentTypeResolver().WithMessagingDocuments());
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == NoIntentInputText), Arg.Any<CancellationToken>()).Returns(NoIntentResponse);
            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == IntentsAndEntitiesText), Arg.Any<CancellationToken>()).Returns(IntentsAndEntitiesResponse);
            ArtificialIntelligenceExtension.AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == MultipleIntentsAndEntitiesText), Arg.Any<CancellationToken>()).Returns(MultipleIntentsAndEntitiesResponse);
        }

        public InputVariableProvider GetTarget()
        {
            return new InputVariableProvider(DocumentSerializer);
        }

        [Fact]
        public async Task CheckIfOneIntentVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(NoIntentMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Builder.Models.Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(NoIntentResponse));
        }

        [Fact]
        public async Task CheckIfMultipleIntentsVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(IntentsAndEntitiesMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Builder.Models.Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(IntentsAndEntitiesResponse));
        }

        [Fact]
        public async Task CheckIfMultipleIntentsAndEntitiesVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(MultipleIntentsAndEntitiesMessage, null, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Builder.Models.Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(MultipleIntentsAndEntitiesResponse));
        }
    }
}