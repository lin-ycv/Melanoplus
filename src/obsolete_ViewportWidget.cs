using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;
using Rhino.DocObjects;
using Rhino.Render;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace Melanoplus
{
    public class obsolete_ViewportWidget : GH_Widget
    {
        public override string Name => "Peek Viewport";
        public override string Description => "Peek at active Rhino viewport with GH canvas";
        public override Bitmap Icon_24x24 => Properties.Resources.viewportRhino;
        public override bool Visible
        {
            get => enabled;
            set
            {
                enabled = value;
                if (value) brush = new TextureBrush(RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer ?? empty, WrapMode.Clamp);
                Sync(Grasshopper.Instances.ActiveCanvas, value);
            }
        }

        private static bool enabled = false, update = true;
        private static Timer aTimer = new Timer() { AutoReset = true, Interval = 250 };
        private static TextureBrush brush = null;
        readonly private static Bitmap empty = new Bitmap(1, 1);

        public obsolete_ViewportWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_viewport", true);
            if (settings.Count > 0)
                enabled = settings.GetValue("Enabled", false);
            else
            {
                settings.SetValue("Enabled", enabled);
                settings.WritePersistentSettings();
            }

        }

        internal static void CanvasCreated(GH_Canvas canvas) => Sync(canvas, enabled);

        private static void Sync(GH_Canvas canvas, bool enable)
        {
            aTimer.Stop();
            canvas.CanvasPaintBackground -= PaintBackground;
            Rhino.Display.DisplayPipeline.PostDrawObjects -= SetTimer;
            aTimer.Elapsed -= SetBrush;
            if (enable)
            {
                canvas.CanvasPaintBackground += PaintBackground;
                Rhino.Display.DisplayPipeline.PostDrawObjects += SetTimer;
                aTimer.Elapsed += SetBrush;
                aTimer.Start();
            }
        }
        private static void PaintBackground(GH_Canvas Canvas)
        {
            Graphics G = Canvas.Graphics;
            G.ResetTransform();
            G.FillRectangle(brush, Canvas.ClientRectangle);
            Grasshopper.GUI.GH_GraphicsUtil.ShadowRectangle(G, new Rectangle(0,0, Math.Min(brush.Image.Width, Canvas.Width),Math.Min(brush.Image.Height,Canvas.Height)) , 20);
            Canvas.Viewport.ApplyProjection(G);

            Canvas.ScheduleRegen(500);
        }
        private static void SetTimer(object sender, Rhino.Display.DrawEventArgs e)
        {
            aTimer.Interval = 250;
            if (update)
            {
                brush = new TextureBrush(e.Display.FrameBuffer, WrapMode.Clamp);
                update = false;
            }
        }

        private static void SetBrush(object sender, ElapsedEventArgs e) => update = true; 
        public override void Render(GH_Canvas Canvas) { }
        public override bool Contains(Point pt_control, PointF pt_canvas) => false;
    }
}