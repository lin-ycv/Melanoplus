using Grasshopper.GUI.Canvas;
using Grasshopper.GUI;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Rhino.Display;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;

namespace Melanoplus
{
    public class CameraSet : GH_Component
    {
        public override Guid ComponentGuid => new Guid("{37C3E9E6-D219-4D02-AF3A-04FFF171AD93}");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.SetCamera;
        private bool enable = false;

        public CameraSet() : base("Set Camera", "Camera",
            "Control Rhino camera. Camera location, direction, and target, relationships are automatically maintained.",
            "Display", "Viewport")
        { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Set", null, "Set camera.", GH_ParamAccess.item);
            pManager.AddPointParameter("camera", null, "Set viewport camera location.", GH_ParamAccess.item);
            pManager.AddPointParameter("target", null, "Set camera target point.", GH_ParamAccess.item);
            pManager.AddVectorParameter("up direction", "up", "Align up direction of camera to a vector", GH_ParamAccess.item);
            pManager.AddNumberParameter("lens length", "lens", "Equivalent for a 35mm camera (24mm x 36mm sensor). [Actual focal length x 36 mm / Sensor width (assuming 3:2 sensor)]", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref enable);
            if (!enable) return;

            RhinoViewport vp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            Point3d camera = vp.CameraLocation, target = vp.CameraTarget;
            Vector3d up = Vector3d.Unset;
            double lens = vp.Camera35mmLensLength;

            DA.GetData(1, ref camera);
            DA.GetData(2, ref target);
            DA.GetData(3, ref up);
            DA.GetData(4, ref lens);

            vp.CameraUp = up;
            vp.Camera35mmLensLength = lens;
            vp.SetCameraLocations(target, camera);
        }
        public override void CreateAttributes()
        {
            m_attributes = new CamAttributes(this);
        }
        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("enable", enable);
            return base.Write(writer);
        }
        public override bool Read(GH_IReader reader)
        {
            reader.GetBoolean("enable");
            return base.Read(reader);
        }

        public void Toggle()
        {
            enable = !enable;
            ExpireSolution(true);
        }
    }
    public class CamAttributes : GH_ComponentAttributes
    {
        public CamAttributes(IGH_Component component) : base(component)
        {
        }
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (base.Owner is CameraSet comp && comp.Params.Input[0].SourceCount < 1)
            {
                comp.Params.Input[0].ClearData();
                comp.Toggle();
                return GH_ObjectResponse.Handled;
            }
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
