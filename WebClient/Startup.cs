using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebClient.Startup))]

namespace WebClient
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.Map("/", spa =>
            {
                spa.Use((context, next) =>
                {
                    context.Request.Path = new PathString("/index.html");

                    return next();
                });

            });
        }
    }
}
