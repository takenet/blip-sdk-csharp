using System.Threading.Tasks;
using NSubstitute;
using Take.Blip.Builder.Actions.DeleteVariable;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class DeleteVariableActionTests : ActionTestsBase
    {
        public DeleteVariableActionTests()
        {
            Settings = new DeleteVariableSettings();
        }
        
        public DeleteVariableSettings Settings { get; }
        
        private DeleteVariableAction GetTarget()
        {
            return new DeleteVariableAction();
        }

        [Fact]
        public async Task ExecuteShouldDeleteFromContext()
        {
            // Arrange
            Settings.Variable = "myVariable";
            var target = GetTarget();
            
            // Act
            await target.ExecuteAsync(Context, Settings, CancellationToken);
            
            // Assert
            Context.Received(1).DeleteVariableAsync(Settings.Variable, CancellationToken);
        }
    }
}