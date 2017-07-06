using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NDHAPI.Startup))]
namespace NDHAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
           // ConfigureAuth(app);
        }
    }
}
