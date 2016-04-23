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

        protected void Button2_Click(object sender, EventArgs e)
        {
            try
            {

                Repository.SatDownloadRepository.Instance.Log(Log.LogLevel.TRACE, "Test Connection");
                using (var session = SatDownloadRepository.Instance.OpenSession())
                {
                    
                    using (var tx = session.BeginTransaction())
                    {
                        var t = new Ticket()
                        {
                            RequestDate = DateTime.Today,
                            DowndloadDateFrom = DateTime.Today,
                            DowndloadDateTo = DateTime.Today,
                            Username = "User",
                            Password = "Password",
                        };
                        session.Save(t);

                        tx.Commit();
                        Repository.SatDownloadRepository.Instance.Log(t, Log.LogLevel.TRACE, "Test Ticket");

                    }
                }
                Label1.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss} - Database Ok", DateTime.Now);
            }
            catch (Exception ex)
            {
                Label1.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss} - Database Nok: {1}", DateTime.Now, ex.Message);
            }
        }

        protected void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                SatDownloadRepository.Instance.BuildSchema();
                Label1.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss} - Build Schema Ok", DateTime.Now);
            }
            catch (Exception ex)
            {
                Label1.Text = string.Format("{0:yyyy-MM-dd HH:mm:ss} - Build Schema Nok {1}", DateTime.Now, ex.Message);
            }
        }
    }
}