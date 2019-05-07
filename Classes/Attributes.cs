using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScheduleBuilderApp.Models;

namespace ScheduleBuilderApp.Classes
{
    public class Attributes
    {
        enum UserRole
        {
            Sales = 1,
            Manager = 99
        }
        public class VerifyLogin : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (UserSession.User == null)
                {
                    var url = HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.AbsolutePath + filterContext.HttpContext.Request.Url.Query);
                    filterContext.Result = new RedirectResult(string.Format("/login", url));
                }
            }
        }
        public class VerifyAdmin : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (UserSession.User.UserRoleID != (int)UserRole.Manager)
                {
                    var url = HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.AbsolutePath + filterContext.HttpContext.Request.Url.Query);
                    filterContext.Result = new RedirectResult(string.Format("/home", url));
                }
            }
        }

        

    }
}