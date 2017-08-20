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

    public class ChartParams
    {
        public int ServiceId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public class ApplicationController : ApiController
    {
        [ActionName("PostApplication")]
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

                if (deadlineApp != null)
                {
                    if (deadlineApp.StartTime.Year < 1753)
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
                else if (sessionApp != null)
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

                foreach (var parameter in parameters)
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

        [HttpPost]
        [ActionName("ServiceStats")]
        public HttpResponseMessage ServiceStats([FromBody]ChartParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetServiceApplicationsData", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@ServiceId", parameters.ServiceId);
                da.SelectCommand.Parameters.AddWithValue("@TimeLimit", new DateTime(parameters.Year, parameters.Month, 1));

                DataSet set = new DataSet();

                try
                {
                    ServiceApplicationsData data = new ServiceApplicationsData();

                    data.Consultants = new Dictionary<string, string>();

                    da.Fill(set);

                    foreach(DataRow row in set.Tables[0].Rows)
                    {
                        data.Consultants.Add(row["Name"].ToString(), row["Email"].ToString());
                        data.AdvancePendingApplications = Convert.ToInt32(row["AdvancePendingCount"].ToString());
                        data.MainPendingApplications = Convert.ToInt32(row["MainPendingCount"].ToString());
                        data.NoBillApplications = Convert.ToInt32(row["NoBillCount"].ToString());
                        data.MainPaidApplications = Convert.ToInt32(row["MainPaidCount"].ToString());
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [HttpPost]
        public HttpResponseMessage PostMark([FromBody]CommentParams estimate)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Estimates VALUES(@AppId, @Mark, @Comment);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@AppId", estimate.ApplicationId);
                cmd.Parameters.AddWithValue("@Mark", estimate.Mark);
                cmd.Parameters.AddWithValue("@Comment", estimate.Comment);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
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

                    if (set.Tables[0].Rows.Count > 0)
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
                        application.DialogueId = Convert.ToInt32(set.Tables[0].Rows[0]["DialogueId"].ToString());

                        if (set.Tables[0].Rows[0]["Mark"] != DBNull.Value)
                        {
                            application.Mark = Convert.ToInt32(set.Tables[0].Rows[0]["Mark"].ToString());
                        }

                        application.FinalEstimate = set.Tables[0].Rows[0]["FinalEstimate"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new Tuple<string, Application, List<PaymentMeasure>>(serviceProviderName, application, servicePaymentMeasures));
        }

        [ActionName("GetUserApplications")]
        public HttpResponseMessage GetUserApplications(string name)
        {
            List<Application> toReturn = new List<Application>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetUserApplications", connection);

                da.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@Username", name);

                try
                {
                    DataSet set = new DataSet();

                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        Application app = new Application();

                        app.Id = Convert.ToInt32(row["AppId"].ToString());
                        app.ServiceName = row["ServiceName"].ToString();
                        app.Username = User.Identity.Name;
                        app.Status = row["Status"].ToString();
                        app.StatusChangeDate = Convert.ToDateTime(row["StatusChangeDate"].ToString());
                        app.ConsultantName = row["ConsultantName"].ToString();
                        app.DialogueId = Convert.ToInt32(row["DialogueId"].ToString());

                        toReturn.Add(app);
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }


        [ActionName("GetServiceApplications")]
        public HttpResponseMessage GetServiceApplications(string name)
        {
            List<Application> toReturn = new List<Application>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetApplicationsForUserServices", connection);

                da.SelectCommand.CommandType = System.Data.CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@Username", name);

                try
                {
                    DataSet set = new DataSet();

                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        Application app = new Application();

                        app.Id = Convert.ToInt32(row["ApplicationId"].ToString());
                        app.ServiceName = row["ServiceName"].ToString();
                        app.Username = row["Username"].ToString();
                        app.Status = row["Status"].ToString();
                        app.StatusChangeDate = Convert.ToDateTime(row["StatusChangeDate"].ToString());
                        app.ConsultantName = row["ConsultantName"].ToString();
                        app.DialogueId = Convert.ToInt32(row["DialogueId"].ToString());


                        toReturn.Add(app);
                    }

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }
    }
}