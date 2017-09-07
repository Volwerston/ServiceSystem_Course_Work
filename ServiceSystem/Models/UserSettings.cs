using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class UserSettings
    {
        public string Email { get; set; }
        public bool ReceiveEmail { get; set; }
    }
}