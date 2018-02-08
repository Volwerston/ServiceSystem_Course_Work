
using System;
using System.Web.Mvc;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public static class ApplicationManager
    {
        public static Application GenerateApplication(int serviceId, FormCollection collection)
        {
            Application toReturn = null;

            switch (collection["applicationType"])
            {
                case "Deadline":
                    toReturn = GenerateDeadlineApplication(collection);

                    toReturn.ServiceType = "Deadline";
                    break;
                case "Session":
                    toReturn = GenerateSessionApplication(collection);

                    toReturn.ServiceType = "Session";
                    break;
                default:
                    toReturn = new Application();

                    toReturn.ServiceType = "Course";
                    break;
            }

            toReturn.Description = collection["details_description"];

            toReturn.ServiceId = serviceId;

            return toReturn;
        }

        private static Application GenerateCourseApplication(FormCollection collection)
        {
            throw new NotImplementedException();
        }

        private static Application GenerateDeadlineApplication(FormCollection collection)
        {
            DeadlineApplication toReturn = new DeadlineApplication();

            if (collection["deadline_type"] == "by_last_date")
            {
                toReturn.HasLastDate = true;

                toReturn.EndTime = Convert.ToDateTime(collection["deadline_last_date"]);

                toReturn.EndTime = toReturn.EndTime.AddHours(
                    Convert.ToInt32(collection["deadline_last_time"].Split(':')[0])
                    );

                toReturn.EndTime = toReturn.EndTime.AddMinutes(
                    Convert.ToInt32(collection["deadline_last_time"].Split(':')[1])
                    );
            }
            else if (collection["deadline_type"] == "from_some_date")
            {
                toReturn.StartTime = Convert.ToDateTime(collection["deadline_start_date"]);

                toReturn.StartTime = toReturn.StartTime.AddHours(
                    Convert.ToInt32(collection["deadline_start_time"].Split(':')[0])
                    );

                toReturn.StartTime = toReturn.StartTime.AddMinutes(
                    Convert.ToInt32(collection["deadline_start_time"].Split(':')[1])
                    );

                switch (collection["deadline_duration_measure"])
                {
                    case "minutes":
                        toReturn.Duration = new TimeSpan(0, 0, Convert.ToInt32(collection["deadline_duration"]), 0);
                        break;
                    case "hours":
                        toReturn.Duration = new TimeSpan(0, Convert.ToInt32(collection["deadline_duration"]), 0, 0);
                        break;
                    case "days":
                        toReturn.Duration = new TimeSpan(Convert.ToInt32(collection["deadline_duration"]), 0, 0, 0);
                        break;
                    case "weeks":
                        toReturn.Duration = new TimeSpan(7 * Convert.ToInt32(collection["deadline_duration"]), 0, 0, 0);
                        break;
                    case "months":
                        toReturn.Duration = new TimeSpan(30 * Convert.ToInt32(collection["deadline_duration"]), 0, 0, 0);
                        break;
                }
            }

            return toReturn;
        }

        private static Application GenerateSessionApplication(FormCollection collection)
        {
            SessionApplication toReturn = new SessionApplication();

            toReturn.SessionStartTime = Convert.ToDateTime(collection["service_day"]);

            toReturn.SessionStartTime = toReturn.SessionStartTime.AddHours(
                Convert.ToInt32(collection["start_time"].Split(':')[0])
                );

            toReturn.SessionStartTime = toReturn.SessionStartTime.AddMinutes(
                Convert.ToInt32(collection["start_time"].Split(':')[1])
                );

            return toReturn;
        }
    }
}