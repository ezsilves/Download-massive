using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SATDownloadApp.Model
{
    public class Log
    {
        public Log()
        {
        }

        public Log(LogLevel level, string msg, params object[] args)
        {
            this.DateTime = DateTime.Now;
            this.Line = string.Format(msg, args);
            this.Level = level;
        }

        public virtual Guid Id { get; set; }

        public virtual DateTime DateTime { get; set; }
        public virtual string TicketId { get; set; }

        public virtual LogLevel Level { get; set; }
        public virtual string Line { get; set; }

        public override int GetHashCode()
        {
            return this.DateTime.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var o = obj as Log;
            if (o != null)
            {
                return this.DateTime == o.DateTime &&
                    this.TicketId == o.TicketId;
            }
            return false;
        }

        public override string ToString()
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss} - Ticket [{1}]: {2}", this.DateTime, this.TicketId, this.Line);
        }

        public enum LogLevel
        {
            DBG= 0,
            TRACE = 1,
            INFO = 2,
            WARN = 3,
            ERR = 4

        }
    }
}