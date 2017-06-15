using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Client.Facts
{
    public class Scenario
    {
        public Setup[] With { get; set; }

        public Condition[] When { get; set; }

        public Action[] Do { get; set; }
    }

    public abstract class Setup : NamedEntity
    {
        public Setup(string name)
            : base(name)
        {

        }

        public abstract Task ArrangeAsync(IDictionary<string, object> context, CancellationToken cancellationToken);
    }

    public abstract class Condition : NamedEntity
    {
        public Condition(string name)
            : base(name)
        {
            
        }      

        public abstract Task<bool> IsMatchAsync(object factProperty, IDictionary<string, object> context, CancellationToken cancellationToken);

    }

    public abstract class Action : NamedEntity
    {
        public Action(string name)
            : base(name)
        {

        }

        public abstract Task ExecuteAsync(IDictionary<string, object> fact, IDictionary<string, object> context, CancellationToken cancellationToken);
    }

    public class NamedEntity
    {
        public NamedEntity(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Name { get; }
    }
}
