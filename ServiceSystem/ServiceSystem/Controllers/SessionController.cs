using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class SessionController : ApiController
    {
        public HttpResponseMessage Post([FromBody]Session service)
        {
            //using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            //{
            //    string cmdString = "INSERT INTO Services(NAME, DESCRIPTION, TYPE_NAME, CATEGORY_NAME, START_DATE, END_DATE, DETAILS_ID, MIN_PRICE, MAX_PRICE) "
            //    + "VALUES(@Name, @Description, @TypeName, @Category, @StartDate, @EndDate, IDENT_CURRENT('Session_Details') + 1, @MinPrice, @MaxPrice);";

            //    Dictionary<string, object> parameters = new Dictionary<string, object>();

            //    parameters.Add("@Name", service.Name);
            //    parameters.Add("@Description", service.Description);
            //    parameters.Add("@TypeName", "Session");
            //    parameters.Add("@Category", service.Category);
            //    parameters.Add("@StartDate", service.StartDate);
            //    parameters.Add("@EndDate", service.EndDate);
            //    parameters.Add("@MinPrice", service.SessionPrice);
            //    parameters.Add("@MaxPrice", service.SessionPrice);


            //        cmdString += "INSERT INTO Session_Details(SESSION_DURATION, TIME_DETAILS) "
            //            + "VALUES(@SessionDuration, @TimeDetails);";

            //        Session buf = service as Session;

            //        parameters.Add("@SessionDuration", buf.SessionDuration.Ticks);
            //        parameters.Add("@TimeDetails", Newtonsoft.Json.JsonConvert.SerializeObject(buf.Days));

            //    for (int i = 0; i < service.Attachments.Count(); ++i)
            //    {
            //        cmdString += "; INSERT INTO ServiceAttachments (NAME, DATA, USED_SERVICE_ID)"
            //            + "VALUES(@Name" + (i + 1).ToString() + ", @Data" + (i + 1).ToString() + ", IDENT_CURRENT('Services'));";

            //        parameters.Add("@Name" + (i + 1).ToString(), service.Attachments[i].Name);
            //        parameters.Add("@Data" + (i + 1).ToString(), service.Attachments[i].Data);
            //    }

            //    SqlCommand cmd = new SqlCommand(cmdString, connection);

            //    foreach (var parameter in parameters)
            //    {
            //        cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
            //    }

            //    connection.Open();
            //    SqlTransaction transaction = connection.BeginTransaction();
            //    cmd.Transaction = transaction;

            //    try
            //    {
            //        cmd.ExecuteNonQuery();
            //        transaction.Commit();

            //        return Request.CreateResponse(HttpStatusCode.OK, "Session service successfully created");
            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback();

            //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            //    }
            //}

            return null;
        }
    }
}