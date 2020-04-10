using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Fleck;

namespace RhWebSocketServer
{
    public sealed class WebSocketServerSingleton
    {
        private static readonly WebSocketServerSingleton instance = new WebSocketServerSingleton();
        private WebSocketServer websocketServer;
        public ConcurrentDictionary<Guid, IWebSocketConnection> sockets = new ConcurrentDictionary<Guid, IWebSocketConnection>();
        static WebSocketServerSingleton()
        {
        }

        private WebSocketServerSingleton()
        {
            
        }

        public static WebSocketServerSingleton Instance
        {
            get
            {
                return instance;
            }
        }
        public WebSocketServer Server
        {
            get
            {
                return websocketServer;
            }
        }

        public bool IsRunning
        {
            get
            {
                return websocketServer != null;
            }
        }

        public void Dispose()
        {
            websocketServer.Dispose();
            websocketServer = null;
        }

        public void Setup(string hostname, int port)
        {
            if (IsRunning)
            {
                Dispose();
            }
            websocketServer = new WebSocketServer("ws://" + hostname + ":" + port);

        }

        public void Start(Action<IWebSocketConnection> config)
        {
            websocketServer.Start(socket =>
            {
                config(socket);
                socket.OnError = (err) => {
                    IWebSocketConnection removingSocket;
                    sockets.TryRemove(socket.ConnectionInfo.Id, out removingSocket);
                };
                socket.OnClose = () => {
                    IWebSocketConnection removingSocket;
                    sockets.TryRemove(socket.ConnectionInfo.Id, out removingSocket);
                };
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Open socket");
                    sockets.TryAdd(socket.ConnectionInfo.Id, socket);
                };
            });
        }


        public void Broadcast(string message, IWebSocketConnection skippingSocket = null)
        {

            foreach (var item in sockets)
            {
                var socket = item.Value;
                if (socket != skippingSocket)
                {
                    if (socket.IsAvailable)
                    {
                        socket.Send(message);
                    } else
                    {
                        IWebSocketConnection removingSocket;
                        sockets.TryRemove(socket.ConnectionInfo.Id, out removingSocket);
                        socket.Close();
                    }
                }
            }
        }

        public void Broadcast(byte[] message, IWebSocketConnection skippingSocket = null)
        {
            foreach (var item in sockets)
            {
                var socket = item.Value;
                if (socket != skippingSocket)
                {
                    if (socket.IsAvailable)
                    {
                        socket.Send(message);
                    }
                    else
                    {
                        IWebSocketConnection removingSocket;
                        sockets.TryRemove(socket.ConnectionInfo.Id, out removingSocket);
                        socket.Close();
                    }
                }
            }
        }
    }
}
