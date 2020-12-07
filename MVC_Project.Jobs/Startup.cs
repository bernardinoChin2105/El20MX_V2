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
            string Dashboardurl = string.Empty;
            ConfigureAuth(app);

            try
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage("DBConnectionString");
                Boolean.TryParse(System.Configuration.ConfigurationManager.AppSettings["Jobs.EnabledJobs"], out NotificationProcessEnabled);
                Dashboardurl = System.Configuration.ConfigurationManager.AppSettings["Jobs.Dashboard.Url"].ToString();

                if (NotificationProcessEnabled)
                {
                    //JobName = System.Configuration.ConfigurationManager.AppSettings["Jobs.EnviarNotificaciones.Name"].ToString();
                    //JobCron = System.Configuration.ConfigurationManager.AppSettings["Jobs.EnviarNotificaciones.Cron"].ToString();

                    //Se agregan aca los N jobs que se necesiten
                    RecurringJob.AddOrUpdate("SATJob_SyncBills", () => SATJob.SyncBills(), "0 11 * * *", TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)"));
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