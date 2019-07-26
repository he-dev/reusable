using System.IO;
using Microsoft.AspNetCore.Builder;

namespace Reusable.Teapot
{
    internal class TeapotStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            //app.UsePathBase(Path.GetDirectoryName(typeof(TeapotStartup).Assembly.Location));
            app.UseMiddleware<TeapotMiddleware>();
        }
    }
}