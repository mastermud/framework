using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MasterMUD
{
    [TestClass]
    public class TcpListenerTests
    {
        private volatile TestListener Listener;

        [TestInitialize]
        public void Setup()
        {
            this.Listener = new TestListener(23);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (this.Listener.Active)
                    this.Listener.Stop();                
            }
            catch (System.Exception)
            {
                this.Listener = null;
            }
        }

        [TestMethod]
        public void Active_Is_True_When_Started()
        {
            Assert.IsFalse(this.Listener.Active);

            this.Listener.Start();

            Assert.IsTrue(this.Listener.Active);
        }

        private sealed class TestListener : MasterMUD.Framework.Networking.TcpListener
        {
            public TestListener(int port) : base(localaddr: System.Net.IPAddress.Loopback, port: port)
            {
            }
        }
    }
}
