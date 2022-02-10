using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NSubstitute;
using Serilog;
using Take.Blip.Builder.Actions.MergeContact;
using Take.Blip.Builder.Actions.Redirect;
using Take.Blip.Builder.Actions.SetVariable;
using Take.Blip.Client.Extensions.Contacts;
using Xunit;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class MergeContactActionTest : ActionTestsBase
    {
        public MergeContactActionTest()
        {
            ContactExtension = Substitute.For<IContactExtension>();
            Logger = Substitute.For<ILogger>();
        }

        //public SetVariableSettings Settings { get; }

        private MergeContactAction GetTarget()
        {
            return new MergeContactAction(ContactExtension, Logger);
        }

        public ILogger Logger { get; }

        public IContactExtension ContactExtension { get; }

        [Fact]
        public async Task ExecuteShouldSuccess()
        {
            // Arrange
            var target = GetTarget();
            var contact = new JObject();
            contact.Add("Name", "contact");
            contact.Add("Email", "contato@email.com");
            contact.Add("Identity", "contato@omni.io");

            // Act
            await target.ExecuteAsync(Context, contact, CancellationToken);

            // Assert
            Context.Received(1);
        }

        [Fact]
        public async Task ExecuteShouldFailure()
        {
            // Arrange
            var target = GetTarget();
            var contact = new JObject();
            contact.Add("NotExistKey", "contact");
            
            // Act
            try
            {
                await target.ExecuteAsync(Context, null, CancellationToken);
            }
            catch (Exception e) {
                Context.DidNotReceive();
            }
        }
    }
}