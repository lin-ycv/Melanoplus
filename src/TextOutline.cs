using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino.Geometry;
using Rhino.Render.ChangeQueue;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Melanoplus.Component
{
    public class TextOutline : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{7A4DF24D-8664-4627-B756-65A732708152}");
        public override GH_Exposure Exposure => (GH_Exposure)65600;
        protected override Bitmap Icon => Properties.Resources.TextOutline;
        public TextOutline() : base("Text Outline", "TraceTxt",
            "Creates curve outlines of text.",
            "Sets", "Text")
        { }
        private IGH_Param[] AddParam = new IGH_Param[3]
        {
            new Param_Integer {Name = names[0,0], NickName = names[0,1], Description = "0- Normal, 1- Bold, 2- Italic", Optional = true, Access = GH_ParamAccess.item  },
            new Param_Boolean {Name = names[1,0], NickName = names[1,1], Description = "Set false for single stroke fonts", Optional = true, Access = GH_ParamAccess.item },
            new Param_Integer {Name = names[2,0], NickName = names[2,1], Description = "Display lower case as small upper case when a scale is provided [1>scale>0]", Optional = true, Access = GH_ParamAccess.item },
        };
        private static readonly string[,] names = new string[4, 2]
        {
            {"style", "s"},
            {"closed", "c"},
            {"scale", "ls"},
            {"Outlines", "Crv"}
        };

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "The text to convert to outline", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "Pln", "Location of text outline", GH_ParamAccess.item);
            pManager.AddTextParameter("font", "ttf", "TrueType Font to use for the text", GH_ParamAccess.item, "Arial");
            pManager.AddNumberParameter("height", "h", "Height of text", GH_ParamAccess.item, 12);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter(names[3, 0], names[3, 1], "Curve outline of input text", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "", font = "";
            double height = 0, lowerScale = 1, tolerance = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            int style = 0;
            bool closed = true;
            Plane plane = Plane.Unset;
            DA.GetData(0, ref text);
            DA.GetData(1, ref plane);
            DA.GetData(2, ref font);
            DA.GetData(3, ref height);
            if (Params.Input.Count > 4)
            {
                DA.GetData(4, ref style);
                DA.GetData(5, ref closed);
                DA.GetData(6, ref lowerScale);
            }
            DA.SetDataList(0, Curve.CreateTextOutlines(text, font, height, style, closed, plane, lowerScale, tolerance));
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            CentralSettings.CanvasFullNamesChanged += CentralSettings_CanvasFullNamesChanged;
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            base.RemovedFromDocument(document);
            CentralSettings.CanvasFullNamesChanged -= CentralSettings_CanvasFullNamesChanged;
        }

        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "Expose additional inputs", Expose, true, Params.Input.Count > 4);
            Menu_AppendItem(menu, "List usable fonts", ListFonts);
            base.AppendAdditionalMenuItems(menu);
            for (int i = menu.Items.Count - 1; i > 0; i--)
                if (menu.Items[i].Text == "Variable Parameters")
                {
                    menu.Items.RemoveAt(i);
                    menu.Items.RemoveAt(i - 1);
                    break;
                }
        }

        private void CentralSettings_CanvasFullNamesChanged()
        {
            if (Params.Input.Count <= 4) return;
            for (int i = 4; i < 7; i++)
            {
                Params.Input[i].NickName = CentralSettings.CanvasFullNames ? names[i-4,0] : names[i - 4, 1];
            }
            Params.Output[0].NickName = CentralSettings.CanvasFullNames ? names[3, 0] : names[3, 1];
        }

        private void Expose(object sender, EventArgs e)
        {
            if (Params.Input.Count > 4)
            {
                for (int i = Params.Input.Count - 1; i > 3; i--)
                    Params.UnregisterInputParameter(Params.Input[i], true);
            }
            else
            {
                foreach (var p in AddParam)
                    Params.RegisterInputParam(p);
                CentralSettings_CanvasFullNamesChanged();
            }
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        private void ListFonts(object sender, EventArgs e)
        {
            var panel = new GH_Panel();
            panel.CreateAttributes();
            panel.Attributes.Pivot = new PointF(
                this.Attributes.Pivot.X - this.Attributes.Bounds.Width - panel.Attributes.Bounds.Width,
                this.Attributes.Pivot.Y - panel.Attributes.Bounds.Height
                );
            panel.UserText = string.Join("\r\n", FontFamily.Families.Select(f => f.Name).ToArray());
            panel.Properties.Multiline = false;
            panel.NickName = "Usable fonts";
            Grasshopper.Instances.ActiveCanvas.Document.AddObject(panel, true);
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index) => false;
        public bool CanRemoveParameter(GH_ParameterSide side, int index) => false;
        public IGH_Param CreateParameter(GH_ParameterSide side, int index) => null;
        public bool DestroyParameter(GH_ParameterSide side, int index) => false;
        public void VariableParameterMaintenance() { }
    }
}
