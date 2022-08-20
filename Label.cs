using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types.Transforms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus
{
    public class LabelWidget : GH_Widget
    {
        public override string Name
        {
            get { return "Label"; }
        }
        public override string Description
        {
            get { return "Display names above components"; }
        }
        public override Bitmap Icon_24x24
        {
            get
            {
                return Properties.Resources.Label;
            }
        }

        public LabelWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);

            if (settings.ConstainsEntry("Exclude"))
            {
                Exclude = new List<string>(settings.GetValue("Exclude", "").Split(','));
            }
            else
            {
                string Exclude_defaults = "Scribble,Panel,Value List,Button,Boolean Toggle,Number Slider,Sketch";
                settings.SetValue("Enabled", enabled);
                settings.SetValue("Exclude", Exclude_defaults);
                settings.WritePersistentSettings();
                Exclude = new List<string>(Exclude_defaults.Split(','));
            }

            Grasshopper.Instances.CanvasCreated += CanvasCreated;
        }

        public static List<string> Exclude = new List<string>();
        private bool enabled = true;

        public override bool Visible
        {
            get { return enabled; }
            set
            {
                GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                if (value == true)
                {
                    enabled = true;
                    Exclude = new List<string>(settings.GetValue("Exclude", "").Split(','));
                    Grasshopper.Instances.ActiveCanvas.CanvasPrePaintObjects += Label;
                }
                else
                {
                    enabled = false;
                    Grasshopper.Instances.ActiveCanvas.CanvasPrePaintObjects -= Label;
                }
                settings.SetValue("Enabled", enabled);
                settings.WritePersistentSettings();
            }
        }

        public override bool Contains(Point pt_control, PointF pt_canvas)
        {
            return false;
        }

        public override void Render(GH_Canvas Canvas)
        {
            return;
        }

        // Canvas Created Event Handler
        static void CanvasCreated(GH_Canvas canvas)
        {
            Grasshopper.Instances.CanvasCreated -= CanvasCreated;
            canvas.CanvasPrePaintObjects += Label;
        }

        // Pain Object Event Handler
        static void Label(GH_Canvas canvas)
        {
            if (!canvas.IsDocument || (GH_Canvas.ZoomFadeLow == 0))
                return;

            canvas.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            foreach (var comp in canvas.Document.Objects)
            {
                
                RectangleF bnd = comp.Attributes.Bounds;
                if (!canvas.Viewport.IsVisible(ref bnd, 20) ||
                    Exclude.Contains(comp.Name) ||
                    comp is GH_Group
                   )
                    continue;
                RectangleF anchor = comp.Attributes.Bounds;
                float x = anchor.X + 0.5f * anchor.Width;
                float y = anchor.Y - 0.1f * canvas.Font.Size;
                string name = "";

                if (comp.ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent")
                    name = comp.Name;
                else
                    name = ((comp.NickName == Grasshopper.Instances.ComponentServer.EmitObjectProxy(comp.ComponentGuid).Desc.NickName) || comp.NickName == string.Empty) ?
                                    comp.Name : comp.NickName;

                if (name.Length > 12)
                {
                    string[] words = name.Split(' ');
                    StringBuilder sb = new StringBuilder();
                    int limit = 0;
                    foreach(var w in words)
                    {
                        if (limit > 12)
                        {
                            sb.Append(Environment.NewLine);
                            limit = 0;
                        }
                        else if(limit > 0)
                            sb.Append(' ');
                        sb.Append(w);
                        limit += w.Length+1;
                    }
                    name = sb.ToString();
                }

                using (Font font = new Font(
                    SystemFonts.IconTitleFont.FontFamily,
                    (float)(SystemFonts.DefaultFont.Size * (GH_Canvas.ZoomFadeMedium <= 6 ? 1.5: (GH_Canvas.ZoomFadeHigh == 0 ? 1 : 0.5))) / Grasshopper.GUI.GH_GraphicsUtil.UiScale,
                    FontStyle.Italic))
                {
                    using (StringFormat alignment = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far })
                    {
                        canvas.Graphics.DrawString(
                            name,
                            font, Brushes.Gray,
                            x, y, alignment);
                    }
                }
            }
        }

    }
}
