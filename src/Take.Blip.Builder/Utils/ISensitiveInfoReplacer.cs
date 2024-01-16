using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Take.Blip.Builder.Utils
{
    public interface ISensitiveInfoReplacer
    {
        string ReplaceCredentials(string value);
    }
}
