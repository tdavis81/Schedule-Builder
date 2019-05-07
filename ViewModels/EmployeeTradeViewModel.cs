using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ScheduleBuilderApp.Models;

namespace ScheduleBuilderApp.ViewModels
{
    public class EmployeeTradeViewModel
    {
        public User User { get; set; }
        public EmployeeTrade EmployeeTrade { get; set; }
    }
}