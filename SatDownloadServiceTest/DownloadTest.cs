using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SatDownloadServiceTest
{
    [TestClass]
    public class DownloadTest
    {
        [TestMethod]
        public void Request()
        {
            var client = new SatDownloadService.SatDownloadServiceSoapClient("SatDownloadServiceSoap");
            var ticket = client.DownloadRequest("AABS741012IH5", "Mexico86", "2015-12-28", "2015-12-30");

            Assert.IsTrue(ticket.ResponseStatus == "Ok");
        }
    }
}
