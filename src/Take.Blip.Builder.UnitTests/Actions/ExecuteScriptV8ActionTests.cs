using Take.Blip.Builder.Actions.ExecuteScript;

namespace Take.Blip.Builder.UnitTests.Actions
{
    public class ExecuteScriptV8ActionTests : ExecuteScriptActionTests
    {
        protected override ExecuteScriptActionBase GetTarget()
        {
            return new ExecuteScriptV8Action();
        }
    }
}
