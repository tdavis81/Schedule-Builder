using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using AutoMapper;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;

namespace ScheduleBuilderApp.Services
{
    public class AdminService
    {
        enum ApprovedStatus{ APPROVED = 1,PENDING = 2,DENIED = 3,OPEN = 4}
        enum AcceptStatus{OPEN = 1,PENDING = 2,APPROVED = 3, DENIED = 4 }

        /* Users  */

        // GET a user matching the id parameter
        public static User GetUser(ScheduleBuilderEntities DB, int id)
        {
            return DB.Users.FirstOrDefault(x => x.ID == id);
        }

        // GET All Users
        public static List<User> GetAllUsers(ScheduleBuilderEntities DB)
        {
            return DB.Users.ToList();
        }

        // GET All User Roles
        public static IEnumerable<UserRole> GetAllUserTypes(ScheduleBuilderEntities DB)
        {
            return DB.UserRoles.ToList();
        }

        // POST for adding a User/Employee to DB
        public static void AddUser(ScheduleBuilderEntities DB, User user)
        {
            var generatedPass = Utils.CreatePassword(8);
            user.IsPasswordResetNeeded = true;
            user.CreatedDate = DateTime.Now;
            user.PasswordFailures = 0;
            user.Password = generatedPass;
            user.HireDate = Convert.ToDateTime(DateTime.Now.ToString("MM/dd/yyyy"));
            DB.Users.Add(user);
            DB.SaveChanges();

            user.Password = Utils.sha256Encrpyt(generatedPass + user.ID + ConfigurationManager.AppSettings["salt"]);
            DB.SaveChanges();
#if !DEBUG
            Email.Send(user.Email, "New User", String.Format("Your email is {0} Your password is: {1}", user.Email, generatedPass));
#endif
        }

        // POST for updating a User/Employee to DB
        public static void UpdateUser(ScheduleBuilderEntities DB, User user)
        {
            var foundUser = DB.Users.FirstOrDefault(x => x.ID == user.ID);
            if (foundUser == null) return;

            foundUser.FirstName = user.FirstName;
            foundUser.LastName = user.LastName;
            foundUser.Email = user.Email;
            foundUser.PhoneNumber = user.PhoneNumber;
            foundUser.EmployeeNumber = user.EmployeeNumber;
            foundUser.IsPasswordResetNeeded = user.IsPasswordResetNeeded;
            foundUser.IsEnabled = user.IsEnabled;
            foundUser.IsFullTime = user.IsFullTime;
            foundUser.PTO = user.PTO;
            foundUser.UserRoleID = user.UserRoleID;
            DB.SaveChanges();
        }

        // DELETE a User from DB
        public static void DeleteUser(ScheduleBuilderEntities DB, int id)
        {
            var user = DB.Users.FirstOrDefault(x => x.ID == id);
            if (user == null) return;
            
            var userEvents = DB.EmployeeEvents.Where(x => x.EmployeeID == id);
            var userTrades = DB.EmployeeTrades.Where(x => x.UserID == id);

            // Delete All Events Associated with User 
            foreach (var userEvent in userEvents)
                DB.EmployeeEvents.Remove(userEvent);
            DB.SaveChanges();

            // Delete All Trades Associated with User
            foreach (var userTrade in userTrades)
                DB.EmployeeTrades.Remove(userTrade);
            DB.SaveChanges();

            DB.Users.Remove(user);
            DB.SaveChanges();
        }


        /* Schedules */

        // GET a Schedule
        public static Schedule GetSchedule(ScheduleBuilderEntities DB, int id)
        {
            return DB.Schedules.Include("ApprovedCode").FirstOrDefault(x => x.ID == id);
        }

        // GET a list of Schedules
        public static List<Schedule> GetAllSchedules(ScheduleBuilderEntities DB)
        {
            return DB.Schedules.Include("ApprovedCode").ToList();
        }

        // GET a list all Employee Events that have the same schedule id as the passed in param
        public static List<EmployeeEvent> GetEmployeeEvents(ScheduleBuilderEntities DB, int id)
        {
            return DB.EmployeeEvents.Include("User").Include("Schedule").Include("ApprovedCode").Where(x => x.ScheduleID == id).ToList();
        }

        // POST to create a new schedule
        public static void CreateSchedule(ScheduleBuilderEntities DB, List<EmployeeEvent> tableData, double totalEmpHours, DateTime weekOfDate, string storeNumber)
        {
            // Save Schedule Information to DB
            var schedule = new Schedule()
            {
                WeekOf = weekOfDate,
                TotalHours = (decimal)totalEmpHours,
                ApprovedStatusID = (int)ApprovedStatus.PENDING,
                StoreNumber = storeNumber
            };
            DB.Schedules.Add(schedule);
            DB.SaveChanges();

            // Add Employee Events to Events Table
            foreach (var data in tableData)
            {
                var empEvent = new EmployeeEvent()
                {
                    ShiftStartTime = data.ShiftStartTime,
                    ShiftEndTime = data.ShiftEndTime,
                    ShiftHours = data.ShiftHours,
                    ThemeColor = data.ThemeColor,
                    EmployeeID = data.EmployeeID,
                    ScheduleID = schedule.ID,
                    ApprovedStatusID = schedule.ApprovedStatusID
                };
                DB.EmployeeEvents.Add(empEvent);
                DB.SaveChanges();
            }
        }

