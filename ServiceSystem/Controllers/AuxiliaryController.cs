using ServiceSystem.Models;
using ServiceSystem.Models.Auxiliary_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Controllers
{
    [Authorize]
    public class AuxiliaryController : Controller
    {
        public ActionResult GetServiceList()
        {
            List<Service> services = new List<Service>();

            try
            {

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", Request.Cookies["access_token"].Value))
                {
                    HttpResponseMessage msg = client.GetAsync("/api/ServiceApi/UserCreatedServices").Result;

                    if (msg.IsSuccessStatusCode)
                    {
                        services = msg.Content.ReadAsAsync<List<Service>>().Result;
                    }
                }
            }
            catch
            {
                services = new List<Service>();
            }

            return PartialView("_ServiceList", services);
        }

        public ActionResult GetNotificationList()
        {
            List<Notification> notifications = new List<Notification>();

            try
            {

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", Request.Cookies["access_token"].Value))
                {
                    HttpResponseMessage msg = client.GetAsync("/api/ServiceApi/UserNotifications").Result;

                    if (msg.IsSuccessStatusCode)
                    {
                        notifications = msg.Content.ReadAsAsync<List<Notification>>().Result;
                    }
                }
            }
            catch
            {
                notifications = new List<Notification>();
            }

            return PartialView("_NotificationList", notifications);
        }


        public ActionResult GetApplicationList()
        {
            List<Application> applications = new List<Application>();

            try
            {

                string name = Request.QueryString["name"];
                string type = Request.QueryString["type"];
                string url = type == "external" ? "/api/Application/GetServiceApplications" : "/api/Application/GetUserApplications";

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", Request.Cookies["access_token"].Value))
                {

                    HttpResponseMessage msg = client.GetAsync(url + "?name=" + name).Result;

                    if (msg.IsSuccessStatusCode)
                    {
                        applications = msg.Content.ReadAsAsync<List<Application>>().Result;
                    }
                }
            }
            catch
            {
                applications = new List<Application>();
            }

            return PartialView("_ApplicationList", applications);
        }



        public ActionResult GetDialogueList()
        {
            List<Dialogue> dialogues = new List<Dialogue>();

            try
            {

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", Request.Cookies["access_token"].Value))
                {
                    HttpResponseMessage msg = client.GetAsync("/api/Dialogue/UserDialogues").Result;

                    if (msg.IsSuccessStatusCode)
                    {
                        dialogues = msg.Content.ReadAsAsync<List<Dialogue>>().Result;
                    }
                }
            }
            catch
            {
                dialogues = new List<Dialogue>();
            }

            return PartialView("_DialogueList", dialogues);
        }
    }
}