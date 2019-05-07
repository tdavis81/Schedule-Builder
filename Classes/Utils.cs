using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ScheduleBuilderApp.Classes
{
    public class Utils
    {
        public static string sha256Encrpyt(string saltedValue)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(saltedValue));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
        public static string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
        public static DateTime DetermineSunday(DateTime from)
        {
            int start = (int)from.DayOfWeek;
            int target = (int)DayOfWeek.Sunday;
            int UserStartDay = 0;

            // If it is Wednesday - Saturday, get (upcoming) saturday
            if (start >= UserStartDay)
            {
                if (target <= start)
                {
                    target += 7;
                }
            }

            return from.AddDays(target - start);
        }

    }
}