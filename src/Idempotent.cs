using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
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
//REF: https://discourse.mcneel.com/t/how-to-trigger-updates-down-only-selected-outputs-of-component/68441

namespace Melanoplus
{
    public class Idempotent : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{02A7816D-D5E4-4691-BA23-49D3CC73C40E}");
        protected override Bitmap Icon => Properties.Resources.idempotent3;
        public override GH_Exposure Exposure => GH_Exposure.secondary | GH_Exposure.obscure;

        private List<bool> UpdateOutput = new List<bool>() { false };
        private List<string> PreviousData = new List<string>() { "" };

        public Idempotent() : base("Idempotent", "Dam",
            "Prevents noise (repeated unchanging data) from expiring downstream components; can also be used as data dam",
            "Params", "Util")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("0", "0", "Data to monitor", GH_ParamAccess.tree);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("0", "0", "Stable output", GH_ParamAccess.tree);
            pManager[0].MutableNickName = false;
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.DisableGapLogic();

            bool noChange = false;
            bool dam = Params.Input.Any(x => x.Name == "breach dam");
            bool breach = true;
            if (dam)
            {
                DA.GetData("breach dam", ref breach);
            }

            string[] currentData = new string[Params.Output.Count];
            for (int i = 0; i < Params.Output.Count; i++)
            {
                noChange |= UpdateOutput[i];
                DA.GetDataTree(i, out GH_Structure<IGH_Goo> data);
                if (breach)
                    DA.SetDataTree(i, data);

                currentData[i] = data.DataDescription(false, true);

                if (UpdateOutput[i])
                {
                    UpdateOutput[i] = false;
                    PreviousData[i] = currentData[i];
                }
            }

            if (noChange || !breach) return;

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

        public override void AddedToDocument(GH_Document document)
        {
            Params.ParameterChanged += Renamed;
            base.AddedToDocument(document);
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Data dam control", Datadam, true, Params.Input.Any(p => p.Name == "breach dam"));
            base.AppendAdditionalMenuItems(menu);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => GH_ParameterSide.Input == side && index <= UpdateOutput.Count;

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => GH_ParameterSide.Input == side && Params.Input.Count > 1 && index < UpdateOutput.Count;

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            UpdateOutput.Insert(index, true);
            PreviousData.Insert(index, "");
            Params.Output.Insert(index, new Param_GenericObject { Name = $"{index}", NickName = $"{index}", Description = "Stable output", Access = GH_ParamAccess.tree, MutableNickName = false });
            return new Param_GenericObject { Name = $"{index}", NickName = $"{index}", Description = "Data to monitor", Access = GH_ParamAccess.tree, Optional = true };
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            UpdateOutput.RemoveAt(index);
            PreviousData.RemoveAt(index);
            Params.UnregisterOutputParameter(Params.Output[index]);
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 0; i < Params.Input.Count; i++)
            {
                if (Params.Input[i].Name != $"{i}" && Params.Input[i].Name != "breach dam")
                {
                    Params.Input[i].Name = $"{i}";
                    Params.Output[i].Name = $"{i}";
                }
            }
        }

        private void Renamed(object s, GH_ParamServerEventArgs e)
        {
            if (e.OriginalArguments.Type == GH_ObjectEventType.NickName)
            {
                Params.Output[e.ParameterIndex].NickName = Params.Input[e.ParameterIndex].NickName;
            }
        }

        private void Datadam(object sender, EventArgs e)
        {
            if (Params.Input.Any(p => p.Name == "breach dam"))
            {
                Params.UnregisterInputParameter(Params.Input.First(p => p.Name == "breach dam"), true);
            }
            else
            {
                Params.RegisterInputParam(new Param_Boolean { Name = "breach dam", NickName = "dam", Description = "Control if data is allowed thru (default: true)", Access = GH_ParamAccess.item, Optional = true, MutableNickName = false });
            }
            Params.OnParametersChanged();
            ExpireSolution(true);
        }
    }
}
