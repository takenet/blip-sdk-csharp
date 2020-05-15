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
using Take.Blip.Builder.Models;
using Take.Blip.Builder.Variables;
using Take.Blip.Client.Extensions.ArtificialIntelligence;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Variables
{
    public class InputVariableProviderTests : CancellationTokenTestsBase
    {
        public InputVariableProviderTests()
        {
            BuilderConfiguration = new BuilderConfiguration();
            DocumentSerializer = new DocumentSerializer(new DocumentTypeResolver().WithMessagingDocuments());
            ArtificialIntelligenceExtension = Substitute.For<IArtificialIntelligenceExtension>();
            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == NoIntentInputText), Arg.Any<CancellationToken>())
                .Returns(NoIntentResponse);
            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == IntentInputText), Arg.Any<CancellationToken>())
                .Returns(IntentResponse);
            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == IntentAndEntityText), Arg.Any<CancellationToken>())
                .Returns(IntentsAndEntitiesResponse);
            ArtificialIntelligenceExtension
                .AnalyzeAsync(Arg.Is<AnalysisRequest>(r => r.Text == MultipleIntentsAndEntitiesText), Arg.Any<CancellationToken>())
                .Returns(MultipleIntentsAndEntitiesResponse);
            ArtificialIntelligenceExtension
                .GetContentResultAsync(Arg.Is<ContentCombination>(cc => cc.Intent == default), Arg.Any<CancellationToken>())
                .Returns(NoContentResult);
            ArtificialIntelligenceExtension
                .GetContentResultAsync(Arg.Is<ContentCombination>(cc => cc.Intent == LearnIntent.Id), Arg.Any<CancellationToken>())
                .Returns(ContentResultWithIntent);
            ArtificialIntelligenceExtension
                .GetContentResultAsync(Arg.Is<ContentCombination>(cc => cc.Intent == IntentAndEntityCombination.Intent &&
                    cc.Entities.SequenceEqual(IntentAndEntityCombination.Entities)), Arg.Any<CancellationToken>())
                .Returns(ContentResultWithIntentAndEntity);
            ArtificialIntelligenceExtension
                .GetContentResultAsync(Arg.Is<ContentCombination>(cc => cc.Intent == IntentAndMultipleEntitiesCombination.Intent &&
                    cc.Entities.SequenceEqual(IntentAndMultipleEntitiesCombination.Entities)), Arg.Any<CancellationToken>())
                .Returns(ContentResultWithIntentAndMultipleEntities);
        }

        public BuilderConfiguration BuilderConfiguration { get; }

        public static string NoIntentInputText = "Test run";

        public static string IntentInputText = "I wanna learn Portuguese";

        public static string IntentAndEntityText = "I would like to buy a mussarela pizza";

        public static string MultipleIntentsAndEntitiesText = "I have a plane and a toy car";

        public static string IntentContentResultText = "I can help you to learn all the languages";

        public static string IntentAndEntityContentResultText = "We don't have mussarela pizza";

        public static string IntentAndMultipleEntitiesContentResultText = "I can fix all your vehicles";

        public static string MessagId = "Message-Id";

        public static IntentionResponse LearnIntent = new IntentionResponse
        {
            Id = "learn-id",
            Name = "Learn language",
            Score = 0.9
        };

        public static IntentionResponse OwnershipIntent = new IntentionResponse
        {
            Id = "ownership-id",
            Name = "Ownership",
            Score = 0.94
        };

        public static IntentionResponse BuyPizzaIntent = new IntentionResponse
        {
            Id = "buy-pizza-id",
            Name = "Buy pizza",
            Score = 0.94
        };

        public static IntentionResponse DriveIntent = new IntentionResponse
        {
            Name = "Drive",
            Score = 0.46
        };

        public static EntityResponse PlaneVehicleEntity = new EntityResponse
        {
            Name = "Vehicle",
            Value = "Plane"
        };

        public static EntityResponse CarVehicleEntity = new EntityResponse
        {
            Name = "Vehicle",
            Value = "Car"
        };

        public static EntityResponse MussarelaPizzaEntity = new EntityResponse
        {
            Name = "Pizza",
            Value = "Mussarela"
        };

        public static ContentCombination IntentCombination = new ContentCombination
        {
            Intent = LearnIntent.Id
        };

        public static ContentCombination IntentAndEntityCombination = new ContentCombination
        {
            Intent = BuyPizzaIntent.Id,
            Entities = new string[] {
                MussarelaPizzaEntity.Value
            }
        };

        public static ContentCombination IntentAndMultipleEntitiesCombination = new ContentCombination
        {
            Intent = OwnershipIntent.Id,
            Entities = new string[] {
                PlaneVehicleEntity.Value,
                CarVehicleEntity.Value,
            }
        };

        public Identity UserIdentiy = new Identity("user", "domain");

        public Message NoIntentMessage = new Message()
        {
            Id = MessagId,
            Content = new PlainText
            {
                Text = NoIntentInputText
            }
        };

        public Message IntentMessage = new Message()
        {
            Id = MessagId,
            Content = new PlainText
            {
                Text = IntentInputText
            }
        };

        public Message IntentAndEntityMessage = new Message()
        {
            Id = MessagId,
            Content = new PlainText
            {
                Text = IntentAndEntityText
            }
        };

        public Message MultipleIntentsAndEntitiesMessage = new Message()
        {
            Id = MessagId,
            Content = new PlainText
            {
                Text = MultipleIntentsAndEntitiesText
            }
        };

        public Message ValidContentResultMessage = new Message()
        {
            Id = MessagId,
            Content = new PlainText
            {
                Text = IntentAndEntityContentResultText
            }
        };

        public AnalysisResponse NoIntentResponse = new AnalysisResponse();

        public AnalysisResponse IntentResponse = new AnalysisResponse
        {
            Intentions = new IntentionResponse[]
            {
                LearnIntent
            }
        };

        public AnalysisResponse IntentsAndEntitiesResponse = new AnalysisResponse
        {
            Intentions = new IntentionResponse[]
            {
                BuyPizzaIntent
            },
            Entities = new EntityResponse[]
            {
                MussarelaPizzaEntity
            }
        };

        public AnalysisResponse MultipleIntentsAndEntitiesResponse = new AnalysisResponse
        {
            Intentions = new IntentionResponse[]
            {
                OwnershipIntent,
                DriveIntent
            },
            Entities = new EntityResponse[]
            {
                PlaneVehicleEntity,
                CarVehicleEntity
            }
        };

        public ContentResult NoContentResult = new ContentResult();

        public ContentResult ContentResultWithIntent = new ContentResult
        {
            Id = Guid.NewGuid().ToString(),
            Result = new Message
            {
                Id = MessagId,
                Content = new PlainText
                {
                    Text = IntentContentResultText
                }
            },
            Combinations = new ContentCombination[] {
                IntentCombination
            }
        };

        public ContentResult ContentResultWithIntentAndEntity = new ContentResult
        {
            Id = Guid.NewGuid().ToString(),
            Result = new Message
            {
                Id = MessagId,
                Content = new PlainText
                {
                    Text = IntentAndEntityContentResultText
                }
            },
            Combinations = new ContentCombination[] {
                IntentAndEntityCombination
            }
        };

        public ContentResult ContentResultWithIntentAndMultipleEntities = new ContentResult
        {
            Id = Guid.NewGuid().ToString(),
            Result = new Message
            {
                Id = MessagId,
                Content = new PlainText
                {
                    Text = IntentAndMultipleEntitiesContentResultText
                }
            },
            Combinations = new ContentCombination[] {
                IntentAndMultipleEntitiesCombination
            }
        };

        public IContext Context;

        public LazyInput LazyInput;

        public DocumentSerializer DocumentSerializer;

        public IArtificialIntelligenceExtension ArtificialIntelligenceExtension;

        public InputVariableProvider GetTarget()
        {
            return new InputVariableProvider(DocumentSerializer);
        }

        [Fact]
        public async Task CheckIfNoIntentVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(NoIntentMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(NoIntentResponse));
        }

        [Fact]
        public async Task CheckIfOneIntentVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(IntentMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(IntentResponse));
        }

        [Fact]
        public async Task CheckIfMultipleIntentsVariableIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(IntentAndEntityMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
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
            LazyInput = new LazyInput(MultipleIntentsAndEntitiesMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actual = await target.GetVariableAsync("analysis", Context, CancellationToken);

            // Assert
            actual.ShouldBe(DocumentSerializer.Serialize(MultipleIntentsAndEntitiesResponse));
        }

        [Fact]
        public async Task CheckIfAssistantResultWithoutCombinationsIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(NoIntentMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actualId = await target.GetVariableAsync("contentAssistant.id", Context, CancellationToken);

            // Assert
            actualId.ShouldBe(NoContentResult.Id);
        }

        [Fact]
        public async Task CheckIfAssistantResultWithIntentCombinationIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(IntentMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actualValue = await target.GetVariableAsync("contentAssistant.result", Context, CancellationToken);
            var actualId = await target.GetVariableAsync("contentAssistant.id", Context, CancellationToken);

            // Assert
            actualValue.ShouldBe(ContentResultWithIntent.Result.Content.ToString());
            actualId.ShouldBe(ContentResultWithIntent.Id);
        }

        [Fact]
        public async Task CheckIfAssistantResultWithIntentAndEntityCombinationIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(IntentAndEntityMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actualValue = await target.GetVariableAsync("contentAssistant.result", Context, CancellationToken);
            var actualId = await target.GetVariableAsync("contentAssistant.id", Context, CancellationToken);

            // Assert
            actualValue.ShouldBe(ContentResultWithIntentAndEntity.Result.Content.ToString());
            actualId.ShouldBe(ContentResultWithIntentAndEntity.Id);
        }

        [Fact]
        public async Task CheckIfAssistantResultWithIntentAndMultipleEntitiesCombinationIsValid()
        {
            // Arrange
            LazyInput = new LazyInput(MultipleIntentsAndEntitiesMessage, UserIdentiy, BuilderConfiguration, DocumentSerializer, null, ArtificialIntelligenceExtension, CancellationToken);
            Context = new ExtensionContext("me", "app", LazyInput, new Flow(), new List<InputVariableProvider>(), null);
            var target = GetTarget();

            // Act
            var actualValue = await target.GetVariableAsync("contentAssistant.result", Context, CancellationToken);
            var actualId = await target.GetVariableAsync("contentAssistant.id", Context, CancellationToken);

            // Assert
            actualValue.ShouldBe(ContentResultWithIntentAndMultipleEntities.Result.Content.ToString());
            actualId.ShouldBe(ContentResultWithIntentAndMultipleEntities.Id);
        }
    }
}