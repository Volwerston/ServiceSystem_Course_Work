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

   
    public class ApplicationController : ApiController
    {
        public HttpResponseMessage Post([FromBody]Application application)
        {

            DeadlineApplication deadlineApp = application as DeadlineApplication;

            SessionApplication sessionApp = application as SessionApplication;

            UndefinedCourseApplication dcApp = application as UndefinedCourseApplication;

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Applications (SERVICE_ID, DESCRIPTION, TIME_DETAILS, USERNAME) "
                    + "VALUES (@ServiceId, @Description, @TimeDetails, @Username);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@ServiceId", application.ServiceId);

                cmd.Parameters.AddWithValue("@Description", application.Description);

                cmd.Parameters.AddWithValue("@Username", "");

                if(deadlineApp != null)
                {
                    Tuple<bool, DateTime, DateTime, long> tuple = new Tuple<bool, DateTime, DateTime, long>(
                        deadlineApp.IsByLastDate,
                        deadlineApp.StartDate,
                        deadlineApp.EndDate,
                        deadlineApp.Duration.Ticks
                        );

                    cmd.Parameters.AddWithValue("@TimeDetails", JsonConvert.SerializeObject(tuple));
                }
                else if(sessionApp != null)
                {
                    Tuple<DateTime, long, long> tuple = new Tuple<DateTime, long, long>(
                        sessionApp.Date,
                        sessionApp.StartTime.Ticks,
                        sessionApp.EndTime.Ticks
                        );

                    cmd.Parameters.AddWithValue("@TimeDetails", JsonConvert.SerializeObject(tuple));
                }
                else if(dcApp != null)
                {
                    cmd.Parameters.AddWithValue("@TimeDetails", JsonConvert.SerializeObject(dcApp.Days));
                }
                else
                {
                    cmd.Parameters.AddWithValue("@TimeDetails", DBNull.Value);
                }

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();

                    return Request.CreateResponse(HttpStatusCode.OK, "Application successfully added");
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }
    }
}