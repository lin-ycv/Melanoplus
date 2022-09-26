using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus
{
    public class TaskMassMul : TaskMassBase
    {
        public override Guid ComponentGuid => new Guid("{7C68C054-BCE5-4B9A-8509-F121EC03EEA7}");
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
        protected override System.Drawing.Bitmap Icon => Instances.ComponentServer.FindObjectByName("MassMultiplication", true, true).Icon;

        public TaskMassMul() : base("Mass Multiplication (Fast)", "MMF",
        "Perform parallel mass multiplication of a list of items",
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
                total *= d.Value;
            }
            else if (n.GetType().Name == "GH_Integer")
            {
                GH_Integer i = (GH_Integer)n;
                total *= i.Value;
            }
        }
    }
}
