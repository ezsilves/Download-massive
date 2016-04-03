using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace SATDownloadApp.Model
{
    public class Ticket
    {

        public const string StatusPending = "pending";
        public const string StatusCancelled = "cancelled";
        public const string StatusProcessing = "processing";
        public const string StatusCompeted = "completed";
        public const string StatusError = "error";

        public Ticket()
        {
            this.RequestDate = DateTime.Today;
        }

        public virtual Guid Id { get; set; }
        public virtual DateTime RequestDate { get; set; }

        public virtual string Username { get; set; }

        [XmlIgnore]
        public virtual string Password { get; set; }

        public virtual DateTime DowndloadDateFrom { get; set;  }

        public virtual DateTime DowndloadDateTo { get; set; }

        public virtual string Status { get; set; }

        public virtual string Reason { get; set; }

        public virtual int TotalFilesToDownload { get; set; }

        public virtual int TotalDownloadedFiles { get; set; }


        private static TimeSpan maxDaysAllowed = new TimeSpan(3, 0, 0, 0);

        public virtual bool Validate()
        {
            var prevStatus = this.Status;
            this.Status = StatusCancelled;
            if(string.IsNullOrEmpty(Username))
            {
                Reason = "Username is null";
                return false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                Reason = "Password is null";
                return false;
            }

            if (DowndloadDateTo < DowndloadDateFrom)
            {
                Reason = "Date To less than Date From";
                return false;
            }

            if ((DowndloadDateTo - DowndloadDateFrom) > maxDaysAllowed)
            {
                Reason = string.Format("Can't request more than {0:N0} Days", maxDaysAllowed.TotalDays);
                return false;
            }
            this.Status = prevStatus;
            return true;
        }


        public virtual bool IsCompleted()
        {
            return this.Status == StatusCompeted;
        }
    }
}