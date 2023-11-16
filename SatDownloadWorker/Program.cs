using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatDownloadWorker
{
    class Program
    {

        static void Main(string[] args)
        {
            try
            {
                using (var app = new SatDownloadWorkerApp(args))
                {
                    app.Init();
                    app.Run();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog appLog = new System.Diagnostics.EventLog();
                appLog.Source = args[0];
                appLog.WriteEntry(string.Format("Application Ended Abnormaly: {0} \r\n {1} ", ex.Message, ex.StackTrace));
                System.Environment.Exit(1);
            }
        }
    }
}
