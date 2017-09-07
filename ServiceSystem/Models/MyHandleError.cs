using ServiceSystem.Models.Auxiliary_Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ServiceSystem.Models
{
    public class MyHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            ExceptionUtility.LogException(filterContext.HttpContext.Server, DateTime.Now, filterContext.Exception);
            filterContext.ExceptionHandled = true;
            var model = new HandleErrorInfo(filterContext.Exception, "Error", "Error");

            filterContext.Result = new ViewResult()
            {
                ViewName = "Error",
                ViewData = new ViewDataDictionary(model)
            };

            base.OnException(filterContext);
        }
    }
}