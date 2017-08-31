using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(HAIAPI.Startup))]
namespace HAIAPI
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
          //  ConfigureAuth(app);
        }
    }
}
