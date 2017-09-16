using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Lime.Protocol.Serialization.Newtonsoft;
using Lime.Protocol.Server;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Take.Blip.Client.Activation;

namespace Take.Blip.Client.Web
{
    public static class ServiceCollectionExtensions
    {
        public static readonly TimeSpan StartTimeout = TimeSpan.FromSeconds(60);

        public static IServiceCollection AddBlip(this IServiceCollection serviceCollection)
        {
            var applicationJsonPath =
                Path.Combine(
                    Path.GetDirectoryName(
                        Assembly.GetEntryAssembly().Location),
                    Bootstrapper.DefaultApplicationFileName);

            if (!File.Exists(applicationJsonPath))
            {
                throw new InvalidOperationException($"Could not find the application file in '{applicationJsonPath}'");
            }

            var application = Application.ParseFromJsonFile(applicationJsonPath);

            if (string.IsNullOrEmpty(application.Identifier))
            {
                var rawApplicationJson = File.ReadAllText(applicationJsonPath);
                var applicationJson = JObject.Parse(rawApplicationJson);
                var authorizationHeader = applicationJson["authorization"]?.ToString();
                if (authorizationHeader != null)
                {
                    var authorization = authorizationHeader.Split(' ')[1].FromBase64();
                    var identifierAndAccessKey = authorization.Split(':');
                    application.Identifier = identifierAndAccessKey[0];
                    application.AccessKey = identifierAndAccessKey[1].ToBase64();
                }
            }

            var workingDir = Path.GetDirectoryName(applicationJsonPath);
            if (string.IsNullOrWhiteSpace(workingDir)) workingDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            var envelopeBuffer = new EnvelopeBuffer();
            var envelopeSerializer = new JsonNetSerializer();
            var clientBuilder = new BlipClientBuilder(
                new WebTransportFactory(envelopeBuffer, envelopeSerializer, application));

            IStoppable stoppable;

            using (var cts = new CancellationTokenSource(StartTimeout))
            {
                stoppable = Bootstrapper
                    .StartAsync(
                        cts.Token, 
                        application,
                        clientBuilder,
                        new TypeResolver(workingDir))
                    .GetAwaiter()
                    .GetResult();
            }
            
            serviceCollection.AddSingleton(application);
            serviceCollection.AddSingleton(stoppable);
            serviceCollection.AddSingleton<IEnvelopeBuffer>(envelopeBuffer);
            serviceCollection.AddSingleton<IEnvelopeSerializer>(envelopeSerializer);
            return serviceCollection;
        }
    }
}

