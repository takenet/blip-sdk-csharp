using System;
using System.Collections.Generic;
using System.Text;
using Lime.Protocol.Network;

namespace Take.Blip.Client
{
    /// <summary>
    /// Defines a service for creating <see cref="ITransport"/> instances.
    /// </summary>
    public interface ITransportFactory
    {
        /// <summary>
        /// Creates a transport for the specified URI.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        ITransport Create(Uri endpoint);
    }
}
