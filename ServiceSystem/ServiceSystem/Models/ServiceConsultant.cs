using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class ServiceConsultant
    {
        public string Id { get; set; }
        public int ServiceId { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string FatherName { get; set; }
        public string Email { get; set; }
        public bool Approved { get; set; }
        public DateTime LastAssignmentTime { get; set; }
        public string Organisation { get; set; }
    }
}