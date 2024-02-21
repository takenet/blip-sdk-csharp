using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Take.Blip.Builder.Utils;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Utils
{
    public class SensisitveInfoReplacerTests : FlowManagerTestsBase
    {
        private readonly ISensitiveInfoReplacer _sensitiveInfoReplacer;
        private const string SETTINGS_HEADER_WITHOUT_VALUES = @"{""headers"":{},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";

        public SensisitveInfoReplacerTests()
        {
            _sensitiveInfoReplacer = new SensitiveInfoReplacer();
        }

        [Fact]
        public void HeadersSubstitutionShouldSucceed()
        {
            //Arrange
            var contentToSubstitute = @"{""headers"":{""BotKey"":""Key AAAAAAAAAAAAA"",""OtherHeader"":""OtherValue"",""Content-Type"":""application/json""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";
            var expectedContent = @"{""headers"":{""BotKey"":""***"",""OtherHeader"":""***"",""Content-Type"":""***""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";

            //Act
            var content = _sensitiveInfoReplacer.ReplaceCredentials(contentToSubstitute);

            //Assert
            content.ShouldBe(expectedContent);
        }

        [Fact]
        public void ContentWithoutHeadersReplaceShouldntChangeAnything()
        {           
            //Act
            var content = _sensitiveInfoReplacer.ReplaceCredentials(SETTINGS_HEADER_WITHOUT_VALUES);

            //Assert
            content.ShouldBe(SETTINGS_HEADER_WITHOUT_VALUES);
        }
    }
}
