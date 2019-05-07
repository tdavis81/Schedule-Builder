using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;
using ScheduleBuilderApp.Services;
using static ScheduleBuilderApp.Classes.Attributes;

namespace ScheduleBuilderApp.Controllers
{
    [VerifyLogin]
    public class AccountController : Controller
    {
        // /Account/ChangePassword
        // View for changing password
        public ActionResult ChangePassword(String resultOut = "")
        {
            if (resultOut == "Password changed successfully")
                ViewBag.result = resultOut;
            else
                ViewBag.error = resultOut;

            return View();
        }

        /* AJAX && Other Requests  */

        // /Account/ResetPassword
        // Request to reset password
        public ActionResult ResetPassword(int id)
        {
            var password = Utils.CreatePassword(8);
            using (var DB = new ScheduleBuilderEntities())
            {
                var user = DB.Users.FirstOrDefault(x => x.ID == id);
                if (user != null)
                    AccountService.UpdatePassword(DB, user.Email, password);
            }
            
            return RedirectToAction("Users", "Admin");
        }

        // /Account/UpdatePassword
        // Request to Update password
        public ActionResult UpdatePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            var result = "";
            using (var DB = new ScheduleBuilderEntities())
            {
                result = AccountService.UpdatePassword(DB, oldPassword, newPassword, confirmNewPassword);
            }
            return RedirectToAction("ChangePassword", "Account", new { resultOut = result });
        }

        // /Account/EmailExists
        // Request to check if an email exists
        public bool EmailExists(string Email)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                var doesEmailExist = HomeService.GetUserByEmail(DB, Email);

                return doesEmailExist != null;
            }
        }
    }
}