        // POST to update an existing schedule
        public static void UpdateSchedule(ScheduleBuilderEntities DB, List<EmployeeEvent> tableData, double totalEmpHours, DateTime weekOfDate, string storeNumber, int scheduleId)
        {
            var schedule = DB.Schedules.FirstOrDefault(x => x.ID == scheduleId);
            var employeeEvents = DB.EmployeeEvents.Where(x => x.ScheduleID == scheduleId);
            if (schedule == null) return;
            
            schedule.WeekOf = weekOfDate;
            schedule.ApprovedStatusID = (int)ApprovedStatus.PENDING;
            schedule.StoreNumber = storeNumber;
            schedule.TotalHours = (decimal) totalEmpHours;
            DB.SaveChanges();
        
            
            //Remove Previous Events 
            foreach (var rEvent in employeeEvents)
                DB.EmployeeEvents.Remove(rEvent);
            DB.SaveChanges();

            // Add Employee Events to Events Table
            foreach (var data in tableData)
            {
                var empEvent = new EmployeeEvent()
                {
                    ShiftStartTime = data.ShiftStartTime,
                    ShiftEndTime = data.ShiftEndTime,
                    ShiftHours = data.ShiftHours,
                    ThemeColor = data.ThemeColor,
                    EmployeeID = data.EmployeeID,
                    ScheduleID = schedule.ID,
                    ApprovedStatusID = schedule.ApprovedStatusID
                };
                DB.EmployeeEvents.Add(empEvent);
                DB.SaveChanges();
            }

        }

        // POST to set approved status to approved/denied for a schedule
        public static void SetApprovedStatus(ScheduleBuilderEntities DB, int id,bool approvedStatus)
        {
            var schedule = DB.Schedules.FirstOrDefault(x => x.ID == id);
            var employeeEvents = DB.EmployeeEvents.Where(x => x.ScheduleID == id).ToList();

            if (schedule != null)
                schedule.ApprovedStatusID = approvedStatus ? (int)ApprovedStatus.APPROVED : (int)ApprovedStatus.DENIED;
                DB.SaveChanges();

            foreach (var empEvent in employeeEvents)
                empEvent.ApprovedStatusID = approvedStatus ? (int)ApprovedStatus.APPROVED : (int)ApprovedStatus.DENIED;
            DB.SaveChanges();

        }

        // POST to reverse an approved/denied status to pending
        public static void ReverseStatus(ScheduleBuilderEntities DB, int id)
        {
            var schedule = DB.Schedules.FirstOrDefault(x => x.ID == id);
            var employeeEvents = DB.EmployeeEvents.Where(x => x.ScheduleID == id).ToList();

            if (schedule != null)
                schedule.ApprovedStatusID = (int)ApprovedStatus.PENDING;
                DB.SaveChanges();

            foreach (var empEvent in employeeEvents)
                empEvent.ApprovedStatusID = (int)ApprovedStatus.PENDING;
            DB.SaveChanges();
        }

        // DELETE request to delete a Schedule 
        public static void DeleteSchedule(ScheduleBuilderEntities DB, int id)
        {
            var schedule = DB.Schedules.FirstOrDefault(x => x.ID == id);
            var employeeEvents = DB.EmployeeEvents.Where(y => y.ScheduleID == id).ToList();

            foreach (var empEvent in employeeEvents)
                DB.EmployeeEvents.Remove(empEvent);
            DB.SaveChanges();

            if (schedule == null) return;
            DB.Schedules.Remove(schedule);
            DB.SaveChanges();
        }


        /* Trades */

        // POST to Approve a trade and swap the shifts
        public static void ApproveTrade(ScheduleBuilderEntities DB, int id)
        {
            var trade = DB.EmployeeTrades.FirstOrDefault(x => x.ID == id);
            if (trade == null) return;
            trade.AcceptStatusID = (int)AcceptStatus.APPROVED;
            trade.ApprovedStatusID = (int)ApprovedStatus.APPROVED;
            DB.SaveChanges();

            var employeeEvent = DB.EmployeeEvents.FirstOrDefault(x => x.ID == trade.EventID);
            if (employeeEvent == null) return;

            if (trade.SwapUserID != null)
                employeeEvent.EmployeeID = (int)trade.SwapUserID;
                DB.SaveChanges();
        }

        // POST to Deny a trade 
        public static void DenyTrade(ScheduleBuilderEntities DB, int id)
        {
            var trade = DB.EmployeeTrades.FirstOrDefault(x => x.ID == id);
            if (trade == null) return;
            trade.AcceptStatusID = (int)AcceptStatus.DENIED;
            trade.ApprovedStatusID = (int)ApprovedStatus.DENIED;
            DB.SaveChanges();
        }
        
    }
}