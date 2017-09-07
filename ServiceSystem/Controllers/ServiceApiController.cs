using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceSystem.Models;
using ServiceSystem.Models.Auxiliary_Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class ServiceParams
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public bool DescriptionSearch { get; set; }
        public int LastID { get; set; }
    }

    public class SystemStats
    {
        public int UsersNumber { get; set; }
        public int ServicesNumber { get; set; }
        public int ApplicationsNumber { get; set; }
        public int DialoguesNumber { get; set; }
    }

    public class AttachmentParams
    {
        public string Name { get; set; }
        public int ServiceId { get; set; }
    }

    [Authorize]
    public class ServiceApiController : ApiController
    {

        [ActionName("UserCreatedServices")]
        public HttpResponseMessage GetServices()
        {
            List<Service> toReturn = new List<Service>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT ID, NAME, TYPE, CATEGORIES from Services WHERE USERNAME=@username;";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@username", User.Identity.Name);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        Service service = new Service();

                        service.Id = Convert.ToInt32(row["ID"].ToString());
                        service.Name = row["NAME"].ToString();
                        service.Type = row["TYPE"].ToString();
                        service.Category = row["CATEGORIES"].ToString();

                        toReturn.Add(service);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }

        [AllowAnonymous]
        [ActionName("SystemStats")]
        [HttpGet]
        public HttpResponseMessage GetSystemStats()
        {
            SystemStats toReturn = new SystemStats();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM dbo.GetServiceData()";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);
                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    DataRow row = set.Tables[0].Rows[0];

                    toReturn.ApplicationsNumber = Convert.ToInt32(row["CurrentAppsNumber"]) + Convert.ToInt32(row["DoneAppsNumber"]);
                    toReturn.ServicesNumber = Convert.ToInt32(row["ServicesNumber"]);
                    toReturn.UsersNumber = Convert.ToInt32(row["UsersNumber"]);
                    toReturn.DialoguesNumber = Convert.ToInt32(row["DialoguesNumber"]);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }

        [AllowAnonymous]
        public HttpResponseMessage GetMediaFiles([FromUri] int service_id)
        {
            List<MediaFile> toReturn = new List<MediaFile>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT ID, URL, DESCRIPTION FROM MediaFiles WHERE SERVICE_ID=@Id";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@Id", service_id);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        MediaFile file = new MediaFile();

                        file.Id = Convert.ToInt32(row["ID"].ToString());
                        file.Path = row["URL"].ToString();
                        file.Description = row["DESCRIPTION"].ToString();
                        file.ServiceId = service_id;

                        toReturn.Add(file);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }

        [ActionName("PostServiceParams")]
        public string Post([FromBody]ServiceParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                List<Service> toReturn = new List<Service>();

                string cmdString = "SELECT TOP 5 * FROM SERVICES WHERE (ID > @Id) AND ";

                List<string> filters = new List<string>();
                Dictionary<string, object> sqlParams = new Dictionary<string, object>();

                sqlParams.Add("@Id", parameters.LastID);

                if (parameters.Name != "")
                {
                    StringBuilder toAdd = new StringBuilder("");

                    if (parameters.DescriptionSearch)
                    {
                        toAdd.Append("((UPPER(NAME) LIKE '%' + UPPER(@Name) + '%')");
                        sqlParams.Add("@Name", parameters.Name);

                        toAdd.Append(" OR (DESCRIPTION LIKE '%' + @Description + '%'))");
                        sqlParams.Add("@Description", parameters.Name);
                    }
                    else
                    {
                        toAdd.Append("(UPPER(NAME) LIKE '%' + UPPER(@Name) + '%')");
                        sqlParams.Add("@Name", parameters.Name);
                    }

                    filters.Add(toAdd.ToString());
                }


                if (parameters.Category != "None")
                {
                    filters.Add("(CATEGORIES=@Category)");
                    sqlParams.Add("@Category", parameters.Category);
                }


                if (parameters.Type != "None")
                {
                    filters.Add("(TYPE = @Type)");
                    sqlParams.Add("@Type", parameters.Type);
                }

                if (filters.Count() != 0)
                {

                    for (int i = 0; i < filters.Count(); ++i)
                    {
                        cmdString += filters[i] + " AND ";
                    }
                }

                cmdString = cmdString.Substring(0, cmdString.Length - 4);

                cmdString += ";";


                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                foreach (var sqlParam in sqlParams)
                {
                    da.SelectCommand.Parameters.AddWithValue(sqlParam.Key, sqlParam.Value);
                }

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        Service service = new Service();

                        service.Id = Convert.ToInt32(row["ID"].ToString());
                        service.Name = row["NAME"].ToString();
                        service.Category = row["CATEGORIES"].ToString();
                        service.Username = row["USERNAME"].ToString();
                        service.Description = row["DESCRIPTION"].ToString();

                        toReturn.Add(service);
                    }

                    if (toReturn.Count() == 0)
                    {
                        toReturn = null;
                    }

                    return JsonConvert.SerializeObject(toReturn);
                }
                catch
                {
                    return null;
                }
            }
        }

        public HttpResponseMessage DeleteMediaFile([FromUri]int id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT URL FROM MediaFiles WHERE ID=@Id; DELETE FROM MediaFiles WHERE ID=@Id";

                SqlCommand cmd = new SqlCommand(cmdString, connection);
                cmd.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    string path = cmd.ExecuteScalar().ToString();

                    if (path[0] == '~')
                    {
                        FileInfo file = new FileInfo(path);

                        if (file.Exists)
                        {
                            file.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [ActionName("GetServiceAttachments")]
        public HttpResponseMessage GetServiceAttachments([FromUri] int service_id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT NAME FROM ServiceAttachments WHERE USED_SERVICE_ID=@ServiceId", connection);

                da.SelectCommand.Parameters.AddWithValue("@ServiceId", service_id);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    List<string> names = new List<string>();

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        names.Add(row["NAME"].ToString());
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, names);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

            }
        }

        [ActionName("GetAttachment")]
        [HttpPost]
        public HttpResponseMessage GetAttachment([FromBody] AttachmentParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT DATA FROM ServiceAttachments WHERE USED_SERVICE_ID=@ServiceId AND NAME = @Name", connection);

                da.SelectCommand.Parameters.AddWithValue("@Serviceid", parameters.ServiceId);
                da.SelectCommand.Parameters.AddWithValue("@Name", parameters.Name);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    byte[] toPass = (byte[])set.Tables[0].Rows[0]["DATA"];

                    return Request.CreateResponse(HttpStatusCode.OK, toPass);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [HttpPost]
        [ActionName("AddMediaFile")]
        public HttpResponseMessage AddMediaFile([FromBody]MediaFile file)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO MediaFiles VALUES(@Url, @Description, @ServiceId);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@Url", file.Path);
                cmd.Parameters.AddWithValue("@Description", file.Description);
                cmd.Parameters.AddWithValue("@ServiceId", file.ServiceId);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }


        [ActionName("UserNotifications")]
        public HttpResponseMessage GetUserNotifications()
        {
            List<Notification> notificatons = new List<Notification>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT ID, SENDER_NAME, RECIPIENT_NAME, SENDING_TIME, TEXT from Notifications WHERE RECIPIENT_NAME=@recipient ORDER BY SENDING_TIME DESC";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@recipient", User.Identity.Name);

                DataSet set = new DataSet();

                try
                {
                    connection.Open();

                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        Notification notification = new Notification();

                        notification.Id = Convert.ToInt32(row["ID"].ToString());
                        notification.SenderName = row["SENDER_NAME"].ToString();
                        notification.RecipientName = row["RECIPIENT_NAME"].ToString();
                        notification.SendingTime = Convert.ToDateTime(row["SENDING_TIME"].ToString());
                        notification.Text = row["TEXT"].ToString();

                        notificatons.Add(notification);
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            if (notificatons.Count() == 0)
            {
                notificatons = null;
            }

            return Request.CreateResponse(HttpStatusCode.OK, notificatons);
        }

        [HttpPost]
        [ActionName("DeleteNotification")]
        public HttpResponseMessage DeleteNotification([FromBody]int note_id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("DELETE FROM Notifications WHERE ID=@Id", connection);

                cmd.Parameters.AddWithValue("@Id", note_id);

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

        [Route("api/ServiceApi/ChangeServiceStatus")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostActivity(JObject res)
        {
            try
            {
                int toPut = res.Value<bool>("IsActive") ? 1 : 0;
                int id = res.Value<int>("Id");

                using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("Update ServiceInfo Set IsActive = @active Where ServiceId=@id", connection))
                    {

                        cmd.Parameters.AddWithValue("@active", toPut);
                        cmd.Parameters.AddWithValue("@id", id);

                        connection.Open();
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
            catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Internal Server Error");
            }
        }

        [ActionName("PostService")]
        public HttpResponseMessage Post([FromBody]Service service)
        {
            Session session = service as Session;

            Course course = service as Course;

            Deadline deadline = service as Deadline;

            string cmdString = "INSERT INTO Services VALUES(@Name, @Categories, @Description, @AdvancePercent, @Properties, @Type, @DetailsId, @Username); INSERT INTO ServiceInfo VALUES(IDENT_CURRENT('Services'), 1);";

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                parameters.Add("@Name", service.Name);
                parameters.Add("@Categories", service.Category);
                parameters.Add("@Description", service.Description);
                parameters.Add("@AdvancePercent", service.AdvancePercent);
                parameters.Add("@Properties", JsonConvert.SerializeObject(service.Properties));
                parameters.Add("@Username", User.Identity.Name);


                if (session != null)
                {
                    parameters.Add("@Type", "Session");

                    cmdString = cmdString.Replace("@DetailsId", "IDENT_CURRENT('Session_Details') + 1");

                    cmdString += "INSERT INTO Session_Details VALUES(@Days, @TimeMeasures, @PaymentMeasures);";

                    parameters.Add("@Days", JsonConvert.SerializeObject(session.Days));
                    parameters.Add("@TimeMeasures", JsonConvert.SerializeObject(session.TimeMeasure));
                    parameters.Add("@PaymentMeasures", JsonConvert.SerializeObject(session.PaymentMeasure));
                }
                else if (course != null)
                {
                    parameters.Add("@Type", "Course");

                    cmdString = cmdString.Replace("@DetailsId", "IDENT_CURRENT('Course_Details') + 1");

                    cmdString += "INSERT INTO Course_Details VALUES(@StartDate, @EndDate, @IsDefined, @CourseParams, @PaymentMeasures);";

                    parameters.Add("@StartDate", course.StartDate);
                    parameters.Add("@EndDate", course.EndDate);
                    parameters.Add("@IsDefined", course.IsDefined);
                    parameters.Add("@CourseParams", JsonConvert.SerializeObject(course.Parameters));
                    parameters.Add("@PaymentMeasures", JsonConvert.SerializeObject(course.PaymentMeasures));
                }
                else if (deadline != null)
                {
                    parameters.Add("@Type", "Deadline");

                    cmdString = cmdString.Replace("@DetailsId", "IDENT_CURRENT('Deadline_Details') + 1");

                    cmdString += "INSERT INTO Deadline_Details VALUES(@TimeMeasures, @PaymentMeasures);";

                    parameters.Add("@TimeMeasures", JsonConvert.SerializeObject(deadline.TimeMeasures));
                    parameters.Add("@PaymentMeasures", JsonConvert.SerializeObject(deadline.PaymentMeasures));
                }

                for (int i = 0; i < service.Attachments.Count(); ++i)
                {
                    cmdString += "INSERT INTO ServiceAttachments(NAME, DATA, USED_SERVICE_ID) "
                        + "VALUES(@Name" + (i + 1).ToString() + ", @Data" + (i + 1).ToString() + ", IDENT_CURRENT('Services'));";

                    parameters.Add("@Name" + (i + 1).ToString(), service.Attachments[i].Name);
                    parameters.Add("@Data" + (i + 1).ToString(), service.Attachments[i].Data);
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

                    return Request.CreateResponse(HttpStatusCode.OK, "Service successfully added");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }
            }
        }

        [AllowAnonymous]
        public Tuple<Service, Dictionary<string, string>> Get([FromUri]int id)
        {
            Tuple<Service, Dictionary<string, string>> toReturn = new Tuple<Service, Dictionary<string, string>>(new Service(), new Dictionary<string, string>());

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM Services INNER JOIN ServiceInfo ON Services.ID = ServiceInfo.ServiceId WHERE Services.ID=@Id";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataAdapter da = new SqlDataAdapter(cmd);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    if (set.Tables[0].Rows.Count > 0)
                    {
                        toReturn.Item1.Id = Convert.ToInt32(set.Tables[0].Rows[0]["ID"].ToString());
                        toReturn.Item1.Category = set.Tables[0].Rows[0]["CATEGORIES"].ToString();
                        toReturn.Item1.Type = set.Tables[0].Rows[0]["TYPE"].ToString();
                        toReturn.Item1.Description = set.Tables[0].Rows[0]["DESCRIPTION"].ToString();
                        toReturn.Item1.Name = set.Tables[0].Rows[0]["NAME"].ToString();
                        toReturn.Item1.Username = set.Tables[0].Rows[0]["USERNAME"].ToString();
                        toReturn.Item1.IsActive = bool.Parse(set.Tables[0].Rows[0]["IsActive"].ToString());
                        toReturn.Item1.AdvancePercent = double.Parse(set.Tables[0].Rows[0]["ADVANCE_PERCENT"].ToString());
                        toReturn.Item1.Properties = JsonConvert.DeserializeObject<List<Property>>(set.Tables[0].Rows[0]["PROPERTIES"].ToString());

                        Dictionary<string, string> details = DetailsStrategy.GetDetails(
                            Convert.ToInt32(set.Tables[0].Rows[0]["DETAILS_ID"].ToString()),
                            set.Tables[0].Rows[0]["TYPE"].ToString(),
                            connection
                            );

                        if (details != null)
                        {
                            foreach (var detail in details)
                            {
                                toReturn.Item2.Add(detail.Key, detail.Value);
                            }

                            return toReturn;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }

            }
        }
    }
}