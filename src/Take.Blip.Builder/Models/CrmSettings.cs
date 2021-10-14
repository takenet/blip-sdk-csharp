using System.ComponentModel.DataAnnotations;
using Take.Blip.Builder.Actions.CreateLead;

namespace Take.Blip.Builder.Models
{
    /// <summary>
    /// Crm processor settings
    /// </summary>
    public class CrmSettings : IValidable
    {
        /// <summary>
        /// Crm name
        /// </summary>
        public Crm Crm { get; set; }

        /// <summary>
        /// Lead identifier
        /// </summary>
        public string LeadId { get; set; }

        /// <summary>
        /// Lead object body
        /// </summary>
        public object LeadBody { get; set; }

        /// <summary>
        /// Return value variable
        /// </summary>
        public string ReturnValue { get; set; }

        /// <summary>
        /// Validate if lead request has a body
        /// </summary>
        public void Validate()
        {
            if (Crm.Equals(null))
            {
                throw new ValidationException(
                    $"The '{nameof(Crm)}' settings value is requied for 'Crm' action");
            }
        }
    }
}
