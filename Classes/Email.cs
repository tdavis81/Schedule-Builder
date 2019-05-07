using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace ScheduleBuilderApp.Classes
{
    public class Email
    {
        public static bool Send(string to, string subject, string body)
        {
            try
            {
                var fromAddress = new MailAddress("ScheduleBuilder@donotreply.com", "ScheduleBuilder");

                var toAddress = new MailAddress(to, to);

                SmtpClient smtp = new SmtpClient()
                {
                    Host = "smtp.gmail.com",
                    Port = 25,
                    EnableSsl = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                var message = new MailMessage(fromAddress, toAddress)
                {
                    IsBodyHtml = true,
                    Body = body.Trim(),
                    Subject = subject.Trim()
                };

                smtp.Send(message);
                return true;
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
                return false;
            }
        }
    }
}