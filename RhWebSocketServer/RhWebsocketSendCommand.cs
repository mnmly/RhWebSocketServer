using System;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace RhWebSocketServer
{
    [System.Runtime.InteropServices.Guid("ab8f414e-209b-4e13-90b1-cd8940e79ab1")]
    public class RhWebsocketSend : Rhino.Commands.Command
    {
        private WebSocketServerSingleton serverManager = WebSocketServerSingleton.Instance;

        public RhWebsocketSend()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RhWebsocketSend Instance { get; private set; }

        public override string EnglishName => "SendWebsocketMessage";

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {
            if (!serverManager.IsRunning)
            {
                RhinoApp.WriteLine("Server is not running");
                return Result.Cancel;
            }
            GetString getString = new GetString();
            getString.SetCommandPrompt("Send the message");
            GetResult result = getString.Get();

            if (getString.CommandResult() != Result.Success)
                return getString.CommandResult();

            serverManager.Broadcast(getString.StringResult());
            return Result.Success;
        }
    }
}
