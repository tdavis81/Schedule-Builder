using Newtonsoft.Json;
using ScheduleBuilderApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace ScheduleBuilderApp.Classes
{
    public class UserSession
    {

        public static User User
        {
            get
            {
                var user = new User();
                try
                {
                    var Json = HttpContext.Current.Request.Cookies["userCookies"].Value;
                    Json = Encoding.UTF8.GetString(MachineKey.Unprotect(Convert.FromBase64String(Json)));
                    user = JsonConvert.DeserializeObject<User>(Json);
                }
                catch
                {
                    return null;
                }
                return user;
            }

        }


        public static HttpCookie CreateUserCookie(User currentUser)
        {
            // create user
            var user = new User()
            {
                ID = currentUser.ID,
                FirstName = currentUser.FirstName,
                LastName = currentUser.LastName,
                Email = currentUser.Email,
                Password = currentUser.Password,
                PhoneNumber = currentUser.PhoneNumber,
                EmployeeNumber = currentUser.EmployeeNumber,
                IsPasswordResetNeeded = currentUser.IsPasswordResetNeeded,
                PasswordFailures = currentUser.PasswordFailures,
                IsEnabled = currentUser.IsEnabled,
                IsFullTime = currentUser.IsFullTime,
                CreatedDate = currentUser.CreatedDate,
                HireDate = currentUser.HireDate,
                PTO = currentUser.PTO,
                UserRoleID = currentUser.UserRoleID,
            };

            HttpCookie userCookies = new HttpCookie("userCookies");
            userCookies.Value = Convert.ToBase64String(MachineKey.Protect(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(user))));
            userCookies.Expires = DateTime.Now.AddDays(1);
            return userCookies;
        }

        public static HttpCookie DeleteUserCookie()
        {
            HttpCookie userCookies = new HttpCookie("userCookies");
            userCookies.Expires = DateTime.Now.AddDays(-1);
            return userCookies;
        }

    }
}