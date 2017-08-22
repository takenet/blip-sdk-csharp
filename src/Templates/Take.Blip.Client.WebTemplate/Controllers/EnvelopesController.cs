using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Lime.Protocol.Serialization;
using Microsoft.AspNetCore.Mvc;
using Take.Blip.Client.Web;

namespace Take.Blip.Client.WebTemplate.Controllers
{
    public class EnvelopesController : Controller
    {
        private readonly IEnvelopeBuffer _envelopeBuffer;
        private readonly IEnvelopeSerializer _envelopeSerializer;

        public EnvelopesController(IEnvelopeBuffer envelopeBuffer, IEnvelopeSerializer envelopeSerializer)
        {
            _envelopeBuffer = envelopeBuffer;
            _envelopeSerializer = envelopeSerializer;
        }

        [HttpPost("messages")]
        [HttpPost("notifications")]
        public Task Post([FromBody]string json, CancellationToken cancellationToken)
        {
            try
            {
                var envelope = _envelopeSerializer.Deserialize(json);
                return _envelopeBuffer.SendAsync(envelope, cancellationToken);
            }
            catch (ArgumentException)
            {
                return BadRequest().AsCompletedTask();
            }
        }
    }
}
