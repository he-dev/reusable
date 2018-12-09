using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Reusable.Teapot.Internal
{
    internal class TeapotStartup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseMiddleware<TeapotMiddleware>();
        }
    }
}