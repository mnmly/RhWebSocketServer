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
        private BlockingCollection<IWebSocketConnection> sockets = new BlockingCollection<IWebSocketConnection>();

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
                socket.OnClose = () => {
                    IWebSocketConnection removingSocket;
                    sockets.TryTake(out removingSocket);
                };
                sockets.TryAdd(socket);
            });
        }

        public void Broadcast(string message, IWebSocketConnection skippingSocket = null)
        {

            foreach(var socket in sockets)
            {
                if (socket != skippingSocket) socket.Send(message);
            }
        }

        public void Broadcast(byte[] message, IWebSocketConnection skippingSocket = null)
        {
            foreach (var socket in sockets)
            {
                if (socket != skippingSocket) socket.Send(message);
            }
        }
    }
}
