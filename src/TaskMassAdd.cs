using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
//using MathComponents.Operations;
using Rhino.Commands;
//REF: https://discourse.mcneel.com/t/mass-addition-speed-up-component/148121/3

namespace Melanoplus
{
    public class TaskMassAdd : GH_TaskCapableComponent<TaskMassAdd.SolveResultsMassAdd>
    {
        public override Guid ComponentGuid => new Guid("{8FC9D17C-DEDF-40EE-B539-2888A5661532}");
        public override GH_Exposure Exposure => GH_Exposure.tertiary | GH_Exposure.obscure;
        protected override System.Drawing.Bitmap Icon => Instances.ComponentServer.FindObjectByName("MassAddition", true, true).Icon;

        public TaskMassAdd() : base("Mass Addittion (Fast)", "MAF",
        "Perform parallel mass addition of a list of items",
        "Maths", "Operators")
        { }

        public class SolveResultsMassAdd
        { public object Result { get; set; } }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        { pManager.AddGenericParameter("Input", "I", "", GH_ParamAccess.list); }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        { pManager.AddGenericParameter("Result", "R", "", GH_ParamAccess.item); }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                ///First pass; collect data and construct tasks
                List<IGH_Goo> numList = new List<IGH_Goo>();

                Task<SolveResultsMassAdd> tskMassAdd = null;

                if (DA.GetDataList(0, numList))
                {
                    tskMassAdd = Task.Run(() => Compute(numList), CancelToken);
                }

                ///Add a null task even if data collection fails.  This keeps the list size in sync with the iterations
                TaskList.Add(tskMassAdd);
                return;
            }

            if (!GetSolveResults(DA, out var results))
            {
                ///Compute right here, right now.
                ///1. Collect
                List<IGH_Goo> numsList = new List<IGH_Goo>();

                if (!DA.GetDataList(0, numsList)) { return; }

                ///2. Compute
                results = Compute(numsList);
            }

            ///3. Set
            if (results != null)
            {
                DA.SetData(0, results.Result);
            }
        }

        SolveResultsMassAdd Compute(List<IGH_Goo> numList)
        {
            var rc = new SolveResultsMassAdd();

            ///Slow
            //List<IGH_QuickCast> list2 = Operation.GooToQuickType(numList);
            //IGH_QuickCast val = list2[0];
            //Addition instance = Addition.Instance;
            //for (int i = 1; i < numList.Count; i++)
            //{
            //    IGH_QuickCast result = null;
            //    string text = instance.Perform(val, list2[i], out result);
            //    if (text != null)
            //    {
            //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, text);
            //        break;
            //    }
            //    val = result;
            //}

            ///Rework below for other cases of GH Types which are valid input to the OOTB Mass Addition (ie Point3d, Vector, Transform, etc.)
            double total = 0.0;
            foreach (var n in numList)
            {
                switch(n)
                {
                    case GH_Number d:
                    case GH_Integer i:
                        total += ((GH_Number)n).Value;
                        break;

                }
                //if (n.GetType().Name == "GH_Number")
                //{
                //    GH_Number d = (GH_Number)n;
                //    total += d.Value;
                //}
                //else if (n.GetType().Name == "GH_Integer")
                //{
                //    GH_Integer i = (GH_Integer)n;
                //    total += i.Value;
                //}
            }
            rc.Result = total;
            return rc;
        }
    }
}
