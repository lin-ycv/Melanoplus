using Grasshopper.Kernel;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus
{
    public class Animate : GH_Component
    {
        public override Guid ComponentGuid => new Guid("{8374F785-B678-450F-B264-9B3ED154E6D2}");
        public override GH_Exposure Exposure => GH_Exposure.primary;
        protected override Bitmap Icon => Properties.Resources.GIF;
        public Animate() : base("Animate Camera", "Animate",
            "Animate camera thru a series of named views.",
            "Display", "Viewport")
        { }
        bool pause = false;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Timeline", "Time", "Scrub between 0 and 1 to animate thru the order in named view.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("save", null, "Append current view to end of series.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("clear", null, "Clear all named view in this series.", GH_ParamAccess.item);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (pause) { pause = false;return; }
            RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            double time = -1;
            bool save = false, clear = false;
            DA.GetData(0, ref time);
            DA.GetData(1, ref save);
            DA.GetData(2, ref clear);

            NamedViewTable nvt = RhinoDoc.ActiveDoc.NamedViews;
            var views = nvt.Where(v => v.Name.StartsWith("melanoplus_"));
            if (save)
            {
                RhinoDoc.ActiveDoc.NamedViews.Add($"melanoplus_{views.GetHashCode()}", vp.Id);
                pause = true;
                return;
            }
            else if (clear && nvt.Any())
            {
                OnPingDocument().ScheduleSolution(5, ClearViews);
                return;
            }
            if (time >= 0 && nvt.Any())
            {
                int count = nvt.Where(v => v.Name.StartsWith("melanoplus_")).Count();
                double progress = (count - 1) * time, remainder = progress % 1.0;
                ViewportInfo A = nvt[(int)Math.Floor(progress)].Viewport, B = nvt[(int)Math.Ceiling(progress)].Viewport;
                vp.Camera35mmLensLength = ((B.Camera35mmLensLength - A.Camera35mmLensLength) * remainder) + A.Camera35mmLensLength;
                Point3d loc = ((B.CameraLocation - A.CameraLocation) * remainder) + A.CameraLocation, tar = ((B.TargetPoint - A.TargetPoint) * remainder) + A.TargetPoint;
                Vector3d updir = ((B.CameraUp - A.CameraUp) * remainder) + A.CameraUp;
                vp.SetCameraLocations(tar, loc);
                vp.CameraUp = updir;
            }
        }

        private void ClearViews(GH_Document doc)
        {
            NamedViewTable nvt = RhinoDoc.ActiveDoc.NamedViews;
            var views = nvt.Where(v => v.Name.StartsWith("melanoplus_"));
            do
            {
                nvt.Delete(views.First().Name);
            } while (views.Any());
        }
    }
}
