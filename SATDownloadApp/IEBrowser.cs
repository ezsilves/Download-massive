using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;


namespace SATDownloadApp
{
    public class IEBrowser : System.Windows.Forms.ApplicationContext
    {
        AutoResetEvent resultEvent;

        private string _currentUrl;
        private bool _supplierStructure;

        string userName, password;
        string htmlResult;
        string htmlCookieTable;
        string htmlInputTable;
        string htmlScriptTable;

        int loginCount;
        int navigationCounter;

        WebBrowser ieBrowser;
        Form form;
        Thread thrd;

        public string HtmlResult
        {
            get { return htmlResult; }
        }

        public int NavigationCounter
        {
            get { return navigationCounter; }
        }

        public string HtmlCookieTable
        {
            get { return "Cookies:" + htmlCookieTable; }
        }

        public string HtmlInputTable
        {
            get { return "Input elements:" + htmlInputTable; }
        }

        public string HtmlScriptTable
        {
            get { return "Script variables:" + htmlScriptTable; }
        }

        /// <summary>
        /// class constructor 
        /// </summary>
        /// <param name="visible">whether or not the form and the WebBrowser control are visiable</param>
        /// <param name="userName">client user name</param>
        /// <param name="password">client password</param>
        /// <param name="resultEvent">functionality to keep the main thread waiting</param>
        public IEBrowser(string url, AutoResetEvent resultEvent)
        {
            this.resultEvent = resultEvent;
            this.htmlResult = null;


            thrd = new Thread(new ThreadStart(
                delegate
                {
                    Init(url);
                    System.Windows.Forms.Application.Run(this);
                }));
            // set thread to STA state before starting
            thrd.SetApartmentState(ApartmentState.STA);
        }


        public void StartRequest()
        {
            thrd.Start();
        }

        // initialize the WebBrowser and the form
        private void Init(string url)
        {
            //scriptCallback = new ScriptCallback(this);

            // create a WebBrowser control
            ieBrowser = new WebBrowser();
            // set the location of script callback functions
            //ieBrowser.ObjectForScripting = scriptCallback;
            // set WebBrowser event handls
            ieBrowser.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(IEBrowser_DocumentCompleted);
            ieBrowser.Navigating += new WebBrowserNavigatingEventHandler(IEBrowser_Navigating);

            //Test IE
            if (false)
            {
                this.form = new System.Windows.Forms.Form();
                this.ieBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
                this.form.Controls.Add(ieBrowser);
                this.form.Visible = false;
            }

            loginCount = 0;
            // initialise the navigation counter
            navigationCounter = 0;
            ieBrowser.Navigate(url);
        }

        // dipose the WebBrowser control and the form and its controls
        protected override void Dispose(bool disposing)
        {
            if (thrd != null)
            {
                thrd.Abort();
                thrd = null;
                return;
            }

            System.Runtime.InteropServices.Marshal.Release(ieBrowser.Handle);
            ieBrowser.Dispose();
            if (form != null) form.Dispose();
            base.Dispose(disposing);
        }

        // Navigating event handle
        void IEBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            // navigation count increases by one
            navigationCounter++;
            // write url into the form's caption
            if (form != null) form.Text = e.Url.ToString();
        }

