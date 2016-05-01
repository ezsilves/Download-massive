using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Hangfire;
using Hangfire.Dashboard;

[assembly: OwinStartup(typeof(SATDownloadApp.Startup))]

namespace SATDownloadApp
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("App");


            //app.UseHangfireDashboard("/SatJobs");
            app.UseHangfireDashboard("/SatJobs", new DashboardOptions
            {
                AuthorizationFilters = new[] { new CustomAuthorizationFilter() }
            });
            app.UseHangfireServer();
        }
    }
}
