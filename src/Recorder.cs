using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus
{
    public class Recorder : GH_Component
    {
        public Recorder() : base("Recorder", "Rec", "Data recorder with inputs for reset and on/off", "Params", "Util") { }
        public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;
        public override Guid ComponentGuid => new Guid("DC3311E5-261D-4E71-8D0D-04069E0FEABC");
        protected override Bitmap Icon
        {
            get { if (enable) return Properties.Resources.recordON;
                return Properties.Resources.recordOFF; }
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {            
            pManager.AddGenericParameter("Data","D","Data to record",GH_ParamAccess.tree);
            pManager[0].Optional = true;
            pManager.AddBooleanParameter("enable", "e", "Enable recording", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("reset", "r", "Reset record", GH_ParamAccess.item,false);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Record","R","Recorded data", GH_ParamAccess.tree);
        }

        GH_Structure<IGH_Goo> dataTree = new GH_Structure<IGH_Goo>();
        bool enable = true;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool reset = false ;
            DA.GetData("enable", ref enable);
            base.DestroyIconCache();
            DA.GetData("reset", ref reset);
            if (reset) { dataTree.Clear(); return; }
            if (enable)
            {
                DA.GetDataTree("Data", out GH_Structure<IGH_Goo> tree);
                for (int branchIndex = 0; branchIndex < tree.PathCount; branchIndex++)
                {
                    for (int index = 0; index < tree[branchIndex].Count(); index++)
                    {
                        dataTree.Append(tree[branchIndex][index],
                            new GH_Path((tree.Paths[branchIndex].Indices)).AppendElement(index));
                    }
                }
            }
            DA.SetDataTree(0, dataTree );
        }
    }
}
