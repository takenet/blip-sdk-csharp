using System.Threading.Tasks;
using Lime.Protocol;
using Shouldly;
using Xunit;

namespace Take.Blip.Builder.UnitTests
{
    public class InputExpirationCountTest
    {
        private readonly IInputExpirationCount _inputExpirationCount;
        private readonly Identity _botIdentity;
        private readonly Identity _userIdentity;

        public InputExpirationCountTest()
        {
            _inputExpirationCount = new InputExpirationCount();
            _botIdentity = Identity.Parse("bot@msging.net");
            _userIdentity = Identity.Parse("user@wa.gw.msging.net");
        }

        [Fact]
        public async Task Increment_WhenSucessAsync()
        {
            // Arrange
            var message = new Message
            {
                Id = EnvelopeId.NewId(),
                From = _userIdentity.ToNode(),
                To = _botIdentity.ToNode(),
            };

            // Act
            await _inputExpirationCount.IncrementAsync(message);
            var result = await _inputExpirationCount.IncrementAsync(message);
            
            // Assert
            result.ShouldBe(2);
        }

        [Fact]
        public async Task TryRemove_WhenSucessAsync()
        {
            // Arrange
            var message = new Message
            {
                Id = EnvelopeId.NewId(),
                From = _userIdentity.ToNode(),
                To = _botIdentity.ToNode(),
            };
            await _inputExpirationCount.IncrementAsync(message);
            await _inputExpirationCount.IncrementAsync(message);
           
            // Act
            var result = await _inputExpirationCount.TryRemoveAsync(message);
            var countActual = await _inputExpirationCount.IncrementAsync(message);
            
            // Assert
            result.ShouldBe(true);
            countActual.ShouldBe(1);
        }
    }
}
