using MasterMUD.Framework.Networking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
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
        public void TcpListener_Is_Reactive_AndAlso_Asynchronous()
        {
            if (!this.Listener.Active)
                this.Listener.Start();

            Assert.IsTrue(this.Listener.Active);

            var session = default(MasterMUD.Framework.Networking.TcpListener.TcpSession);
            var subscription = this.Listener.Subscribe(subject =>
            {
                if (session == null)
                    session = subject;
            });
            
            Assert.AreEqual(0, this.Listener.TotalConnections);

            using (subscription)
            {
                try
                {
                    using (var tcpClient = new System.Net.Sockets.TcpClient())
                    {
                        if (false == tcpClient.ConnectAsync(host: System.Net.IPAddress.Loopback.ToString(), port: ListenerPort).Wait(millisecondsTimeout: 333 * 3))
                            Assert.Fail("Couldn't connect to host.");

                        Assert.IsNotNull(session, message: "The session was not obtained asynchronously inside of the subscription.");

                        Assert.AreNotEqual(0, this.Listener.TotalConnections);
                    }
                }
                catch (System.Exception ex)
                {
                    System.Console.Error.WriteLine(ex);
                }

                this.Listener.Disconnect(session);                
            }

            Assert.AreEqual(0, this.Listener.TotalConnections);
        }

        private sealed class TestListener : MasterMUD.Framework.Networking.TcpListener
        {
            public int TotalConnections => base.Sessions.Count;

            public TestListener(int port) : base(localaddr: System.Net.IPAddress.Loopback, port: port)
            {
            }

            protected override async void ConnectAsync(TcpClient tcpClient)
            {
                var client = tcpClient;

                base.ConnectAsync(tcpClient);

                await Task.Delay(33, base.CancellationTokenSource.Token);
            }
        }
    }
}
