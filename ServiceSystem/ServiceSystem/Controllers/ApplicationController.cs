using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
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
                string cmdString = "INSERT INTO Applications VALUES(@ServiceId, @ServiceType, @Description, @Username, @DetailsId, @Status, GETDATE() );";

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@ServiceId", application.ServiceId);
                parameters.Add("@ServiceType", application.ServiceType);
                parameters.Add("@Description", application.Description);
                parameters.Add("@Username", User.Identity.Name);
                parameters.Add("@Status", "PENDING");

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


        [ActionName("GetServiceApplications")]
        public HttpResponseMessage GetServiceApplications()
        {
            List<Application> toReturn = new List<Application>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetApplicationsForUserServices", connection);

                da.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@Username", User.Identity.Name);

                try
                {
                    DataSet set = new DataSet();

                    da.Fill(set);

                    foreach(DataRow row in set.Tables[0].Rows)
                    {
                        Application app = new Application();

                        app.Id = Convert.ToInt32(row["ApplicationId"].ToString());
                        app.ServiceName = row["ServiceName"].ToString();
                        app.Username = row["Username"].ToString();
                        app.Status = row["Status"].ToString();
                        app.StatusChangeDate = Convert.ToDateTime(row["StatusChangeDate"].ToString());

                        toReturn.Add(app);
                    }

                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }
    }
}