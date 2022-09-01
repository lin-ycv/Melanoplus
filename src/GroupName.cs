using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Melanoplus
{
    public class GroupName : GH_Component
    {
        public override Guid ComponentGuid => new Guid("AC5E4059-5037-47B5-B59B-049ED083D6C6");
        public override GH_Exposure Exposure => GH_Exposure.primary | GH_Exposure.obscure;
        protected override Bitmap Icon => Properties.Resources.multiline;

        public GroupName() : base ("Multiline Name", "GroupName",
              "Adds multiline name to selected group(s)",
              "Params", "Util"){ }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Name", null, "Name for group", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Set", null, "Set true to rename selected group(s)", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string title = string.Empty;
            bool rename = false;
            DA.GetData(0, ref title);
            DA.GetData(1, ref rename);

            if (rename == true && Grasshopper.Instances.ActiveCanvas.Document.SelectedCount > 0)
            {
                foreach (var o in Grasshopper.Instances.ActiveCanvas.Document.SelectedObjects())
                {
                    if (o is GH_Group grp)
                        o.NickName = title;
                }
            }
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (this.Params.Input[1].SourceCount == 0)
            {
                var panel = new GH_Panel();
                panel.CreateAttributes();
                panel.Attributes.Pivot = new PointF(
                    this.Attributes.Pivot.X - this.Attributes.Bounds.Width - panel.Attributes.Bounds.Width,
                    this.Attributes.Pivot.Y - panel.Attributes.Bounds.Height
                    );
                document.AddObject(panel, false);
                this.Params.Input[0].AddSource(panel);
                panel.UserText = "1. Enter name of group here\r\n2. Select the group(s)\r\n3. Click button";

                var button = new GH_ButtonObject();
                button.CreateAttributes();
                button.Attributes.Pivot = new PointF(
                  this.Attributes.Pivot.X - this.Attributes.Bounds.Width - button.Attributes.Bounds.Width,
                  this.Attributes.Pivot.Y);
                document.AddObject(button, false);
                this.Params.Input[1].AddSource(button);
                this.ExpireSolution(true);
                document.ScheduleSolution(1);
            }
        }
    }
}
