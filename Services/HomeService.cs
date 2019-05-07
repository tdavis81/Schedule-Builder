using Newtonsoft.Json;
using ScheduleBuilderApp.Classes;
using ScheduleBuilderApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ScheduleBuilderApp.Services
{
    public class HomeService
    {
        private enum ApprovedStatus { APPROVED = 1, PENDING = 2, DENIED = 3, OPEN = 4 };
        private enum AcceptStatus { OPEN = 1, PENDING = 2, ACCEPTED = 3, DENIED = 4 };

        // GET Returns a list of View Types for /Home/ 
        public static List<SelectListItem> GetAllViewTypes()
        {
            var viewTypes = new List<SelectListItem>
            {
                new SelectListItem {Value = "-1", Text = "All Employees"},
                new SelectListItem {Value = UserSession.User.ID.ToString(), Text = UserSession.User.FirstName}
            };
            
            return viewTypes;
        }

        // GET returns a list of employee events depending on the ID they have choose either ALL or Signed In user
        public static List<EmployeeEvent> GetAllEvents(ScheduleBuilderEntities DB, int ViewTypeID)
        {
            if (ViewTypeID == 0 || ViewTypeID == -1)
                return DB.EmployeeEvents.Include("User").Include("Schedule").Include("ApprovedCode").Where(x => x.ApprovedStatusID == (int)ApprovedStatus.APPROVED).ToList();
            else
                return DB.EmployeeEvents.Include("User").Include("Schedule").Include("ApprovedCode").Where(x => x.ApprovedStatusID == (int)ApprovedStatus.APPROVED && x.EmployeeID == ViewTypeID).ToList();
        }

        // GET return a single Employee Trade
        public static EmployeeTrade GetEmployeeTrade(ScheduleBuilderEntities DB, int id)
        {
            return DB.EmployeeTrades.Include("User").Include("EmployeeEvent").FirstOrDefault(x => x.ID == id);
        }

        // GET returns a list of Employee Trades
        public static List<EmployeeTrade> GetAllTrades(ScheduleBuilderEntities DB)
        {
            return DB.EmployeeTrades.Include("EmployeeEvent").Include("AcceptCode").Include("ApprovedCode").Include("User").ToList();
        }

        // DELETE an employee trade
        public static void DeleteEmployeeTrade(ScheduleBuilderEntities DB,int id)
        {
            var trade = DB.EmployeeTrades.FirstOrDefault(x => x.ID == id);
            if (trade == null) return;
            DB.EmployeeTrades.Remove(trade);
            DB.SaveChanges();
        }
        
        // POST Update an Employee Trade
        public static void UpdateEmployeeTrade(ScheduleBuilderEntities DB, EmployeeTrade empTrade)
        {
            if (empTrade.ID == 0)
            {
                var newTrade = new EmployeeTrade()
                {
                    EventID = empTrade.EventID,
                    Position = empTrade.Position,
                    AcceptStatusID = (int)AcceptStatus.OPEN,
                    ApprovedStatusID = (int)ApprovedStatus.OPEN,
                    UserID = UserSession.User.ID
                };
                DB.EmployeeTrades.Add(newTrade);
                DB.SaveChanges();
            }
            else
            {
                var trade = DB.EmployeeTrades.FirstOrDefault(x => x.ID == empTrade.ID);
                if (trade == null) return;
                trade.EventID = empTrade.EventID;
                trade.Position = empTrade.Position;
                DB.SaveChanges();
            }

        }

        // POST Modify and accept status set to pending and add swap user id to DB
        public static void ModifyAcceptStatus(ScheduleBuilderEntities DB, EmployeeTrade empTrade)
        {
            var trade = DB.EmployeeTrades.FirstOrDefault(x => x.ID == empTrade.ID);
            if (trade == null) return;

            trade.AcceptStatusID = (int)AcceptStatus.PENDING;
            trade.ApprovedStatusID = (int)ApprovedStatus.PENDING;
            trade.SwapUserID = empTrade.SwapUserID;
            DB.SaveChanges();
            
        }

        public static User GetUserByEmail(ScheduleBuilderEntities DB, string email)
        {
            return DB.Users.FirstOrDefault(x => x.Email == email);
        }
    }
}