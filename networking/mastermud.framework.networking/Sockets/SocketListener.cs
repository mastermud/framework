namespace MasterMUD.Framework.Networking.Sockets
{
    public abstract class SocketListener : System.IDisposable
    {
        public readonly System.Net.IPAddress Address;
        public readonly System.Int32 Port;
        private volatile System.Net.IPEndPoint EndPoint;
        private volatile System.Net.Sockets.Socket Socket;
        private volatile bool IsDisposed;

        public SocketListener(System.Net.IPAddress address, System.Int32 port, System.Net.Sockets.SocketType type, System.Net.Sockets.ProtocolType protocol)
        {
            this.Address = address;
            this.Port = port;
            this.EndPoint = new System.Net.IPEndPoint(address: this.Address, port: this.Port);
            this.Socket = new System.Net.Sockets.Socket(addressFamily: this.EndPoint.AddressFamily, socketType: System.Net.Sockets.SocketType.Stream, protocolType: System.Net.Sockets.ProtocolType.Tcp);
        }

        protected abstract void Connect(System.Net.Sockets.Socket connection);

        protected virtual void Dispose(bool disposing)
        {
            if (false == this.IsDisposed)
            {
                this.IsDisposed = true;

                if (true == disposing)
                    try
                    {
                        this.Socket?.Dispose();
                    }
                    catch (System.Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }

                this.Socket = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public abstract void Start();
        public abstract void Stop();

        ~SocketListener()
        {
            this.Dispose(disposing: false);
        }
    }
}