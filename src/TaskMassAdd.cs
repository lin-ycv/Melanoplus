using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;

namespace Melanoplus
{
    public class TaskMassAdd : TaskMassBase
    {
        public override Guid ComponentGuid => new Guid("{8FC9D17C-DEDF-40EE-B539-2888A5661532}");
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
        protected override System.Drawing.Bitmap Icon => Instances.ComponentServer.FindObjectByName("MassAddition", true, true).Icon;

        public TaskMassAdd() : base("Mass Addittion (Fast)", "MAF",
        "Perform parallel mass addition of a list of items",
        "Maths", "Operators")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        { pManager.AddGenericParameter("Input", "I", "", GH_ParamAccess.list); }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        { pManager.AddGenericParameter("Result", "R", "", GH_ParamAccess.item); }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Solve(DA);
        }

        protected override void ComputeLogic(ref double total, object n)
        {
            if (n.GetType().Name == "GH_Number")
            {
                GH_Number d = (GH_Number)n;
                total += d.Value;
            }
            else if (n.GetType().Name == "GH_Integer")
            {
                GH_Integer i = (GH_Integer)n;
                total += i.Value;
            }
        }
    }
}
