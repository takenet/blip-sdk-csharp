using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using NSubstitute;
using Shouldly;
using Take.Blip.Client.Extensions.Contacts;
using Take.Blip.Client.Extensions.Directory;
using Take.Blip.Client.Receivers;
using Xunit;

namespace Take.Blip.Client.UnitTests.Receivers
{
    public class ContactMessageReceiverBaseTests : TestsBase
    {
        public ContactMessageReceiverBaseTests()
        {
            ContactExtension = Substitute.For<IContactExtension>();
            DirectoryExtension = Substitute.For<IDirectoryExtension>();
            CacheExpiration = TimeSpan.FromMilliseconds(250);
        }
        
        public IContactExtension ContactExtension { get; }
        
        public IDirectoryExtension DirectoryExtension { get; set; }

        public bool CacheLocally { get; set; }

        public TimeSpan CacheExpiration { get; set; }
        
        public TestContactMessageReceiver GetTarget()
        {
            return new TestContactMessageReceiver(
                ContactExtension,
                DirectoryExtension,
                CacheLocally,
                CacheExpiration
                );
        }
        
        [Fact]
        public async Task ReceiveMessageShouldGetContactFromContacts()
        {
            // Arrange
            var message = Dummy.CreateMessage();
            var identity = message.From.ToIdentity();
            var contact = Dummy.CreateContact();
            contact.Identity = identity;
            ContactExtension
                .GetAsync(identity, Arg.Any<CancellationToken>())
                .Returns(contact);

            var target = GetTarget();
            
            // Act
            await target.ReceiveAsync(message, CancellationToken);

            // Assert
            target.ReceivedItems.Count.ShouldBe(1);
            target.ReceivedItems[0].message.ShouldBe(message);
            var actualContact = target.ReceivedItems[0].contact;
            foreach (var property in typeof(Contact).GetProperties())
            {
                property
                    .GetValue(actualContact)
                    .ShouldBe(
                        property.GetValue(contact));
            }
        }
        
        [Fact]
        public async Task ReceiveMessageShouldGetContactFromDirectoryWhenNotInContacts()
        {
            // Arrange
            var message = Dummy.CreateMessage();
            var identity = message.From.ToIdentity();
            var account = Dummy.CreateAccount();
            account.Identity = identity;
            DirectoryExtension
                .GetDirectoryAccountAsync(identity, Arg.Any<CancellationToken>())
                .Returns(account);

            var target = GetTarget();
            
            // Act
            await target.ReceiveAsync(message, CancellationToken);

            // Assert
            target.ReceivedItems.Count.ShouldBe(1);
            target.ReceivedItems[0].message.ShouldBe(message);
            var actualContact = target.ReceivedItems[0].contact;
            actualContact.Name.ShouldBe(account.FullName);
            
            foreach (var property in typeof(ContactDocument).GetProperties())
            {
                property
                    .GetValue(actualContact)
                    .ShouldBe(
                        property.GetValue(account));
            }
        }
        
        [Fact]
        public async Task ReceiveMessageTwiceShouldGetContactFromCacheOnSecondTime()
        {
            // Arrange
            CacheLocally = true;
            var message = Dummy.CreateMessage();
            var identity = message.From.ToIdentity();
            var contact = Dummy.CreateContact();
            contact.Identity = identity;
            ContactExtension
                .GetAsync(identity, Arg.Any<CancellationToken>())
                .Returns(contact);

            var target = GetTarget();
            
            // Act
            await target.ReceiveAsync(message, CancellationToken);
            await target.ReceiveAsync(message, CancellationToken);

            // Assert

            ContactExtension.Received(1).GetAsync(identity, CancellationToken);
        }
        
        [Fact]
        public async Task ReceiveMessageTwiceShouldGetContactFromContactsTwiceWhenCaseIsDisabled()
        {
            // Arrange
            CacheLocally = false;
            var message = Dummy.CreateMessage();
            var identity = message.From.ToIdentity();
            var contact = Dummy.CreateContact();
            contact.Identity = identity;
            ContactExtension
                .GetAsync(identity, Arg.Any<CancellationToken>())
                .Returns(contact);

            var target = GetTarget();
            
            // Act
            await target.ReceiveAsync(message, CancellationToken);
            await target.ReceiveAsync(message, CancellationToken);

            // Assert

            ContactExtension.Received(2).GetAsync(identity, CancellationToken);
        }
        
        [Fact]
        public async Task ReceiveMessageTwiceShouldGetContactFromContactsTwiceWhenCaseExpires()
        {
            // Arrange
            CacheLocally = true;
            var message = Dummy.CreateMessage();
            var identity = message.From.ToIdentity();
            var contact = Dummy.CreateContact();
            contact.Identity = identity;
            ContactExtension
                .GetAsync(identity, Arg.Any<CancellationToken>())
                .Returns(contact);

            var target = GetTarget();
            
            // Act
            await target.ReceiveAsync(message, CancellationToken);
            await Task.Delay(CacheExpiration + CacheExpiration);
            await target.ReceiveAsync(message, CancellationToken);

            // Assert

            ContactExtension.Received(2).GetAsync(identity, CancellationToken);
        }
    }

    public class TestContactMessageReceiver : ContactMessageReceiverBase
    {
        public TestContactMessageReceiver(
            IContactExtension contactExtension,
            IDirectoryExtension directoryExtension,
            bool cacheLocally = true,
            TimeSpan cacheExpiration = default) : base(contactExtension,
            directoryExtension,
            cacheLocally,
            cacheExpiration)
        {
            ReceivedItems = new List<(Message, Contact)>();
        }
        
        public List<(Message message, Contact contact)> ReceivedItems { get; }

        protected override Task ReceiveAsync(Message message, Contact contact, CancellationToken cancellationToken = default(CancellationToken))
        {
            ReceivedItems.Add((message, contact));
            return Task.CompletedTask;
        }
    }
}