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

#pragma warning disable 4014

namespace Take.Blip.Builder.UnitTests
{
    public class LazyInputTests : FlowManagerTestsBase, IDisposable

    {
        [Fact]
        public async Task ArtificialIntelligenceExtensionAnalyzeAsyncShouldBeExecuted()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var MessageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson),
                Metadata = new Dictionary<string, string> { { "ai.analyzable", "true" }, { "key", "value" } }
            };
            
            Input = new LazyInput(
               MessageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await Input.AnalyzedContent;

            // Assert
            Input.Analyzable.ShouldBe(true);
            await ArtificialIntelligenceExtension.Received(1).AnalyzeAsync(Arg.Is<AnalysisRequest>(
                a => a.Text.Equals(requestJson)
            ), CancellationToken);
        }

        [Fact]
        public async Task ArtificialIntelligenceExtensionAnalyzeAsyncShouldNotBeExecuted()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var MessageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson),
                Metadata = new Dictionary<string, string> { { "key", "value" } }
            };

            Input = new LazyInput(
               MessageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await Input.AnalyzedContent;

            // Assert
            Input.Analyzable.ShouldBe(false);
            await ArtificialIntelligenceExtension.Received(0).AnalyzeAsync(Arg.Is<AnalysisRequest>(
                a => a.Text.Equals(requestJson)
            ), CancellationToken);
        }

        [Fact]
        public async Task ArtificialIntelligenceExtensionAnalyzeAsyncShouldNotBeExecutedWithoutMetadata()
        {
            // Arrange
            var requestJson = "{\"dialogState\":\"STARTED\",\"intent\":{\"confirmationStatus\":\"NONE\",\"name\":\"PlanMyTrip\",\"slots\":{\"fromCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"fromCity\"},\"SlotName\":{\"confirmationStatus\":\"NONE\",\"name\":\"string\",\"resolutions\":{\"resolutionsPerAuthority\":[{\"authority\":\"string\",\"status\":{\"code\":\"ER_SUCCESS_MATCH\"},\"values\":[{\"value\":{\"name\":\"string\",\"id\":\"string\"}}]}]},\"value\":\"string\"},\"toCity\":{\"confirmationStatus\":\"NONE\",\"name\":\"toCity\",\"value\":\"Chicago\"},\"travelDate\":{\"confirmationStatus\":\"NONE\",\"name\":\"travelDate\"}}},\"locale\":\"en-US\",\"reason\":\"USER_INITIATED\",\"requestId\":\"amzn1.echo-api.request.1\",\"timestamp\":\"2019-03-23T00:34:14.000Z\",\"type\":\"IntentRequest\"}";
            var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson);

            var MessageWithAnalyzable = new Message()
            {
                Content = new JsonDocument(json, MediaType.ApplicationJson)
            };

            Input = new LazyInput(
               MessageWithAnalyzable,
               new BuilderConfiguration(),
               new DocumentSerializer(new DocumentTypeResolver()),
               new EnvelopeSerializer(new DocumentTypeResolver()),
               ArtificialIntelligenceExtension,
               CancellationToken);

            // Act
            await Input.AnalyzedContent;

            // Assert
            Input.Analyzable.ShouldBe(false);
            await ArtificialIntelligenceExtension.Received(0).AnalyzeAsync(Arg.Is<AnalysisRequest>(
                a => a.Text.Equals(requestJson)
            ), CancellationToken);
        }
    }
}