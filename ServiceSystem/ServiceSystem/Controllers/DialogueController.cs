using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceSystem.Controllers
{

    public class DialogueParams
    {
        public int DialogueId { get; set; }
        public DateTime dt { get; set; }
    }

    public class DialogueController : ApiController
    {
        public HttpResponseMessage PostDialogues([FromBody]Message mes)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "INSERT INTO Messages VALUES(@SenderName, GETDATE(), @Text, @RoomId);";

                SqlCommand cmd = new SqlCommand(cmdString, connection);

                cmd.Parameters.AddWithValue("@SenderName", mes.SenderName);
                cmd.Parameters.AddWithValue("@Text", mes.Text);
                cmd.Parameters.AddWithValue("@RoomId", mes.RoomId);

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


        [HttpGet]
        public HttpResponseMessage GetDialogues([FromUri] int dialogue_id)
        {
            List<Message> toReturn = new List<Message>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Messages WHERE DIALOGUE_ID=@Id", connection);

                da.SelectCommand.Parameters.AddWithValue("@Id", dialogue_id);

                DataSet set = new DataSet();

                da.Fill(set);

                foreach (DataRow row in set.Tables[0].Rows)
                {
                    Message message = new Message();

                    message.Id = Convert.ToInt32(row["ID"].ToString());
                    message.SendingTime = Convert.ToDateTime(row["SENDING_TIME"].ToString());
                    message.SenderName = row["SENDER_NAME"].ToString();
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
            List<Message> toReturn = new List<Message>();
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                string cmdString = null;

                Dictionary<string, object> parameters = new Dictionary<string, object>();

                if (dt == DateTime.MinValue)
                {
                    cmdString = "SELECT * FROM Messages";
                }
                else
                {
                    cmdString = "SELECT * FROM Messages WHERE (DATEDIFF(second, @date, SENDING_TIME) > 0)";

                    parameters.Add("@date", dt);
                }

                SqlDataAdapter da = new SqlDataAdapter(cmdString, connection);

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
                    message.SenderName = row["SENDER_NAME"].ToString();
                    message.Text = row["TEXT"].ToString();
                    message.RoomId = Convert.ToInt32(row["DIALOGUE_ID"].ToString());

                    toReturn.Add(message);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, toReturn);
        }
    }
}
