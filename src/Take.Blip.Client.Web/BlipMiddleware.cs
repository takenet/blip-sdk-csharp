using Microsoft.AspNetCore.Http;
using System;
using System.IO;
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

        public BlipMiddleware(
            RequestDelegate next, 
            IEnvelopeBuffer envelopeBuffer, 
            IEnvelopeSerializer envelopeSerializer)
        {
            _next = next;
            _envelopeBuffer = envelopeBuffer;
            _envelopeSerializer = envelopeSerializer;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.ContentType != null
                && context.Request.ContentType.Equals("application/json") &&
                (context.Request.Path.StartsWithSegments("/messages")
                 || context.Request.Path.StartsWithSegments("/notifications")))
            {
                try
                {
                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var json = await reader.ReadToEndAsync();
                        var envelope = _envelopeSerializer.Deserialize(json);
                        await _envelopeBuffer.SendAsync(envelope, context.RequestAborted);
                        context.Response.StatusCode = (int)HttpStatusCode.Accepted;
                    }
                }
                catch (Exception ex)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    context.Response.ContentType = "text/plain";
                    using (var writer = new StreamWriter(context.Response.Body))
                    {
                        await writer.WriteAsync(ex.ToString());
                    }
                }
                return;
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
