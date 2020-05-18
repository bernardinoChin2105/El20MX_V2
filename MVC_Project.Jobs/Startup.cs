using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hangfire;
using Microsoft.Owin;
using Owin;
using MVC_Project.Jobs.App_Code;
using MVC_Project.Jobs;

[assembly: OwinStartupAttribute(typeof(MVC_Project.Jobs.Startup))]
namespace MVC_Project.Jobs
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            bool NotificationProcessEnabled = false;
            string JobName = string.Empty;
            string JobCron = string.Empty;
            string Dashboardurl = string.Empty;

            ConfigureAuth(app);

            try
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage("testConectionString");
                Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["Jobs.EnabledJobs"], out NotificationProcessEnabled);
                Dashboardurl = System.Configuration.ConfigurationManager.AppSettings["Jobs.Dashboard.Url"].ToString();

                if (NotificationProcessEnabled)
                {
                    JobName = System.Configuration.ConfigurationManager.AppSettings["Jobs.EnviarNotificaciones.Name"].ToString();
                    JobCron = System.Configuration.ConfigurationManager.AppSettings["Jobs.EnviarNotificaciones.Cron"].ToString();
                    RecurringJob.AddOrUpdate(JobName, () => DemoJob.DemoMethod(), JobCron, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));
                }

                app.UseHangfireDashboard(Dashboardurl, new DashboardOptions
                {
                    DisplayStorageConnectionString = false,
                    Authorization = new[] { new JobsAuthorizationFilter() },
                });
                app.UseHangfireServer();
            }
            catch
            {
                throw;
            }
        }
    }
}