using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Models.Auxiliary_Classes
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                var request = filterContext.RequestContext.HttpContext.Request;
                List<string> roles = new List<string>();
                string name = filterContext.RequestContext.HttpContext.User.Identity.Name;

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.BaseAddress = new Uri(request.Url.Scheme + "://" + request.Url.Authority + request.ApplicationPath.TrimEnd('/'));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.Cookies["access_token"].Value);

                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage msg = client.GetAsync("/api/Account/GetUserRoles?email=" + name).Result;

                    if (msg.IsSuccessStatusCode)
                    {
                        roles = msg.Content.ReadAsAsync<List<string>>().Result;
                    }
                }

                if (!roles.Contains("admin"))
                {
                    filterContext.Result = new ViewResult()
                    {
                        ViewName = "Error"
                    };
                }
            }
            catch
            {
                filterContext.Result = new ViewResult()
                {
                    ViewName = "Error"
                };
            }
                base.OnAuthorization(filterContext);
        }
    }
}