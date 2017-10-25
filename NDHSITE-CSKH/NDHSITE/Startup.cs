using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(NDHSITE.Startup))]
namespace NDHSITE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
