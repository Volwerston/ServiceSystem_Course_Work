using Newtonsoft.Json;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
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
            else if(serviceType == "Course")
            {
                toReturn = GetCourseDetails(detailsId, con);
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
                    List<TimeMeasure> measures = JsonConvert.DeserializeObject<List<TimeMeasure>>(set.Tables[0].Rows[0]["TIME_MEASURES"].ToString());

                    toReturn.Add("Тривалість", StringifyTimeMeasures(measures));

                    List<PaymentMeasure> pMeasures = JsonConvert.DeserializeObject<List<PaymentMeasure>>
                        (set.Tables[0].Rows[0]["PAYMENT_MEASURES"].ToString());

                    toReturn.Add("Вартість", StringifyPaymentMeasures(pMeasures));
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

        private static string StringifyTimeMeasures(List<TimeMeasure> measures)
        {
            StringBuilder durationColumnBuilder = new StringBuilder("");

            foreach (TimeMeasure measure in measures)
            {

                StringBuilder minDurationStr = new StringBuilder();
                StringBuilder maxDurationStr = new StringBuilder();

                int minMonths = 0, minWeeks = 0, minDays = 0;

                if (measure.MinDuration.Days / 30 > 0)
                {
                    minMonths = measure.MinDuration.Days / 30;
                    minDurationStr.Append(minMonths.ToString());
                    minDurationStr.Append(" місяців ");
                }

                if ((measure.MinDuration.Days - 30 * minMonths) / 7 > 0)
                {
                    minWeeks = (measure.MinDuration.Days - 30 * minMonths) / 7;
                    minDurationStr.Append(minWeeks.ToString());
                    minDurationStr.Append(" тижнів ");
                }

                if ((measure.MinDuration.Days - 30 * minMonths - 7 * minWeeks) > 0)
                {
                    minDays = measure.MinDuration.Days - 30 * minMonths - 7 * minWeeks;
                    minDurationStr.Append(minDays.ToString());
                    minDurationStr.Append(" днів ");
                }

                if (measure.MinDuration.Hours > 0)
                {
                    minDurationStr.Append(measure.MinDuration.Hours.ToString());
                    minDurationStr.Append(" годин ");
                }

                if (measure.MinDuration.Minutes > 0)
                {
                    minDurationStr.Append(measure.MinDuration.Minutes.ToString());
                    minDurationStr.Append(" хвилин ");
                }

                int maxMonths = 0, maxWeeks = 0, maxDays = 0;

                if (measure.MaxDuration.Days / 30 > 0)
                {
                    maxMonths = measure.MaxDuration.Days / 30;
                    maxDurationStr.Append(maxMonths.ToString());
                    maxDurationStr.Append(" місяців ");
                }

                if ((measure.MaxDuration.Days - 30 * maxMonths) / 7 > 0)
                {
                    maxWeeks = (measure.MaxDuration.Days - 30 * maxMonths) / 7;
                    maxDurationStr.Append(maxWeeks.ToString());
                    maxDurationStr.Append(" тижнів ");
                }

                if ((measure.MaxDuration.Days - 30 * maxMonths - 7 * maxWeeks) > 0)
                {
                    maxDays = measure.MaxDuration.Days - 30 * maxMonths - 7 * maxWeeks;
                    maxDurationStr.Append(maxDays.ToString());
                    maxDurationStr.Append(" днів ");
                }

                if (measure.MaxDuration.Hours > 0)
                {
                    maxDurationStr.Append(measure.MaxDuration.Hours.ToString());
                    maxDurationStr.Append(" годин ");
                }

                if (measure.MaxDuration.Minutes > 0)
                {
                    maxDurationStr.Append(measure.MaxDuration.Minutes.ToString());
                    maxDurationStr.Append(" хвилин ");
                }

                durationColumnBuilder.Append("<h4> Від: ");
                durationColumnBuilder.Append(minDurationStr.ToString());
                durationColumnBuilder.Append("</h4> <h4> До: ");
                durationColumnBuilder.Append(maxDurationStr.ToString());
                durationColumnBuilder.Append(" </h4>");

                if (measure.Description != null)
                {
                    durationColumnBuilder.Append("<p>");
                    durationColumnBuilder.Append(measure.Description);
                    durationColumnBuilder.Append("</p>");
                }

                durationColumnBuilder.Append("<br/>");
                durationColumnBuilder.Append("<br/>");
                durationColumnBuilder.Append("<br/>");
            }

            return durationColumnBuilder.ToString();
        }

        private static string StringifyPaymentMeasures(List<PaymentMeasure> measures)
        {
            StringBuilder priceColumnBuilder = new StringBuilder("");

            foreach (PaymentMeasure measure in measures)
            { 
                string toAppend = String.Format("{0} (currency) / {1}", measure.PricePerUnit.ToString(),
                 measure.ValueMeasure);

                if (measure.Currency == "hryvnia")
                {
                    toAppend = toAppend.Replace("(currency)", CultureInfo.GetCultureInfo("uk-UA").NumberFormat.CurrencySymbol);
                }
                else if (measure.Currency == "dollar")
                {
                    toAppend = toAppend.Replace("(currency)", CultureInfo.GetCultureInfo("en-US").NumberFormat.CurrencySymbol);
                }
                else
                {
                    toAppend = toAppend.Replace("(currency)", CultureInfo.GetCultureInfo("en-GB").NumberFormat.CurrencySymbol);
                }

                priceColumnBuilder.Append("<h4>");
                priceColumnBuilder.Append(toAppend);
                priceColumnBuilder.Append("</h4>");

                if (measure.Description != null)
                {
                    priceColumnBuilder.Append("<p>");
                    priceColumnBuilder.Append(measure.Description);
                    priceColumnBuilder.Append("</p>");
                }

                priceColumnBuilder.Append("<br/>");
                priceColumnBuilder.Append("<br/>");
            }

            return priceColumnBuilder.ToString();
        }

        private static string StringifyDaysMeasures(List<Day> days)
        {
            StringBuilder weekBuilder = new StringBuilder("");

            string[] daysStr = { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд" };

            foreach (Day day in days)
            {
                weekBuilder.AppendFormat("{0}: {1} - {2} <br/>",
                                daysStr[(int)day.DayOfWeek - 1],
                               day.StartTime.ToString(@"hh\:mm"),
                                day.EndTime.ToString(@"hh\:mm"));
            }

            return weekBuilder.ToString();
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
                    List<Day> days = JsonConvert.DeserializeObject<List<Day>>(set.Tables[0].Rows[0]["DAYS"].ToString());

                    toReturn.Add("Час надання послуги", StringifyDaysMeasures(days));

                    List<TimeMeasure> measures = JsonConvert.DeserializeObject<List<TimeMeasure>>(set.Tables[0].Rows[0]["TIME_MEASURES"].ToString());

                    toReturn.Add("Тривалість", StringifyTimeMeasures(measures));

                    List<PaymentMeasure> pMeasures = JsonConvert.DeserializeObject<List<PaymentMeasure>>
                        (set.Tables[0].Rows[0]["PAYMENT_MEASURES"].ToString());

                    toReturn.Add("Вартість", StringifyPaymentMeasures(pMeasures));
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

        private static Dictionary<string, string> GetCourseDetails(int detailsId, SqlConnection con)
        {
            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            string cmdString = "SELECT * FROM Course_Details WHERE ID=@Id;";

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
                {
                    toReturn.Add("Початок", set.Tables[0].Rows[0]["START_DATE"].ToString());
                    toReturn.Add("Кінець", set.Tables[0].Rows[0]["END_DATE"].ToString());

                    List<PaymentMeasure> measures = JsonConvert.DeserializeObject<List<PaymentMeasure>>(
                        set.Tables[0].Rows[0]["PAYMENT_MEASURES"].ToString()
                        );

                    toReturn.Add("Вартість", StringifyPaymentMeasures(measures));

                    if (set.Tables[0].Rows[0]["COURSE_PARAMETERS"].ToString() != "null")
                    {
                        bool isDefined = Convert.ToBoolean(set.Tables[0].Rows[0]["IS_DEFINED"].ToString());

                        if (isDefined)
                        {
                            DefinedCourseParams parameters = JsonConvert.DeserializeObject<DefinedCourseParams>(
                                set.Tables[0].Rows[0]["COURSE_PARAMETERS"].ToString()
                                );

                            toReturn.Add("Розклад", StringifyDaysMeasures(parameters.Days));
                        }
                        else
                        {
                            UndefinedCourseParams parameters = JsonConvert.DeserializeObject<UndefinedCourseParams>(
                                set.Tables[0].Rows[0]["COURSE_PARAMETERS"].ToString()
                                );

                            StringBuilder daysBuilder = new StringBuilder("");

                            foreach (var node in parameters.Days)
                            {
                                daysBuilder.AppendFormat("День {0} : {1} хв <br/>",
                                    node.Key.ToString(), node.Value.ToString());
                            }

                            toReturn.Add("Розклад", daysBuilder.ToString());
                        }
                    }
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
        [ActionName("PostServiceParams")]
        public string Post([FromBody]ServiceParams parameters)
        {
            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                List<Service> toReturn = new List<Service>();

                string cmdString = "SELECT TOP 3 * FROM SERVICES WHERE (ID > @Id) AND ";

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
                    filters.Add("(CATEGORIES=@Category)");
                    sqlParams.Add("@Category", parameters.Category);
                }

                if(parameters.DescriptionSearch)
                {
                    filters.Add("(DESCRIPTION LIKE '%' + @Description + '%')");
                    sqlParams.Add("@Description", parameters.Name);
                }

                if(parameters.Type != "None")
                {
                    filters.Add("(TYPE = @Type)");
                    sqlParams.Add("@Type", parameters.Type);
                }

                if(filters.Count() != 0)
                {

                    for(int i = 0; i < filters.Count(); ++i)
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

        [ActionName("PostService")]
        public HttpResponseMessage Post([FromBody]Service service)
        {
            Session session = service as Session;

            Course course = service as Course;

            Deadline deadline = service as Deadline;

            string cmdString = "INSERT INTO Services VALUES(@Name, @Categories, @Description, @AdvancePercent, @Properties, @Type, @DetailsId);";

            Dictionary<string, object> parameters = new Dictionary<string, object>();

            using (SqlConnection connection = new SqlConnection(System.Web.Configuration.WebConfigurationManager.ConnectionStrings["DBCS"].ConnectionString))
            {

                parameters.Add("@Name", service.Name);
                parameters.Add("@Categories", service.Category);
                parameters.Add("@Description", service.Description);
                parameters.Add("@AdvancePercent", service.AdvancePercent);
                parameters.Add("@Properties", JsonConvert.SerializeObject(service.Properties));


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

                    return Request.CreateResponse(HttpStatusCode.OK, "Service successfully added");
                }
                catch(Exception ex)
                {
                    transaction.Rollback();

                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
                }             
            }
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
                        toReturn.Item1.Category = set.Tables[0].Rows[0]["CATEGORIES"].ToString();
                        toReturn.Item1.Description = set.Tables[0].Rows[0]["DESCRIPTION"].ToString();
                        toReturn.Item1.Name = set.Tables[0].Rows[0]["NAME"].ToString();
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