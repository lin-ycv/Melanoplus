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
//REF: https://discourse.mcneel.com/t/how-to-trigger-updates-down-only-selected-outputs-of-component/68441

namespace Melanoplus
{
    public class WIP_Choke : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{02A7816D-D5E4-4691-BA23-49D3CC73C40E}");
        protected override Bitmap Icon => base.Icon;
        public override GH_Exposure Exposure => GH_Exposure.hidden;

        private List<bool> UpdateOutput = new List<bool>() { true };
        private List<string> PreviousData = new List<string>() { "" };

        public WIP_Choke() : base("Choke Point", "Choke",
            "Prevents noise (repeated unchanging data) from expiring downstream components",
            "Params", "Util")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data 0", "D0", "Data to monitor", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Out 0", "0", "Stable output", GH_ParamAccess.tree);
            //pManager[0].Optional = true;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.DisableGapLogic();

            bool noChange = false;

            string[] currentData = new string[Params.Output.Count];
            for (int i = 0; i < Params.Output.Count; i++)
            {
                noChange |= UpdateOutput[i];
                DA.GetDataTree(i, out GH_Structure<IGH_Goo> data);
                DA.SetDataTree(i, data);

                currentData[i] = data.DataDescription(false, true);

                if (UpdateOutput[i])
                {
                    UpdateOutput[i] = false;
                    PreviousData[i] = currentData[i];
                }
            }

            if (noChange) return;

            bool scheduleSolution = false;
            for (int i = 0; i < Params.Output.Count; i++)
            {
                if (string.Equals(PreviousData[i], currentData[i]))
                    continue;
                UpdateOutput[i] = true;
                PreviousData[i] = currentData[i];
                scheduleSolution = true;
            }

            if (scheduleSolution)
                OnPingDocument()?.ScheduleSolution(5, (d) => ExpireSolution(false));
        }

        protected override void ExpireDownStreamObjects()
        {
            for (int i = 0; i < Params.Output.Count; i++)
                if (UpdateOutput[i])
                    Params.Output[i].ExpireSolution(false);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => GH_ParameterSide.Input == side;

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => GH_ParameterSide.Input == side;

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            UpdateOutput.Insert(index, true);
            PreviousData.Insert(index, "");
            return new Param_GenericObject {Name=$"NEW {index}", NickName = $"D{index}", Description = "Data to monitor", Access = GH_ParamAccess.tree, Optional = true };
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return false;
        }

        public void VariableParameterMaintenance()
        {
            for(int i =0; i < Params.Input.Count; i++)
            {
                if (Params.Input[i].Name.StartsWith("NEW"))
                {
                    Params.Output.Insert(i, new Param_GenericObject { Name = $"Out {i}", NickName = $"{i}", Description = "Stable output", Access = GH_ParamAccess.tree });
                    Params.Input[i].Name = $"Data {i}";
                }
                else if (Params.Input[i].Name.StartsWith("Data") && Params.Input[i].NickName != i.ToString())
                {
                    Params.Input[i].Name = $"Data {i}";
                    Params.Input[i].NickName = $"D{i}";
                }
            }
            for (int i = 0; i < Params.Output.Count; i++)
            {
                if (Params.Output[i].Name.StartsWith("Out") && Params.Output[i].NickName != i.ToString())
                {
                    Params.Output[i].Name = $"Out {i}";
                    Params.Output[i].NickName = $"{i}";
                }
            }
        }
    }
}
