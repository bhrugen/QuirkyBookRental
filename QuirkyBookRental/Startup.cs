using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(QuirkyBookRental.Startup))]
namespace QuirkyBookRental
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
