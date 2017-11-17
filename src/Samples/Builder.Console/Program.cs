using System.Threading.Tasks;
using SimpleInjector;
using Take.Blip.Builder;
using Take.Blip.Builder.Actions;
using Take.Blip.Builder.Hosting;
using Take.Blip.Builder.Models;

namespace Builder.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {

            // 1.0
            var container = new Container();
            container.RegisterBuilder();



            var flow = Flow.ParseFromJsonFile("Flow.json");
            flow.Validate();

        }
    }
}
