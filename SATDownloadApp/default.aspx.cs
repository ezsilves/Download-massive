using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;

using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using SATDownloadApp.Model;
using SATDownloadApp.Repository;

namespace SATDownloadApp
{



    public partial class _default : System.Web.UI.Page
    {

        IEBrowser browser;

        protected void Page_Load(object sender, EventArgs e)
        {
        }


        protected void Button1_Click(object sender, EventArgs e)
        {
            //if (TextBox1.Text.Length == 0) return;
            //if (TextBox2.Text.Length == 0) return;

            AutoResetEvent resultEvent = new AutoResetEvent(false);
            string result = null;

            var url = string.Format("https://cfdiau.sat.gob.mx/nidp/app/login?id=SATUPCFDiCon&sid=0&option=credential&sid=0&Ecom_User_ID={0}&Ecom_Password={1}", "AABS741012IH5", "Mexico86");

            browser = new IEBrowser(url, resultEvent);

            //TODO: Esto me tiene que devolver un Awaitable!
            browser.StartRequest();

            // wait for the third thread getting result and setting result event
            EventWaitHandle.WaitAll(new AutoResetEvent[] { resultEvent });
            // the result is ready later than the result event setting somtimes 
            while (browser == null || browser.HtmlResult == null) Thread.Sleep(5);

            result = browser.HtmlResult;

            resultEvent.Reset();

            browser.NavigateTo("https://portalcfdi.facturaelectronica.sat.gob.mx/ConsultaReceptor.aspx", resultEvent);
            EventWaitHandle.WaitAll(new AutoResetEvent[] { resultEvent });

            result = browser.HtmlResult;

            browser.Dispose();

        }

        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (var session = SatDownloadRepository.Instance.OpenSession())
                {
                    using (var tx = session.BeginTransaction())
                    {
                        var t = new Ticket()
                        {
                            RequestDate = DateTime.Today,
                            Username = "User",
                            Password = "Password",
                        };
                        session.Save(t);

                        tx.Commit();
                    }
                }
                Label1.Text = "Database Ok";
            }
            catch (Exception ex)
            {
                Label1.Text = "Database NOk: " + ex.Message;
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                SatDownloadRepository.Instance.BuildSchema();
                Label1.Text = "Build Schema Ok";
            }
            catch (Exception ex)
            {
                Label1.Text = "Build Schema NOk: " + ex.Message;
            }
            
        }


    }
}