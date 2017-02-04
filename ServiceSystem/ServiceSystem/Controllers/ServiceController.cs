using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Controllers
{
    public class ServiceController : Controller
    {

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


        // GET: Service
        public ActionResult Index()
        {
            Dictionary<string, string> layouts = GetConstructorBlocks();

            foreach (var layout in layouts)
            {
                ViewData[layout.Key] = layout.Value;
            }

            return View();
        }

        public ActionResult ServiceSearch()
        {
            Dictionary<string, string> categories = new Dictionary<string, string>();

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage message = client.GetAsync("api/Category").Result;

                if(message.IsSuccessStatusCode)
                {
                    categories = message.Content.ReadAsAsync<Dictionary<string, string>>().Result;
                }
            }

            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Оберіть категорію", Value = "None" });

            foreach(var category in categories)
            {
                items.Add(new SelectListItem { Text = category.Key, Value = category.Value });
            }

            ViewData["CATEGORIES"] = items;

            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection collection, IEnumerable<HttpPostedFileBase> service_attachments)
        {
            Service service = ServiceManager.GenerateService(collection, service_attachments);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/ServiceApi/PostService", service, formatter).Result;
            }

          return RedirectToAction("Index");
        }
 
        public ActionResult ServiceDetails(int id)
        {
            Tuple<Service, Dictionary<string, string>> toReturn = null;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");

                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

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

        /*
        public ActionResult ApplicationForm(int serviceId, string serviceType)
        {
            if (serviceType == "Deadline")
            {
                ViewData["LAYOUT"] = GetApplicationBlockLayout("BaseDeadline");
                ViewData["BY_LAST_DATE"] = GetApplicationBlockLayout("ByLastDateDeadline");
                ViewData["FROM_SOME_DATE"] = GetApplicationBlockLayout("FromSomeDateDeadline");
            }
            else
            {
                ViewData["LAYOUT"] = GetApplicationBlockLayout(serviceType);
                ViewData["BY_LAST_DATE"] = "";
                ViewData["FROM_SOME_DATE"] = "";
            }

            ViewData["SERVICE_ID"] = serviceId;

                return View();
        }
        */

        public ActionResult Foo()
        {
            Dictionary<string, string> toPass = GetConstructorBlocks();

            foreach(var node in toPass)
            {
                ViewData[node.Key] = node.Value;
            }

            return View();
        }


        [HttpPost]
        public ActionResult ApplicationForm(int serviceId, FormCollection collection)
        {

            Application toAdd = ApplicationManager.GenerateApplication(serviceId, collection);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/Application", toAdd,formatter).Result;
            }

                return RedirectToAction("Index");
        }
    }
}