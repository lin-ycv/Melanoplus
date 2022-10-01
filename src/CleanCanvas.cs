using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace Melanoplus.Util
{
    public class CleanCanvas
    {
        internal static void Clean(GH_Document document)
        {
            if (document == null || document.ObjectCount == 0) return;
            try
            {
                var objs = document.Objects.Where(o => o.ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent").ToList();
                var group = document.Objects.Where(o => o is GH_Group g && g.Colour.A == 0);
                if (group.Count() > 0 &&
                    MessageBox.Show("Also remove transparent groups?", "Clean Canvs", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    objs.AddRange(group);
                document.RemoveObjects(objs, false);
                Grasshopper.Instances.ActiveCanvas.Refresh();
            }
            catch (Exception er)
            {
                RhinoApp.WriteLine(er.Message);
            }
        }
    }
}
