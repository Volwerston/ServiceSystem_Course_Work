using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Models
{
    public class Day
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class ServiceManager
    {
        public static Service GenerateService(FormCollection collection, IEnumerable<HttpPostedFileBase> attachments)
        {
            Service toReturn = null;

            switch(collection["service_type"])
            {
                case "deadline":
                    toReturn = GenerateDeadline(collection);
                    break;
                case "session":
                    toReturn = GenerateSession(collection);
                    break;
                case "course":
                    toReturn = GenerateCourse(collection);
                    break;
            }

            toReturn.Name = collection["service_name"];

            toReturn.Description = collection["service_description"];

            toReturn.Category = collection["service_category"];

            toReturn.AdvancePercent = 0.5; // CHANGE!!!        

            toReturn.Attachments = new List<File>();

            toReturn.Properties = new List<Property>();

            int a = 1;

            while(collection["property_name_" + a.ToString()] != null)
            {
                Property property  = new Property();

                property.Name = collection["property_name_" + a.ToString()];

                property.Type = collection["property_type_" + a.ToString()];

                property.Value = collection["property_widget_value_" + a.ToString()];

                toReturn.Properties.Add(property);

                ++a;
            }

            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    if (attachment != null)
                    {
                        File file = new File();

                        file.Name = attachment.FileName;
                        file.Data = new byte[attachment.ContentLength];
                        attachment.InputStream.Read(file.Data, 0, attachment.ContentLength);

                        toReturn.Attachments.Add(file);
                    }
                }
            }

            return toReturn;
        }

        private static Session GenerateSession(FormCollection collection)
        {
            Session toReturn = new Session();

            string[] days = new string[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };

            toReturn.Days = new List<Day>();

            for(int i = 0; i < days.Count(); ++i)
            {
                if (collection[days[i]] == "on")
                {
                    toReturn.Days.Add(new Day
                    {
                        DayOfWeek = (DayOfWeek)(i+1),
                        StartTime = new TimeSpan(
                            Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[0]),
                            Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[1]),
                            0),
                        EndTime = new TimeSpan(
                            Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[0]),
                            Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[1]),
                            0),
                    });
                }
            }

            toReturn.TimeMeasure = new List<TimeMeasure>();

            int a = 1;

            while (collection["range_type_" + a.ToString()] != null)
            {
                TimeMeasure measure = new TimeMeasure();

                int maxDays = 0, maxHours = 0, maxMinutes = 0;

                if (collection["service_max_measure_" + a.ToString()] != null)
                {
                    switch (collection["service_max_measure_" + a.ToString()])
                    {
                        case "minutes":
                            maxMinutes = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "hours":
                            maxHours = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "days":
                            maxDays = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "weeks":
                            maxDays = 7 * Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "months":
                            maxDays = 30 * Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                    }
                }

                measure.MaxDuration = new TimeSpan(maxDays, maxHours, maxMinutes, 0);

                int minDays = 0, minHours = 0, minMinutes = 0;

                if (collection["service_min_measure_" + a.ToString()] != null)
                {
                    switch (collection["service_min_measure_" + a.ToString()])
                    {
                        case "minutes":
                            minMinutes = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "hours":
                            minHours = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "days":
                            minDays = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "weeks":
                            minDays = 7 * Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "months":
                            minDays = 30 * Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                    }
                }

                measure.MinDuration = new TimeSpan(minDays, minHours, minMinutes, 0);

                toReturn.TimeMeasure.Add(measure);

                ++a;
            }

            a = 1;

            toReturn.PaymentMeasure = new List<PaymentMeasure>();

            while(collection["currency_" + a.ToString()] != null)
            {
                PaymentMeasure measure = new PaymentMeasure();

                measure.Currency = collection["currency_" + a.ToString()];

                measure.ValueMeasure = collection["measure_" + a.ToString()];

                measure.PricePerUnit = Convert.ToDouble(collection["price_" + a.ToString()]);

                toReturn.PaymentMeasure.Add(measure);

                ++a;
            }

            return toReturn;
        }

        private static Deadline GenerateDeadline(FormCollection collection)
        {
            Deadline toReturn = new Deadline();
            

            toReturn.TimeMeasures = new List<TimeMeasure>();

            int a = 1;

            while (collection["range_type_" + a.ToString()] != null)
            {
                TimeMeasure measure = new TimeMeasure();

                int maxDays = 0, maxHours = 0, maxMinutes = 0;

                if (collection["service_max_measure_" + a.ToString()] != null)
                {
                    switch (collection["service_max_measure_" + a.ToString()])
                    {
                        case "minutes":
                            maxMinutes = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "hours":
                            maxHours = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "days":
                            maxDays = Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "weeks":
                            maxDays = 7 * Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                        case "months":
                            maxDays = 30 * Convert.ToInt32(collection["service_max_duration_" + a.ToString()]);
                            break;
                    }
                }

                measure.MaxDuration = new TimeSpan(maxDays, maxHours, maxMinutes, 0);

                int minDays = 0, minHours = 0, minMinutes = 0;

                if (collection["service_min_measure_" + a.ToString()] != null)
                {
                    switch (collection["service_min_measure_" + a.ToString()])
                    {
                        case "minutes":
                            minMinutes = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "hours":
                            minHours = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "days":
                            minDays = Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "weeks":
                            minDays = 7 * Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                        case "months":
                            minDays = 30 * Convert.ToInt32(collection["service_min_duration_" + a.ToString()]);
                            break;
                    }
                }

                measure.MinDuration = new TimeSpan(minDays, minHours, minMinutes, 0);

                toReturn.TimeMeasures.Add(measure);

                ++a;
            }

            a = 1;

            toReturn.PaymentMeasures = new List<PaymentMeasure>();

            while (collection["currency_" + a.ToString()] != null)
            {
                PaymentMeasure measure = new PaymentMeasure();

                measure.Currency = collection["currency_" + a.ToString()];

                measure.ValueMeasure = collection["measure_" + a.ToString()];

                measure.PricePerUnit = Convert.ToDouble(collection["price_" + a.ToString()]);

                toReturn.PaymentMeasures.Add(measure);

                ++a;
            }

            return toReturn;
        }

        /*
        private static DefinedCourse GenerateDefinedCourse(FormCollection collection)
        {
            DefinedCourse toReturn = new DefinedCourse();

            toReturn.Price = Convert.ToDouble(collection["price"]);
            
            if(collection["participantsLimit"] == "on")
            {
                toReturn.ParticipantsLimited = true;
                toReturn.ParticipantsNumber = Convert.ToInt32(collection["participants"]);
            }

            string[] days = new string[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };

            toReturn.Days = new List<Day>();

            for(int i = 0; i < days.Count(); ++i)
            {
                if (collection[days[i]] == "on")
                {
                    toReturn.Days.Add(new Day
                    {
                        DayOfWeek = (DayOfWeek)(i+1),
                        StartTime = new TimeSpan(
                            Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[0]),
                            Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[1]),
                            0),
                        EndTime = new TimeSpan(
                            Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[0]),
                            Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[1]),
                            0),
                    });
                }
            }

            return toReturn;
        }
        */

        private static Course GenerateCourse(FormCollection collection)
        {

            Course toReturn = new Course();

            toReturn.IsDefined = collection["is_week_defined"] == "on" ? true : false;

            toReturn.StartDate = Convert.ToDateTime(collection["service_start_date"]);

            toReturn.EndDate = Convert.ToDateTime(collection["service_end_date"]);

            if(collection["is_time_defined"] == "on")
            {
                toReturn.StartDate = toReturn.StartDate.AddHours(Convert.ToDouble(collection["service_start_time"].Split(':')[0]));
                toReturn.StartDate = toReturn.StartDate.AddMinutes(Convert.ToDouble(collection["service_start_time"].Split(':')[1]));

                toReturn.EndDate = toReturn.EndDate.AddHours(Convert.ToDouble(collection["service_end_time"].Split(':')[0]));
                toReturn.EndDate = toReturn.EndDate.AddMinutes(Convert.ToDouble(collection["service_end_time"].Split(':')[1]));
            }

            if(collection["week_gradation_type"] == "day_hour")
            {
                string[] days = new string[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };

                DefinedCourseParams parameters = new DefinedCourseParams();
                
                parameters.Days = new List<Day>();

                for (int i = 0; i < days.Count(); ++i)
                {
                    if (collection[days[i]] == "on")
                    {
                        parameters.Days.Add(new Day
                        {
                            DayOfWeek = (DayOfWeek)(i + 1),
                            StartTime = new TimeSpan(
                                Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[0]),
                                Convert.ToInt32(collection[days[i] + "StartTime"].Split(':')[1]),
                                0),
                            EndTime = new TimeSpan(
                                Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[0]),
                                Convert.ToInt32(collection[days[i] + "EndTime"].Split(':')[1]),
                                0),
                        });
                    }
                }

                toReturn.Parameters = parameters;
            }
            else if(collection["week_gradation_type"] == "day_duration")
            {
                int b = 1;

                UndefinedCourseParams parameters = new UndefinedCourseParams();

                parameters.Days = new Dictionary<int, int>();

                while(collection["day" + b.ToString() + "Duration"] != null)
                {
                    parameters.Days.Add(b, int.Parse(collection["day" + b.ToString() + "Duration"]));
                    ++b;
                }

                toReturn.Parameters = parameters;
            }

            return toReturn;
        }
        
    }
 

    public class File
    {
        public string Name { get; set; }
        public byte[] Data { get; set; }
    }

    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<File> Attachments { get; set; }
        public double AdvancePercent { get; set; }
        public List<Property> Properties { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}