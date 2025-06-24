using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Take.Blip.Client.Extensions.Builder.Checkout.Documents
{
    public class ProductCheckout
    {
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "price")]
        public decimal Price { get; set; }

        [DataMember(Name = "quantity")]
        public int Quantity { get; set; }
    }
}
