using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace SATDownloadApp
{
    /// <summary>
    /// Descripción breve de wsDownload
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class wsDownload : System.Web.Services.WebService
    {

        [WebMethod]
        public string Download()
        {

            try
            {
                return "OK";
            }
            catch (Exception)
            {
                
                throw;
            }

        }
        
        [WebMethod]
        public string newTicket()
        {
            return "Hola a todos";
        }

        [WebMethod]
        public string getTicketStatus()
        {
            return "Hola a todos";
        }

        [WebMethod]
        public string getTicketInfo()
        {
            return "Hola a todos";
        }

    }
}
