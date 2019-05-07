using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;
using ScheduleBuilderApp.Services;
using ScheduleBuilderApp.ViewModels;
using static ScheduleBuilderApp.Classes.Attributes;

namespace ScheduleBuilderApp.Controllers
{
    [VerifyLogin][VerifyAdmin]
    public class AdminController : Controller
    {   
        enum AcceptStatus{ OPEN = 1,   PENDING = 2,ACCEPTED = 3,DENIED = 4}

        /* Schedules */

        //  /Admin/
        // Shows a table of all schedules that have been created.
        public ActionResult Index()
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                ViewBag.Schedules = AdminService.GetAllSchedules(DB);
            }
            return View();
        }

        // /Admin/AddSchedule
        // View to add a new schedule.
        public ActionResult AddSchedule(DateTime? endDate)
        {
            //Get Next Sunday
            var sunday = endDate ?? Utils.DetermineSunday(DateTime.Now);
            var weekDays = new List<TimesheetDays>();

            // Set Week Days 
            for (var dayIndex = 0; dayIndex <= 6; dayIndex++)
            {
                var currentDate = sunday.AddDays(+dayIndex);
                var item = new TimesheetDays()
                {
                    Date = currentDate
                };
                weekDays.Add(item);
            }

            ViewBag.Sunday = sunday;
            ViewBag.Days = weekDays.OrderBy(d => d.Date);
            
            using (var DB = new ScheduleBuilderEntities())
            {
                ViewBag.Employees = AdminService.GetAllUsers(DB);
            }
            return View();
        }

        // /Admin/EditSchedule/*id*
        // View for editing a schedule.
        public ActionResult EditSchedule(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                var schedule = AdminService.GetSchedule(DB, id);
                var sunday = schedule.WeekOf == null ? Utils.DetermineSunday(DateTime.Now) : schedule.WeekOf;
                var weekDays = new List<TimesheetDays>();
                
                // Set week days
                for (var dayIndex = 0; dayIndex <= 6; dayIndex++)
                {
                    var currentDate = sunday.AddDays(+dayIndex);
                    var item = new TimesheetDays()
                    {
                        Date = currentDate
                    };
                    weekDays.Add(item);
                }

                DB.Configuration.LazyLoadingEnabled = false;
                ViewBag.Schedule = schedule;
                ViewBag.EmployeeEvents = AdminService.GetEmployeeEvents(DB, id);
                ViewBag.Employees = AdminService.GetAllUsers(DB);
                ViewBag.Days = weekDays.OrderBy(d => d.Date);
            }
            return View();
        }
        
        // AJAX Call Endpoints

        // /Admin/CreateSchedule
        // Request to take Schedule data and add to DB
        public ActionResult CreateSchedule(List<EmployeeEvent> tableData, double totalEmpHours, DateTime weekOfDate, string storeNumber)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.CreateSchedule(DB,tableData,totalEmpHours,weekOfDate,storeNumber);
            }
            return RedirectToAction("Index", "admin");
        }

        // /Admin/UpdateSchedule
        // Request to take Schedule data and update in DB
        public ActionResult UpdateSchedule(List<EmployeeEvent> tableData, double totalEmpHours, DateTime weekOfDate, string storeNumber, int scheduleId)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.UpdateSchedule(DB, tableData, totalEmpHours, weekOfDate, storeNumber, scheduleId);
            }
            return RedirectToAction("Index", "admin");
        }

        // /Admin/SetApprovedStatus
        // Request to set approved/deny status on a schedule
        public ActionResult SetApprovedStatus(int id, bool approvedStatus)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.SetApprovedStatus(DB, id, approvedStatus);
            }
            return RedirectToAction("Index", "Admin");
        }

        // /Admin/DeleteSchedule/*id*
        // Delete Request to delete a schedule.
        public ActionResult DeleteSchedule(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.DeleteSchedule(DB, id);
            }
            return RedirectToAction("Index", "Admin");
        }

        // /Admin/ReverseStatus
        // Request to reverse an approved status from denied/approved back to pending
        public ActionResult ReverseStatus(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.ReverseStatus(DB, id);
            }
            return RedirectToAction("EditSchedule", "Admin", new { id = id });
        }


        /* Users */

        //  /Admin/Users
        // Shows a table of all Users in DB
        public ActionResult Users()
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                ViewBag.Users = AdminService.GetAllUsers(DB);
            }
            return View();
        }

        // /Admin/AddUser
        // View for adding a user - returns dynamic View
        public ActionResult AddUser()
        {
            UserFormViewModel viewModel;
            using (var DB = new ScheduleBuilderEntities())
            {
                viewModel = new UserFormViewModel()
                {
                    UserRoles = AdminService.GetAllUserTypes(DB)
                };
            }
            return View("SaveUser",viewModel);
        }

        // /Admin/EditUser
        // View for editing a user - returns dynamic View
        public ActionResult EditUser(int id)
        {
            UserFormViewModel viewModel;
            using (var DB = new ScheduleBuilderEntities())
            {
                viewModel = new UserFormViewModel()
                {
                    User = AdminService.GetUser(DB, id),
                    UserRoles = AdminService.GetAllUserTypes(DB)
                };
            }
            
            return View("SaveUser", viewModel);
        }
    
        // /Admin/TradeAlerts
        // View for showing a table of all Trade Alerts for an Admin to Approve/Deny
        public ActionResult TradeAlerts()
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                ViewBag.EmployeeTrades = HomeService.GetAllTrades(DB).Where(x => x.AcceptStatusID != (int)AcceptStatus.OPEN ).ToList();
                ViewBag.Employees = AdminService.GetAllUsers(DB);
            }
            return View();
        }

        // /Admin/Save
        // Save Endpoint to Add/Update a User
        public ActionResult Save(User user)
        {   
            using (var DB = new ScheduleBuilderEntities())
            {   
                if (user.ID == 0)
                   AdminService.AddUser(DB, user);
                else
                    AdminService.UpdateUser(DB, user);
            }
            return RedirectToAction("Users", "Admin");
        }

        // /Admin/Delete/*id*
        // Delete Endpoint to Delete a User
        public ActionResult Delete(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.DeleteUser(DB, id);
            }
            return RedirectToAction("Users", "Admin");
        }

        // /Admin/ApproveTrade/*id*
        // Endpoint for admin to approve a trade
        public ActionResult ApproveTrade(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.ApproveTrade(DB,id);
            }
            return RedirectToAction("TradeAlerts", "Admin");
        }

        // /Admin/DenyTrade/*id*
        // Endpoint for admin to deny a trade
        public ActionResult DenyTrade(int id)
        {
            using (var DB = new ScheduleBuilderEntities())
            {
                AdminService.DenyTrade(DB,id);
            }
            return RedirectToAction("TradeAlerts", "Admin");
        }
    }
}