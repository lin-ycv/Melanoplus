using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace Melanoplus
{
    public class WIP_Viewport : GH_Component
    {
        public WIP_Viewport() : base("Sync Viewport", "Sync", "Syncs canvas background with active Rhino viewport", "Params", "Input") { }
        public override GH_Exposure Exposure => GH_Exposure.hidden; //GH_Exposure.quarternary | GH_Exposure.obscure;
        public override Guid ComponentGuid => new Guid("{401ED444-C9DD-4670-B36E-3647D04783D7}");
        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            //aTimer = new Timer();
            //aTimer.Interval = 500;
            //aTimer.AutoReset = false;
            //aTimer.Enabled = true;
            //aTimer.Elapsed += (s,e)=> { bmp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer; restart = true; /*aTimer.Stop();*/ };
            bmp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer;
            //bmp = Rhino.Display.DisplayPipeline.DrawToBitmap(Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport, Grasshopper.Instances.ActiveCanvas.Size.Width, Grasshopper.Instances.ActiveCanvas.Size.Height);
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Enable", "E", "Sync canvas with viewport", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        //private static Timer aTimer;
        static dynamic bmp;
        //static bool restart = false;
        DateTime dt = DateTime.Now;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            Grasshopper.Instances.ActiveCanvas.CanvasPaintBackground -= PaintBackground;
            bmp = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer;
            bool e = false;
            DA.GetData(0, ref e);
            if (e)
            {
                Grasshopper.Instances.ActiveCanvas.CanvasPaintBackground += PaintBackground;
            }
            
        }

        private void PaintBackground(Grasshopper.GUI.Canvas.GH_Canvas Canvas)
        {
            DateTime dateTime = DateTime.Now;
            if ((dateTime - dt).TotalMilliseconds > 500) bmp= Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer;
            dt = dateTime;


            //using(Bitmap bmp = Rhino.Display.DisplayPipeline.DrawToBitmap(RhinoDocument.Views.ActiveView.ActiveViewport, Canvas.Bounds.Width, Canvas.Bounds.Height)){
            //if(!CompareBitmapsFast(prev, bmp)){
            //RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(Canvas.Size);
            bmp.MakeTransparent();
            Graphics G = Canvas.Graphics;
            G.ResetTransform();
            G.DrawImage(bmp, 0, 0,Canvas.Width,Canvas.Height);

            Grasshopper.GUI.GH_GraphicsUtil.ShadowRectangle(G, Canvas.ClientRectangle, 20, 20);
            Canvas.Viewport.ApplyProjection(G);

            //Canvas.ScheduleRegen(500);
            //Grasshopper.Instances.ActiveCanvas.Document.ScheduleSolution(500, (d) => Grasshopper.Instances.ActiveCanvas.Refresh());
        }
    }
}
