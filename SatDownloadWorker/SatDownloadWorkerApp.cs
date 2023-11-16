using SatDownloadLibrary.Model;
using SatDownloadLibrary.Repositories;
using SatDownloadWorker.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SatDownloadWorker
{
    class SatDownloadWorkerApp : IDisposable
    {
        private string[] args;

        public SatDownloadWorkerApp(string[] args)
        {
            this.args = args;
        }

        public void Init()
        {
            Console.WriteLine("Iniciando Servicio");
        }

        public void Run()
        {
            while (true)
            {
                Ticket pending = SatDownloadRepository.Instance.GetPendigTicket();

                if (pending == null)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("Procesando ticket: {0}", pending);
                    ProcessTicket(pending);
                    Console.WriteLine("ticket [{0}] procesado", pending);
                }
            }
        }

        private void ProcessTicket(Ticket pending)
        {
            var downloader = new Util.SatDownloader(pending);

            downloader.Init();
            var t = downloader.Start();
            
            if(!t.Join(Settings.Default.WorkerTimeout * 1000))
            {
                downloader.ExitThread();
                pending.Status = Ticket.StatusCancelled;
                pending.Reason = "Timeout";
                SatDownloadRepository.Instance.Save(pending);
            }
        }

        public void Dispose()
        {
            
        }
    }
}
