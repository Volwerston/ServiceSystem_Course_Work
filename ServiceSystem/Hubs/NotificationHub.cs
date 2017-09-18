using Microsoft.AspNet.SignalR;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using Owin;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Caching;

namespace ServiceSystem.Hubs
{
    public class NotificationHub : Hub
    {
        public void SendNotification(string message, string user)
        {
            int room_id = Convert.ToInt32(Context.QueryString["room_id"]);
            Message mes = new Message();

            StringBuilder builder = new StringBuilder(HttpUtility.HtmlEncode(message));
            builder = builder.Replace("\n", "\\n");
            builder = builder.Replace("\r", "\\r");
            mes.Text = builder.ToString();
            mes.SenderEmail = user;
            mes.RoomId = room_id;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/");

                HttpResponseMessage response = client.PostAsJsonAsync("api/Dialogue/PostDialogues", mes).Result;
            }
        }

        public void Disconnect(string userName)
        {
            ObjectCache cache = MemoryCache.Default;

            List<string> current_users = cache["current_users"] as List<string>;

            if (current_users == null)
            {
                current_users = new List<string>();

                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration =
                    DateTimeOffset.Now.AddHours(1);

                cache.Set("current_users", current_users, policy);
            }
            else
            {
                string off_user = Context.User.Identity.Name;
                current_users.Remove(Context.User.Identity.Name);

                CacheItemPolicy policy = new CacheItemPolicy();
                policy.AbsoluteExpiration =
                    DateTimeOffset.Now.AddHours(1);

                cache.Set("current_users", current_users, policy);

                int room_id = Convert.ToInt32(Context.QueryString["room_id"]);

                if (current_users.Where(x => x == off_user).Count() == 0)
                {
                    var offUserCredentials = new
                    {
                        Name = off_user
                    };

                    Clients.All.setUserOffline(offUserCredentials);
                }
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            ObjectCache cache = MemoryCache.Default;

            List<string> current_users = cache["current_users"] as List<string>;

            if(current_users == null)
            {
                current_users = new List<string>();
            }

            if (!current_users.Contains(Context.User.Identity.Name))
            {
                current_users.Add(Context.User.Identity.Name);
            }

            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration =
                DateTimeOffset.Now.AddHours(1);

            cache.Set("current_users", current_users, policy);

            int room_id = -1;

            if (Context.QueryString["room_id"] != null)
            {
                room_id = Convert.ToInt32(Context.QueryString["room_id"]);

                RenderDialogueMessages(room_id);
            }

            string new_user = Context.User.Identity.Name;

            var newUserCredentials = new
            {
                Name = new_user
            };

            Clients.All.setUserOnline(newUserCredentials);

            return base.OnConnected();
        }

        public void RenderDialogueMessages(int room_id)
        {
            List<Message> toPass = new List<Message>();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.ApplicationPath.TrimEnd('/') + "/");

                HttpResponseMessage response = client.GetAsync("api/Dialogue/GetDialogues?dialogue_id=" + room_id.ToString()).Result;

                if (response.IsSuccessStatusCode)
                {
                    toPass = response.Content.ReadAsAsync<List<Message>>().Result;
                    toPass = toPass.OrderByDescending(x => x.SendingTime).ToList();
                }
            }

            List<string> statuses = new List<string>();

            foreach (var message in toPass)
            {
                StringBuilder builder = new StringBuilder(message.Text);
                builder = builder.Replace("\\r", "\r");
                builder = builder.Replace("\\n", "\n");
                message.Text = builder.ToString();

                if (IsOnline(message.SenderEmail))
                {
                    statuses.Add("ONLINE");
                }
                else
                {
                    statuses.Add("OFFLINE");
                }
            }

            Clients.User(Context.User.Identity.Name).refreshNotification(toPass, statuses);
        }


        public static bool IsOnline(string userName)
        {
            ObjectCache cache = MemoryCache.Default;

            List<string> current_users = cache["current_users"] as List<string>;

            return current_users.Where(x => x == userName).Count() > 0;
        }
    }
}