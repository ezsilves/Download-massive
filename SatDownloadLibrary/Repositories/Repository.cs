using NHibernate;
using NHibernate.Cfg;
using NHibernate.Criterion;
using SatDownloadLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SatDownloadLibrary.Repositories
{
    public class SatDownloadRepository
    {

        public static SatDownloadRepository Instance { get; private set; }

        static SatDownloadRepository()
        {
            Instance = new SatDownloadRepository();
        }

        private SatDownloadRepository()
        {

        }

        private ISessionFactory factory;

        public ISession OpenSession()
        {
            if (factory == null)
                Configure();
            return this.factory.OpenSession();
        }

        private void Configure()
        {
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(this.GetType().Assembly);
            this.factory = cfg.BuildSessionFactory();
        }

        public void BuildSchema()
        {
            var cfg = new NHibernate.Cfg.Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Ticket).Assembly);

            var se = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);

            se.Execute(false, true, false);
        }

        public void Save(Ticket o)
        {
            using (var s = this.OpenSession())
            {
                using (var tx = s.BeginTransaction())
                {
                    s.SaveOrUpdate(o);

                    tx.Commit();
                }
            }
        }


        public void Save(SatXml xml, ISession s) 
        {
            s.Save(xml);
        }

        public List<SatXml> GetXmls(Ticket t)
        {
            
            using (var session = SatDownloadRepository.Instance.OpenSession())
            {
                var c = session.CreateCriteria<SatXml>();
                c.Add(Expression.Eq("RequestTicket", t));

                return c.List<SatXml>().ToList();
                
            }
        }

        public Ticket GetTicket(Guid guid)
        {

            using (var session = SatDownloadRepository.Instance.OpenSession())
            {
                return session.Get<Ticket>(guid);
            }
        }

        public void Log(Log.LogLevel type, string msg, params object[] args)
        {
            var logLine = new Log(type, msg, args);
            logLine.Level = type;
            Log(logLine);
        }

        public void Log(Ticket ticket, Log.LogLevel type, string msg, params object[] args)
        {
            var logLine = new Log(type, msg, args);
            logLine.TicketId = ticket.Id.ToString();
            Log(logLine);
        }

        private void Log(Log logLine)
        {
            try
            {
                using (var s = this.OpenSession())
                {
                    using (var tx = s.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
                    {
                        s.Save(logLine);
                        tx.Commit();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public Ticket GetPendigTicket()
        {
            try
            {
                using (var session = SatDownloadRepository.Instance.OpenSession())
                {
                    var c = session.CreateCriteria<Ticket>();
                    c.Add(Expression.Eq("Status", Ticket.StatusPending));
                    c.AddOrder(Order.Asc("RequestDate"));
                    c.SetMaxResults(1);

                    return c.List<Ticket>().FirstOrDefault();
                }
            }
            catch (Exception)
            {
            }

            return null;
        }
    }
}