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

namespace Melanoplus
{
    //should use menu option to switch between text and path to image
    public class R_HUD : GH_Component, IGH_VariableParameterComponent
    {
        public R_HUD() : base("Rhino HUD", "HUD", "Displays gh data on the viewport", "Melanoplus", "Display") { }
        public override Guid ComponentGuid => new Guid("{95390F76-AA4C-46D1-9D35-E605C4758835}");
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Position", "XY", "Position of HUD on the viewport [0-1]", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager.AddGenericParameter("Data", "D", "Data to be displayed", GH_ParamAccess.item);
            pManager[1].Optional = true;
            foreach (var p in strParam)
                Params.RegisterInputParam(p);
        }
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (isBitmap)
                foreach (var p in strParam)
                    Params.UnregisterInputParameter(p, true);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (bmp != null) bmp.Dispose();
            if (!DA.GetData(0, ref coor)) return;
            object data = new object();
            if (!DA.GetData(1, ref data)) { str = null; bmp = null; return; }
            if (coor.X > 1) coor.X = 1;
            else if (coor.X < 0) coor.X = 0;
            if (coor.Y > 1) coor.Y = 1;
            else if (coor.Y < 0) coor.Y = 0;
            arrow = new Line(new Point3d(0, 0, 0), coor);
            GH_Convert.ToString(data, out str, GH_Conversion.Both);
            if ((data as GH_ObjectWrapper) != null && (data as GH_ObjectWrapper).Value is Bitmap b)
            {
                bmp = new DisplayBitmap(b);
                str = null;
            }
            if (str != null && Params.Count() == 2)
            {
                isBitmap = false;
                foreach (var p in strParam)
                    Params.RegisterInputParam(p);
                Params.OnParametersChanged();
                return;
            }
            else if (str == null && Params.Count() != 2)
            {
                Grasshopper.Instances.ActiveCanvas.Document.ScheduleSolution(5, Callback);
                isBitmap = true;
                return;
            }
            if (!isBitmap)
            {
                if (!DA.GetData(2, ref colour)) colour = Color.Black;
                if (!DA.GetData(3, ref mid)) mid = false;
                if (!DA.GetData(4, ref size)) size = 12;
            }
        }
        private void Callback(GH_Document doc)
        {
            var clean = Params.Input.Where(p => strParam.Any(s => s.Name == p.Name)).ToArray();
            foreach (var p in clean)
            {
                Params.UnregisterInputParameter(p, true);
            }
            Params.OnParametersChanged();
        }

        Point3d coor;
        string str;
        Color colour = Color.Black;
        bool mid = false, isBitmap = false;
        Line arrow;
        DisplayBitmap bmp;
        int size = 12;
        readonly IGH_Param[] strParam = new IGH_Param[3]
        {
            new Param_Colour {Name = "colour", NickName = "c", Description = "Colour of displayed data [Optional]", Optional = true, Access = GH_ParamAccess.item  },
            new Param_Boolean {Name = "centered", NickName = "m", Description = "Position as Mid-point [Optional]", Optional = true, Access = GH_ParamAccess.item },
            new Param_Integer {Name = "size", NickName = "s", Description = "Size of text [Optional]", Optional = true, Access = GH_ParamAccess.item },
        };

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportMeshes(args);
            if (Locked || Hidden || (str == null && bmp == null)) return;
            var view = RhinoDoc.ActiveDoc.Views.ActiveView.Bounds;
            Point2d anchor = new Point2d(view.Width * coor.X, view.Height * coor.Y);
            if (str != null)
                args.Display.Draw2dText(str, colour, anchor, mid, size);
            else if (bmp != null)
                args.Display.DrawBitmap(bmp, (int)anchor.X, (int)anchor.Y);

        }
        public override BoundingBox ClippingBox => BoundingBox.Union(base.ClippingBox, arrow.BoundingBox);
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
