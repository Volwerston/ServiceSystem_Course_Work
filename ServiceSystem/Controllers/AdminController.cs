using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceSystem.Models.Auxiliary_Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Controllers
{

    public class DateParams
    {
        public DateTime dt { get; set; }
    }

    public class AdminController : Controller
    {
        [AdminAuthorize]
        public ActionResult ErrorLog()
        {
            return View();
        }

        [AdminAuthorize]
        public string GetDateExceptions()
        {
            DateTime dt = new DateTime();
            DateTime.TryParse(Request.QueryString["dt"], out dt);
            if (dt == default(DateTime)) return "{}";

            string fileName = String.Format(Server.MapPath(String.Format("~/App_Data/ErrorLog/{0}/{1}/{2}.txt", dt.Year, dt.Month, dt.Day)));

            List<Exception> toReturn = new List<Exception>();

            if(new FileInfo(fileName).Exists)
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader rdr = new StreamReader(fs))
                    {
                        return rdr.ReadToEnd();
                    }
                }
            }
            else
            {
                return JsonConvert.SerializeObject(toReturn);
            }
        }
    }
}