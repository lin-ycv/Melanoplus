using Grasshopper.Kernel.Types;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper;
//REF: https://discourse.mcneel.com/t/mass-addition-speed-up-component/148121/3

namespace Melanoplus
{
    public abstract class TaskMassBase : GH_TaskCapableComponent<TaskMassBase.SolveResultsMass>
    {
        public TaskMassBase(string name, string nickname, string description, string category, string subCategory) : 
            base(name, nickname, description, category, subCategory) { }

        public class SolveResultsMass
        { public GH_Number Result { get; set; } }

        protected void Solve(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                ///First pass; collect data and construct tasks
                List<object> numList = new List<object>();

                Task<SolveResultsMass> tskMass = null;

                if (DA.GetDataList<object>(0, numList))
                {
                    tskMass = Task.Run(() => Compute(numList), CancelToken);
                }

                ///Add a null task even if data collection fails.  This keeps the list size in sync with the iterations
                TaskList.Add(tskMass);
                return;
            }

            if (!GetSolveResults(DA, out var results))
            {
                ///Compute right here, right now.
                ///1. Collect
                List<object> numsList = new List<object>();

                if (!DA.GetDataList<object>(0, numsList)) { return; }

                ///2. Compute
                results = Compute(numsList);
            }

            ///3. Set
            if (results != null)
            {
                DA.SetData(0, results.Result);
            }
        }

        protected SolveResultsMass Compute(List<object> numList)
        {
            var rc = new SolveResultsMass();

            ///Rework below for other cases of GH Types which are valid input to the OOTB Mass Addition (ie Point3d, Vector, Transform, etc.)
            double total = 0.0;
            foreach (var n in numList)
            {
                ComputeLogic(ref total, n);
            }
            rc.Result = new GH_Number(total);
            return rc;
        }

        protected abstract void ComputeLogic(ref double total, object n);
    }
}
