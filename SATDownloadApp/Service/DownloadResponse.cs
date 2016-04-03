using SATDownloadApp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SATDownloadApp.Service
{
    public class DownloadResponse : ServiceResponse
    {

        public const string StatusIncomplete = "incomplete";

        public DownloadResponse()
        { 
        }

        public DownloadResponse(List<SatXml> files)
        {
            this.Files = files;
            this.ResponseStatus = TicketResponse.StatusOk;
            this.ResponseMessage = "Download Completed";
        }

        public DownloadResponse(Exception ex)
        {
            this.ResponseStatus = TicketResponse.StatusException;
            this.StackTrace = ex.StackTrace;
            this.ResponseMessage = ex.Message;
        }

        public Ticket Ticket { get; set; }
        public List<SatXml> Files { get; set;  }
    }
}