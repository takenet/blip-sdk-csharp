using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Take.Blip.Builder.Utils
{
    public interface ISensitiveInfoReplacer
    {
        string ReplaceCredentials(string value);
    }
}
