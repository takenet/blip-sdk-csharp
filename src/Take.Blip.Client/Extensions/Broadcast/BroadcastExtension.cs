using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Network;
using Takenet.Iris.Messaging.Resources;

namespace Take.Blip.Client.Extensions.Broadcast
{
    public class BroadcastExtension : ExtensionBase, IBroadcastExtension
    {
        private static readonly Node DistributionListAddress = Node.Parse($"postmaster@broadcast.{Constants.DEFAULT_DOMAIN}");
        
        /// <summary>
        /// Initializes a new instance of the <see cref="BroadcastExtension"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public BroadcastExtension(ISender sender)
            : base(sender)
        {
        }

        public async Task CreateDistributionListAsync(string listName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri("/lists"),
                Resource = new JsonDocument(DistributionList.MediaType)
                {
                    {"identity", listIdentity.ToString()}
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public Task<DocumentCollection> GetRecipientsAsynGetAllDistributionListsAsync(int skip = 0, int take = 100, CancellationToken cancellationToken = default(CancellationToken))
        {
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri("/lists"),
            };

            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public async Task DeleteDistributionListAsync(string listName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/lists/{Uri.EscapeDataString(listIdentity.ToString())}")
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task AddRecipientAsync(string listName, Identity recipientIdentity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            if (recipientIdentity == null) throw new ArgumentNullException(nameof(recipientIdentity));

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Set,
                Uri = new LimeUri($"/lists/{Uri.EscapeDataString(listIdentity.ToString())}/recipients"),
                Resource = new IdentityDocument()
                {
                    Value = recipientIdentity
                }
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task DeleteRecipientAsync(string listName, Identity recipientIdentity, CancellationToken cancellationToken = default(CancellationToken))
        {
            var listIdentity = GetListIdentity(listName);
            if (recipientIdentity == null) throw new ArgumentNullException(nameof(recipientIdentity));

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Delete,
                Uri = new LimeUri($"/lists/{Uri.EscapeDataString(listIdentity.ToString())}/recipients/{Uri.EscapeDataString(recipientIdentity.ToString())}")
            };

            await ProcessCommandAsync(requestCommand, cancellationToken);
        }

        public async Task<bool> HasRecipientAsync(string listName, Identity recipientIdentity, CancellationToken cancellationToken = new CancellationToken())
        {
            var listIdentity = GetListIdentity(listName);
            if (recipientIdentity == null) throw new ArgumentNullException(nameof(recipientIdentity));

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/lists/{Uri.EscapeDataString(listIdentity.ToString())}/recipients/{Uri.EscapeDataString(recipientIdentity.ToString())}")
            };

            try
            {
                await ProcessCommandAsync(requestCommand, cancellationToken);
                return true;
            }
            catch (LimeException ex) when (ex.Reason.Code == ReasonCodes.COMMAND_RESOURCE_NOT_FOUND)
            {
                return false;
            }
        }

        public Task<DocumentCollection> GetRecipientsAsync(string listName, int skip = 0, int take = 100, CancellationToken cancellationToken = new CancellationToken())
        {
            var listIdentity = GetListIdentity(listName);

            var requestCommand = new Command()
            {
                Id = EnvelopeId.NewId(),
                To = DistributionListAddress,
                Method = CommandMethod.Get,
                Uri = new LimeUri($"/lists/{Uri.EscapeDataString(listIdentity.ToString())}/recipients?$skip={skip}&$take={take}")
            };

            return ProcessCommandAsync<DocumentCollection>(requestCommand, cancellationToken);
        }

        public Identity GetListIdentity(string listName)
        {
            if (string.IsNullOrWhiteSpace(listName))
            {
                throw new ArgumentException("The list name cannot be null or whitespace.", nameof(listName));
            }
            return new Identity(listName, DistributionListAddress.Domain);
        }

        public Task SendMessageAsync(string listName, Document content, string id = null, CancellationToken cancellationToken = new CancellationToken())
        {
            var message = new Message
            {
                Id = id,
                To = GetListIdentity(listName).ToNode(),
                Content = content
            };

            return Sender.SendMessageAsync(message, cancellationToken);
        }
    }
}