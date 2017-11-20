using System;
using System.Text;

namespace Take.Blip.Builder.Actions
{
    public interface IActionProvider
    {
        IAction Get(string name);
    }
}
