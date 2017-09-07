using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceSystem.Models
{
    public class Dialogue
    {
        public int Id { get; set; }
        public int MessagesNumber { get; set; }
        public int ParticipantsNumber { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastChangeTime { get; set; }
    }
}