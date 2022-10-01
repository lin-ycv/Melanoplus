using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus.Component
{
    public class LoopEnd : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{29134008-9A26-4D2A-BF65-DDEAA5146C50}");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.LoopEnd;
        public LoopEnd() : base("End Loop", "Loop <",
            "End of a loop.",
            "Sets", "Loop")
        { }

        internal int iteration = 0, max = 0;
        internal bool fin = false;

        internal List<GH_Structure<IGH_Goo>> Data = new List<GH_Structure<IGH_Goo>>();

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("⇌", null, "Link to Begin Loop", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Max", "M", "Max iterations.", GH_ParamAccess.item, 2);
            pManager.AddGenericParameter("0", null, "Data of loop.", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("0", null, "Data for loop", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var beginLinkAtt = this.Params.Input[0].Sources;
            var beginComp = (GH_Component)beginLinkAtt[0].Attributes.Parent.DocObject;
            if (beginLinkAtt.Count != 1 || beginComp.ComponentGuid != new Guid("{9B3CDFE2-7DCF-45F6-A695-59F129ECC4A9}"))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not connected to Begin Loop component");
                return;
            }

            // How to handle cluster...?
            //if (OnPingDocument().Owner is Grasshopper.Kernel.Special.GH_Cluster)
            //{
                //OnPingDocument().ExpireSolution();
                //var id = this.InstanceGuid;
                //var dupdoc = GH_Document.DuplicateDocument(OnPingDocument());
                //var dupcom = (GH_Component)dupdoc.FindObject(id, true);
                //var dupbegin = (GH_Component)dupdoc.FindObject(beginComp.InstanceGuid, true);
                //for (int i = 0; i < dupbegin.Params.Input.Count; i++)
                //{
                //    dupbegin.Params.Input[i].Sources.Clear();
                //    dupbegin.Params.Input[i].ClearData();
                //    dupbegin.Params.Input[i].AddVolatileDataTree(beginComp.Params.Input[i].VolatileData);
                //}
            //    dupdoc.SolutionEnd += (s, e) =>
            //    {
            //        for (int i = 0; i < Params.Output.Count; i++)
            //        {
            //            DA.SetDataTree(i, dupcom.Params.Output[i].VolatileData);
            //        }
            //    };
            //    return;
            //}


            DA.GetData(1, ref max);

            if (iteration > max)
                iteration = -1;

            Data.Clear();
            if (iteration == max)
            {
                for (int i = 0; i < Params.Output.Count; i++)
                {
                    DA.GetDataTree(i + 2, out GH_Structure<IGH_Goo> value);
                    Data.Add(value.Duplicate());
                    DA.SetDataTree(i, value);
                }
                Message = "Finished";
            }
            else
            {
                for (int i = 2; i < Params.Input.Count; i++)
                {
                    DA.GetDataTree(i, out GH_Structure<IGH_Goo> value);
                    Data.Add(value.Duplicate());
                }
                iteration++;
                beginComp.ExpireSolution(true);
                Message = "Iteration: " + iteration;
            }
            if (OnPingDocument().Owner is Grasshopper.Kernel.Special.GH_Cluster)
            {
                OnPingDocument().ExpireSolution();
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => (side == GH_ParameterSide.Input && index > 2);

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => (side == GH_ParameterSide.Input && index > 2);

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Params.Output.Insert(index - 2, new Param_GenericObject { Name = (index - 2).ToString(), NickName = (index - 2).ToString(), Access = GH_ParamAccess.tree });
            return new Param_GenericObject { Name = (index - 2).ToString(), NickName = (index - 2).ToString(), Access = GH_ParamAccess.tree };
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            Params.UnregisterOutputParameter(Params.Output[index - 2]);
            return true;
        }

        public void VariableParameterMaintenance()
        {
            return;
        }
    }
}
