using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScheduleBuilderApp.Models;

namespace ScheduleBuilderApp.ViewModels
{
    public class UserFormViewModel
    {
        public User User { get; set; }
        public IEnumerable<UserRole> UserRoles { get; set; }
    }
}