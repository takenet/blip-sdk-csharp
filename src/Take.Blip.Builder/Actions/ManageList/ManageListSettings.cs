using System;
using Take.Blip.Builder.Models;

namespace Take.Blip.Builder.Actions.ManageList
{
    public class ManageListSettings : IValidable
    {
        public ManageListSettingsAction Action { get; set; }

        public string ListName { get; set; }

        public void Validate()
        {
            if (string.IsNullOrEmpty(ListName))
            {
                throw new ArgumentException($"The '{nameof(ListName)}' settings value is required for '{nameof(ManageListAction)}' action");
            }
        }
    }
}