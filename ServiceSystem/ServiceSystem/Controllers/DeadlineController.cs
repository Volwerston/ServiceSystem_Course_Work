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
    public class DeadlineController : ApiController
    {
        public HttpResponseMessage Post([FromBody]Deadline service)
        {
            return null;
            //using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            //{
            //    string cmdString = "INSERT INTO Services(NAME, DESCRIPTION, TYPE_NAME, CATEGORY_NAME, START_DATE, END_DATE, DETAILS_ID, MIN_PRICE, MAX_PRICE) "
            //    + "VALUES(@Name, @Description, @TypeName, @Category, @StartDate, @EndDate, IDENT_CURRENT('Deadline_Details') + 1, @MinPrice, @MaxPrice);";

            //    Dictionary<string, object> parameters = new Dictionary<string, object>();

            //    parameters.Add("@Name", service.Name);
            //    parameters.Add("@Description", service.Description);
            //    parameters.Add("@TypeName", "Deadline");
            //    parameters.Add("@Category", service.Category);
            //    parameters.Add("@StartDate", service.StartDate);
            //    parameters.Add("@EndDate", service.EndDate);
            //    parameters.Add("@MinPrice", service.MinPrice);
            //    parameters.Add("@MaxPrice", service.MaxPrice);

            //    cmdString += "INSERT INTO Deadline_Details(MIN_DURATION, MAX_DURATION) "
            //            + "VALUES(@MinDuration, @MaxDuration);";

            //        Deadline buf = service as Deadline;

            //        parameters.Add("@MinDuration", buf.MinDuration.Ticks);
            //        parameters.Add("@MaxDuration", buf.MaxDuration.Ticks);

                
            //    for (int i = 0; i < service.Attachments.Count(); ++i)
            //    {
            //        cmdString += "INSERT INTO ServiceAttachments(NAME, DATA, USED_SERVICE_ID) "
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

            //        return Request.CreateResponse(HttpStatusCode.OK, "Deadline service successfully created");
            //    }
            //    catch (Exception ex)
            //    {
            //        transaction.Rollback();

            //        return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            //    }
            //}
        }
    }
}