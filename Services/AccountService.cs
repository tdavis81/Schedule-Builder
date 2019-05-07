using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;

namespace ScheduleBuilderApp.Services
{
    public class AccountService
    {
        // GET a user with the matching userID parameter
        public static User GetUser(ScheduleBuilderEntities DB, int userId)
        {
            return DB.Users.FirstOrDefault(x => x.ID == userId);
        }

        // Update a users password
        public static void UpdatePassword(ScheduleBuilderEntities DB, string userEmail, string newPassword)
        {
            var user = HomeService.GetUserByEmail(DB, userEmail);
            if (user == null) return;

            var passHash = Utils.sha256Encrpyt(newPassword + user.ID + ConfigurationManager.AppSettings["salt"]);
            user.Password = passHash;
            user.IsPasswordResetNeeded = false;
            DB.SaveChanges();
#if !DEBUG
            Email.Send(user.Email, "Reset Password", String.Format("Your password has been reset to: {0}", newPassword));
#endif
        }

        public static string UpdatePassword(ScheduleBuilderEntities DB, string oldPassword, string newPassword, string confirmNewPassword)
        {
            var user = AccountService.GetUser(DB, UserSession.User.ID);
            if (user == null) return "User not found.";

            var passHash = Utils.sha256Encrpyt(oldPassword + user.ID + ConfigurationManager.AppSettings["salt"]);
            
            if (passHash == user.Password)
            {
                if (newPassword == confirmNewPassword)
                {
                    AccountService.UpdatePassword(DB, user.Email, newPassword);
                    return "Password changed successfully";
                }
                else
                {
                    return "Passwords do not match!";
                }
            }
            else
            {
                return "Current Password is incorrect!";
            }
        }

    }
}