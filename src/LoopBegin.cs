using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus
{
    public class LoopBegin : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{9B3CDFE2-7DCF-45F6-A695-59F129ECC4A9}");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.LoopBegin;
        public LoopBegin() : base("Begin Loop", "Loop >",
            "Begining of a loop.",
            "Sets", "Loop")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("reset", "r", "Reset loop.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("0", null, "Data for loop.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("⇌", null, "Link to End Loop", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Iteration", "i", "Current iteration index.", GH_ParamAccess.item);
            pManager.AddGenericParameter("0", null, "Data for loop", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var endLinkAtt = this.Params.Output[0].Recipients;
            var endComp = (LoopEnd)endLinkAtt[0].Attributes.Parent.DocObject;
            if (endLinkAtt.Count != 1 || endComp.ComponentGuid != new Guid("{29134008-9A26-4D2A-BF65-DDEAA5146C50}"))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not connected to End Loop component");
                return;
            }

            DA.SetData(1, endComp.iteration);
            bool reset = false;
            DA.GetData(0, ref reset);

            if (reset || endComp.iteration == 0)
            {
                endComp.iteration = 1;
                for (int i = 1; i < Params.Input.Count; i++)
                {
                    DA.GetDataTree(i, out GH_Structure<IGH_Goo> value);
                    if(value==null) value = endComp.Data[i - 1];
                    DA.SetDataTree(i+1, value);
                }
            }
            else
            {
                for (int i = 2; i < Params.Output.Count; i++)
                {
                    var value = endComp.Data[i - 2];
                    DA.SetDataTree(i, value);
                }
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)=> (side == GH_ParameterSide.Input && index > 1) ;

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => (side == GH_ParameterSide.Input && index > 1);

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Params.Output.Insert(index + 1, new Param_GenericObject { Name = (index-1).ToString(), NickName = (index - 1).ToString(), Access = GH_ParamAccess.tree });
            return new Param_GenericObject { Name = (index - 1).ToString(), NickName = (index - 1).ToString(), Access = GH_ParamAccess.tree };
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            Params.UnregisterOutputParameter(Params.Output[index + 1]);
            return true;
        }

        public void VariableParameterMaintenance()
        {
            return;
        }
    }
}
