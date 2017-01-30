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

            string[] layouts = new string[] {"baseBlock", "deadlineBlock", "cooperationBlock", "sessionBlock",
                "courseBlock", "definedCourseBlock", "undefinedCourseBlock" };

            foreach (string layout in layouts)
            {
                StreamReader reader = new StreamReader("C:\\Users\\Юра\\Desktop\\ServiceSystem\\ServiceSystem\\Common\\service_" + layout + "Layout.txt");

                string buf = "";
                StringBuilder toInsert = new StringBuilder("");

                while ((buf = reader.ReadLine()) != null)
                {
                    toInsert.Append(buf);
                }

                toReturn.Add(layout, toInsert.ToString());

                reader.Close();
            }

            return toReturn;
        }


        [NonAction]
        public string GetApplicationBlockLayout(string blockType)
        {
            StreamReader reader = new StreamReader("C:\\Users\\Юра\\Desktop\\ServiceSystem\\ServiceSystem\\Common\\application_" + blockType + "BlockLayout.txt");

            StringBuilder builder = new StringBuilder("");

            string buf = "";

            while( (buf = reader.ReadLine()) != null)
            {
                builder.Append(buf);
            }

            return builder.ToString();
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
        public ActionResult Index(FormCollection collection, IEnumerable<HttpPostedFileBase> serviceAttachments)
        {
            Service service = ServiceManager.GenerateService(collection, serviceAttachments);

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:49332/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = null;

                if (collection["constructorType"] == "Deadline")
                {
                    response = client.PostAsJsonAsync("api/Deadline", service as Deadline).Result;
                }
                else if(collection["constructorType"] == "Session")
                {
                    response = client.PostAsJsonAsync("api/Session", service as Session).Result;
                }
                else if (collection["constructorType"] == "DefinedCourse")
                {
                    response = client.PostAsJsonAsync("api/DefinedCourse", service as DefinedCourse).Result;
                }
                else if (collection["constructorType"] == "UndefinedCourse")
                {
                    response = client.PostAsJsonAsync("api/UndefinedCourse", service as UndefinedCourse).Result;
                }
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

            ViewData["PARAMS"] = toReturn.Item2;

            return View(toReturn.Item1);
        }

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


        public ActionResult Foo()
        {
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