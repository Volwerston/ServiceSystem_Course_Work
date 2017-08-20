using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string SenderEmail { get; set; }
        public string SenderFullName { get; set; }
        public DateTime SendingTime { get; set; }
        public string Text { get; set; }
        public int RoomId { get; set; }
    }
}