using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SATDownloadApp.Service
{
    public abstract class ServiceResponse
    {
        public const string StatusException = "Exception";
        public const string StatusOk = "Ok";
        public const string StatusUnknown = "Unknown";


        public ServiceResponse()
        {
            this.ResponseStatus = StatusUnknown;
            this.ResponseMessage = "Nothing";
        }

        public string ResponseStatus { get; set; }

        public string ResponseMessage { get; set; }

        public string StackTrace { get; set; }

    }
}