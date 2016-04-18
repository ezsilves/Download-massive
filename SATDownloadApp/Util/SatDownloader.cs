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

        public const string AuthUrl2 = "https://cfdiau.sat.gob.mx/nidp/app?sid=0";

        public const string DownloadPage = "https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaReceptor.aspx";

        public const string Logout = "https://portalcfdi.facturaelectronica.sat.gob.mx/logout.aspx?salir=y";

        public const string DownloadBase = "https://portalcfdi.facturaelectronica.sat.gob.mx/{0}";

        private List<SatXml> FilesToDownload;

        private string cookie;

        private Ticket ticket;

        private Thread thread;

        WebBrowser ieBrowser;

        private bool completed = false;

        System.Windows.Forms.Form form;

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
            this.form = new System.Windows.Forms.Form();

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
                ieBrowser = new WebBrowser();
                ieBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(IEBrowser_AuthUrl1_DocumentCompleted);
                var url = string.Format(AuthUrl1, this.Username, this.Password);
                ieBrowser.Navigate(url);

                this.ieBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
                this.form.Controls.Add(ieBrowser);
                this.form.Visible = false;

                System.Windows.Forms.Application.Run(this);

            }
            catch (ThreadAbortException)
            {
            }

        }

        private void IEBrowser_AuthUrl1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.ieBrowser.Document == null)
                return;


            if (this.ieBrowser.Document.Body.InnerHtml.Contains("Clave CIEC incorrecta"))
            {
                lock (ticket)
                {
                    ticket.Status = Ticket.StatusError;
                    ticket.Reason = "Clave CIEC incorrecta";

                    Repository.SatDownloadRepository.Instance.Save(ticket);

                    this.completed = true;
                    return;
                }
            }

            //TODO OK?
            ieBrowser.DocumentCompleted -= IEBrowser_AuthUrl1_DocumentCompleted;

            ieBrowser.DocumentCompleted += IEBrowser_DownloadPage_DocumentCompleted;
            this.ieBrowser.Navigate(DownloadPage);

        }

        private void IEBrowser_DownloadPage_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (this.ieBrowser.Url.ToString() != DownloadPage)
            {
                Application.DoEvents();
                this.ieBrowser.Navigate(DownloadPage);
                return;
            }

            this.ieBrowser.DocumentCompleted -= IEBrowser_DownloadPage_DocumentCompleted;

            lock (ticket)
            {
                bool ok = true;

                for (DateTime i = ticket.DowndloadDateFrom; i <= ticket.DowndloadDateTo; i = i.AddDays(1))
                {

                    this.GetReceivedDocuments(i);
                    if (ticket.Status == Ticket.StatusError)
                    {
                        ok = false;
                        break;
                    }

                    //this.ieBrowser.Refresh();
                    //Application.DoEvents();
                }

                var cookie = CookieReader.GetCookie(this.ieBrowser.Url.ToString());

                //TODO: hasta aca se tiene se que sincronizar.

                if (ok)
                {
                    ticket.Status = Ticket.StatusDownloading;
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

                if (ok)
                {
                    ticket.Status = Ticket.StatusCompeted;
                }

                Repository.SatDownloadRepository.Instance.Save(ticket);

                this.ieBrowser.DocumentCompleted += IEBrowser_Completed_DocumentCompleted;
                this.ieBrowser.Navigate(new Uri(Logout));
            }

        }

        private void IEBrowser_Completed_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
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
                    Thread.Sleep(1000);
                    Application.DoEvents();

                    //TODO: No esta reconociendo el día
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

                this.form.Visible = false;
                this.form.Close();
                this.form.Controls.Clear();
                this.form.Dispose();
            }
            catch (Exception)
            {

                throw;
            }

            base.Dispose(disposing);
        }
    }
}