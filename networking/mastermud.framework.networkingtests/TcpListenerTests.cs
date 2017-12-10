using MasterMUD.Framework.Networking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace MasterMUD
{
    [TestClass]
    public class TcpListenerTests
    {
        private const int ListenerPort = 2323;

        private volatile TestListener Listener;

        [TestInitialize]
        public void Setup()
        {
            this.Listener = new TestListener(port: ListenerPort);
            this.Listener.Start();
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
            if (!this.Listener.Active)
            {
                this.Listener.Start();
                Assert.IsTrue(this.Listener.Active);
            }
            else
            {
                Assert.Inconclusive(message: "Active was already True.");
            }
        }

        [TestMethod]
        public void Asynchronous_Reactive_Connection_Becomes_Session()
        {
            var tcpClient = new System.Net.Sockets.TcpClient();

            var task = new Task(async () =>
            {
                try
                {
                    await tcpClient.ConnectAsync(host: System.Net.IPAddress.Loopback.ToString(), port: ListenerPort);
                }
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine(ex);
                }
            });

            var awaiter = task.GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                Assert.AreNotEqual(notExpected: 0, actual: this.Listener.TotalConnections);
            });

            task.Start();
            task.Wait();

            Assert.AreEqual(expected: 1, actual: this.Listener.TotalConnections);

            tcpClient.Close();
            tcpClient.Dispose();
        }

        private sealed class TestListener : MasterMUD.Framework.Networking.TcpListener
        {
            public int TotalConnections { get; private set; }

            public TestListener(int port) : base(localaddr: System.Net.IPAddress.Loopback, port: port)
            {
            }

            protected override void Connect(TcpSession session)
            {
                this.TotalConnections += 1;
                base.Connect(session);
            }
        }
    }
}
