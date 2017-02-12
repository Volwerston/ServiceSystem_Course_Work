﻿using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Runtime.Caching;

namespace ServiceSystem.Controllers
{

    public class ServiceController : Controller
    {
        private Dictionary<string, string> constructorBlocks;

        [NonAction]
        public Dictionary<string, string> GetConstructorBlocks()
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string[] paths = new string[] { "week_gradation_widget_hours", "week_gradation_widget_duration",
                                            "duration_measure_from_till", "duration_measure_from", "duration_measure_till",
                                            "service_session_block", "duration_measure_widget", "price_measure_widget",
                                            "default_property_widget", "service_deadline_block", "service_course_block",
                                            "week_gradation_widget"};

            foreach (var path in paths)
            {
                StreamReader reader = new StreamReader(Server.MapPath("~/Common/" + path + ".txt"));

                StringBuilder builder = new StringBuilder("");

                string buf = "";

                while ((buf = reader.ReadLine()) != null)
                {
                    builder.Append(buf);
                }

                toReturn.Add(path, builder.ToString());
            }

            return toReturn;
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult ConfirmMail(string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage message = client.GetAsync("api/Account/RegisterConfirmation?token=" + token.ToString()).Result;

                if (message.IsSuccessStatusCode)
                {
                    return View("MailConfirmSuccess");
                }
            }

            return View("MailConfirmError");
        }

        public ActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Main");
            }
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }



        [HttpPost]
        public ActionResult ChangePassword(string email)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://localhost:49332/");

                HttpResponseMessage response = client.GetAsync("api/Account/ChangeUserPassword?email=" + email).Result;

                if(response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("ChangePassword");
        }

        public ActionResult SetNewPassword(string request_id)
        {
            ViewData["request_id"] = request_id;

            return View();
        }

        [HttpPost]
        public ActionResult SetNewPassword(string request_id, string password, string confirm_password)
        {
            if (password != confirm_password)
            {
                return RedirectToAction("SetNewPassword", request_id);
            }

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://localhost:49332/");

                HttpResponseMessage response = client.PostAsJsonAsync("api/Account/SetNewUserPassword",
                    new Tuple<string, string, string>(request_id, password, confirm_password)
                    ).Result;

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("SetNewPassword", "Service", request_id);
        }

        [HttpPost]
        public ActionResult Index(string email, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri("http://localhost:49332/");

                HttpResponseMessage response = client.PostAsync(
                    "api/Account/CanLogin", new Tuple<string, string>(email, password), new JsonMediaTypeFormatter()
                    ).Result;

                if(response.IsSuccessStatusCode)
                {
                    ViewData["password"] = password;
                    ViewData["email"] = email;

                    return View("GetToken");
                }
            }

                return RedirectToAction("Index");
        }

        [HttpPost]
        public string GetToken(string token)
        {
            ObjectCache cache = MemoryCache.Default;

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration =
                DateTimeOffset.Now.AddHours(1);

            cache.Set("access_token", token, policy);

            return JsonConvert.SerializeObject("OK");
        }

        // GET: Service
        [Authorize]
        public ActionResult Main()
        {
            if (constructorBlocks == null)
            {
                constructorBlocks = GetConstructorBlocks();
            }

            foreach (var block in constructorBlocks)
            {
                ViewData[block.Key] = block.Value;
            }
            
            return View();
        }

        [Authorize]
        public ActionResult ServiceSearch()
        {
            Dictionary<string, string> categories = new Dictionary<string, string>();

            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                HttpResponseMessage message = client.GetAsync("api/Category").Result;

                if (message.IsSuccessStatusCode)
                {
                    categories = message.Content.ReadAsAsync<Dictionary<string, string>>().Result;
                }
            }

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Оберіть категорію", Value = "None" });

            foreach (var category in categories)
            {
                items.Add(new SelectListItem { Text = category.Key, Value = category.Value });
            }

            ViewData["CATEGORIES"] = items;

            return View();
        }

        [HttpPost]
        public ActionResult ServiceDetails(int serviceId, FormCollection collection)
        {
            Application toAdd = ApplicationManager.GenerateApplication(serviceId, collection);

            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);
  
                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/Application", toAdd, formatter).Result;
            }

            return RedirectToAction("ServiceDetails", new { id = serviceId });
        }

        public ActionResult ApplicationDetails(int id)
        {
            Tuple<string, Application, List<PaymentMeasure>> applicationCredentials = null;

            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.BaseAddress = new Uri("http://localhost:49332");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                HttpResponseMessage response = client.GetAsync("api/Application/GetApplicationById?id=" + id.ToString()).Result;

                if(response.IsSuccessStatusCode)
                {
                    applicationCredentials = response.Content.ReadAsAsync<Tuple<string, Application, List<PaymentMeasure>>>().Result;
                }
            }

            if(applicationCredentials != null && 
                (User.Identity.Name == applicationCredentials.Item1 ||
                User.Identity.Name == applicationCredentials.Item2.Username))
            {
                ViewData["ServiceProviderName"] = applicationCredentials.Item1;
                ViewData["ServicePaymentMeasures"] = applicationCredentials.Item3;

                return View(applicationCredentials.Item2);
            }

                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ApplicationDetails(FormCollection collection)
        {
            return null;
        }

        [HttpPost]
        public ActionResult Main(FormCollection collection, IEnumerable<HttpPostedFileBase> service_attachments)
        {
            Service service = ServiceManager.GenerateService(collection, service_attachments);
            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/ServiceApi/PostService", service, formatter).Result;      
            }

            return RedirectToAction("Main");
        }

        [Authorize]
        public ActionResult InternalAccountPage()
        {
            UserInfoViewModel model = new UserInfoViewModel
            {
                    Email = User.Identity.Name,
                    FirstName = User.Identity.GetFirstName(),
                    LastName = User.Identity.GetLastName(),
                    FatherName = User.Identity.GetFatherName(),
                    Organisation = User.Identity.GetOrganisation(),
                    HasRegistered = true,
                    LoginProvider = null
            };

                return View(model);
        }

        [Authorize]
        public ActionResult ServiceDetails(int id)
        {
            Tuple<Service, Dictionary<string, string>> toReturn = null;
            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                HttpResponseMessage response = client.GetAsync("api/ServiceApi?id=" + id.ToString()).Result;

                if(response.IsSuccessStatusCode)
                {
                    toReturn = response.Content.ReadAsAsync<Tuple<Service, Dictionary<string, string>>>().Result;
                }
            }

            if (toReturn.Item2 != null)
            {
                ViewData["PARAMS"] = toReturn.Item2;
            }
            else
            {
                ViewData["PARAMS"] = new Dictionary<string, string>();
            }

            return View(toReturn.Item1);
        }
    }
}