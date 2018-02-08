using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace ServiceSystem.Models.Auxiliary_Classes
{

    public static class ExceptionUtility
    {
        public static void LogException(HttpServerUtilityBase Server, DateTime dt, Exception exc)
        {
            StringBuilder directoryPath = new StringBuilder();
            StringBuilder filePath = new StringBuilder();

            filePath.AppendFormat(Server.MapPath("~/App_Data/ErrorLog/{0}/{1}/{2}.txt"),
                dt.Year, dt.Month, dt.Day);
            directoryPath.AppendFormat(Server.MapPath("~/App_Data/ErrorLog/{0}/{1}"), dt.Year, dt.Month, dt.Day);

            if (!Directory.Exists(directoryPath.ToString()))
            {
                Directory.CreateDirectory(directoryPath.ToString());
            }

            if (!(System.IO.File.Exists(filePath.ToString())))
            {
                System.IO.File.Create(filePath.ToString()).Close();
            }

            List<Exception> e = new List<Exception>();

            using (FileStream fs = new FileStream(filePath.ToString(), FileMode.Open, FileAccess.Read))
            {
                using (StreamReader rdr = new StreamReader(fs))
                {
                    if (fs.Length > 0)
                    {
                        e = JsonConvert.DeserializeObject<List<Exception>>(rdr.ReadToEnd());
                    }
                }
            }

            exc.Data.Add("ExactTime", DateTime.Now);
            e.Add(exc);

            using (FileStream fs = new FileStream(filePath.ToString(), FileMode.Truncate, FileAccess.Write))
            {
                byte[] arr = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(e));
                fs.Write(arr, 0, arr.Length);
            }
        }
    }
}