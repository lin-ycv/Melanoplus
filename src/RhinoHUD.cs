using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//REF: https://www.youtube.com/watch?v=B92iN7mOPbk

namespace Melanoplus
{
    public class RhinoHUD : GH_Component, IGH_VariableParameterComponent
    {
        public override Guid ComponentGuid => new Guid("{95390F76-AA4C-46D1-9D35-E605C4758835}");
        protected override Bitmap Icon => Properties.Resources.DataHUD;
        public override GH_Exposure Exposure => GH_Exposure.quarternary | GH_Exposure.obscure;
        public override BoundingBox ClippingBox => BoundingBox.Union(base.ClippingBox, arrow.BoundingBox);

        private Point3d coor;
        private string str;
        private Color colour;
        private bool mid, isBitmap;
        private Line arrow;
        private DisplayBitmap bmp;
        private int size;
        private readonly IGH_Param[] strParam = new IGH_Param[3]
        {
            new Param_Colour {Name = "colour", NickName = "c", Description = "Colour of displayed data [Optional]", Optional = true, Access = GH_ParamAccess.item  },
            new Param_Boolean {Name = "centered", NickName = "m", Description = "Position as Mid-point [Optional]", Optional = true, Access = GH_ParamAccess.item },
            new Param_Integer {Name = "size", NickName = "s", Description = "Size of text [Optional]", Optional = true, Access = GH_ParamAccess.item },
        };
        public RhinoHUD() : base("Rhino HUD", "HUD", 
            "Displays gh data on the viewport", 
            "Display", "Preview") { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data to be displayed", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddPointParameter("position", "p", "Position of HUD on the viewport [0-1]", GH_ParamAccess.item, new Point3d(0.1, 0.9, 0));
            pManager[1].Optional = true;
            foreach (var p in strParam)
                Params.RegisterInputParam(p);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (bmp != null) bmp.Dispose();
            if (!DA.GetData("position", ref coor)) return;
            object data = new object();
            if (!DA.GetData("Data", ref data)) { str = null; bmp = null; return; }
            if (coor.X > 1) coor.X = 1;
            else if (coor.X < 0) coor.X = 0;
            if (coor.Y > 1) coor.Y = 1;
            else if (coor.Y < 0) coor.Y = 0;
            arrow = new Line(new Point3d(0, 0, 0), coor);
            GH_Convert.ToString(data, out str, GH_Conversion.Both);
            try
            {
                if (isBitmap)
                {
                    if (System.Web.MimeMapping.GetMimeMapping(str).StartsWith("image/"))
                    {
                        Bitmap b = new Bitmap(str);
                        bmp = new DisplayBitmap(b);
                    }
                    else
                    {
                        isBitmap = false;
                        Grasshopper.Instances.ActiveCanvas.Document.ScheduleSolution(5, InputChange);
                        return;
                    }
                }
            }
            catch(Exception e)
            {
                RhinoApp.WriteLine(e.Message);
            }
            if (!isBitmap)
            {
                if (!DA.GetData(2, ref colour)) colour = Color.Black;
                if (!DA.GetData(3, ref mid)) mid = false;
                if (!DA.GetData(4, ref size)) size = 12;
            }
        }
        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
            if (Locked || Hidden || (str == null && bmp == null)) return;
            var view = RhinoDoc.ActiveDoc.Views.ActiveView.Bounds;
            Point2d anchor = new Point2d(view.Width * coor.X, view.Height * coor.Y);
            if (isBitmap)
                args.Display.DrawBitmap(bmp, (int)anchor.X, (int)anchor.Y);
            else if (str != null)
                args.Display.Draw2dText(str, colour, anchor, mid, size);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendItem(menu, "Display Bitmap", Handler, true, isBitmap);
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (isBitmap)
                InputChange(null);
        }
        private void Handler(object sender, EventArgs e)
        {
            isBitmap = !isBitmap;
            InputChange(null);
        }
        private void InputChange(GH_Document doc)
        {
            if (isBitmap)
                for (int i = 4; i > 1; i--)
                {
                    Params.UnregisterInputParameter(Params.Input[i], true);
                }
            else
                foreach (var p in strParam)
                    Params.RegisterInputParam(p);
            Params.OnParametersChanged();
            ExpireSolution(true);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("Bitmap", isBitmap);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            isBitmap = reader.GetBoolean("Bitmap");
            return base.Read(reader);
        }
        public bool CanInsertParameter(GH_ParameterSide side, int index) => false;
        public bool CanRemoveParameter(GH_ParameterSide side, int index) => false;
        public IGH_Param CreateParameter(GH_ParameterSide side, int index) => null;
        public bool DestroyParameter(GH_ParameterSide side, int index) => false;
        public void VariableParameterMaintenance() { }
    }
}
