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
                string cmdString = "INSERT INTO Applications VALUES(@ServiceId, @ServiceType, @Description, @Username, @DetailsId, @Status, GETDATE(), NULL, NULL);";

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@ServiceId", application.ServiceId);
                parameters.Add("@ServiceType", application.ServiceType);
                parameters.Add("@Description", application.Description);
                parameters.Add("@Username", User.Identity.Name);
                parameters.Add("@Status", "NO_BILL");

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
                    parameters.Add("@Duration", JsonConvert.SerializeObject(deadlineApp.Duration));

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

        [ActionName("GetApplicationById")]
        public HttpResponseMessage GetAppById(int id)
        {

            string serviceProviderName = "";
            Application application = new Application();
            List<PaymentMeasure> servicePaymentMeasures = new List<PaymentMeasure>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetApplicationById", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@Id", id);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    if(set.Tables[0].Rows.Count > 0)
                    {
                        application.Id = Convert.ToInt32(set.Tables[0].Rows[0]["Id"].ToString());
                        application.Username = set.Tables[0].Rows[0]["Username"].ToString();
                        application.StatusChangeDate = Convert.ToDateTime(set.Tables[0].Rows[0]["StatusChangeDate"].ToString());
                        application.Status = set.Tables[0].Rows[0]["Status"].ToString();
                        application.ServiceName = set.Tables[0].Rows[0]["Name"].ToString();
                        application.Description = set.Tables[0].Rows[0]["Description"].ToString();
                        serviceProviderName = set.Tables[0].Rows[0]["ServiceProviderName"].ToString();
                        servicePaymentMeasures = JsonConvert.DeserializeObject<List<PaymentMeasure>>(set.Tables[0].Rows[0]["PaymentMeasures"].ToString());
                        application.ConsultantName = set.Tables[0].Rows[0]["ConsultantName"].ToString();
                    }
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new Tuple<string, Application, List<PaymentMeasure>>(serviceProviderName, application, servicePaymentMeasures));        
        }

        [ActionName("GetUserApplications")]
        public HttpResponseMessage GetUserApplications()
        {
            List<Application> toReturn = new List<Application>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetUserApplications", connection);

                da.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@Username", User.Identity.Name);

                try
                {
                    DataSet set = new DataSet();

                    da.Fill(set);

                    foreach(DataRow row in set.Tables[0].Rows)
                    {
                        Application app = new Application();

                        app.Id = Convert.ToInt32(row["AppId"].ToString());
                        app.ServiceName = row["ServiceName"].ToString();
                        app.Username = User.Identity.Name;
                        app.Status = row["Status"].ToString();
                        app.StatusChangeDate = Convert.ToDateTime(row["StatusChangeDate"].ToString());
                        app.ConsultantName = row["ConsultantName"].ToString();

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
                        app.ConsultantName = row["ConsultantName"].ToString();


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