using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using System.Net;

namespace Take.Blip.Client.Web
{
    public class BlipMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly IEnvelopeSerializer _envelopeSerializer;

        public BlipMiddleware(RequestDelegate next, IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer envelopeSerializer)
        {
            _next = next;
            _envelopeBuffer = envelopeBuffer;
            _envelopeSerializer = envelopeSerializer;
        }

        public async Task Invoke(HttpContext context, CancellationToken cancellationToken)
        {
            if (context.Request.ContentType != null
                && context.Request.ContentType.Equals("application/json") &&
                (context.Request.Path.StartsWithSegments("/messages")
                 || context.Request.Path.StartsWithSegments("/notifications")))
            {
                using (var reader = new StreamReader(context.Request.Body))
                {
                    var json = await reader.ReadToEndAsync();
                    var envelope = _envelopeSerializer.Deserialize(json);
                    await _envelopeBuffer.SendAsync(envelope, cancellationToken);

                    context.Response.StatusCode = (int)HttpStatusCode.Accepted;                    
                    return;
                }   
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