        // DocumentCompleted event handle
        void IEBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            int anio;
            int num;
            int selectedItem1;
            int i;
            int j;
            bool flag;
            bool flag1;
            if (this.ieBrowser.Document != null)
            {
                if (this.ieBrowser.Url == new Uri("https://cfdiau.sat.gob.mx/nidp/app?sid=0"))
                {
                        this.ieBrowser.Navigate("https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaReceptor.aspx");
                }
                //if (this.ieBrowser.Url == new Uri("https://cfdiau.sat.gob.mx/nidp/app/login?sid=0&sid=0"))
                //{
                    //this.Message("Clave CIEC incorrecta, favor de verificar RFC y Clave.");
                //}

                flag = (this.ieBrowser.Url != new Uri("https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaReceptor.aspx") ? true : !(this.ieBrowser.Url != new Uri(this._currentUrl)));
                if (!flag)
                {
                    anio = 2014; // (int)this.cmbYear.SelectedItem;

                    //if (!this.rbtDay.Checked) //Verifica si la descarga NO es diaria
                    //{
                        int MesIni = 3; //int.Parse(this.MesIni_txt.SelectedItem.ToString());
                        int MesFin = 4; //int.Parse(this.MesFin_txt.SelectedItem.ToString());

                        for (i = MesIni; i <= MesFin; i++)
                        {
                            //this.Message(string.Format("Procesando documentos recibidos del ejercicio {0}, periodo {1} ...", anio, i));
                            this.GetReceivedDocuments(anio, i);
                        }
                    //}
                    //else
                    //{

                        //int DiaIni = int.Parse(this.DiaIni_txt.SelectedItem.ToString());
                        //int DiaFin = int.Parse(this.DiaFin_txt.SelectedItem.ToString());
                        //int Mes = int.Parse(this.mes_txt.SelectedItem.ToString());

                        //for (i = DiaIni; i <= DiaFin; i++)
                        //{
                            //this.Message(string.Format("Procesando documentos recibidos de la fecha  {0}/{1}/{2} ...", i.ToString("00"), Mes.ToString("00"), anio));
                            //this.GetReceivedDocuments(anio, Mes, i);
                        //}
                    //}

                    this.FinishSession();
                }

                //flag1 = (this.ieBrowser.Url != new Uri("https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaEmisor.aspx") ? true : !(this.ieBrowser.Url != new Uri(this.lblStatus.Text)));
                //if (!flag1)
                //{
                //    anio = (int)this.cmbYear.SelectedItem;

                //    if (!this.rbtDay.Checked) //Verifica si la descarga NO es diaria
                //    {
                //        int MesIni = int.Parse(this.MesIni_txt.SelectedItem.ToString());
                //        int MesFin = int.Parse(this.MesFin_txt.SelectedItem.ToString());

                //        for (i = MesIni; i <= MesFin; i++)
                //        {
                //            //this.Message(string.Format("Procesando documentos emitidos del ejercicio {0}, periodo {1} ...", anio, i));
                //            this.GetSendedDocuments(anio, i);
                //        }
                //    }
                //    else
                //    {
                //        int DiaIni = int.Parse(this.DiaIni_txt.SelectedItem.ToString());
                //        int DiaFin = int.Parse(this.DiaFin_txt.SelectedItem.ToString());
                //        int Mes = int.Parse(this.mes_txt.SelectedItem.ToString());

                //        for (i = DiaIni; i <= DiaFin; i++)
                //        {
                //            //this.Message(string.Format("Procesando documentos emitidos de la fecha  {0}/{1}/{2} ...", i.ToString("00"), Mes.ToString("00"), anio));
                //            this.GetSendedDocuments(anio, Mes, i);
                //        }
                //    }
                //    this.FinishSession();
                //}
                while (true)
                {
                    Application.DoEvents();
                    if (!this.ieBrowser.IsBusy)
                    {
                        break;
                    }
                }
                this._currentUrl = this.ieBrowser.Url.ToString();
            }
        }

        private void FinishSession()
        {
            try
            {
                //this.Message(string.Format("Cerrar sesion usuario {0}.", this.cmbAccount.SelectedItem));
                this.ieBrowser.Navigate(new Uri("https://portalcfdi.facturaelectronica.sat.gob.mx/logout.aspx?salir=y"));
                //this.Message("Proceso terminado.");
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }

        private void ProcessResult()
        {
            DateTime dateTime;
            string str1;
            bool flag;
            string str2;
            try
            {
                Regex regex = new Regex("'.*?'");
                if (!(this.ieBrowser.Document == null))
                {
                    HtmlElementCollection elementsByTagName = this.ieBrowser.Document.GetElementsByTagName("table");
                    foreach (System.Windows.Forms.HtmlElement htmlElement in elementsByTagName)
                    {
                        foreach (System.Windows.Forms.HtmlElement elementsByTagName1 in htmlElement.GetElementsByTagName("tr"))
                        {
                            HtmlElementCollection elementsByName = elementsByTagName1.GetElementsByTagName("img").GetElementsByName("BtnDescarga");
                            if (elementsByName.Count == 1)
                            {
                                string str3 = string.Format("https://portalcfdi.facturaelectronica.sat.gob.mx/{0}", regex.Match(elementsByName[0].OuterHtml).Value.Replace("'", ""));
                                IEnumerable<System.Windows.Forms.HtmlElement> htmlElements = elementsByTagName1.GetElementsByTagName("span").Cast<System.Windows.Forms.HtmlElement>();
                                IEnumerable<System.Windows.Forms.HtmlElement> htmlElements1 = htmlElements.Where<System.Windows.Forms.HtmlElement>((System.Windows.Forms.HtmlElement span) =>
                                {
                                    bool innerText = span.InnerText != "";
                                    return innerText;
                                });
                                string str4 = htmlElements1.Aggregate<System.Windows.Forms.HtmlElement, string>("", (string current, System.Windows.Forms.HtmlElement span) =>
                                {
                                    string str = string.Concat(current, span.InnerText, "|");
                                    return str;
                                });
                                string vDownloadPath = @"c:\cfdi\";
                                string vAccount = @"AABS741012IH5";
                                char[] chrArray = new char[] { '|' };
                                string[] strArrays = str4.Split(chrArray);
                                DateTime.TryParse(strArrays[5], out dateTime);
                                string[] text = new string[] { vDownloadPath, vAccount, null, null, null, null };
                                string[] strArrays1 = text;
                                //str1 = (this.rbtReceived.Checked ? "Received" : "Sended");
                                str1 = (true ? "Received" : "Sended");
                                strArrays1[2] = str1;
                                int year = dateTime.Year;
                                text[3] = year.ToString("0000");
                                year = dateTime.Month;
                                text[4] = year.ToString("00");
                                year = dateTime.Day;
                                text[5] = year.ToString("00");
                                string str5 = Path.Combine(text);
                                if (!Directory.Exists(str5))
                                {
                                    Directory.CreateDirectory(str5);
                                }
                                string str6 = Path.Combine(str5, string.Format("{0}_{1}_{2}.xml", strArrays[1], strArrays[3], strArrays[0]));
                                this.DownloadFile(str3, str6);
                                //flag = (!this._supplierStructure ? true : !this.rbtReceived.Checked);
                                flag = (!this._supplierStructure ? true : false);
                                if (!flag)
                                {
                                    text = new string[] { vDownloadPath, vAccount, null, null, null, null, null };
                                    string[] strArrays2 = text;
                                    //str2 = (this.rbtReceived.Checked ? "Received" : "Sended");
                                    str2 = (true ? "Received" : "Sended");
                                    strArrays2[2] = str2;
                                    text[3] = strArrays[1];
                                    year = dateTime.Year;
                                    text[4] = year.ToString("0000");
                                    year = dateTime.Month;
                                    text[5] = year.ToString("00");
                                    year = dateTime.Day;
                                    text[6] = year.ToString("00");
                                    str5 = Path.Combine(text);
                                    if (!Directory.Exists(str5))
                                    {
                                        Directory.CreateDirectory(str5);
                                    }
                                    str6 = Path.Combine(str5, string.Format("{0}_{1}_{2}.xml", strArrays[1], strArrays[3], strArrays[0]));
                                    this.DownloadFile(str3, str6);
                                }
                            }
                        }
                    }
                }
                else
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }


        private void GetReceivedDocuments(int year, int month)
        {
            bool flag;
            bool flag1;
            bool flag2;
            try
            {
                if (this.ieBrowser.Document != null)
                {
                    System.Windows.Forms.HtmlElement elementById = this.ieBrowser.Document.GetElementById("ctl00_MainContent_RdoFechas");
                    if (elementById != null)
                    {
                        elementById.InvokeMember("click");
                    }
                    while (true)
                    {
                        flag = true;
                        Application.DoEvents();
                        System.Windows.Forms.HtmlElement htmlElement = this.ieBrowser.Document.GetElementById("DdlAnio");
                        flag1 = (htmlElement == null ? true : !(htmlElement.GetAttribute("disabled") != "disabled"));
                        if (!flag1)
                        {
                            break;
                        }
                    }
                    System.Windows.Forms.HtmlElement elementById1 = this.ieBrowser.Document.GetElementById("DdlAnio");
                    if (elementById1 != null)
                    {
                        elementById1.SetAttribute("value", year.ToString(CultureInfo.InvariantCulture));
                    }
                    System.Windows.Forms.HtmlElement htmlElement1 = this.ieBrowser.Document.GetElementById("ctl00_MainContent_CldFecha_DdlMes");
                    if (htmlElement1 != null)
                    {
                        htmlElement1.SetAttribute("value", month.ToString("0"));
                    }
                    System.Windows.Forms.HtmlElement elementById2 = this.ieBrowser.Document.GetElementById("ctl00_MainContent_BtnBusqueda");
                    if (elementById2 != null)
                    {
                        elementById2.InvokeMember("click");
                    }
                    int num = 0;
                    while (true)
                    {
                        flag = true;
                        Application.DoEvents();
                        System.Windows.Forms.HtmlElement htmlElement2 = this.ieBrowser.Document.GetElementById("ctl00_MainContent_UpdateProgress1");
                        if (!(htmlElement2 == null))
                        {
                            flag2 = (htmlElement2.Style != "display: none;" ? true : num <= 100000);
                            if (!flag2)
                            {
                                break;
                            }
                            num++;
                        }
                    }
                    if (this.ieBrowser.Document.GetElementById("ctl00_MainContent_PnlLimiteRegistros") != null)
                    {
                        MessageBox.Show("La consulta realizada solo muestra los primeros 500 registros, se recomienda hacer la descarga por día", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    this.ProcessResult();
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }


        public void NavigateTo(string url, AutoResetEvent ev)
        {
            this.resultEvent = ev;
            this.thrd = new Thread(() =>
            {
                ieBrowser.Navigate(url);
            });
            this.thrd.Start();
        }



        private void DownloadFile(string address, string fileName)
        {
            try
            {
                string cookie = CookieReader.GetCookie(this.ieBrowser.Url.ToString());
                WebClient webClient = new WebClient();
                if (File.Exists(fileName))
                {
                    //this.Message(string.Format("El archivo {0} ya existe.\n", fileName));
                }
                else
                {
                    webClient.Headers.Add(HttpRequestHeader.Cookie, cookie);
                    webClient.DownloadFile(address, fileName);
                    //this.Message(string.Format("Creando el archivo {0}.\n", fileName));
                }
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }
        }


    }
}