using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace ServiceSystem.Models
{
    public enum MailType
    {
        MESSAGE,
        NOTIFICATION,
        CONSULTANT_INVITATION
    }

    public static class MailManager
    {
        public static bool SendMessage(Dictionary<string, string> values, MailType type)
        {
            string smtpHost = "smtp.gmail.com";
            int smtpPort = 587;
            string smtpUserName = "btsemail1@gmail.com";
            string smtpUserPass = "btsadmin";

            SmtpClient client = new SmtpClient(smtpHost, smtpPort);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(smtpUserName, smtpUserPass);
            client.EnableSsl = true;

            string msgFrom = smtpUserName;
            string msgTo = values["username"];

            string msgSubject = "";

            string msgBody = "";

            switch(type)
            {
                case MailType.MESSAGE:
                msgBody = GetMessageLetterText(values);
                    msgSubject = "New Messages in Service System";
                    break;
                case MailType.NOTIFICATION:
                    msgSubject = "New Notifications in Service System";
                    msgBody = GetNotificationLetterText(values);
                    break;
                case MailType.CONSULTANT_INVITATION:
                    msgSubject = "New Invitations in Service System";
                    msgBody = GetConsultantInvitationLetterText(values);
                    break;
            }

            MailMessage message = new MailMessage(msgFrom, msgTo, msgSubject, msgBody);

            message.IsBodyHtml = true;

            bool toReturn = true;

            try
            { 
                client.Send(message);
            }
            catch
            {
                toReturn = false;
            }

            return toReturn;
        }

        private static string GetConsultantInvitationLetterText(Dictionary<string,string> values)
        {
            StringBuilder builder = new StringBuilder("");

            builder.Append("Шановний ");
            builder.Append(values["username"]);
            builder.Append("<br/>");

            builder.Append("Вас запрошено консультантом <a href='http://service.local.com/Service/ServiceDetails/");
            builder.Append(values["service_id"]);
            builder.Append("'>проекту</a><br/>");

            return builder.ToString();
        }

        private static string GetNotificationLetterText(Dictionary<string, string> values)
        {
            StringBuilder builder = new StringBuilder("");

            builder.Append("Шановний ");
            builder.Append(values["username"]);
            builder.Append("<br/>");

            builder.Append("Ви отримали нове сповіщення. Щоб переглянути його, авторизуйтеся в <a href='http://service.local.com'>системі</a><br/>");

            return builder.ToString();
        }
        private static string GetMessageLetterText(Dictionary<string, string> values)
        {
            StringBuilder builder = new StringBuilder("");

            builder.Append("Шановний ");
            builder.Append(values["username"]);
            builder.Append("<br/>");

            builder.Append("Ви отримали нове повідомлення у діалозі ");
            builder.Append("<a href='http://service.local.com/Service/Dialogue/");
            builder.Append(values["dialogue_id"]);
            builder.Append("'>");
            builder.Append(values["dialogue_id"]);
            builder.Append("</a><br/>");

            return builder.ToString();
        }
    }
}