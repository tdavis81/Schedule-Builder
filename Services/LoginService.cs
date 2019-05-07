using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ScheduleBuilderApp.DataLayer
{
    public class LoginService
    {

        // POST return a message to make sure user can sign in and set Cookie if they do
        public static string ValidateLogin(ScheduleBuilderEntities DB,string Email, String Password)
        {
            var userFound = DB.Users.FirstOrDefault(u => u.Email == Email);

            if(userFound != null)
            {
                if (userFound.PasswordFailures < 3)
                {
                    if(userFound.IsEnabled)
                    {
                        var passHash = Utils.sha256Encrpyt(Password + userFound.ID + ConfigurationManager.AppSettings["salt"]);
                        if (passHash == userFound.Password)
                        {
                            userFound.PasswordFailures = 0;
                            DB.SaveChanges();
                            HttpContext.Current.Response.SetCookie(UserSession.CreateUserCookie(userFound));
                            return "Success";
                        }
                        else
                        {
                            userFound.PasswordFailures += 1;
                            DB.SaveChanges();
                            return $"The password was incorrect. You now have {3 - userFound.PasswordFailures} chances left.";
                        }
                    }
                    else
                    {
                        return $"The User with the Email {Email} is disabled, please contact an Admin to Reactivate it.";
                    } 
                }
                else
                {
                    return "You've exceeded the login attemps, please contact an Admin to have it reset.";
                }
                
            }
            else 
            {
                return $" I'm sorry but there is no account with the Email - {Email} that exists.";
            }
        }

        // Logout and remove cookie
        public static void Logout()
        {
            HttpContext.Current.Response.SetCookie(UserSession.DeleteUserCookie());
        }
    }
}