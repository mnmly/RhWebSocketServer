using System;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RhWebSocketServer
{
    public class RhWebSocketServerCommand : Rhino.Commands.Command
    {
        public override string EnglishName => "StartWebSocketServer";
        public WebSocketServerSingleton serverManager = WebSocketServerSingleton.Instance;

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {

            int portNumber = 8181;
            string hostnameString = "127.0.0.1";
            bool activate = serverManager.IsRunning;

            GetString gp = new GetString();
            gp.SetCommandPrompt("Websocket Server");
            gp.SetDefaultString(hostnameString);

            Rhino.Input.Custom.OptionInteger portNumberOption = new Rhino.Input.Custom.OptionInteger(portNumber);
            OptionToggle activeOption = new OptionToggle(activate, "Off", "On");

            gp.AddOptionToggle("Activate", ref activeOption);
            gp.AddOptionInteger("Port", ref portNumberOption);

            while (true)
            {
                GetResult get_rc = gp.Get();

                if (gp.CommandResult() != Result.Success)
                    return gp.CommandResult();

                if (get_rc == GetResult.String)
                {
                    hostnameString = gp.StringResult();
                }
                else if (get_rc == GetResult.Option)
                {
                    continue;
                }
                break;
            }

            portNumber = portNumberOption.CurrentValue;
            activate = activeOption.CurrentValue;

            if (activate)
            {
                serverManager.Setup(hostnameString, portNumber);
                serverManager.Start(socket =>
                {
                    socket.OnOpen = () => RhinoApp.WriteLine("Open");
                    socket.OnClose = () => RhinoApp.WriteLine("Close!");
                    socket.OnMessage = (message) =>
                    {
                        try
                        {
                            var obj = JObject.Parse(message);
                            if (obj.ContainsKey("action") && obj.GetValue("action").ToString() == "broadcast") {
                                serverManager.Broadcast(obj.GetValue("data").ToString(), socket);
                                return;
                            }
                        } catch (JsonReaderException exception)
                        {
                            
                        }
                        //socket.Send(message);
                    };
                });
            } else
            {
                RhinoApp.WriteLine("Closing server");
                serverManager.Dispose();
            }

            return Result.Success;
        }
    }
}