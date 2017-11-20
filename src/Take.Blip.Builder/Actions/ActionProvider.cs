using System.Collections.Generic;
using System.Linq;

namespace Take.Blip.Builder.Actions
{
    public class ActionProvider : IActionProvider
    {
        private readonly IDictionary<string, IAction> _actionDictionary;

        public ActionProvider(IEnumerable<IAction> actions)
        {
            _actionDictionary = actions
                .ToDictionary(a => a.Type, a => a);
        }

        public IAction Get(string name) => _actionDictionary[name];
    }
}