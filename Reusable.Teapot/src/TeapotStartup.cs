using Microsoft.AspNetCore.Builder;

namespace Reusable.Teapot
{
    internal class TeapotStartup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<TeapotMiddleware>();
        }
    }
}