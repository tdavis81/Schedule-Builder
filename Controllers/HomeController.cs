using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScheduleBuilderApp.Models;
using ScheduleBuilderApp.Services;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.ViewModels;
using static ScheduleBuilderApp.Classes.Attributes;

namespace ScheduleBuilderApp.Controllers
{
    [VerifyLogin]
    public class HomeController : Controller
    {
        // /Home/
        // This collects a users shifts/all users shifts and builds a calendar for them
        public ActionResult Index(int ViewTypeID = 0)
        {
            ViewBag.EmployeeEvents = GetCalendarDaysJson(ViewTypeID);
            ViewBag.ViewTypes = HomeService.GetAllViewTypes();
            ViewBag.ViewTypeID = ViewTypeID;

            return View();
        }

        // /Home/EmployeeTrades
        // This shows a Table of all Active Employee Trades that are Pending/Accepted
        public ActionResult EmployeeTrades()
        {
            using (var DB = new ScheduleBuilderEntities())
            {
               ViewBag.EmployeeTrades = HomeService.GetAllTrades(DB);
            }
            return View();
        }

        // /Home/AddEmployeeTrade
        // This is for when a Employee wants to create a Shift Trade
        public ActionResult AddEmployeeTrade()
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                var shifts = HomeService.GetAllEvents(DB, UserSession.User.ID).Where(x => x.ShiftStartTime > DateTime.Now).OrderBy(y => y.ShiftStartTime).ToList();
                foreach (var shift in shifts)
                {
                    shift.ThemeColor = $"{shift.ShiftStartTime.ToString("MM/dd/yyyy h:mm tt")} - {shift.ShiftEndTime.ToString("h:mm tt")}";
                }
                ViewBag.EmployeeShifts = shifts;
            }
            return View();
        }

        // /Home/EditEmployeeTrade
        // This is for an employee to update an OPEN Trade
        public ActionResult EditEmployeeTrade(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                var shifts = HomeService.GetAllEvents(DB, UserSession.User.ID).Where(x => x.ShiftStartTime > DateTime.Now).ToList();
                
                foreach (var shift in shifts)
                {
                    shift.ThemeColor = $"{shift.ShiftStartTime.ToString("MM/dd/yyyy h:mm tt")} - {shift.ShiftEndTime.ToString("h:mm tt")}";
                }
                ViewBag.EmployeeShifts = shifts;
                ViewBag.EmployeeTrade = HomeService.GetEmployeeTrade(DB, id);
            }
            return View();
        }

        // /Home/ViewEmployeeTrade
        // This is for other employees to view that shift trade or view/accept it
        public ActionResult ViewEmployeeTrade(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                ViewBag.EmployeeTrade = HomeService.GetEmployeeTrade(DB, id);
            }
            return View();
        }

       
        /* AJAX CALLS BELOW */
        
        // Request to get an All Shifts or All Session Users Shifts
        public string GetCalendarDaysJson(int ViewTypeID = 0)
        {
            var days = new List<EmployeeEvent>();

            using (ScheduleBuilderEntities DB = new ScheduleBuilderEntities())
            {
                 days = HomeService.GetAllEvents(DB, ViewTypeID);
            }
            
            var json = JsonConvert.SerializeObject(days.Select(d => new
            {
                title = $"{d.User.FirstName}-{d.ShiftStartTime.ToString("h:mm tt")}-{d.ShiftEndTime.ToString("h:mm tt")}",
                userName = $"{d.User.FirstName} {d.User.LastName}",
                start = $"{d.ShiftStartTime}",
                end = $"{d.ShiftEndTime}",
                allDay = true,
                color = "orange",
                employeeId = d.EmployeeID,
                scheduleId = d.ScheduleID,
                approvedID = d.ApprovedStatusID
            }));

            return json;
        }

        // /Home/SaveEmployeeTrade/*EmployeeTradeObject*
        // Adding/Updating an Employee Shift Trade
        public ActionResult SaveEmployeeTrade(EmployeeTrade empTrade)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                HomeService.UpdateEmployeeTrade(DB, empTrade);
            }

            return RedirectToAction("EmployeeTrades", "Home");
        }

        // /Home/DeleteEmplyoeeTrade/id
        // Deleting an Employee Shift Trade
        public ActionResult DeleteEmployeeTrade(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                HomeService.DeleteEmployeeTrade(DB,id);
            }
            return RedirectToAction("EmployeeTrades", "Home");
        }

        // /Home/RequestEmployeeTrade/EmployeeTradeObject
        // When an employee accepts a shift it gets put in queue as pending for the admin to approve it.
        public ActionResult RequestEmployeeTrade(EmployeeTrade empTrade)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                HomeService.ModifyAcceptStatus(DB, empTrade);
            }
            return RedirectToAction("EmployeeTrades", "Home");
        }
    }
}