using System;
using System.Net;
using MasterMUD.Framework;

namespace MasterMUD.Framework.Networking
{
    public abstract class TcpListener
    {
        private readonly Connection _connection;

        public bool Active => _connection.Active;

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
        }

        public virtual void Start() => this.Start(backlog: -1);

        public void Start(int backlog)
        {
            if (true == this.Active)
                return;

            try
            {
                _connection.Start(backlog: backlog);
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
                _connection.Stop();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }            
        }

        private sealed class Connection : System.Net.Sockets.TcpListener
        {
            private readonly TcpListener _listener;

            public new bool Active => base.Active;

            public Connection(TcpListener listener) : base(localaddr: listener.Address, port: listener.Port)
            {
                this._listener = listener;
            }

            public new void Start()
            {
                this.Start(backlog: -1);
            }

            public new void Start(int backlog)
            {
                if (backlog > 0)
                {
                    base.Start(backlog: backlog);
                }
                else
                {
                    base.Stop();
                }
            }

            public new void Stop()
            {
                base.Stop();
            }
        }
    }
}
