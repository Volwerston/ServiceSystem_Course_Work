using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public class DetailsStrategy
    {
        public static Dictionary<string, string> GetDetails(int detailsId, string serviceType, SqlConnection con)
        {

            Dictionary<string, string> toReturn = null;

            if (serviceType == "Deadline")
            {
                toReturn = GetDeadlineDetails(detailsId, con);
            }
            else if (serviceType == "Session")
            {
                toReturn = GetSessionDetails(detailsId, con);
            }
            else if (serviceType == "Course")
            {
                toReturn = GetCourseDetails(detailsId, con);
            }

            toReturn.Add("SERVICE_TYPE", serviceType);

            return toReturn;
        }

        private static Dictionary<string, string> GetDeadlineDetails(int detailsId, SqlConnection con)
        {
            string cmdString = "SELECT * FROM Deadline_Details WHERE ID=@Id";

            Dictionary<string, string> toReturn = new Dictionary<string, string>();

            SqlDataAdapter da = new SqlDataAdapter(cmdString, con);
            da.SelectCommand.Parameters.AddWithValue("@Id", detailsId);

            DataSet set = new DataSet();

            try
            {
                da.Fill(set);

                if (set.Tables[0].Rows.Count > 0)
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

                if (set.Tables[0].Rows.Count > 0)
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

                    if (set.Tables[0].Rows[0]["COURSE_PARAMS"].ToString() != "null")
                    {
                        bool isDefined = Convert.ToBoolean(set.Tables[0].Rows[0]["IS_DEFINED"].ToString());

                        if (isDefined)
                        {
                            DefinedCourseParams parameters = JsonConvert.DeserializeObject<DefinedCourseParams>(
                                set.Tables[0].Rows[0]["COURSE_PARAMS"].ToString()
                                );

                            toReturn.Add("Розклад", StringifyDaysMeasures(parameters.Days));
                        }
                        else
                        {
                            UndefinedCourseParams parameters = JsonConvert.DeserializeObject<UndefinedCourseParams>(
                                set.Tables[0].Rows[0]["COURSE_PARAMS"].ToString()
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
    }
}