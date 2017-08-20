using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class MediaFile
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
    }
}