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
    public class ServiceParams
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public double ServiceMinPrice { get; set; }
        public double ServiceMaxPrice { get; set; }
        public DateTime ServiceStartDate { get; set; }
        public DateTime ServiceEndDate { get; set; }
        public int LastID { get; set; }
    }

    public class DetailsStrategy
    {
        public static Dictionary<string, string> GetDetails(int detailsId, string serviceType, SqlConnection con)
        {

            Dictionary<string, string> toReturn = null;

            if(serviceType == "Deadline")
            {
                toReturn = GetDeadlineDetails(detailsId, con);
            }
            else if(serviceType == "Session")
            {
                toReturn = GetSessionDetails(detailsId, con);
            }
            else if(serviceType == "UndefinedCourse")
            {
                toReturn = GetUndefinedCourseDetails(detailsId, con);
            }
            else if(serviceType == "DefinedCourse")
            {
                toReturn = GetDefinedCourseDetails(detailsId, con);
            }

            toReturn.Add("SERVICE_TYPE", serviceType);

            return toReturn;
        }

        private static  Dictionary<string, string> GetDeadlineDetails(int detailsId, SqlConnection con)
        {
            string cmdString = "SELECT * FROM Deadline_Details WHERE ID=@Id";

            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if(set.Tables[0].Rows.Count > 0)
                {
                    TimeSpan minDuration = new TimeSpan(Convert.ToInt64(set.Tables[0].Rows[0]["MIN_DURATION"]));
                    TimeSpan maxDuration = new TimeSpan(Convert.ToInt64(set.Tables[0].Rows[0]["MAX_DURATION"]));

                    StringBuilder minDurationStr = new StringBuilder();
                    StringBuilder maxDurationStr = new StringBuilder();

                    int minMonths = 0, minWeeks = 0, minDays = 0;

                    if(minDuration.Days / 30 > 0)
                    {
                        minMonths = minDuration.Days / 30;
                        minDurationStr.Append(minMonths.ToString());
                        minDurationStr.Append(" місяців ");
                    }

                    if((minDuration.Days - 30*minMonths)/7 > 0)
                    {
                        minWeeks = (minDuration.Days - 30 * minMonths) / 7;
                        minDurationStr.Append(minWeeks.ToString());
                        minDurationStr.Append(" тижнів ");
                    }

                    if((minDuration.Days - 30*minMonths - 7*minWeeks) > 0)
                    {
                        minDays = minDuration.Days - 30 * minMonths - 7 * minWeeks;
                        minDurationStr.Append(minDays.ToString());
                        minDurationStr.Append(" днів ");
                    }

                    if(minDuration.Hours > 0)
                    {
                        minDurationStr.Append(minDuration.Hours.ToString());
                        minDurationStr.Append(" годин ");
                    }

                    if(minDuration.Minutes > 0)
                    {
                        minDurationStr.Append(minDuration.Minutes.ToString());
                        minDurationStr.Append(" хвилин ");
                    }
                    
                    int maxMonths = 0, maxWeeks = 0, maxDays = 0;

                    if (maxDuration.Days / 30 > 0)
                    {
                        maxMonths = maxDuration.Days / 30;
                        maxDurationStr.Append(maxMonths.ToString());
                        maxDurationStr.Append(" місяців ");
                    }

                    if ((maxDuration.Days - 30 * maxMonths) / 7 > 0)
                    {
                        maxWeeks = (maxDuration.Days - 30 * maxMonths) / 7;
                        maxDurationStr.Append(maxWeeks.ToString());
                        maxDurationStr.Append(" тижнів ");
                    }

                    if ((maxDuration.Days - 30 * maxMonths - 7 * maxWeeks) > 0)
                    {
                        maxDays = maxDuration.Days - 30 * maxMonths - 7 * maxWeeks;
                        maxDurationStr.Append(maxDays.ToString());
                        maxDurationStr.Append(" днів ");
                    }

                    if (maxDuration.Hours > 0)
                    {
                        maxDurationStr.Append(maxDuration.Hours.ToString());
                        maxDurationStr.Append(" годин ");
                    }

                    if (maxDuration.Minutes > 0)
                    {
                        maxDurationStr.Append(maxDuration.Minutes.ToString());
                        maxDurationStr.Append(" хвилин ");
                    }

                    toReturn.Add("Тривалість:", minDurationStr.ToString() + " - " + maxDurationStr.ToString());
                }
                else
                {
                    toReturn = null;
                }
            }
            catch
            {
                toReturn = null;
            }

            return toReturn;
        }

        private static Dictionary<string, string> GetSessionDetails(int detailsId, SqlConnection con)
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string cmdString = "SELECT * FROM Session_Details WHERE ID=@Id;";

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if(set.Tables[0].Rows.Count > 0)
                {
                    TimeSpan duration = new TimeSpan(Convert.ToInt64(set.Tables[0].Rows[0]["SESSION_DURATION"].ToString()));

                    toReturn.Add("Тривалість сеансу", duration.Minutes.ToString() + " хв");

                    Day[] days = JsonConvert.DeserializeObject<Day[]>(set.Tables[0].Rows[0]["TIME_DETAILS"].ToString());

                    string[] dayNames = new string[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

                    StringBuilder daySchedule = new StringBuilder("");

                    for (int i = 0; i < days.Count(); ++i)
                    {
                        daySchedule.Append(dayNames[(int)days[i].DayOfWeek - 1].ToString());
                        daySchedule.Append(": " + days[i].StartTime + " - " + days[i].EndTime + " <br/>");
                    }

                    toReturn.Add("Час надання послуги", daySchedule.ToString());
                }
                else
                {
                    toReturn = null;
                }
            }
            catch
            {
                toReturn = null;
            }

            return toReturn;
        }

        private static Dictionary<string, string> GetDefinedCourseDetails(int detailsId, SqlConnection con)
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string cmdString = "SELECT * FROM DefinedCourse_Details WHERE ID=@Id;";

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
                {
                    int clientsLimit = Convert.ToInt32(set.Tables[0].Rows[0]["CLIENTS_LIMIT"].ToString());

                    if(clientsLimit == 0)
                    {
                        toReturn.Add("Кількість учасників", "необмежена");
                    }
                    else
                    {
                        toReturn.Add("Кількість учасників", clientsLimit.ToString() + " осіб");
                    }


                    Day[] days = JsonConvert.DeserializeObject<Day[]>(set.Tables[0].Rows[0]["TIME_DETAILS"].ToString());

                    string[] dayNames = new string[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

                    StringBuilder daySchedule = new StringBuilder("");

                    for (int i = 0; i < days.Count(); ++i)
                    {
                        daySchedule.Append(dayNames[(int)days[i].DayOfWeek - 1].ToString());
                        daySchedule.Append(": " + days[i].StartTime + " - " + days[i].EndTime + " <br/>");
                    }

                    toReturn.Add("Час проведення курсу", daySchedule.ToString());
                }
                else
                {
                    toReturn = null;
                }
            }
            catch
            {
                toReturn = null;
            }

            return toReturn;
        }

        private static Dictionary<string, string> GetUndefinedCourseDetails(int detailsId, SqlConnection con)
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string cmdString = "SELECT * FROM UndefinedCourse_Details WHERE ID=@Id;";

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
                {
                    int[] days = JsonConvert.DeserializeObject<int[]>(set.Tables[0].Rows[0]["TIME_DETAILS"].ToString());

                    StringBuilder daySchedule = new StringBuilder("");
                
                    for(int i = 0; i < days.Count(); ++i)
                    {
                        daySchedule.Append("День " + (i + 1).ToString());

                        daySchedule.Append(": " + days[i].ToString() + " хвилин " + "<br/>");
                    }

                    toReturn.Add("Час проведення курсу:", daySchedule.ToString());
                }
                else
                {
                    toReturn = null;
                }
            }
            catch
            {
                toReturn = null;
            }

            return toReturn;
        }
    }

    public class ServiceApiController : ApiController
    {
        public string Post([FromBody]ServiceParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                List<Service> toReturn = new List<Service>();

                string cmdString = "SELECT TOP 1 * FROM SERVICES WHERE (ID > @Id) AND ";

                List<string> filters = new List<string>();
                Dictionary<string, object> sqlParams = new Dictionary<string, object>();

                sqlParams.Add("@Id", parameters.LastID);

                if(parameters.Name != "")
                {
                    filters.Add("(UPPER(NAME) LIKE '%' + UPPER(@Name) + '%')");
                    sqlParams.Add("@Name", parameters.Name);
                }

                if(parameters.Category != "None")
                {
                    filters.Add("(CATEGORY_NAME=@Category)");
                    sqlParams.Add("@Category", parameters.Category);
                }

                if(parameters.ServiceMinPrice != default(double))
                {
                    filters.Add("(MIN_PRICE<=@MinPrice)");
                    sqlParams.Add("@MinPrice", parameters.ServiceMinPrice);

                    filters.Add("(MAX_PRICE>=@MaxPrice)");
                    sqlParams.Add("@MaxPrice", parameters.ServiceMinPrice);
                }

                if(parameters.ServiceStartDate != default(DateTime))
                {
                    filters.Add("(DATEDIFF(day,START_DATE,@StartDate) >= 0)");
                    sqlParams.Add("@StartDate", parameters.ServiceStartDate);

                    filters.Add("(DATEDIFF(day,@EndDate,END_DATE) >= 0)");
                    sqlParams.Add("@EndDate", parameters.ServiceEndDate);
                }

                if(filters.Count() != 0)
                {

                    for(int i = 0; i < filters.Count(); ++i)
                    {
                        cmdString += filters[i] + " AND ";
                    }

                    cmdString = cmdString.Substring(0, cmdString.Length - 4);

                    cmdString += ";";
                }


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
                        service.Category = row["CATEGORY_NAME"].ToString();

                        service.Description = row["DESCRIPTION"].ToString();

                        //service.StartDate = Convert.ToDateTime(row["START_DATE"].ToString());
                        //service.EndDate = Convert.ToDateTime(row["END_DATE"].ToString());

                       // service.MinPrice = Convert.ToDouble(row["MIN_PRICE"].ToString());
                       // service.MaxPrice = Convert.ToDouble(row["MAX_PRICE"].ToString());

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

        public HttpResponseMessage Put([FromBody] Service service)
        {

            Session session = service as Session;

            Course course = service as Course;

            Deadline deadline = service as Deadline;

            return Request.CreateResponse(HttpStatusCode.OK, "");
        }

        public Tuple<Service, Dictionary<string,string>> Get([FromUri]int id)
        {
            Tuple<Service, Dictionary<string, string>> toReturn = new Tuple<Service, Dictionary<string, string>>(new Service(), new Dictionary<string, string>());

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {
                string cmdString = "SELECT * FROM Services WHERE ID=@Id";

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
                        toReturn.Item1.Category = set.Tables[0].Rows[0]["CATEGORY_NAME"].ToString();
                        toReturn.Item1.Description = set.Tables[0].Rows[0]["DESCRIPTION"].ToString();
                        //toReturn.Item1.EndDate = Convert.ToDateTime(set.Tables[0].Rows[0]["END_DATE"].ToString());
                        //toReturn.Item1.StartDate = Convert.ToDateTime(set.Tables[0].Rows[0]["START_DATE"].ToString());
                        //toReturn.Item1.Name = set.Tables[0].Rows[0]["NAME"].ToString();
                        //toReturn.Item1.MinPrice = Convert.ToDouble(set.Tables[0].Rows[0]["MIN_PRICE"].ToString());
                        //toReturn.Item1.MaxPrice = Convert.ToDouble(set.Tables[0].Rows[0]["MAX_PRICE"].ToString());

                        Dictionary<string, string> details = DetailsStrategy.GetDetails(
                            Convert.ToInt32(set.Tables[0].Rows[0]["DETAILS_ID"].ToString()),
                            set.Tables[0].Rows[0]["TYPE_NAME"].ToString(),
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