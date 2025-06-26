using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Lime.Protocol;

namespace Take.Blip.Client.Extensions.Builder.Checkout.Documents
{
    [DataContract]
    public class CheckoutDocument : Document
    {
        public const string MIME_TYPE = "application/vnd.iris.builder.checkout+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        public CheckoutDocument() : base(MediaType)
        {

        }

        [DataMember(Name = "customer")]
        public CustomerCheckout Customer { get; set; }

        [DataMember(Name = "products")]
        public List<ProductCheckout> Products { get; set; }

        [DataMember(Name = "currency")]
        public string Currency { get; set; }

        [DataMember(Name = "channel")]
        public string Channel { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }
    }
}