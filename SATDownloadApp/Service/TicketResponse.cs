using SATDownloadApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SATDownloadApp.Service
{
    public class TicketResponse : ServiceResponse
    {

        public TicketResponse()
        {
 
        }

        public TicketResponse(Ticket t)
        {
            this.Ticket = t;
            this.ResponseStatus = TicketResponse.StatusOk;
        }

        public TicketResponse(Exception ex)
        {
            this.ResponseStatus = TicketResponse.StatusException;
            this.StackTrace = ex.StackTrace;
            this.ResponseMessage = ex.Message;
        }

        public Ticket Ticket { get; set; }


    }
}