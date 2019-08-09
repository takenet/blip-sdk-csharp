using System;
using System.Threading.Tasks;
using NSubstitute;
using Take.Blip.Builder.Actions.SetVariable;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class SetVariableActionTests : ActionTestsBase
    {
        public SetVariableActionTests()
        {
            Settings = new SetVariableSettings();
        }
        
        public SetVariableSettings Settings { get; }
        
        private SetVariableAction GetTarget()
        {
            return new SetVariableAction();
        }

        [Fact]
        public async Task ExecuteShouldSetOnContext()
        {
            // Arrange
            Settings.Variable = "myVariable";
            Settings.Value = "myValue";
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            Context.Received(1).SetVariableAsync(Settings.Variable,  Settings.Value, CancellationToken, default);
        }
        
        [Fact]
        public async Task ExecuteWithExpirationShouldSetOnContext()
        {
            // Arrange
            var expiration = TimeSpan.FromSeconds(30);
            Settings.Variable = "myVariable";
            Settings.Value = "myValue";
            Settings.Expiration = expiration.TotalSeconds;
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            Context.Received(1).SetVariableAsync(Settings.Variable,  Settings.Value, CancellationToken, expiration);
        }
    }
}