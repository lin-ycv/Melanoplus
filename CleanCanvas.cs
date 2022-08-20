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

namespace Melanoplus
{
    public class CleanCanvas : GH_ButtonObject
    {
        public override string Name => "Clean Canvas";
        public override string NickName => "Clean";
        public override string Description => "Cleans the canvas by removing placerholders (and transparent groups)";
        public override string Category => "Melanoplus";
        public override string SubCategory => "Tools";
        protected override Bitmap Icon => Properties.Resources.CleanCanvas;
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("{F60D4A50-A317-414A-A317-C819C2863418}");

        public CleanCanvas() : base() { }
        public override void CreateAttributes()
        {
            m_attributes = new CleanCanvasAttributes(this);
        }

        internal static void Clean(GH_Document document)
        {
            if (document == null) return;
            try
            {
                DialogResult dialogResult = MessageBox.Show("Also remove transparent groups?", "Clean Canvs", MessageBoxButtons.YesNo,MessageBoxIcon.Asterisk);
                var objs = document.Objects.Where(o => {
                    if (o.ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent") return true;
                    if (dialogResult == DialogResult.Yes && o.ToString() == "Grasshopper.Kernel.Special.GH_Group" && ((GH_Group)o).Colour.A==0) return true;
                    return false;
                }).ToArray();
                document.RemoveObjects(objs, false);
            }
            catch (Exception er)
            {
                RhinoApp.WriteLine(er.Message);
            }
        }

        public class CleanCanvasAttributes : GH_Attributes<CleanCanvas>
        {
            public CleanCanvasAttributes(CleanCanvas owner) : base(owner)
            {
            }

            public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                Clean(sender.Document);
                sender.Refresh();
                return base.RespondToMouseUp(sender, e);
            }
        }

    }
}
