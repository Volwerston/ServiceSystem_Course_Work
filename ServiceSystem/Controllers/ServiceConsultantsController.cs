using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace ServiceSystem.Controllers
{
    public class ConsultantParams
    {
        public string Name { get; set; }
        public string Organisation{get;set;}
        public int LastID { get; set; }
        public int ServiceId { get; set; }
    }

    public class MailConsultantParams
    {
        public int ServiceId { get; set; }
        public string Email { get; set; }
    }

    public class ConsultantChartParams
    {
        public string Email { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int ServiceId { get; set; }
    }

    public class FeedbackParams
    {
        public int ServiceId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Email { get; set; }
    }

    public class FeedbackInfo
    {
        public DateTime Date { get; set; }
        public int Mark { get; set; }
        public string Comment { get; set; }
    }

    public class ServiceConsultantsController : ApiController
    {
        [NonAction]
        public static void WriteMessage(int consultant_id, string account_id, SqlConnection connection)
        {
            if (account_id == null)
            {
                string cmdString = "SELECT A.EMAIL FROM ServiceConsultants A INNER JOIN AccountSettings B ON A.EMAIL = B.USERNAME WHERE A.Id = @id AND B.MAIL_MESSAGES_ENABLED = 1";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@id", consultant_id);

                DataSet set = new DataSet();

                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
                {
                    string email = set.Tables[0].Rows[0]["EMAIL"].ToString();

                    Dictionary<string, string> toPass = new Dictionary<string, string>();

                    toPass.Add("username", email);

                    MailManager.SendMessage(toPass, MailType.NOTIFICATION);
                }
            }
            else
            {
                string cmdString = "SELECT A.Email FROM AspNetUsers A INNER JOIN AccountSettings B ON A.Email = B.USERNAME WHERE A.Id=@id AND B.MAIL_MESSAGES_ENABLED = 1";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@id", account_id);

                DataSet set = new DataSet();

                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
                {
                    string email = set.Tables[0].Rows[0]["Email"].ToString();

                    Dictionary<string, string> toPass = new Dictionary<string, string>();

                    toPass.Add("username", email);

                    MailManager.SendMessage(toPass, MailType.NOTIFICATION);
                }
            }
        }


        [ActionName("PostConsultantParams")]
        public HttpResponseMessage Post([FromBody]ConsultantParams cParams)
        {
            StringBuilder cmdString = new StringBuilder("SELECT Id,FirstName, LastName, FatherName, Email, Organisation FROM AspNetUsers ");
            cmdString.Append(" WHERE Email NOT IN(Select EMAIL FROM ServiceConsultants WHERE SERVICE_ID = @serviceId) AND (Email != @provider)");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters.Add("@Id", cParams.LastID);
            parameters.Add("@serviceId", cParams.ServiceId);
            parameters.Add("@provider", User.Identity.Name);

            List<string> names = cParams.Name.Split(' ').ToList();

            names.RemoveAll(x => x == "");

            for(int i = 0; i < names.Count(); ++i)
            {
                cmdString.Append(" AND (");
                cmdString.AppendFormat("(UPPER(FirstName) LIKE '%' + UPPER(@name{0}) + '%') OR ", (i + 1).ToString());
                cmdString.AppendFormat("(UPPER(LastName) LIKE '%' + UPPER(@name{0}) + '%') OR ", (i + 1).ToString());
                cmdString.AppendFormat("(UPPER(FatherName) LIKE '%' + UPPER(@name{0}) + '%'))", (i + 1).ToString());

                parameters.Add(String.Format("@name{0}", (i + 1).ToString()), names[i]);
            }

            if(!String.IsNullOrEmpty(cParams.Organisation))
            {
                string org = cParams.Organisation.Trim();

                cmdString.Append(" AND (UPPER(Organisation) LIKE '%' + UPPER(@org) + '%')");

                parameters.Add("@org", org);
            }

            cmdString.Append(";");

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(cmdString.ToString(), connection);

                foreach(var kvp in parameters)
                {
                    da.SelectCommand.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                try
                {
                    DataSet set = new DataSet();
                    connection.Open();

                    da.Fill(set);

                    List<ServiceConsultant> toReturn = new List<ServiceConsultant>();

                    int i = cParams.LastID;

                    while (i < Math.Min(set.Tables[0].Rows.Count, cParams.LastID + 10))
                    {
                        ServiceConsultant consultant = new ServiceConsultant();

                        consultant.Id = set.Tables[0].Rows[i]["Id"].ToString();
                        consultant.Name = set.Tables[0].Rows[i]["FirstName"].ToString();
                        consultant.Surname = set.Tables[0].Rows[i]["LastName"].ToString();
                        consultant.FatherName = set.Tables[0].Rows[i]["FatherName"].ToString();
                        consultant.Email = set.Tables[0].Rows[i]["Email"].ToString();
                        consultant.Organisation = set.Tables[0].Rows[i]["Organisation"].ToString();
                        consultant.ServiceId = cParams.ServiceId;

                        toReturn.Add(consultant);

                        ++i;
                    }
                    

                    if(toReturn.Count == 0)
                    {
                        toReturn = null;
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(toReturn));
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [HttpPost]
        [ActionName("ConsultantStats")]
        public HttpResponseMessage ConsultantCharts([FromBody]ConsultantChartParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetConsultantApplicationsData", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@ServiceId", parameters.ServiceId);
                da.SelectCommand.Parameters.AddWithValue("@TimeLimit", new DateTime(parameters.Year, parameters.Month, 1));
                da.SelectCommand.Parameters.AddWithValue("@ConsultantEmail", parameters.Email);

                DataSet set = new DataSet();

                try
                {
                    ServiceConsultantsData data = new ServiceConsultantsData();

                    da.Fill(set);

                     if(set.Tables[0].Rows.Count > 0)
                    {
                        data.AdvancePendingApplications = Convert.ToInt32(set.Tables[0].Rows[0]["AdvancePendingCount"].ToString());
                        data.MainPendingApplications = Convert.ToInt32(set.Tables[0].Rows[0]["MainPendingCount"].ToString());
                        data.NoBillApplications = Convert.ToInt32(set.Tables[0].Rows[0]["NoBillCount"].ToString());
                        data.MainPaidApplications = Convert.ToInt32(set.Tables[0].Rows[0]["MainPaidCount"].ToString());
                        if (set.Tables[0].Rows[0]["AverageMark"].ToString() != "")
                        {
                            data.AverageMark = Convert.ToDouble(set.Tables[0].Rows[0]["AverageMark"].ToString());
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, data);
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [Authorize]
        [HttpPost]
        public HttpResponseMessage ConfirmConsultant([FromBody]bool confirm)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                StringBuilder cmdString = new StringBuilder();

                if(confirm)
                {
                    cmdString.Append("UPDATE ServiceConsultants SET Approved=1 WHERE EMAIL=@mail");
                }
                else
                {
                    cmdString.Append("DELETE FROM ServiceConsultants WHERE EMAIL=@mail");
                }

                SqlCommand cmd = new SqlCommand(cmdString.ToString(), connection);

                cmd.Parameters.AddWithValue("@mail", User.Identity.Name);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [Authorize]
        [HttpPost]
        public HttpResponseMessage DeleteConsultant([FromBody]int id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                try
                {
                    WriteMessage(id, null, connection);

                    SqlCommand cmd = new SqlCommand("spDeleteConsultant", connection);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    int code = (int)cmd.ExecuteScalar();

                    if (code == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.MethodNotAllowed,
                            "Impossible to delete consultant since he has not finished dialogues");
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, "OK");
                    }
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [HttpPost]
        [ActionName("AddMailConsultant")]
        public HttpResponseMessage AddMailConsultant([FromBody]MailConsultantParams parameters)
        {
            if (!string.IsNullOrEmpty(parameters.Email))
            {
                using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
                {
                    SqlCommand cmd = new SqlCommand("spAddMailConsultant", connection);

                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@ConsultantEmail", parameters.Email);
                    cmd.Parameters.AddWithValue("@ServiceId", parameters.ServiceId);

                    try
                    {
                        connection.Open();

                        int returnCode = (int)cmd.ExecuteScalar();

                        if (returnCode > 0)
                        {
                            Dictionary<string, string> toPass = new Dictionary<string, string>();
                            toPass.Add("username", parameters.Email);
                            toPass.Add("service_id", parameters.ServiceId.ToString());

                            MailManager.SendMessage(toPass, MailType.CONSULTANT_INVITATION);

                            return Request.CreateResponse(HttpStatusCode.OK, "User successfully invited");
                        }
                        else
                        {
                            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User is already a consultant of this project");
                        }
                    }
                    catch (Exception ex)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, "Fill in the email form, please");
        }

        [HttpPost]
        [ActionName("AddConsultant")]
        public HttpResponseMessage AddConsultant([FromBody]ServiceConsultant consultant)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                WriteMessage(0, consultant.Id, connection);
                SqlCommand cmd = new SqlCommand("spAddConsultant", connection);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserId", consultant.Id);
                cmd.Parameters.AddWithValue("@ServiceId", consultant.ServiceId);

                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                try
                {
                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }


            return Request.CreateResponse(HttpStatusCode.OK, "OK");
        }

        [ActionName("GetFeedbacks")]
        [HttpPost]
        public HttpResponseMessage GetFeedbacks([FromBody]FeedbackParams parameters)
        {
            List<FeedbackInfo> toReturn = new List<FeedbackInfo>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetConsultantFeedbacks", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@ConsultantEmail", parameters.Email);
                da.SelectCommand.Parameters.AddWithValue("@ServiceId", parameters.ServiceId);
                da.SelectCommand.Parameters.AddWithValue("@TimeLimit", new DateTime(parameters.Year, parameters.Month, 1));

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach(DataRow row in set.Tables[0].Rows)
                    {
                        FeedbackInfo info = new FeedbackInfo();

                        info.Mark = Convert.ToInt32(row["Mark"].ToString());
                        info.Date = Convert.ToDateTime(row["PaymentTime"].ToString());
                        info.Comment = row["Comment"].ToString();

                        toReturn.Add(info);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, toReturn);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [ActionName("GetServiceConsultants")]
        public HttpResponseMessage Get([FromUri]int service_id)
        {
            List<ServiceConsultant> toReturn = new List<ServiceConsultant>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetServiceConsultants", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                da.SelectCommand.Parameters.AddWithValue("@ServiceId", service_id);
                
                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        ServiceConsultant consultant = new ServiceConsultant();

                        consultant.Id = row["Id"].ToString();
                        consultant.ServiceId = Convert.ToInt32(row["ServiceId"].ToString());
                        consultant.Approved = Convert.ToBoolean(row["Approved"].ToString());
                        consultant.Name = row["Name"].ToString();
                        consultant.Surname = row["Surname"].ToString();
                        consultant.FatherName = row["FatherName"].ToString();
                        consultant.Email = row["Email"].ToString();
                        consultant.LastAssignmentTime = Convert.ToDateTime(row["LastAssignTime"]);
                        consultant.Organisation = row["Organisation"].ToString();

                        toReturn.Add(consultant);
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, toReturn);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
    }
}
