using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Rhino;
using Rhino.DocObjects;
using System;
using System.Drawing;
using System.Timers;

namespace Melanoplus
{
    public class ViewportBGWidget : GH_Widget
    {
        public override string Name => "Peek Viewport";
        public override string Description => "Non-interactive Rhino viewport as canvas background (CPU Heavy)";
        public override Bitmap Icon_24x24 => Properties.Resources.viewportBGRhino;
        public override bool Visible
        {
            get => enabled;
            set
            {
                enabled = value;
                Sync(Grasshopper.Instances.ActiveCanvas, value);
            }
        }

        private static bool enabled = false, update = true;
        private static Timer aTimer = new Timer() { AutoReset = true, Interval = 500 };
        private static Bitmap bmp = new Bitmap(1, 1);

        public ViewportBGWidget()
        {
            aTimer.Elapsed += Trigger;
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_viewport", true);
            //if (settings.Count > 0)
            //    enabled = settings.GetValue("Enabled", false);
            //else
            //{
            //    settings.SetValue("Enabled", enabled);
            //    settings.WritePersistentSettings();
            //}

        }

        internal static void CanvasCreated(GH_Canvas canvas) => Sync(canvas, enabled);

        private static void Sync(GH_Canvas canvas, bool enable)
        {
            canvas.CanvasPaintBackground -= PaintBackground;
            aTimer.Stop();
            if(enable)
            {
                Grasshopper.Instances.ActiveCanvas.CanvasPaintBackground += PaintBackground;
                aTimer.Start();
            }
        }
        private static void PaintBackground(GH_Canvas Canvas)
        {
            Graphics G = Canvas.Graphics;

            G.ResetTransform();

            if (update)
            {
                bmp = RhinoDoc.ActiveDoc.Views.ActiveView.CaptureToBitmap(Canvas.Size);
                var mode = Rhino.RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.DisplayMode.EnglishName;
                if (mode == "Wireframe" || mode == "Shaded")
                    bmp.MakeTransparent();
                update = false;
            }

            G.DrawImage(bmp, 0, 0);
            Grasshopper.GUI.GH_GraphicsUtil.ShadowRectangle(G, Canvas.ClientRectangle);

            Canvas.Viewport.ApplyProjection(G);
        }
        void Trigger(object sender, EventArgs e)
        {
            update = true;
        }

        public override void Render(GH_Canvas Canvas) { }
        public override bool Contains(Point pt_control, PointF pt_canvas) => false;
    }
}