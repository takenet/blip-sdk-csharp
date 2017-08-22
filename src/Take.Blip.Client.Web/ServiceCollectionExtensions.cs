using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Take.Blip.Client.Web
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBlip(this IServiceCollection serviceCollection)
        {

            serviceCollection.AddSingleton<IEnvelopeBuffer, EnvelopeBuffer>();
            return serviceCollection;
        }
    }
}
