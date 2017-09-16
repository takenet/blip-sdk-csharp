using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Builder;

namespace Take.Blip.Client.Web
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBlip(this IApplicationBuilder applicationBuilder) 
            => applicationBuilder.UseMiddleware<BlipMiddleware>();
    }
}
