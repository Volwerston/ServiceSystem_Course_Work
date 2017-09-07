using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Common
{
    public class SampleAuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (String.IsNullOrEmpty(filterContext.HttpContext.User.Identity.Name))
            {
                filterContext.Result = new ViewResult { ViewName = "Error" };
            }
        }
    }
}