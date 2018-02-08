using System;

namespace ServiceSystem.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string RecipientName { get; set; }
        public DateTime SendingTime { get; set; }
        public string Text { get; set; }
    }
}