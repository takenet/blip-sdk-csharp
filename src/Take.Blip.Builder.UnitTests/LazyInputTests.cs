using Lime.Protocol;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using Shouldly;
using Take.Blip.Builder.Models;
using Takenet.Iris.Messaging.Resources.ArtificialIntelligence;
using Xunit;
using Input = Take.Blip.Builder.Models.Input;
using Newtonsoft.Json;
using Lime.Protocol.Serialization.Newtonsoft;
using System.Threading;

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class LazyInputTests : FlowManagerTestsBase, IDisposable

    {
        [Fact]
        public async Task MessageWithAnalyzableMetadataShouldExecuteArtificialIntelligenceAnalyse()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var messageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson),
                Metadata = new Dictionary<string, string> { { "ai.analyzable", "true" }, { "key", "value" } }
            };
            
            var jsonInput = new LazyInput(
               messageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await jsonInput.AnalyzedContent;

            // Assert
            jsonInput.Analyzable.ShouldBe(true);
            await ArtificialIntelligenceExtension.Received(1).AnalyzeAsync(Arg.Is<AnalysisRequest>(
                a => a.Text.Equals(requestJson)
            ), CancellationToken);
        }

        [Fact]
        public async Task MessageWithoutAnalyzableMetadataShouldNotExecuteArtificialIntelligenceAnalyse()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var messageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson),
                Metadata = new Dictionary<string, string> { { "key", "value" } }
            };

            var jsonInput = new LazyInput(
               messageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await jsonInput.AnalyzedContent;

            // Assert
            jsonInput.Analyzable.ShouldBe(false);
            await ArtificialIntelligenceExtension.Received(0).AnalyzeAsync(
                Arg.Any<AnalysisRequest>(), Arg.Any<CancellationToken>()
            );
        }

        [Fact]
        public async Task MessageWithoutMetadataShouldNotExecuteArtificialIntelligenceAnalyse()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var messageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson)
            };

            var jsonInput = new LazyInput(
               messageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await jsonInput.AnalyzedContent;

            // Assert
            jsonInput.Analyzable.ShouldBe(false);
            await ArtificialIntelligenceExtension.Received(0).AnalyzeAsync(
                Arg.Any<AnalysisRequest>(), Arg.Any<CancellationToken>()
            );
        }
    }
}