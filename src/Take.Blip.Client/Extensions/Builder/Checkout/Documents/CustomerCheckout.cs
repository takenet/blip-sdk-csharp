using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Take.Blip.Client.Extensions.Builder.Checkout.Documents
{
    public class CustomerCheckout
    {
        [DataMember(Name = "identity")]
        public string Identity { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "surname")]
        public string Surname { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "documentType")]
        public string DocumentType { get; set; }

        [DataMember(Name = "document")]
        public string Document { get; set; }
    }

}
