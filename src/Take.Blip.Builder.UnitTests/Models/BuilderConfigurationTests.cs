using System;
using System.Collections.Generic;
using System.Globalization;
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
            var minimumIntentScoreValue = 0.512;            
            var configuration = new Dictionary<string, string>()
            {
                {"builder:minimumIntentScore", minimumIntentScoreValue.ToString(CultureInfo.InvariantCulture)}
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
            var minimumIntentScoreValue = 0.512;            
            var stateExpiration = TimeSpan.Parse("00:30:00");
            var actionExecutionTimeout = 30.121412;
            var configuration = new Dictionary<string, string>()
            {
                {"builder:minimumIntentScore", minimumIntentScoreValue.ToString(CultureInfo.InvariantCulture)},
                {"builder:stateExpiration", stateExpiration.ToString()},
                {"builder:actionExecutionTimeout", actionExecutionTimeout.ToString(CultureInfo.InvariantCulture)},
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