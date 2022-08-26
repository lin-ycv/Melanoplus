using Eto.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.SettingsControls;
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
        internal static Font font = GH_FontServer.StandardItalic;

        public LabelWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
            var cvt = new FontConverter();
            if (settings.ConstainsEntry("Exclude"))
            {
                Exclude = new List<string>(settings.GetValue("Exclude", "").Split(','));
                font = (settings.GetValue("Font", "") is string str) ? cvt.ConvertFromInvariantString(str) as Font : GH_FontServer.StandardItalic;
            }
            else
            {
                string Exclude_defaults = "Scribble,Panel,Value List,Button,Boolean Toggle,Number Slider,Sketch";
                settings.SetValue("Enabled", enabled);
                settings.SetValue("Exclude", Exclude_defaults);
                settings.SetValue("Font", cvt.ConvertToInvariantString(font));
                settings.WritePersistentSettings();
                Exclude = new List<string>(Exclude_defaults.Split(','));
            }
        }

        public static List<string> Exclude = new List<string>();
        private bool enabled = false;

        public override bool Visible
        {
            get { return enabled; }
            set
            {
                
                GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                if (value == true)
                {
                    enabled = true;
                    Instances.ActiveCanvas.CanvasPrePaintObjects += Label;
                }
                else
                {
                    enabled = false;
                    Instances.ActiveCanvas.CanvasPrePaintObjects -= Label;
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
                    name = ((comp.NickName == Instances.ComponentServer.EmitObjectProxy(comp.ComponentGuid).Desc.NickName) || comp.NickName == string.Empty) ?
                                    comp.Name : comp.NickName;

                if (name.Length > 12)
                {
                    string[] words = name.Split(' ');
                    StringBuilder sb = new StringBuilder();
                    int limit = 0;
                    foreach (var w in words)
                    {
                        if (limit > 12)
                        {
                            sb.Append(Environment.NewLine);
                            limit = 0;
                        }
                        else if (limit > 0)
                            sb.Append(' ');
                        sb.Append(w);
                        limit += w.Length + 1;
                    }
                    name = sb.ToString();
                }

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
    public class GH_LabelSettingsUI : IGH_SettingFrontend
    {
        public string Category => "Widgets";

        public IEnumerable<string> Keywords => new string[1]
        {
        "Font",
        };

        public string Name => "Label";

        public System.Windows.Forms.Control SettingsUI()
        {
            var fontpicker = new GH_FontControl
            {
                Font = LabelWidget.font,
                Name = "FontPicker",
                Size = new Size(553, 75),
                BorderStyle = BorderStyle.None,
            };
            fontpicker.FontChanged += (s, e) => { 
                LabelWidget.font = fontpicker.Font; 
                Instances.ActiveCanvas.Refresh();
                GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                settings.SetValue("Font", new FontConverter().ConvertToInvariantString(LabelWidget.font));
                settings.WritePersistentSettings();
            };
            return fontpicker;
        }

        System.Windows.Forms.Control IGH_SettingFrontend.SettingsUI()
        {
            return this.SettingsUI();
        }
    }
}
