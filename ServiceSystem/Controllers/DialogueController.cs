using ServiceSystem.Hubs;
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

    public class DialogueParams
    {
        public int DialogueId { get; set; }
        public DateTime dt { get; set; }
    }

    public class ConsultantDialogueParams
    {
        public int ServiceId { get; set; }
        public string CustomerEmail { get; set; }
    }

    public class DialogueController : ApiController
    {

        private static void WriteMessage(Message mes, SqlConnection connection)
        {
            string cmdString = "SELECT A.USERNAME FROM DialogueUser A INNER JOIN AccountSettings B ON A.USERNAME = B.USERNAME WHERE A.ROOM_ID = @id AND B.MAIL_MESSAGES_ENABLED = 1";

            SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

            da.SelectCommand.Parameters.AddWithValue("@id", mes.RoomId);

            DataSet set = new DataSet();

            da.Fill(set);

            List<string> users = new List<string>();

            foreach (DataRow row in set.Tables[0].Rows)
            {
                string user = row["USERNAME"].ToString();

                users.Add(user);
            }


            foreach (string user in users)
            {
                if (!NotificationHub.IsOnline(user))
                {
                    Dictionary<string, string> toPass = new Dictionary<string, string>();

                    toPass.Add("username", user);
                    toPass.Add("dialogue_id", mes.RoomId.ToString());

                    MailManager.SendMessage(toPass, MailType.MESSAGE);
                }
            }
        }


        [ActionName("UserDialogues")]
        public HttpResponseMessage GetDialogues()
        {
            List<Dialogue> toReturn = new List<Dialogue>();
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = @"SELECT A.ID as Id,A.CREATION_TIME as CreationTime, COUNT(DISTINCT B.ID) as MessagesNumber,MAX(B.SENDING_TIME) as LastMessageTime,
                                     (SELECT TOP 1 SENDER_NAME + ': ' + TEXT FROM Messages WHERE SENDING_TIME=MAX(B.SENDING_TIME)) as LastMessage,
                                     COUNT(DISTINCT C.ID) as ParticipantsNumber
                                     FROM Dialogues A
                                     LEFT JOIN Messages B ON A.ID = B.DIALOGUE_ID
                                     INNER JOIN DialogueUser C ON A.ID = C.ROOM_ID
                                     WHERE A.ID IN (SELECT ROOM_ID FROM DialogueUser WHERE USERNAME = @username)
                                     GROUP BY A.ID, A.CREATION_TIME	";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@username", User.Identity.Name);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach(DataRow row in set.Tables[0].Rows)
                    {
                        Dialogue d = new Dialogue();

                        d.Id = Convert.ToInt32(row["Id"].ToString());
                        d.MessagesNumber = Convert.ToInt32(row["MessagesNumber"].ToString());
                        d.ParticipantsNumber = Convert.ToInt32(row["ParticipantsNumber"].ToString());

                        if(row["LastMessage"] != DBNull.Value)
                        {
                            StringBuilder builder = new StringBuilder(row["LastMessage"].ToString());
                            builder = builder.Replace("\\r", "\r");
                            builder = builder.Replace("\\n", "\n");
                            d.LastMessage = builder.ToString();
                        }
                        else
                        {
                            d.LastMessage = "-";
                        }
                        if(row["LastMessageTime"] != DBNull.Value)
                        {
                            d.LastChangeTime = Convert.ToDateTime(row["LastMessageTime"].ToString());
                        }
                        else
                        {
                            d.LastChangeTime = Convert.ToDateTime(row["CreationTime"].ToString());
                        }

                        toReturn.Add(d);
                    }
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            if(toReturn.Count() == 0)
            {
                toReturn = null;
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }

        public HttpResponseMessage PostDialogues([FromBody]Message mes)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Messages VALUES(@SenderName, GETDATE(), @Text, @RoomId);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@SenderName", mes.SenderEmail);
                cmd.Parameters.AddWithValue("@Text", mes.Text);
                cmd.Parameters.AddWithValue("@RoomId", mes.RoomId);

                try
                {
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();

                    WriteMessage(mes, connection);

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        [HttpGet]
        [ActionName("IsFromApplication")]
        public HttpResponseMessage IsFromApplication([FromUri]int dialogue_id)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT COUNT(ID) FROM Applications WHERE DIALOGUE_ID=@id";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@id", dialogue_id);

                try
                {
                    connection.Open();

                    bool toReturn = Convert.ToInt32(cmd.ExecuteScalar().ToString()) > 0;

                    return Request.CreateResponse(HttpStatusCode.OK, toReturn);
                }
                catch(Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }


        [HttpPost]
        [ActionName("CreateDialogue")]
        public HttpResponseMessage CreateDialogue([FromBody] ConsultantDialogueParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("spAssignConsultant", connection);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ServiceId", parameters.ServiceId);
                cmd.Parameters.AddWithValue("@CustomerEmail", parameters.CustomerEmail);

                SqlTransaction transaction = null;

                string mail_to = null;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();
                    cmd.Transaction = transaction;

                    mail_to = cmd.ExecuteScalar().ToString();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        transaction.Rollback();
                    }
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

                    Dictionary<string, string> toPass = new Dictionary<string, string>();

                    toPass.Add("username", mail_to);

                    MailManager.SendMessage(toPass, MailType.NOTIFICATION);

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
            }
        }


        [HttpPost]
        public HttpResponseMessage DeleteDialogue([FromBody]DeleteDialogueParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "DELETE FROM Dialogues WHERE ID = @id; DELETE FROM DialogueUser WHERE ROOM_ID=@id; DELETE FROM Messages WHERE DIALOGUE_ID=@id;";

                cmdString += "INSERT INTO ClientFeedback VALUES(@ConsultantName, @Mark, @Comment)";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@id", parameters.DialogueId);
                cmd.Parameters.AddWithValue("@ConsultantName", parameters.ConsultantName);
                cmd.Parameters.AddWithValue("@Mark", parameters.Mark);
                cmd.Parameters.AddWithValue("@Comment", parameters.Comment);

                SqlTransaction transaction = null;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    cmd.Transaction = transaction;

                    cmd.ExecuteNonQuery();

                    transaction.Commit();

                    return Request.CreateResponse(HttpStatusCode.OK, "OK");
                }
                catch(Exception ex)
                {
                    if(transaction != null)
                    {
                        transaction.Rollback();
                    }

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }

            }
        }

        [HttpGet]
        public HttpResponseMessage GetDialogues([FromUri] int dialogue_id)
        {
            List<Message> toReturn = new List<Message>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = @"SELECT B.LastName + ' ' + B.FirstName + ' ' + B.FatherName as FullName, A.DIALOGUE_ID, A.ID, A.TEXT, A.SENDER_NAME, A.SENDER_NAME, A.SENDING_TIME
                                     FROM Messages A
                                     INNER JOIN AspNetUsers B
                                     ON B.Email = A.SENDER_NAME
                                     WHERE DIALOGUE_ID=@Id";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@Id", dialogue_id);

                DataSet set = new DataSet();

                da.Fill(set);

                foreach (DataRow row in set.Tables[0].Rows)
                {
                    Message message = new Message();

                    message.SenderFullName = row["FullName"].ToString();
                    message.Id = Convert.ToInt32(row["ID"].ToString());
                    message.SendingTime = Convert.ToDateTime(row["SENDING_TIME"].ToString());
                    message.SenderEmail = row["SENDER_NAME"].ToString();
                    message.Text = row["TEXT"].ToString();
                    message.RoomId = Convert.ToInt32(row["DIALOGUE_ID"].ToString());

                    toReturn.Add(message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }

        public HttpResponseMessage GetDialogueParticipants([FromUri]int dialogue_id)
        {
            List<string> participants = new List<string>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT USERNAME FROM DialogueUser WHERE ROOM_ID=@Id";

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

                da.SelectCommand.Parameters.AddWithValue("@Id", dialogue_id);

                DataSet set = new DataSet();

                try
                {
                    da.Fill(set);

                    foreach (DataRow row in set.Tables[0].Rows)
                    {
                        participants.Add(row["USERNAME"].ToString());
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, participants);
                }
                catch (Exception ex)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }

        
        [HttpPost]
        public HttpResponseMessage GetLatestDialogues([FromBody] DateTime dt)
        {
            List<Message> messages = new List<Message>();
            List<string> participants = new List<string>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                Dictionary<string, object> parameters = new Dictionary<string, object>();

                parameters.Add("@time", dt);

                SqlDataAdapter da = new SqlDataAdapter("spGetLatestMessageInfo", connection);

                da.SelectCommand.CommandType = CommandType.StoredProcedure;

                foreach (var parameter in parameters)
                {
                    da.SelectCommand.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                DataSet set = new DataSet();

                da.Fill(set);

                foreach (DataRow row in set.Tables[0].Rows)
                {
                    Message message = new Message();

                    message.Id = Convert.ToInt32(row["ID"].ToString());
                    message.SendingTime = Convert.ToDateTime(row["SENDING_TIME"].ToString());
                    message.SenderEmail = row["SENDER_NAME"].ToString();
                    message.Text = row["TEXT"].ToString();
                    message.RoomId = Convert.ToInt32(row["DIALOGUE_ID"].ToString());
                    message.SenderFullName = row["FullName"].ToString();

                    messages.Add(message);
                }

                foreach(DataRow row in set.Tables[1].Rows)
                {
                    participants.Add(row["USERNAME"].ToString());
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, new Tuple<List<Message>, List<string>>(messages, participants));
        }
    }

    public class DeleteDialogueParams
    {
        public int DialogueId { get; set; }
        public int Mark { get; set; }
        public string Comment { get; set; }
        public string ConsultantName { get; set; }
    }
}
