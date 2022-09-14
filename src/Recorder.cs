using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Melanoplus
{
    public class Recorder : GH_Component
    {
        public override Guid ComponentGuid => new Guid("DC3311E5-261D-4E71-8D0D-04069E0FEABC");
        public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
        protected override Bitmap Icon => enable ?
                Properties.Resources.recordON :
                Properties.Resources.recordOFF;

        private GH_Structure<IGH_Goo> dataTree = new GH_Structure<IGH_Goo>();
        private bool enable = true;
        private int limit = 0;

        public Recorder() : base("Recorder", "Rec",
            "Data recorder with inputs for reset and on/off",
            "Params", "Util")
        { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to record", GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("enable", "e", "Enable recording", GH_ParamAccess.item);
            pManager[1].Optional = true;
            pManager.AddBooleanParameter("reset", "r", "Reset record", GH_ParamAccess.item);
            pManager[2].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Record", "R", "Recorded data", GH_ParamAccess.tree);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false;
            if (!DA.GetData(1, ref enable))
                Params.Input[1].AddVolatileData(new GH_Path(0), 0, enable);
            if (enable ? Icon.RawFormat.Guid == Properties.Resources.recordOFF.RawFormat.Guid : Icon.RawFormat.Guid == Properties.Resources.recordON.RawFormat.Guid)
                base.DestroyIconCache();
            if (!DA.GetData(2, ref reset))
                Params.Input[2].AddVolatileData(new GH_Path(0), 0, reset);
            if (reset) { dataTree.Clear(); return; }
            if (enable)
            {
                DA.GetDataTree("Data", out GH_Structure<IGH_Goo> tree);
                for (int branchIndex = 0; branchIndex < tree.PathCount; branchIndex++)
                {
                    for (int index = 0; index < tree[branchIndex].Count(); index++)
                    {
                        dataTree.Append(tree[branchIndex][index],
                            new GH_Path(tree.Paths[branchIndex].Indices).AppendElement(index));
                    }
                    if (limit > 0 && dataTree[branchIndex].Count() > limit)
                    {
                        var data = dataTree[branchIndex].Skip(dataTree[branchIndex].Count() - limit).ToList();
                        dataTree[branchIndex].Clear();
                        dataTree.AppendRange(data);
                    }
                }
            }

            DA.SetDataTree(0, dataTree);
        }

        public override void CreateAttributes()
        {
            m_attributes = new RecAttributes(this);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Clear record", Reset, Params.Input[2].SourceCount < 1);
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Data limit - 0 for none", null, false);
            Menu_AppendTextItem(menu, limit.ToString(), LimitKeyDown, LimitChanged, true);
            base.AppendAdditionalMenuItems(menu);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("reE", enable);
            writer.SetInt32("reL", limit);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            enable = reader.GetBoolean("reE");
            limit = reader.GetInt32("reL");
            return base.Read(reader);
        }

        internal void Toggle()
        {
            enable = !enable;
            ExpireSolution(true);
        }

        internal void Reset(object s, EventArgs e)
        {
            dataTree.Clear();
            ExpireSolution(true);
        }

        private void LimitKeyDown(GH_MenuTextBox sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && GH_Convert.ToInt32(sender.Text, out int value, GH_Conversion.Both) && limit != value)
            {
                limit = Math.Abs(value);
                ExpireSolution(true);
            }
        }

        private void LimitChanged(GH_MenuTextBox sender, string text)
        {
            if (GH_Convert.ToInt32(text, out var _, GH_Conversion.Both))
                sender.TextBoxItem.ForeColor = SystemColors.WindowText;
            else
                sender.TextBoxItem.ForeColor = Color.Red;
        }
    }

    public class RecAttributes : GH_ComponentAttributes
    {
        public RecAttributes(IGH_Component component) : base(component)
        {
        }
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (base.Owner is Recorder comp && comp.Params.Input[1].SourceCount < 1)
            {
                comp.Params.Input[1].ClearData();
                comp.Toggle();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
