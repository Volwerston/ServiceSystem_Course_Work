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

            switch(collection["constructorType"])
            {
                case "Deadline":
                    toReturn = GenerateDeadline(collection);
                    break;
                case "Session":
                    toReturn = GenerateSession(collection);
                    break;
                case "UndefinedCourse":
                    toReturn = GenerateUndefinedCourse(collection);
                    break;
                case "DefinedCourse":
                    toReturn = GenerateDefinedCourse(collection);
                    break;
            }

            toReturn.Name = collection["serviceName"];

            toReturn.Description = collection["serviceDescription"];

            toReturn.Category = collection["serviceCategory"];

            toReturn.StartDate = Convert.ToDateTime(collection["serviceStartDate"]);

            toReturn.EndDate = Convert.ToDateTime(collection["serviceEndDate"]);

            

            toReturn.Attachments = new List<File>();

            foreach(var attachment in attachments)
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

            return toReturn;
        }

        private static Session GenerateSession(FormCollection collection)
        {
            Session toReturn = new Session();

            toReturn.SessionDuration =
                new TimeSpan(Convert.ToInt32(collection["sessionDuration"]) / 60,
                             Convert.ToInt32(collection["sessionDuration"]) % 60, 0);

            toReturn.SessionPrice = Convert.ToDouble(collection["price"]);

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

        private static Deadline GenerateDeadline(FormCollection collection)
        {
            Deadline toReturn = new Deadline();

            toReturn.MinPrice = Convert.ToDouble(collection["serviceMinPrice"]);
            toReturn.MaxPrice = Convert.ToDouble(collection["serviceMaxPrice"]);

            int minDays = 0, minHours = 0, minMinutes = 0;

            switch (collection["serviceMinMeasure"])
            {
                case "minutes":
                    minMinutes = Convert.ToInt32(collection["serviceMinLength"]);
                    break;
                case "hours":
                    minHours = Convert.ToInt32(collection["serviceMinLength"]);
                    break;
                case "days":
                    minDays = Convert.ToInt32(collection["serviceMinLength"]);
                    break;
                case "weeks":
                    minDays = 7*Convert.ToInt32(collection["serviceMinLength"]);
                    break;
                case "months":
                    minDays = 30*Convert.ToInt32(collection["serviceMinLength"]);
                    break;
            }

            toReturn.MinDuration = new TimeSpan(minDays, minHours, minMinutes, 0);

            int maxDays = 0, maxHours = 0, maxMinutes = 0;

            switch (collection["serviceMaxMeasure"])
            {
                case "minutes":
                    maxMinutes = Convert.ToInt32(collection["serviceMaxLength"]);
                    break;
                case "hours":
                    maxHours = Convert.ToInt32(collection["serviceMaxLength"]);
                    break;
                case "days":
                    maxDays = Convert.ToInt32(collection["serviceMaxLength"]);
                    break;
                case "weeks":
                    minDays = 7 * Convert.ToInt32(collection["serviceMaxLength"]);
                    break;
                case "months":
                    maxDays = 30 * Convert.ToInt32(collection["serviceMaxLength"]);
                    break;
            }

            toReturn.MaxDuration = new TimeSpan(maxDays, maxHours, maxMinutes, 0);

            return toReturn;
        }

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

        private static UndefinedCourse GenerateUndefinedCourse(FormCollection collection)
        {
            UndefinedCourse toReturn = new UndefinedCourse();

            toReturn.Price = Convert.ToDouble(collection["price"]);

            toReturn.NumOfDays = Convert.ToInt32(collection["numOfDays"]);

            toReturn.DurationsPerDay = new List<int>();

            string[] days = new string[] { "One", "Two", "Three", "Four", "Five", "Six", "Seven" };

            int count = 0;

            while(collection["day" + days[count] + "Duration"] != "")
            {
                toReturn.DurationsPerDay.Add(
                    Convert.ToInt32(collection["day" + days[count++] + "Duration"])
                    );
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
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<File> Attachments { get; set; }
        public double MinPrice { get; set; }
        public double MaxPrice { get; set; }
    }
}