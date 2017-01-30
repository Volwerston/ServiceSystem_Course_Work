using System.Web.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using ServiceSystem.Controllers;

namespace ServiceSystem.Models
{

    public static class ApplicationManager
    {
        public static Application GenerateApplication(int serviceId, FormCollection collection)
        {
            Application toReturn = null;

            switch(collection["applicationType"])
            {
                case "Deadline":
                    toReturn = GenerateDeadlineApplication(collection);
                    break;
                case "Session":
                    toReturn = GenerateSessionApplication(collection);
                    break;
                case "UndefinedCourse":
                    toReturn = GenerateUndefinedCourseApplication(collection);
                    break;
                case "DefinedCourse":
                    toReturn = new Application(); 
                    break;
            }

            toReturn.ServiceId = serviceId;

            toReturn.Description = collection["applicationDescription"];

            return toReturn;
        }

        private static Application GenerateDeadlineApplication(FormCollection collection)
        {
            DeadlineApplication toReturn = new DeadlineApplication();

            if(collection["deadlineType"] == "byLastDate")
            {
                toReturn.IsByLastDate = true;

                toReturn.EndDate = Convert.ToDateTime(collection["deadlineLastDate"]);

                toReturn.EndDate = toReturn.EndDate.AddHours(int.Parse(collection["deadlineLastDateTime"].Split(':')[0]));

                toReturn.EndDate = toReturn.EndDate.AddMinutes(int.Parse(collection["deadlineLastDateTime"].Split(':')[1]));
            }
            else if(collection["deadlineType"] == "fromSomeDate")
            {
                toReturn.IsByLastDate = false;

                toReturn.StartDate = Convert.ToDateTime(collection["deadlineStartDate"]);

                toReturn.StartDate = toReturn.StartDate.AddHours(int.Parse(collection["deadlineStartDateTime"].Split(':')[0]));

                toReturn.StartDate = toReturn.StartDate.AddMinutes(int.Parse(collection["deadlineStartDateTime"].Split(':')[1]));

                if(collection["deadlineDurationMeasure"] == "minutes")
                {
                    toReturn.Duration = new TimeSpan(0, 0, int.Parse(collection["deadlineDuration"]), 0);
                }
                else if(collection["deadlineDurationMeasure"] == "hours")
                {
                    toReturn.Duration = new TimeSpan(0, int.Parse(collection["deadlineDuration"]), 0, 0);
                }
                else if(collection["deadlineDurationMeasure"] == "days")
                {
                    toReturn.Duration = new TimeSpan(int.Parse(collection["deadlineDuration"]), 0, 0, 0);
                }
                else if(collection["deadlineDurationMeasure"] == "weeks")
                {
                    toReturn.Duration = new TimeSpan(7 * int.Parse(collection["deadlineDuration"]), 0, 0, 0);
                }
                else if(collection["deadlineDurationMeasure"] == "months")
                {
                    toReturn.Duration = new TimeSpan(30 * int.Parse(collection["deadlineDuration"]), 0, 0, 0);
                }
            }

            return toReturn;
        }

        private static Application GenerateSessionApplication(FormCollection collection)
        {
            SessionApplication toReturn = new SessionApplication();

            toReturn.Date = Convert.ToDateTime(collection["sessionDate"]);

            toReturn.StartTime = new TimeSpan(
                int.Parse(collection["sessionStartTime"].Split(':')[0]),
                int.Parse(collection["sessionStartTime"].Split(':')[1]),
                0
                );


            toReturn.EndTime = new TimeSpan(
                int.Parse(collection["sessionEndTime"].Split(':')[0]),
                int.Parse(collection["sessionEndTime"].Split(':')[1]),
                0
                );

            return toReturn;
        }

        private static Application GenerateUndefinedCourseApplication(FormCollection collection)
        {
            UndefinedCourseApplication toReturn = new UndefinedCourseApplication();

            string[] days = new string[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun" };

            toReturn.Days = new List<Day>();

            for (int i = 0; i < days.Count(); ++i)
            {
                if (collection[days[i]] == "on")
                {
                    toReturn.Days.Add(new Day
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

            return toReturn;
        }
    }

    public class Application
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Description { get; set; }
    }
}