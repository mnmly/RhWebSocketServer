using System;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.UI;

namespace RhWebSocketServer
{
    [System.Runtime.InteropServices.Guid("50c9b9e8-42c2-4871-a943-b9768380e186"),
         Rhino.Commands.CommandStyle(Rhino.Commands.Style.ScriptRunner)
]

    public class RhWebsocketExportAndNotify : Rhino.Commands.Command
    {

        private WebSocketServerSingleton serverManager = WebSocketServerSingleton.Instance;
        public RhWebsocketExportAndNotify()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static RhWebsocketExportAndNotify Instance { get; private set; }

        public override string EnglishName => "ExportAndNotify";

        protected override Result RunCommand(Rhino.RhinoDoc doc, RunMode mode)
        {

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "glTF 1.0/2.0 binary (ShapeDiver) (*.glb)";
            dlg.DefaultExt = ".glb";
            dlg.Title = "Save glTF file";
            dlg.FileName = "Export.glb";

            var result = dlg.ShowSaveDialog();
            var filename = dlg.FileName;

            if (result) {

                string strCommand = @"_-Export " + filename + " Enter";
                
                RhinoApp.RunScript(strCommand, false);
            }

            if (serverManager.IsRunning)
            {
                serverManager.Broadcast(filename);
            }

            return Result.Success;
        }
    }
}
