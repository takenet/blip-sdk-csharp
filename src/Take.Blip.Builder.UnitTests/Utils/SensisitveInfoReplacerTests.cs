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
        private readonly ISensitiveInfoReplacer _sensisitveInfoReplacer;

        public SensisitveInfoReplacerTests()
        {
            _sensisitveInfoReplacer = new SensitiveInfoReplacer();
        }

        [Fact]
        public void HeadersSubstitutionShouldSucceed()
        {
            //Arrange
            var contentToSubstitute = @"{""headers"":{""BotKey"":""Key AAAAAAAAAAAAA"",""OtherHeader"":""OtherValue"",""Content-Type"":""application/json""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";
            var expectedContent = @"{""headers"":{""BotKey"":""***"",""OtherHeader"":""***"",""Content-Type"":""***""},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";

            //Act
            var content = _sensisitveInfoReplacer.ReplaceCredentials(contentToSubstitute);

            //Assert
            content.ShouldBe(expectedContent);
        }

        [Fact]
        public void ContentWithoutHeadersReplaceShouldntChangeAnything()
        {
            //Arrange
            var contentToSubstitute = @"{""headers"":{},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";
            var expectedContent = @"{""headers"":{},""method"":""GET"",""uri"":""https://enz557qv71nso.x.pipedream.net""}";

            //Act
            var content = _sensisitveInfoReplacer.ReplaceCredentials(contentToSubstitute);

            //Assert
            content.ShouldBe(expectedContent);
        }
    }
}
