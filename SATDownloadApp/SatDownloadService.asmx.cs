using Hangfire;
using SATDownloadApp.Model;
using SATDownloadApp.Repository;
using SATDownloadApp.Service;
using SATDownloadApp.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace SATDownloadApp
{
    /// <summary>
    /// Descripción breve de SatDownloadService
    /// </summary>
    [WebService(Namespace = "SatDownloadService")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class SatDownloadService : System.Web.Services.WebService
    {

        /// <summary>
        /// TODO: no devolver el ticket, solo el Id
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [WebMethod]
        public TicketResponse DownloadRequest(string username, string password, string from, string to)
        {

            DateTime downloadFrom = DateTime.MinValue;
            DateTime downloadTo = DateTime.MinValue;

            try
            {
                downloadFrom = Parse(from);
                downloadTo = Parse(to);
            }
            catch (Exception)
            {
                var resp = new TicketResponse();
                resp.ResponseStatus = Ticket.StatusCancelled;
                resp.ResponseMessage = "Invalid Date Format";
                return resp;
            }


            try
            {
                var ticket = new Ticket()
                {
                    Status = Ticket.StatusPending,
                    DowndloadDateFrom = downloadFrom,
                    DowndloadDateTo = downloadTo,
                    Username = username,
                    Password = password,
                };

                var ret = new TicketResponse(ticket);
                if (ticket.Validate())
                {
                    SatDownloadRepository.Instance.Save(ticket);
                    ret.ResponseMessage = "Your request will be processed";

                    BackgroundJob.Enqueue(() => ProcesTicket(ticket));
                }
                else
                {
                    ret.ResponseStatus = Ticket.StatusCancelled;
                    ret.ResponseMessage = ticket.Reason;
                }

                return ret;
            }
            catch (Exception ex)
            {
                return new TicketResponse(ex);
            }

        }

        public void ProcesTicket(Ticket ticket)
        {
            var downloader = new SatDownloader(ticket);
            downloader.Init();
            downloader.Start().Join();
            
        }

        private DateTime Parse(string d)
        {
            var formats = new string[] { "yyyyMMdd", "yyyy-MM-dd" };

            return DateTime.ParseExact(d, formats, CultureInfo.CurrentCulture.DateTimeFormat, DateTimeStyles.AssumeLocal);
        }

        [WebMethod]
        public TicketResponse GetTicketStatus(string guid)
        {
            try
            {

                var t = SatDownloadRepository.Instance.GetTicket(Guid.Parse(guid));
                var ret = new TicketResponse(t);
                ret.ResponseMessage = "Ticket found";
                return ret;


            }
            catch (Exception ex)
            {
                return new TicketResponse(ex);
            }

        }

        [WebMethod]
        public DownloadResponse GetDownloadInfo(string guid)
        {

            var ret = new DownloadResponse();

            try
            {

                var t = SatDownloadRepository.Instance.GetTicket(Guid.Parse(guid));

                if (t == null)
                {
                    ret.ResponseStatus = DownloadResponse.StatusUnknown;
                    ret.ResponseMessage = "Ticket not found";
                }
                else if (t.IsCompleted())
                {
                    ret.Ticket = t;
                    ret.Files = SatDownloadRepository.Instance.GetXmls(t);
                    ret.ResponseStatus = DownloadResponse.StatusOk;
                    ret.ResponseMessage = "Download Complete";
                }
                else
                {
                    ret.ResponseStatus = DownloadResponse.StatusIncomplete;
                    ret.ResponseMessage = "Download Incomplete";
                }
            }
            catch (FormatException)
            {
                ret.ResponseStatus = DownloadResponse.StatusUnknown;
                ret.ResponseMessage = "Invalid Ticket Id";
 
            }
            catch (Exception ex)
            {
                ret.ResponseStatus = ServiceResponse.StatusException;
                ret.ResponseMessage = ex.Message;
                ret.StackTrace = ex.StackTrace;
            }

            return ret;
        }

    }
}
