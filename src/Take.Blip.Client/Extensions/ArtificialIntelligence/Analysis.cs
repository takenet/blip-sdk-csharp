using System.Runtime.Serialization;
using Lime.Protocol;

namespace Takenet.Iris.Messaging.Resources
{
    [DataContract]
    public class Analysis : Document
    {
        public const string MIME_TYPE = "application/vnd.talkservice.analysis+json";
        public static readonly MediaType MediaType = MediaType.Parse(MIME_TYPE);

        /// <summary>
        /// Initializes a new instance of the <see cref="Analysis"/> class.
        /// </summary>
        public Analysis() :
            base(MediaType)
        {
        }

        [DataMember(Name = "identifier")]
        public string Identifier { get; set; }

        [DataMember(Name = "sentence")]
        public string Sentence { get; set; }

        [DataMember(Name = "classifier")]
        public string Classifier { get; set; }

        [DataMember(Name = "confidence")]
        public double Confidence { get; set; }

        [DataMember(Name = "uncertain")]
        public bool Uncertain { get; set; }

        [DataMember(Name = "answer")]
        public string Answer { get; set; }

        [DataMember(Name = "valuenames")]
        public string[] ValueNames { get; set; }

        [DataMember(Name = "values")]
        public string[] Values { get; set; }

        [DataMember(Name = "guessname")]
        public string[] GuessName { get; set; }

        [DataMember(Name = "guessconfidence")]
        public string[] GuessConfidence { get; set; }

        [DataMember(Name = "guessanswer")]
        public string[] GuessAnswer { get; set; }

        [DataMember(Name = "diagnostic")]
        public string Diagnostic { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }
    }
}
