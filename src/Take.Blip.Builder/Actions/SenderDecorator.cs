using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder.Actions
{
    public class SenderDecorator : ISender
    {
        private readonly ISender _sender;

        public SenderDecorator(ISender sender)
        {
            _sender = sender;
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            return _sender.SendMessageAsync(message, cancellationToken);
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            return _sender.SendNotificationAsync(notification, cancellationToken);
        }

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            return _sender.SendCommandAsync(command, cancellationToken);
        }

        public Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            return _sender.ProcessCommandAsync(requestCommand, cancellationToken);
        }
    }

    public static class EnvelopeTransformationContext
    {
        private static readonly AsyncLocal<IEnvelopeInformation> _envelopeInformation = new AsyncLocal<IEnvelopeInformation>();

        public static IEnvelopeInformation Current => _envelopeInformation.Value;
        
        /// <summary>
        /// Creates a new context for the specified envelope type.
        /// </summary>
        /// <param name="envelopeInformation"></param>
        /// <returns></returns>
        public static IDisposable Create(IEnvelopeInformation envelopeInformation)
        {
            // TODO: Create a stack to support multiple levels of contexts
            _envelopeInformation.Value = envelopeInformation;
            return new ClearEnvelopeTransformationContext();
        }

        private sealed class ClearEnvelopeTransformationContext : IDisposable
        {
            public void Dispose()
            {
                _envelopeInformation.Value = null;
            }
        }
        
    }

    public interface IEnvelopeInformation
    {
        string Id { get; }
        
        Node From { get; }
        
        Node To { get; }
        
        IDictionary<string, string> Metadata { get; }
    }
}