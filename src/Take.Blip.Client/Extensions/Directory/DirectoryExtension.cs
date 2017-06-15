using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Directory
{
    public class DirectoryExtension : ExtensionBase, IDirectoryExtension
    {
        public const string URI_FORMAT = "lime://{0}/accounts/{1}";
        public const string POSTMASTER_FORMAT = "postmaster@{0}";

        public DirectoryExtension(ISender sender) : base(sender)
        {
        }

        public Task<Account> GetDirectoryAccountAsync(Identity identity, CancellationToken cancellationToken)
        {
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (string.IsNullOrWhiteSpace(identity.Domain)) throw new ArgumentException("Invalid identity domain", nameof(identity));

            var requestCommand = CreateGetCommandRequest(
                string.Format(URI_FORMAT, identity.Domain, Uri.EscapeDataString(identity.Name)),
                Node.Parse(string.Format(POSTMASTER_FORMAT, identity.Domain)));

            return ProcessCommandAsync<Account>(requestCommand, cancellationToken);
        }
    }
}