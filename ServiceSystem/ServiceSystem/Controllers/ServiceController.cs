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

namespace ServiceSystem.Controllers
{

    public class ServiceController : Controller
    {
        private Dictionary<string, string> constructorBlocks;

        private void GeneratePDFDocument(Bill toAdd, BillType type)
        {
            string pathToAdd = String.Format(Server.MapPath(@"~\Common\Bills") + @"\{0}\{1}\{2}\{3}" 
                                            + (type == BillType.ADVANCE ? "_advance" : "_main") + ".pdf",
                                              toAdd.StatusChangeTime.Year.ToString(),
                                              toAdd.StatusChangeTime.Month.ToString(),
                                              toAdd.StatusChangeTime.Day.ToString(),
                                              toAdd.Id);

            DirectoryInfo info = new DirectoryInfo(Server.MapPath(@"~\Common"));

            if (info.GetDirectories().Where(x => x.Name == "Bills").Count() == 0)
            {
                info = info.CreateSubdirectory("Bills");
            }
            else
            {
                info = new DirectoryInfo(info.FullName + @"\Bills");
            }

            if (info.GetDirectories().Where(x => x.Name == toAdd.StatusChangeTime.Year.ToString()).Count() == 0)
            {
                info = info.CreateSubdirectory(toAdd.StatusChangeTime.Year.ToString());
            }
            else
            {
                info = new DirectoryInfo(info.FullName + @"\" + toAdd.StatusChangeTime.Year.ToString());
            }


            if (info.GetDirectories().Where(x => x.Name == toAdd.StatusChangeTime.Month.ToString()).Count() == 0)
            {
                info = info.CreateSubdirectory(toAdd.StatusChangeTime.Month.ToString());
            }
            else
            {
                info = new DirectoryInfo(info.FullName + @"\" + toAdd.StatusChangeTime.Month.ToString());
            }

            if (info.GetDirectories().Where(x => x.Name == toAdd.StatusChangeTime.Day.ToString()).Count() == 0)
            {
                info = info.CreateSubdirectory(toAdd.StatusChangeTime.Day.ToString());
            }
            else
            {
                info = new DirectoryInfo(info.FullName + @"\" + toAdd.StatusChangeTime.Day.ToString());
            }

            BaseFont baseFont = BaseFont.CreateFont(Server.MapPath("~/Common/arial.ttf"), BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
            Font font = new Font(baseFont, Font.DEFAULTSIZE, Font.NORMAL);

            Document document = new Document(PageSize.LETTER, 10, 10, 42, 35);

            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(pathToAdd, FileMode.Create));


            document.Open();

            Paragraph p = new Paragraph("Квитанція №" + toAdd.Id.ToString(), font);
            p.Alignment = Element.ALIGN_CENTER;
            p.SpacingAfter = 30;

            document.Add(p);

            PdfPTable table = new PdfPTable(2);

            table.AddCell(new Phrase("Послугу надав", font));
            table.AddCell(new Phrase(toAdd.ProviderLastName + " " + toAdd.ProviderFirstName + " " + toAdd.ProviderFatherName, font));

            table.AddCell(new Phrase("Замовник", font));
            table.AddCell(new Phrase(toAdd.CustomerLastName + " " + toAdd.CustomerFirstName + " " + toAdd.CustomerFatherName, font));

            table.AddCell(new Phrase("Ціна", font));

            if (type == BillType.ADVANCE)
            {
                table.AddCell((toAdd.Price * toAdd.AdvancePercent / Convert.ToDouble(100)).ToString());
            }
            else
            {
                table.AddCell((toAdd.Price * (1 - toAdd.AdvancePercent / Convert.ToDouble(100))).ToString());
            }

            table.AddCell(new Phrase("Валюта", font));

            switch (toAdd.Currency)
            {
                case "hryvnia":
                    table.AddCell(new Phrase("гривня", font));
                    break;
                case "dollar":
                    table.AddCell(new Phrase("євро", font));
                    break;
                case "euro":
                    table.AddCell(new Phrase("долар", font));
                    break;
            }

            table.AddCell(new Phrase("Сплатити до", font));

            if (type == BillType.ADVANCE)
            {
                table.AddCell(toAdd.AdvanceTimeLimit.ToString());
            }
            else
            {
                table.AddCell(toAdd.MainTimeLimit.ToString());
            }

            table.HorizontalAlignment = Element.ALIGN_CENTER;


            document.Add(table);

            document.Close();
        }

        private Tuple<byte[],Bill> GetApplication(int application_id)
        {

            Bill bill = null;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

                HttpResponseMessage response = client.GetAsync("api/Bill?application_id=" + application_id).Result;

                if (response.IsSuccessStatusCode)
                {
                    bill = response.Content.ReadAsAsync<Bill>().Result;
                }
            }

            if (bill != null)
            {
                string fullPath = String.Format(Server.MapPath(@"~\Common\Bills") + @"\{0}\{1}\{2}\{3}"
                                                + (bill.Status == "ADVANCE_PENDING" ? "_advance" : "_main") + ".pdf",
                                      bill.StatusChangeTime.Year.ToString(),
                                      bill.StatusChangeTime.Month.ToString(),
                                      bill.StatusChangeTime.Day.ToString(),
                                      bill.Id);

                FileInfo fileInfo = new FileInfo(fullPath);

                if (!fileInfo.Exists)
                {
                    if (bill.Status == "ADVANCE_PENDING")
                    {
                        GeneratePDFDocument(bill, BillType.ADVANCE);
                    }
                    else if (bill.Status == "MAIN_PENDING")
                    {
                        GeneratePDFDocument(bill, BillType.MAIN);
                    }

                    fileInfo = new FileInfo(fullPath);
                }

                long fileLength = fileInfo.Length;
                FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                BinaryReader br = new BinaryReader(fs);

                return new Tuple<byte[], Bill>(br.ReadBytes((int)fileLength), bill);
            }
            else
            {
                return new Tuple<byte[], Bill>(null, null);
            }
        }

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
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
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
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Main", "Service", null);
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

                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

                HttpResponseMessage response = client.GetAsync("api/Account/ChangeUserPassword?email=" + email).Result;

                if (response.IsSuccessStatusCode)
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

                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

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
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

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

        // GET: Service
        public ActionResult Main(string message)
        {
            ViewData["message"] = message;

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
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
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
        public ActionResult OnSuccess(WMData wmData)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
                HttpResponseMessage response = client.PostAsJsonAsync("api/Webmoney/PostTransactionData", wmData).Result;

                if(!response.IsSuccessStatusCode)
                {
                    try
                    {
                        string _message = response.Content.ReadAsAsync<string>().Result;

                        return RedirectToAction("Main", "Service", new { message = _message });
                    }
                    catch
                    {
                        return RedirectToAction("ServiceSearch", "Service");
                    }
                }
                else
                {
                    return RedirectToAction("InternalAccountPage", "Service");
                }
            }
        }

        [HttpPost]
        public ActionResult ServiceDetails(int serviceId, FormCollection collection)
        {
            Application toAdd = ApplicationManager.GenerateApplication(serviceId, collection);

            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
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
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                HttpResponseMessage response = client.GetAsync("api/Application/GetApplicationById?id=" + id.ToString()).Result;

                if(response.IsSuccessStatusCode)
                {
                    applicationCredentials = response.Content.ReadAsAsync<Tuple<string, Application, List<PaymentMeasure>>>().Result;
                }
            }

            if (User.Identity.Name == applicationCredentials.Item2.Username)
            {

                Tuple<byte[], Bill> application_data = null;

                try
                {
                    application_data = GetApplication(id);

                    if (application_data.Item2 != null)
                    {
                        ViewData["Bill"] = application_data.Item2;
                    }


                if (application_data.Item1 != null)
                {
                    var base64 = Convert.ToBase64String(application_data.Item1);
                    string toAdd = string.Format("data:application/pdf;base64, {0}", base64);
                    ViewData["FileSource"] = toAdd;
                }

                }
                catch (Exception ex)
                {
                    return RedirectToAction("Main", "Service", new { message = ex.Message });
                }
            }



            if (applicationCredentials != null && 
                (User.Identity.Name == applicationCredentials.Item1 ||
                User.Identity.Name == applicationCredentials.Item2.Username))
            {
                ViewData["ServiceProviderName"] = applicationCredentials.Item1;
                ViewData["ServicePaymentMeasures"] = applicationCredentials.Item3;
                ViewData["ApplicationId"] = id;

                return View(applicationCredentials.Item2);
            }

                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ApplicationDetails(FormCollection collection)
        { 
            Bill toAdd = Bill.GenerateBill(collection);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

                HttpResponseMessage response = client.PostAsJsonAsync("api/Bill", toAdd).Result;

                if(response.IsSuccessStatusCode)
                { 
                    return RedirectToAction("ApplicationDetails", "Service", new { id = toAdd.ApplicationId });
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Main(FormCollection collection, IEnumerable<HttpPostedFileBase> service_attachments)
        {
            Service service = ServiceManager.GenerateService(collection, service_attachments);
            ObjectCache cache = MemoryCache.Default;

            using (HttpClient client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");
                client.DefaultRequestHeaders.Clear();

                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", cache["access_token"] as string);

                JsonMediaTypeFormatter formatter = new JsonMediaTypeFormatter();

                formatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;

                HttpResponseMessage message = client.PostAsync("api/ServiceApi/PostService", service, formatter).Result;      
            }

            return RedirectToAction("Main", "Service", null);
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
                client.BaseAddress = new Uri("http://servicesystem.somee.com/");

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