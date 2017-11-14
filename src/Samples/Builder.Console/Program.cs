using System.Threading.Tasks;
using Take.Blip.Builder;

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
            var flow = Flow.ParseFromJsonFile("Flow.json");

            // 1.0
        }
    }
}
