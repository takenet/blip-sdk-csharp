using System.Collections.Generic;
using Shouldly;
using Take.Blip.Builder.Models;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Models
{
    public class BuilderConfigurationTests
    {
        [Fact]
        public void EmptyDictionaryShouldCreateEmptyBuilderConfiguration()
        {
            // Arrange
            var configuration = new Dictionary<string, string>();
            
            // Act
            var builderConfiguration = BuilderConfiguration.FromDictionary(configuration);
            
            // Assert
            builderConfiguration.ShouldNotBeNull();
            builderConfiguration.StateExpiration.ShouldBeNull();
            builderConfiguration.MinimumIntentScore.ShouldBeNull();
            builderConfiguration.ActionExecutionTimeout.ShouldBeNull();
        }
        
        [Fact]
        public void WithOneKeyShouldCreateCorrespondentBuilderConfiguration()
        {
            // Arrange
            var minimumIntentScoreValue = "0.512";            
            var configuration = new Dictionary<string, string>()
            {
                {"builder:minimumIntentScore", minimumIntentScoreValue}
            };
            
            // Act
            var builderConfiguration = BuilderConfiguration.FromDictionary(configuration);
            
            // Assert
            builderConfiguration.ShouldNotBeNull();
            builderConfiguration.MinimumIntentScore.ShouldBe(minimumIntentScoreValue);
            builderConfiguration.StateExpiration.ShouldBeNull();
            builderConfiguration.ActionExecutionTimeout.ShouldBeNull();
        }
        
        [Fact]
        public void WithOtherKeysShouldCreateCorrespondentBuilderConfiguration()
        {
            // Arrange
            var minimumIntentScoreValue = "0.512";            
            var stateExpiration = "00:30:00";
            var actionExecutionTimeout = "30.121412";
            var configuration = new Dictionary<string, string>()
            {
                {"builder:minimumIntentScore", minimumIntentScoreValue},
                {"builder:stateExpiration", stateExpiration},
                {"builder:actionExecutionTimeout", actionExecutionTimeout},
                {"myConfigurationKey", "anyValue"}
            };
            
            // Act
            var builderConfiguration = BuilderConfiguration.FromDictionary(configuration);
            
            // Assert
            builderConfiguration.ShouldNotBeNull();
            builderConfiguration.MinimumIntentScore.ShouldBe(minimumIntentScoreValue);
            builderConfiguration.StateExpiration.ShouldBe(stateExpiration);
            builderConfiguration.ActionExecutionTimeout.ShouldBe(actionExecutionTimeout);
        }
    }
}