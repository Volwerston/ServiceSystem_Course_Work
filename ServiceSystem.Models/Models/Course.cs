using System;
using System.Collections.Generic;

namespace ServiceSystem.Models
{
    public class Course : Service
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDefined { get; set; }
        public CourseParams Parameters { get; set; }
        public List<PaymentMeasure> PaymentMeasures { get; set; }
    }

    public class CourseParams
    {
    }

    public class DefinedCourseParams : CourseParams
    {
        public List<Day> Days { get; set; }
    }

    public class UndefinedCourseParams : CourseParams
    {
        public Dictionary<int, int> Days { get; set; }
    }
}