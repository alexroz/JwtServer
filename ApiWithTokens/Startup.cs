using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(ApiWithTokens.Startup))]

namespace ApiWithTokens
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureAuth(app);
        }
    }
}
