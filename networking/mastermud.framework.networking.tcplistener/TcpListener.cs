using System;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Linq;
using MasterMUD.Framework;

namespace MasterMUD.Framework.Networking
{
    public abstract class TcpListener : IObservable<TcpListener.TcpSession>
    {
        private volatile int Backlog;

        private volatile IDisposable ConnectionSubjectSubscription;

        protected System.Threading.CancellationTokenSource CancellationTokenSource { get; private set; }

        protected TcpServer TcpServerConnection { get; private set; }

        protected System.Reactive.Subjects.ISubject<TcpListener.TcpSession> ConnectionSubject { get; private set; }

        protected System.IObservable<TcpListener.TcpSession> ConnectionObservable { get; private set; }

        public bool Active => this.TcpServerConnection.Active;

        public System.Net.IPAddress Address { get; }

        public int Port { get; }

        protected System.Collections.Generic.HashSet<TcpListener.TcpSession> Sessions { get; }

        protected TcpListener(int port, int backlog = -1) : this(localaddr: System.Net.IPAddress.Any, port: port, backlog: backlog)
        {
        }

        protected TcpListener(System.Net.IPEndPoint localEP, int backlog = -1) : this(localaddr: localEP.Address, port: localEP.Port, backlog: backlog)
        {
        }

        protected TcpListener(System.Net.IPAddress localaddr, int port, int backlog = -1)
        {
            this.Address = localaddr;
            this.Port = port;
            this.Backlog = backlog;
            this.CancellationTokenSource = new System.Threading.CancellationTokenSource();
            this.ConnectionSubject = new System.Reactive.Subjects.Subject<TcpListener.TcpSession>();
            this.ConnectionObservable = this.ConnectionSubject.AsObservable();
            this.Sessions = new System.Collections.Generic.HashSet<TcpSession>();
            this.TcpServerConnection = new TcpServer(host: this);
        }

        protected virtual async void ConnectAsync(System.Net.Sockets.TcpClient tcpClient)
        {
            var connection = tcpClient;

            try
            {
                await System.Threading.Tasks.Task.Delay(millisecondsDelay: 333, cancellationToken: this.CancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);

                try
                {
                    connection.Dispose();
                }
                catch (Exception ex2)
                {
                    Console.Error.WriteLine(ex2);
                }

                return;
            }

            var endpoint = connection.Client.RemoteEndPoint.ToString();
            var address = endpoint.Substring(0, endpoint.IndexOf(':'));
            var port = int.Parse(endpoint.Substring(address.Length + 1));

            var session = TcpSession.Create(address: address, port: port, connection: connection);

            Sessions.Add(session);

            this.ConnectionSubject.OnNext(session);
        }

        public virtual void Disconnect(TcpSession session)
        {
            if (this.Sessions.Contains(session) && this.Sessions.Remove(session))
                try
                {
                    session.Connection.Close();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
                finally
                {
                    try
                    {
                        session.Connection.Dispose();
                    }
                    catch (Exception ex2)
                    {
                        Console.Error.WriteLine(ex2);
                    }
                    finally
                    {
                        session = null;
                    }
                }
        }

        public void Start()
        {
            if (true == this.Active)
                return;

            this.TcpServerConnection.Start(backlog: this.Backlog);

            if (true == this.Active)
            {
                this.ConnectionSubjectSubscription = Observable.While(condition: () => true == this.Active, source: Observable.FromAsync(this.TcpServerConnection.AcceptTcpClientAsync)).Subscribe(onNext: this.ConnectAsync);
            }
        }

        protected virtual void Start(int backlog)
        {
            if (backlog != this.Backlog)
            {
                this.Backlog = backlog;
            }

            this.Start();
        }

        public virtual void Stop()
        {
            if (false == this.Active)
                return;

            try
            {
                this.ConnectionSubjectSubscription?.Dispose();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            finally
            {
                this.ConnectionSubjectSubscription = null;
                this.TcpServerConnection.Stop();
            }
        }

        public virtual IDisposable Subscribe(IObserver<TcpListener.TcpSession> observer) => this.ConnectionObservable.Subscribe(observer);

        protected sealed class TcpServer : System.Net.Sockets.TcpListener
        {
            private readonly TcpListener Host;

            public new bool Active => base.Active;

            protected internal TcpServer(TcpListener host) : base(localaddr: host.Address, port: host.Port)
            {
                this.Host = host;
            }

            public new void Start() => this.Start(backlog: -1);

            public new void Start(int backlog)
            {
                try
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
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }

            public new void Stop()
            {
                try
                {
                    base.Stop();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        public sealed class TcpSession
        {
            protected internal static TcpSession Create(string address, int port, System.Net.Sockets.TcpClient connection) => new TcpSession(address: address, port: port, connection: connection);

            public string Address { get; private set; }

            public int Port { get; private set; }

            protected internal System.Net.Sockets.TcpClient Connection { get; private set; }

            private TcpSession(string address, int port, System.Net.Sockets.TcpClient connection)
            {
                this.Address = address;
                this.Port = port;
                this.Connection = connection;
            }
        }
    }
}