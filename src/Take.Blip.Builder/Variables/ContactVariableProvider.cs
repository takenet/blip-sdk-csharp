using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Resources;
using Lime.Protocol;
using Lime.Protocol.Network;
using Lime.Protocol.Serialization;
using Serilog;
using Take.Blip.Client.Extensions.Contacts;

namespace Take.Blip.Builder.Variables
{
    public class ContactVariableProvider : UserVariableProviderBase<Contact>
    {
        public const string CONTACT_EXTRAS_VARIABLE_PREFIX = "extras.";
        public const string CONTACT_SERIALIZED_PROPERTY = "serialized";
        public const string MIGRATION_FLAG_KEY = "MigratedToGuidIdentity"; // Nome da variável no builder
        public const string CONTACT_IDENTITY_VARIABLE = "identity"; // Nome da variável para o Identity
        public const string WHATSAPP_DOCUMENT = "wa.gw.msging.net"; // Tipo de documento para WhatsApp
        private readonly IContactExtension _contactExtension;
        private readonly IDocumentSerializer _documentSerializer;
        private readonly ILogger _logger;
        public ContactVariableProvider(IContactExtension contactExtension, IDocumentSerializer documentSerializer, ILogger logger)
            : base(VariableSource.Contact, ContextExtensions.CONTACT_KEY, logger)
        {
            _contactExtension = contactExtension;
            _documentSerializer = documentSerializer;
            _logger = logger;
        }

        protected override Task<Contact> GetAsync(Identity userIdentity, CancellationToken cancellationToken)
            => _contactExtension.GetAsync(userIdentity, cancellationToken);

        // MUDANÇA: Recebendo o IContext
        protected override string GetProperty(Contact item, string propertyName, IContext context)
        {
            // INTERCEPTAÇÃO: Regra de Fallback para o Identity
            bool isWhatsAppContact = item.Identity != null &&
                                       string.Equals(item.Identity.Domain, WHATSAPP_DOCUMENT, StringComparison.OrdinalIgnoreCase);

            _logger.Information("ContactVariableProvider: Property requested: {PropertyName}, IsWhatsAppContact: {IsWhatsAppContact}", propertyName, isWhatsAppContact);

            if (propertyName.Equals(CONTACT_IDENTITY_VARIABLE, StringComparison.OrdinalIgnoreCase) && isWhatsAppContact)
            {
                _logger.Information("ContactVariableProvider: Handling Identity property for WhatsApp contact.");
                bool isMigrated = false;

                // 2. Tenta ler a flag de configuração do fluxo
                if (context?.Flow?.Configuration != null &&
                    context.Flow.Configuration.TryGetValue(MIGRATION_FLAG_KEY, out var flagValue))
                {
                    bool.TryParse(flagValue, out isMigrated);
                }

                _logger.Information("ContactVariableProvider: Migration flag value: {IsMigrated}", isMigrated);
                // 3. Se NÃO migrou, aplica o fallback para o telefone
                if (!isMigrated)
                {
                    _logger.Information("ContactVariableProvider: Applying fallback to phone number for WhatsApp contact.");
                    return item.PhoneNumber ?? item.Identity?.ToString();
                }

                // Se NÃO for WhatsApp (outros canais) OU se a flag ESTIVER ATIVA (migrado), 
                // retorna o Identity original (GUID para o novo WA, ou o padrão dos outros canais).
                return item.Identity?.ToString();
            }

            // Comportamentos originais mantidos
            if (propertyName.StartsWith(CONTACT_EXTRAS_VARIABLE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var extraVariableName = propertyName.Remove(0, CONTACT_EXTRAS_VARIABLE_PREFIX.Length);
                if (item.Extras != null && item.Extras.TryGetValue(extraVariableName, out var extraVariableValue))
                {
                    return extraVariableValue;
                }
                return null;
            }
            else if (propertyName.Equals(CONTACT_SERIALIZED_PROPERTY, StringComparison.OrdinalIgnoreCase))
            {
                return _documentSerializer.Serialize(item);
            }

            // Repassa o context para a classe base
            return base.GetProperty(item, propertyName, context);
        }
    }
}