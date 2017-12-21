using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace Take.Blip.Builder.Variables
{
    public class RandomVariableProvider : IVariableProvider
    {
        private const string CHARS = "abcdefghijklmnopqrstuvwxyz0123456789";
        private readonly Random _random;
        
        public RandomVariableProvider()
        {
            _random = new Random();
        }

        public VariableSource Source => VariableSource.Random;

        public Task<string> GetVariableAsync(string name, Identity user, CancellationToken cancellationToken) 
            => GetVariable(name).AsCompletedTask();

        private string GetVariable(string name)
        {
            switch (name)
            {
                case "guid":
                    return Guid.NewGuid().ToString();

                case "integer":
                    return _random.Next().ToString();

                case "string":
                    return new string(
                        Enumerable.Repeat(CHARS, 10)
                            .Select(s => s[_random.Next(s.Length)])
                            .ToArray());
            }

            return null;
        }
    }
}