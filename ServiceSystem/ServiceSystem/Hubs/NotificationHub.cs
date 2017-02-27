using Microsoft.AspNet.SignalR;
using ServiceSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Web;

namespace ServiceSystem.Hubs
{
    public class NotificationHub : Hub
    {
        public void SendNotification(string message, string user)
        {
            int room_id = Convert.ToInt32(Context.QueryString["room_id"]);
            Message mes = new Message();

            mes.Text = message;
            mes.SenderName = user;
            mes.RoomId = room_id;

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://localhost:49332/");

                HttpResponseMessage response = client.PostAsJsonAsync("api/Dialogue/PostDialogues", mes).Result;
            }
        }

        public override System.Threading.Tasks.Task OnConnected()
        {
            int room_id = Convert.ToInt32(Context.QueryString["room_id"]);

            List<Message> toPass = new List<Message>();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.BaseAddress = new Uri("http://localhost:49332/");

                HttpResponseMessage response = client.GetAsync("api/Dialogue/GetDialogues?dialogue_id=" + room_id.ToString()).Result;

                if(response.IsSuccessStatusCode)
                {
                    toPass = response.Content.ReadAsAsync<List<Message>>().Result;
                }
            }

                Clients.User(Context.User.Identity.Name).refreshNotification(toPass);

            return base.OnConnected();
        }
    }
}