using SATDownloadApp.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace SATDownloadApp.Util
{
    public class SatDownloader : System.Windows.Forms.ApplicationContext
    {
        public string Username { get { return ticket.Username; } }
        public string Password { get { return ticket.Password; } }

        public const string AuthUrl1 = "https://cfdiau.sat.gob.mx/nidp/app/login?id=SATUPCFDiCon&sid=0&option=credential&sid=0&Ecom_User_ID={0}&Ecom_Password={1}";

        public const string AuthUrl2 = "https://cfdiau.sat.gob.mx/nidp/jsp/content.jsp";

        public const string DownloadPage = "https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaReceptor.aspx";

        public const string Logout = "https://portalcfdi.facturaelectronica.sat.gob.mx/logout.aspx?salir=y";

        public const string DownloadBase = "https://portalcfdi.facturaelectronica.sat.gob.mx/{0}";

        private List<SatXml> FilesToDownload;

        private string cookie;

        private Ticket ticket;

        private Thread thread;

        WebBrowser ieBrowser;

        private bool completed = false;

        //System.Windows.Forms.Form form;

        public SatDownloader(Ticket ticket)
        {
            this.ticket = ticket;
            this.FilesToDownload = new List<SatXml>();
        }

        public void Init()
        {
            ticket.Status = Ticket.StatusProcessing;
            Repository.SatDownloadRepository.Instance.Save(ticket);

            thread = new Thread(RunThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Name = "Worker Ticket: " + ticket.Id;
            //this.form = new System.Windows.Forms.Form();

        }

        public Thread Start()
        {
            thread.Start();
            return thread;
        }

        private void RunThread()
        {
            try
            {
                Repository.SatDownloadRepository.Instance.Log(this.ticket,Log.LogLevel.TRACE, "RunThread - load browser");

                ieBrowser = new WebBrowser();
                ieBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(IEBrowser_AuthUrl1_DocumentCompleted);
                var url = string.Format(AuthUrl1, this.Username, this.Password);
                ieBrowser.Navigate(url);

                Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "RunThread - browser navigate");

                System.Windows.Forms.Application.Run(this);

            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                ticket.Status = Ticket.StatusError;
                ticket.Reason = string.Format("RunThread  - {0} \r\n {1}", ex.Message, ex.StackTrace);
                Repository.SatDownloadRepository.Instance.Save(ticket);
            }

        }

        private void IEBrowser_AuthUrl1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.ieBrowser.Document == null)
                return;

            Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "Auth 1 - Ok");

            //TODO OK?
            ieBrowser.DocumentCompleted -= IEBrowser_AuthUrl1_DocumentCompleted;

            ieBrowser.DocumentCompleted += IEBrowser_AuthUrl2_DocumentCompleted;
            this.ieBrowser.Navigate(AuthUrl2);
            Application.DoEvents();
        }

        private int tryAuth2Url = 0;

        private void IEBrowser_AuthUrl2_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            if (this.ieBrowser.Url.ToString() != AuthUrl2)
            {
                Application.DoEvents();
                tryAuth2Url++;
                if (tryAuth2Url >= 10)
                {
                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.WARN, "Can't Access Auth 2: \r\nCurrent Url: {0}\r\nContent:\r\n{1}", this.ieBrowser.Url, this.ieBrowser.Document.Body.InnerHtml);

                    End();
                    return;
                }
                this.ieBrowser.Navigate(AuthUrl2);
                return;
            }

            var str1 = "Your session has been authenticated";
            var str2 = "Se ha autenticado su sesión";

            if (!this.ieBrowser.Document.Body.InnerHtml.Contains(str1) &&  
                !this.ieBrowser.Document.Body.InnerHtml.Contains(str2))
            {
                lock (ticket)
                {

                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "Auth - failed");

                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.DBG, "Url: {0} \r\n Auth Response: {1}", this.ieBrowser.Url, this.ieBrowser.Document.Body.InnerHtml);

                    ticket.Status = Ticket.StatusError;
                    ticket.Reason = "Incorrect user or password";

                    Repository.SatDownloadRepository.Instance.Save(ticket);

                    End();
                    return;
                }
            }

            Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "Auth 2 - Ok");

            ieBrowser.DocumentCompleted -= IEBrowser_AuthUrl2_DocumentCompleted;

            ieBrowser.DocumentCompleted += IEBrowser_DownloadPage_DocumentCompleted;
            this.ieBrowser.Navigate(DownloadPage);
            Application.DoEvents();
        }


        private int tryDownloadPage = 0;

        private void IEBrowser_DownloadPage_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.ieBrowser.Url.ToString() != DownloadPage)
            {
                Application.DoEvents();
                tryDownloadPage++;
                if (tryDownloadPage >= 10)
                {
                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.WARN, "Can't Access Download: \r\nCurrent Url: {0}\r\nContent:\r\n{1}", this.ieBrowser.Url, this.ieBrowser.Document.Body.InnerHtml);

                    End();
                    return;
                }
                this.ieBrowser.Navigate(DownloadPage);
                return;
            }

            this.ieBrowser.DocumentCompleted -= IEBrowser_DownloadPage_DocumentCompleted;

            lock (ticket)
            {
                bool ok = true;


                for (DateTime i = ticket.DowndloadDateFrom; i <= ticket.DowndloadDateTo; i = i.AddDays(1))
                {
                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "Accessing Files for {0:yyyy-MM-dd}", i);

                    this.GetReceivedDocuments(i);
                    if (ticket.Status == Ticket.StatusError)
                    {
                        ok = false;
                        Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.ERR, "Error accessing files: {0}", ticket.Reason);
                        break;
                    }

                    //this.ieBrowser.Refresh();
                    //Application.DoEvents();
                }

                var cookie = CookieReader.GetCookie(this.ieBrowser.Url.ToString());

                //TODO: hasta aca se tiene se que sincronizar.

                if (ok)
                {
                    Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "Downloading {0} Files", FilesToDownload.Count);

                    ticket.Status = Ticket.StatusDownloading;
                    ticket.Reason = "Downloading";
                    ticket.TotalFilesToDownload = FilesToDownload.Count;
                    Repository.SatDownloadRepository.Instance.Save(ticket);
                    using (var s = Repository.SatDownloadRepository.Instance.OpenSession())
                    {
                        using (var tx = s.BeginTransaction())
                        {
                            foreach (var ftd in FilesToDownload)
                            {

                                try
                                {
                                    ftd.Document = DownloadFile(ftd.FileAddress, cookie);
                                    s.Save(ftd);
                                    ticket.TotalDownloadedFiles++;
                                }
                                catch (Exception ex)
                                {
                                    ok = false;
                                    ticket.Status = Ticket.StatusError;
                                    ticket.Reason = ex.Message;
                                }
                            }
                            tx.Commit();
                        }
                    }
                }

                ticket.Reason = "";

                if (ok)
                {
                    ticket.Status = Ticket.StatusCompeted;
                    ticket.Reason = "Done";
                }

                Repository.SatDownloadRepository.Instance.Save(ticket);
                Repository.SatDownloadRepository.Instance.Log(this.ticket, Log.LogLevel.TRACE, "End", FilesToDownload.Count);

                this.ieBrowser.DocumentCompleted += IEBrowser_Completed_DocumentCompleted;
                this.ieBrowser.Navigate(new Uri(Logout));
            }

        }

        private void IEBrowser_Completed_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            End();
        }

        private void End()
        {
            this.completed = true;
            Application.ExitThread();
        }


        private void GetReceivedDocuments(DateTime date)
        {
            try
            {
                HtmlElement htmlElement = null;

                HtmlElement elementById = this.ieBrowser.Document.GetElementById("ctl00_MainContent_RdoFechas");
                HtmlElement elementByIdn = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_LblFecha");

                if (elementById != null)
                {
                    elementByIdn.InnerText = "<--Click";
                    elementById.InvokeMember("click");
                }

                WaitProgress();

                do
                {
                    Application.DoEvents();
                    Thread.Sleep(10);
                    htmlElement = this.ieBrowser.Document.GetElementById("DdlAnio");
                }
                while (htmlElement == null || htmlElement.GetAttribute("disabled") == "disabled");

                HtmlElement DdlAnio = htmlElement;
                if (DdlAnio != null)
                {
                    DdlAnio.SetAttribute("value", date.Year.ToString(CultureInfo.InvariantCulture));
                }

                HtmlElement DdlMes = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_DdlMes");
                if (DdlMes != null)
                {
                    DdlMes.SetAttribute("value", date.Month.ToString("0"));
                }

                do
                {
                    Application.DoEvents();
                    Thread.Sleep(500);
                    Application.DoEvents();

                    HtmlElement DdlDia = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_DdlDia");
                    if (DdlDia != null)
                    {
                        DdlDia.SetAttribute("value", date.Day.ToString("00"));
                    }

                    HtmlElement BtnBusqueda = this.ieBrowser.Document.GetElementById("ctl00_MainContent_BtnBusqueda");
                    if (BtnBusqueda != null)
                    {
                        BtnBusqueda.InvokeMember("click");
                    }

                    WaitProgress();

                    if (this.ieBrowser.Document.GetElementById("ctl00_MainContent_PnlLimiteRegistros") != null)
                    {
                        throw new Exception("Download Limit Reached, use lower time range to download");
                    }

                }
                while (!IsPageForDate(date));
                //while (true)
                //    Application.DoEvents();
                //ctl00_MainContent_PnlNoResultados
                this.ProcessResult();

            }
            catch (Exception ex)
            {
                ticket.Status = Ticket.StatusError;
                ticket.Reason = ex.Message;
            }
        }

        private bool IsPageForDate(DateTime date)
        {
            var htmlElement = this.ieBrowser.Document.GetElementById("DdlAnio");
            if (htmlElement == null)
                return false;

            var year = htmlElement.NextSibling.Children[1].InnerText;

            bool flag = year == date.Year.ToString();

            htmlElement = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_DdlMes");

            var month = htmlElement.NextSibling.Children[1].InnerText;

            flag = flag && htmlElement != null &&
                   month == date.Month.ToString("00");


            htmlElement = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_DdlDia");

            var day = htmlElement.NextSibling.Children[1].InnerText;

            flag = flag && htmlElement != null &&
                    day == date.Day.ToString("00");

            return flag;
        }

        private void WaitProgress()
        {
            TimeSpan limit = new TimeSpan(0, 0, 30);
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var flag = false;
            do
            {
                flag = true;
                Thread.Sleep(100);
                Application.DoEvents();
                Thread.Sleep(10);
                HtmlElement htmlElement2 = this.ieBrowser.Document.GetElementById("ctl00_MainContent_UpdateProgress1");

                if (htmlElement2 != null)
                {
                    if (htmlElement2.Style == "display: none;" || sw.Elapsed > limit)
                    {
                        flag = false;
                    }
                }

                if (sw.Elapsed > limit)
                {
                    throw new TimeoutException(string.Format("Timeout {0} waiting progress windows to close, try again in a few minutes", limit));
                }

            }
            while (flag);

            Application.DoEvents();
            Thread.Sleep(10);
        }

        private void ProcessResult()
        {


            try
            {
                Regex regex = new Regex("'.*?'");
                if (!(this.ieBrowser.Document == null))
                {
                    HtmlElementCollection elementsByTagName = this.ieBrowser.Document.GetElementsByTagName("table");
                    foreach (HtmlElement htmlElement in elementsByTagName)
                    {
                        foreach (HtmlElement elementsByTagName1 in htmlElement.GetElementsByTagName("tr"))
                        {
                            HtmlElementCollection elementsByName = elementsByTagName1.GetElementsByTagName("img").GetElementsByName("BtnDescarga");
                            if (elementsByName.Count == 1)
                            {
                                string fileAdress = string.Format(DownloadBase, regex.Match(elementsByName[0].OuterHtml).Value.Replace("'", ""));

                                Guid IdExterno = Guid.Empty;

                                foreach (HtmlElement span in elementsByTagName1.GetElementsByTagName("span"))
                                {
                                    if (Guid.TryParse(span.InnerText, out IdExterno))
                                    {
                                        break;
                                    }
                                }

                                var satXml = new SatXml()
                                {
                                    Id = Guid.NewGuid(),
                                    ExternalId = IdExterno.ToString(),
                                    RequestTicket = this.ticket,
                                    FileAddress = fileAdress
                                };

                                FilesToDownload.Add(satXml);
                            }
                        }
                    }
                }
                else
                {
                    ticket.Status = Ticket.StatusError;
                    ticket.Reason = "Couldn't get document";

                    return;
                }
            }
            catch (Exception ex)
            {
                ticket.Status = Ticket.StatusError;
                ticket.Reason = ex.Message;
            }

        }

        private string DownloadFile(string address, string cookie)
        {
            try
            {

                WebClient webClient = new WebClient();
                webClient.Headers.Add(HttpRequestHeader.Cookie, cookie);
                var file = webClient.DownloadData(address);

                return System.Text.UTF8Encoding.UTF8.GetString(file);
            }
            catch (Exception)
            {
                throw;
            }
            return string.Empty;
        }

        // dipose the WebBrowser control and the form and its controls
        protected override void Dispose(bool disposing)
        {
            if (thread != null &&
                (thread.ThreadState == ThreadState.WaitSleepJoin ||
                 thread.ThreadState == ThreadState.Running))
            {
                thread.Abort();
                thread = null;
            }
            try
            {
                System.Runtime.InteropServices.Marshal.Release(ieBrowser.Handle);

                //this.form.Visible = false;
                //this.form.Close();
                //this.form.Controls.Clear();
                //this.form.Dispose();
            }
            catch (Exception)
            {

                throw;
            }

            base.Dispose(disposing);
        }
    }
}