using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Melanoplus.DevTools
{
    public class UnloadGH
    {
        public static void Unload()
        {
            //// UI items don't get loaded on relaunch
            //Grasshopper.Instances.DocumentServer.RemoveAllDocuments();
            //Grasshopper.Instances.DocumentEditor.CloseForReal();

            // Viewport widgets prevent proper reload if enabled when unloaded
            var widgets = Grasshopper.Instances.ActiveCanvas.Widgets.Where(w => (w.Name == "Rhino Viewport" || w.Name == "Peek Viewport"));//.Select(w=>w.Visible = false);
            foreach(var w in widgets)
                w.Visible = false;
            Grasshopper.Instances.UnloadAllObjects();
        }
    }
}