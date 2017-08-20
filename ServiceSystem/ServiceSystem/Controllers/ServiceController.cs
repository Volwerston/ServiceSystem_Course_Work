using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Runtime.Caching;

using iTextSharp;
using iTextSharp.text.pdf;
using iTextSharp.text;
using ServiceSystem.Common;
using ServiceSystem.Models.Auxiliary_Classes;

namespace ServiceSystem.Controllers
{

    public class ServiceController : Controller
    {

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult ConfirmMail(string token)
        {
            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage message = client.GetAsync("api/Account/RegisterConfirmation?token=" + token.ToString()).Result;

                if (message.IsSuccessStatusCode)
                {
                    return View("MailConfirmSuccess");
                }
            }

            return View("MailConfirmError");
        }

        public ActionResult Index(string message)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Main", "Service", null);
            }

            try
            {
                if (message != null)
                {
                    string[] parameters = message.Split('|');
                    ViewData["message"] = parameters[1];
                    ViewData["message_state"] = parameters[0];
                }

                using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
                {
                    HttpResponseMessage response = client.GetAsync("api/ServiceApi/SystemStats").Result;

                    if (response.IsSuccessStatusCode)
                    {
                        SystemStats stats = response.Content.ReadAsAsync<SystemStats>().Result;

                        ViewData["UsersNumber"] = stats.UsersNumber;
                        ViewData["ServicesNumber"] = stats.ServicesNumber;
                        ViewData["ApplicationsNumber"] = stats.ApplicationsNumber;
                        ViewData["DialoguesNumber"] = stats.DialoguesNumber;
                    }
                    else
                    {
                        return View("Error");
                    }
                }


                return View();
            }
            catch
            {
                return View("Error");
            }
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(string email)
        {
            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage response = client.GetAsync("api/Account/ChangeUserPassword?email=" + email).Result;

                return RedirectToAction("Index");
            }
        }

        public ActionResult SetNewPassword(string request_id, string message = "")
        {
            ViewData["request_id"] = request_id;

            if(message != "")
            {
                ViewData["message"] = message;
            }

            return View();
        }

        [HttpPost]
        public ActionResult SetNewPassword(string request_id, string password, string confirm_password)
        {
            if (password != confirm_password || password == "" || confirm_password == "")
            {
                return RedirectToAction("SetNewPassword", "Service", new { request_id = request_id, message = "Passwords do not match" });
            }

            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage response = client.PostAsJsonAsync("api/Account/SetNewUserPassword",
                    new Tuple<string, string, string>(request_id, password, confirm_password)
                    ).Result;

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("SetNewPassword", "Service",new { request_id = request_id, message = "Internal Server Error" });
        }

        [HttpPost]
        [ActionName("Index")]
        public ActionResult Index_Post(string email, string password)
        {
            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage response = client.PostAsync(
                    "api/Account/CanLogin", new Tuple<string, string>(email, password), new JsonMediaTypeFormatter()
                    ).Result;

                if (response.IsSuccessStatusCode)
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

        [SampleAuthorize]
        public ActionResult Description()
        {
            return View();
        }

        // GET: Service
        [SampleAuthorize]
        public ActionResult Main(string message)
        {
            ViewData["message"] = message;

            Dictionary<string, string> constructorBlocks = DocumentGenerator.GetConstructorBlocks();

            foreach (var block in constructorBlocks)
            {
                ViewData[block.Key] = block.Value;
            }

            return View();
        }

        [SampleAuthorize]
        public ActionResult ServiceSearch()
        {
            try
            {
                Dictionary<string, string> categories = new Dictionary<string, string>();

                ObjectCache cache = MemoryCache.Default;

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
                {
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
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult OnSuccess(WMData wmData)
        {
            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage response = client.PostAsJsonAsync("api/Webmoney/PostTransactionData", wmData).Result;

                if(!response.IsSuccessStatusCode)
                {
                    try
                    {
                        string _message = response.Content.ReadAsAsync<string>().Result;

                        return RedirectToAction("Index", "Service", new { message = _message });
                    }
                    catch
                    {
                        return RedirectToAction("Index", "Service");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Service");
                }
            }
        }

        [HttpPost]
        public string GetServiceConsultants(int service_id)
        {
            List<ServiceConsultant> consultants = new List<ServiceConsultant>();

            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage message = client.GetAsync("api/ServiceConsultants/GetServiceConsultants?service_id=" + service_id.ToString()).Result;

                if(message.IsSuccessStatusCode)
                {
                    consultants = message.Content.ReadAsAsync<List<ServiceConsultant>>().Result;
                }
            }

            if(consultants.Count() == 0)
            {
                consultants = null;
            }

            return JsonConvert.SerializeObject(consultants);
        }

        [HttpPost]
        public ActionResult ServiceDetails(int serviceId, FormCollection collection)
        {
            Application toAdd = ApplicationManager.GenerateApplication(serviceId, collection);

            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
            {
                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/Application/PostApplication", toAdd, formatter).Result;
            }

            return RedirectToAction("ServiceDetails", new { id = serviceId });
        }

        public ActionResult ApplicationDetails(int id)
        {
            try
            {
                Tuple<string, Application, List<PaymentMeasure>> applicationCredentials = null;

                ObjectCache cache = MemoryCache.Default;

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
                {
                    HttpResponseMessage response = client.GetAsync("api/Application/GetApplicationById?id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        applicationCredentials = response.Content.ReadAsAsync<Tuple<string, Application, List<PaymentMeasure>>>().Result;
                    }
                }

                Tuple<byte[], Bill, Dictionary<string, string>> application_data = null;

                if (applicationCredentials.Item2.Status != "NO_BILL" && applicationCredentials.Item2.Status != "MAIN_PAID")
                {
                    application_data = DocumentGenerator.GetApplication(id, applicationCredentials.Item2, Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/");

                    if (application_data.Item2 != null)
                    {
                        ViewData["Bill"] = application_data.Item2;
                    }

                    if (application_data.Item3 != null)
                    {
                        if (application_data.Item3.ContainsKey("WMPurse"))
                        {
                            ViewData["WMPurse"] = application_data.Item3["WMPurse"];
                        }
                    }


                    if (application_data.Item1 != null)
                    {
                        var base64 = Convert.ToBase64String(application_data.Item1);
                        string toAdd = string.Format("data:application/pdf;base64, {0}", base64);
                        ViewData["FileSource"] = toAdd;
                    }

                }

                if (applicationCredentials != null &&
                    (User.Identity.Name == applicationCredentials.Item1 ||
                    User.Identity.Name == applicationCredentials.Item2.Username ||
                    User.Identity.Name == applicationCredentials.Item2.ConsultantName))
                {
                    ViewData["ServiceProviderName"] = applicationCredentials.Item1;
                    ViewData["ServicePaymentMeasures"] = applicationCredentials.Item3;
                    ViewData["ApplicationId"] = id;

                    return View(applicationCredentials.Item2);
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult ApplicationDetails(FormCollection collection)
        { 
            Bill toAdd = BillGenerator.GenerateBill(collection);

            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage response = client.PostAsync("api/Bill", toAdd, formatter).Result;

                if(response.IsSuccessStatusCode)
                { 
                    return RedirectToAction("ApplicationDetails", "Service", new { id = toAdd.ApplicationId });
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult DownloadAttachment(string attachment_name, int service_id)
        {
            AttachmentParams parameters = new AttachmentParams()
            {
                Name = attachment_name,
                ServiceId = service_id
            };

            MemoryCache cache = MemoryCache.Default;

            using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
            {
                HttpResponseMessage response = client.PostAsJsonAsync("/api/ServiceApi/GetAttachment", parameters).Result;

                if (response.IsSuccessStatusCode)
                {
                    byte[] data = response.Content.ReadAsAsync<byte[]>().Result;


                    FileContentResult result = new FileContentResult(data, "application/octet-stream")
                    {
                        FileDownloadName = attachment_name
                    };

                    return result;
                }
            }

            return new FileContentResult(null, "");
        }

        [HttpPost]
        public ActionResult Main(FormCollection collection, IEnumerable<HttpPostedFileBase> service_attachments)
        {
            Service service = ServiceManager.GenerateService(collection, service_attachments);
            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
            {
                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/ServiceApi/PostService", service, formatter).Result;
            }

            return RedirectToAction("Main", "Service", null);
        }

        [SampleAuthorize]
        public ActionResult ConsultantSearch(int service_id)
        {
            ViewData["ServiceId"] = service_id;

            return View();
        }

        [SampleAuthorize]
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

        [SampleAuthorize]
        public ActionResult ServiceDetails(int id)
        {
            try
            {
                Tuple<Service, Dictionary<string, string>> toReturn = null;
                ObjectCache cache = MemoryCache.Default;

                using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
                {
                    HttpResponseMessage response = client.GetAsync("api/ServiceApi?id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        toReturn = response.Content.ReadAsAsync<Tuple<Service, Dictionary<string, string>>>().Result;

                        HttpResponseMessage consultantsResponse = client.GetAsync("api/ServiceConsultants/?service_id=" + id).Result;

                        if (consultantsResponse.IsSuccessStatusCode)
                        {
                            List<ServiceConsultant> consultants = consultantsResponse.Content.ReadAsAsync<List<ServiceConsultant>>().Result;

                            if (consultants.Count() == 0)
                            {
                                consultants = null;
                            }

                            ViewData["consultants"] = consultants;
                        }

                        response = client.GetAsync("/api/ServiceApi/GetServiceAttachments?service_id=" + id.ToString()).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            List<string> names = response.Content.ReadAsAsync<List<string>>().Result;

                            ViewData["Attachments"] = names;
                        }
                    }

                    response = client.GetAsync("api/FAQ/Get?service_id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        List<FAQ> toPass = response.Content.ReadAsAsync<List<FAQ>>().Result;

                        ViewData["FAQ"] = toPass;
                    }

                    response = client.GetAsync("api/ServiceApi/GetMediaFiles?service_id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        List<MediaFile> files = response.Content.ReadAsAsync<List<MediaFile>>().Result;

                        ViewData["media_files"] = files;
                    }
                }

                if (!ViewData.ContainsKey("consultants"))
                {
                    ViewData["consultants"] = null;
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
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult GetConsultantApplications(string email, int year, int month, int service_id)
        {

            ConsultantChartParams parameters = new ConsultantChartParams
            {
                Year = year,
                Month = month,
                ServiceId = service_id,
                Email = email
            };

            ServiceConsultantsData data = null;

            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage message = client.PostAsJsonAsync("/api/ServiceConsultants/ConsultantStats", parameters).Result;

                if (message.IsSuccessStatusCode)
                {
                    data = message.Content.ReadAsAsync<ServiceConsultantsData>().Result;
                }
            }

            if (data != null)
            {
                return PartialView("_ConsultantStats", data);
            }

            return null;
        }

        [SampleAuthorize]
        public ActionResult ServiceCharts(int id)
        {
            try
            {
                using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
                {
                    HttpResponseMessage response = client.GetAsync("/api/ServiceApi/Get?id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        Tuple<Service, Dictionary<string, string>> result = response.Content.ReadAsAsync<Tuple<Service, Dictionary<string, string>>>().Result;

                        if (result != null && result.Item1.Username == User.Identity.Name)
                        {
                            ViewData["service_id"] = id;
                            return View();
                        }
                    }
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult GetMonthApplications(int year, int month, int service_id)
        {

            ChartParams parameters = new ChartParams
            {
                Year = year,
                Month = month,
                ServiceId = service_id
            };

            ServiceApplicationsData data = null;

            using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
            {
                HttpResponseMessage message = client.PostAsJsonAsync("/api/Application/ServiceStats", parameters).Result;

                if (message.IsSuccessStatusCode)
                {
                    data = message.Content.ReadAsAsync<ServiceApplicationsData>().Result;
                }
            }

            if (data != null)
            {
                return PartialView("_ServiceStats", data);
            }

            return null;       
        }

        [HttpPost]
        public ActionResult AddMediaFile(FormCollection collection, HttpPostedFileBase local_file)
        {
            MediaFile file = new MediaFile();

            file.ServiceId = Convert.ToInt32(collection["service_id"]);

            if (local_file != null)
            {
                DocumentGenerator.GenerateLocalDocument(local_file, collection["service_name"]);

                file.Path = "~/Common/MediaFiles/" + collection["service_name"] + "/" + local_file.FileName;
            }
            else
            {
                file.Path = collection["url_file"];
            }

            file.Description = collection["media_description"];
            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = WebApiClient.InitializeAuthorizedClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/", "Bearer", cache["access_token"] as string))
            {
                HttpResponseMessage response = client.PostAsJsonAsync("/api/ServiceApi/AddMediaFile", file).Result;
            }

                return RedirectToAction("ServiceDetails", new { id = Convert.ToInt32(collection["service_id"]) });
        }

        [SampleAuthorize]
        public ActionResult Dialogue(int id)
        {
            try
            {
                List<string> participants = null;

                using (HttpClient client = WebApiClient.InitializeClient(Request.Url.Scheme + "://" + Request.Url.Authority + Request.ApplicationPath.TrimEnd('/') + "/"))
                {
                    HttpResponseMessage response = client.GetAsync("api/Dialogue/GetDialogueParticipants?dialogue_id=" + id.ToString()).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        participants = response.Content.ReadAsAsync<List<string>>().Result;

                        response = client.GetAsync("api/Dialogue/IsFromApplication?dialogue_id=" + id.ToString()).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            ViewData["IsFromApplication"] = response.Content.ReadAsAsync<bool>().Result;
                        }
                    }
                }

                if (participants != null && participants.Where(x => x == User.Identity.Name).Count() > 0)
                {
                    ViewData["CustomerEmail"] = participants[0];
                    ViewData["ConsultantEmail"] = participants[1];
                    ViewData["room_id"] = id;

                    return View();
                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View("Error");
            }
        }
    }
}