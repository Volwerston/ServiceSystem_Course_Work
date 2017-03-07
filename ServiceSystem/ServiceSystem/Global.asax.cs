using Microsoft.AspNet.SignalR;
using ServiceSystem.Controllers;
using ServiceSystem.Hubs;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ServiceSystem
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RegisterNotification();
            LastRun = DateTime.Now;
        }

        private void RegisterNotification()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["DBCS"].ConnectionString;

            string commandText = "Select dbo.Messages.ID, dbo.Messages.TEXT, dbo.Messages.SENDER_NAME,dbo.Messages.SENDING_TIME, dbo.Messages.DIALOGUE_ID From dbo.Messages";

            SqlDependency.Start(connectionString);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(commandText, connection))
                {
                    connection.Open();
                    var sqlDependency = new SqlDependency(command);

                    sqlDependency.OnChange += new OnChangeEventHandler(sqlDependency_OnChange);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                    }
                }
            }
        }
        DateTime LastRun;
        private void sqlDependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            if (e.Info == SqlNotificationInfo.Insert)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();

                List<Message> objList = new List<Message>();

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.BaseAddress = new Uri("http://localhost:49332/");

                    HttpResponseMessage response = client.PostAsJsonAsync("api/Dialogue/GetLatestDialogues", LastRun).Result;

                    if(response.IsSuccessStatusCode)
                    {
                        objList = response.Content.ReadAsAsync<List<Message>>().Result;
                    }
                }

                LastRun = DateTime.Now;

                foreach (var item in objList)
                {
                    context.Clients.All.addLatestNotification(item);
                }

            }
            RegisterNotification();
        }
    }
}
