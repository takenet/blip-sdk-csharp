using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Take.Blip.Client;

namespace Take.Blip.Builder
{
    /// <summary>
    /// Defines a decorator of <see cref="ISender"/> that uses a <see cref="EnvelopeInterceptorFactory{TEnvelope}"/> for handling envelopes before sending.
    /// </summary>
    public sealed class InterceptorSenderDecorator : ISender
    {
        private readonly ISender _sender;

        public InterceptorSenderDecorator(ISender sender)
        {
            _sender = sender;
        }

        public Task SendMessageAsync(Message message, CancellationToken cancellationToken)
        {
            return _sender.SendMessageAsync(Intercept(message), cancellationToken);
        }

        public Task SendNotificationAsync(Notification notification, CancellationToken cancellationToken)
        {
            return _sender.SendNotificationAsync(Intercept(notification), cancellationToken);
        }

        public Task SendCommandAsync(Command command, CancellationToken cancellationToken)
        {
            return _sender.SendCommandAsync(Intercept(command), cancellationToken);
        }

        public Task<Command> ProcessCommandAsync(Command requestCommand, CancellationToken cancellationToken)
        {
            return _sender.ProcessCommandAsync(Intercept(requestCommand), cancellationToken);
        }

        private static TEnvelope Intercept<TEnvelope>(TEnvelope envelope) where TEnvelope : Envelope
        {
            return EnvelopeInterceptorFactory<TEnvelope>.Interceptor?.Invoke(envelope) ?? envelope;
        }
    }
    
    /// <summary>
    /// Allows defining an interceptor for envelopes in a scope.
    /// </summary>
    /// <typeparam name="TEnvelope"></typeparam>
    internal static class EnvelopeInterceptorFactory<TEnvelope> where TEnvelope : Envelope
    {
        private static readonly AsyncLocal<Func<TEnvelope, TEnvelope>> _interceptor = new AsyncLocal<Func<TEnvelope, TEnvelope>>();
        
        public static Func<TEnvelope, TEnvelope> Interceptor => _interceptor.Value;
        
        public static IDisposable Create(Func<TEnvelope, TEnvelope> interceptor)
        {
            if (_interceptor.Value != null)
            {
                throw new InvalidOperationException("The interceptor is already defined");
            }
            
            // TODO: Create a stack to support multiple levels of contexts
            _interceptor.Value = interceptor;
            return new ClearSenderInterceptor();
        }

        private sealed class ClearSenderInterceptor : IDisposable
        {
            public void Dispose()
            {
                _interceptor.Value = null;
            }
        }
    }
}