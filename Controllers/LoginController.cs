using ScheduleBuilderApp.DataLayer;
using ScheduleBuilderApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static ScheduleBuilderApp.Classes.Attributes;

namespace ScheduleBuilderApp.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        // Login page where the user will sign in
        public ActionResult Index(string error = "")
        {
            ViewBag.error = error;
            return View();
        }


        /* Form && AJAX Requests */

        // /Login/AttemptLogin
        // When a User tries to sign in check if there valid
        [HttpPost]
        public ActionResult AttemptLogin(string Email, string Password)
        {
            var successfulLogin = "Success";
            using(var DB = new ScheduleBuilderEntities())
            {
                var loginStatus = LoginService.ValidateLogin(DB,Email, Password);
                if (loginStatus == successfulLogin)
                    return RedirectToAction("Index", "Home");
                else
                    return RedirectToAction("Index", "Login", new { error = loginStatus });
            }
        }

        // /Login/Logout
        // When a user signs out, delete the cookie and session.
        [VerifyLogin]
        public ActionResult Logout()
        {
            LoginService.Logout();
            return RedirectToAction("Index", "Login");
        }
    }
}