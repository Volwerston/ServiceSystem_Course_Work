using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace ServiceSystem.Controllers
{

   
    [Authorize]
    public class ApplicationController : ApiController
    {
        public HttpResponseMessage Post([FromBody]Application application)
        {

           DeadlineApplication deadlineApp = application as DeadlineApplication;

           SessionApplication sessionApp = application as SessionApplication;

           using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
           {
                string cmdString = "INSERT INTO Applications VALUES(@ServiceId, @ServiceType, @Description, @Username, @DetailsId);";

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@ServiceId", application.ServiceId);
                parameters.Add("@ServiceType", application.ServiceType);
                parameters.Add("@Description", application.Description);
                parameters.Add("@Username", "");

                if(deadlineApp != null)
                {
                    if(deadlineApp.StartTime.Year < 1753)
                    {
                        deadlineApp.StartTime = new DateTime(1900, 1, 1);
                    }

                    if (deadlineApp.EndTime.Year < 1753)
                    {
                        deadlineApp.EndTime = new DateTime(1900, 1, 1);
                    }

                    cmdString = cmdString.Replace("@DetailsId", "IDENT_CURRENT('Deadline_Application_Details') + 1");

                    cmdString += "INSERT INTO Deadline_Application_Details VALUES(@HasLastDate, @StartTime, @EndTime, @Duration);";

                    parameters.Add("@HasLastDate", deadlineApp.HasLastDate);
                    parameters.Add("@StartTime", deadlineApp.StartTime);
                    parameters.Add("@EndTime", deadlineApp.EndTime);
                    parameters.Add("@Duration", deadlineApp.Duration);

                }
                else if(sessionApp != null)
                {
                    cmdString = cmdString.Replace("@DetailsId", "IDENT_CURRENT('Session_Application_Details') + 1");

                    cmdString += "INSERT INTO Session_Application_Details VALUES (@SessionStart);";

                    parameters.Add("@SessionStart", sessionApp.SessionStartTime);
                }
                else
                {
                    cmdString = cmdString.Replace("@DetailsId", "0");
                }

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                foreach(var parameter in parameters)
                {
                    cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    transaction.Commit();

                    return Request.CreateResponse(HttpStatusCode.OK, "Application successfully added");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }
    }
}