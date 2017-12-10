using System;
using System.Net;
using MasterMUD.Framework;

namespace MasterMUD.Framework.Networking
{
    public abstract class TcpListener
    {
        private readonly TcpConnection _tcpConnection;

        public bool Active => _tcpConnection.Active;

        public IPAddress Address { get; }

        public int Port { get; }

        protected TcpListener(int port) : this(localaddr: IPAddress.Any, port: port)
        {
        }

        protected TcpListener(IPEndPoint localEP) : this(localaddr: localEP.Address, port: localEP.Port)
        {
        }

        protected TcpListener(IPAddress localaddr, int port)
        {
            this.Address = localaddr;
            this.Port = port;
            this._tcpConnection = new TcpConnection(this);
        }

        public virtual void Start() => this.Start(backlog: -1);

        public void Start(int backlog)
        {
            if (true == this.Active)
                return;

            try
            {
                this._tcpConnection.Start(backlog: backlog);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        public virtual void Stop()
        {
            if (false == this.Active)
                return;

            try
            {
                _tcpConnection.Stop();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
        }

        private sealed class TcpConnection : System.Net.Sockets.TcpListener
        {
            private readonly TcpListener _listener;

            public new bool Active => base.Active;

            public TcpConnection(TcpListener listener) : base(localaddr: listener.Address, port: listener.Port)
            {
                this._listener = listener;
            }

            public new void Start()
            {
                this.Start(backlog: -1);
            }

            public new void Start(int backlog)
            {
                if (backlog >= 1)
                {
                    base.Start(backlog: backlog);
                }
                else
                {
                    base.Start();
                }
            }

            public new void Stop()
            {
                base.Stop();
            }
        }
    }
}
