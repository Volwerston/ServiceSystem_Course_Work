using System;

namespace ServiceSystem.Models
{
    public class SessionApplication: Application
    {
        public int DetailsId { get; set; }
        public DateTime SessionStartTime { get; set; }
    }
}