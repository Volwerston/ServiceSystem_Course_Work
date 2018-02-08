using System;
using System.Collections.Generic;

namespace ServiceSystem.Models
{
    public class Day
    {
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
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
        public string Type { get; set; }
        public List<File> Attachments { get; set; }
        public double AdvancePercent { get; set; }
        public List<Property> Properties { get; set; }
        public string Username { get; set; }
        public bool IsActive { get; set; }
    }

    public class Property
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
    }
}