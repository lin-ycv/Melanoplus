using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Base;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.SettingsControls;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types.Transforms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus
{
    public class LabelWidget : GH_Widget
    {
        public override string Name { get => "Label"; }
        public override string Description { get => "Display names above components"; }
        public override Bitmap Icon_24x24 { get => Properties.Resources.Label; }
        public override bool Visible
        {
            get => enabled;
            set
            {
                Handler(Instances.ActiveCanvas, value);
                enabled = value;
                GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                settings.SetValue("Enabled", enabled);
                settings.WritePersistentSettings();
            }
        }

        internal static Font font = GH_FontServer.StandardItalic;
        internal static List<string> Exclude = new List<string>();
        private static bool enabled = false;
        internal static SolidBrush brush = (SolidBrush)Brushes.Gray;
        readonly private static StringFormat alignment = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        internal static bool shownickname = false;

        public LabelWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
            if (settings.Count > 0)
            {
                try { TryRead(); }
                catch { Defaults(); }
            }
            else
                Defaults();

            void TryRead()
            {
                enabled = settings.GetValue("Enabled", false);
                Exclude = new List<string>(settings.GetValue("Exclude", "").Split(','));
                var fDe = settings.GetValue("Font", "").Split('|');
                font = new Font(fDe[0], float.Parse(fDe[1]), (FontStyle)Convert.ToInt32(fDe[2]));
                brush.Color = settings.GetValue("Color", Color.Gray);
                shownickname = settings.GetValue("Nickname", false);
            }
            void Defaults()
            {
                string Exclude_defaults = "Colour Swatch,Scribble,Panel,Value List,Button,Boolean Toggle,Number Slider,Sketch";
                settings.SetValue("Enabled", enabled);
                settings.SetValue("Exclude", Exclude_defaults);
                settings.SetValue("Font", string.Join("|", new string[] { font.FontFamily.Name, font.SizeInPoints.ToString(), ((int)font.Style).ToString() }));
                settings.WritePersistentSettings();
                Exclude = new List<string>(Exclude_defaults.Split(','));
            }
        }

        public override bool Contains(Point pt_control, PointF pt_canvas) => false;
        public override void Render(GH_Canvas Canvas) { }

        internal static void CanvasCreated(GH_Canvas canvas)
        {
            Handler(canvas, enabled);
        }
        private static void Handler(GH_Canvas canvas, bool value)
        {
            canvas.CanvasPrePaintObjects -= Label;
            if (value == true)
            {
                canvas.CanvasPrePaintObjects += Label;
            }
        }
        private static void Label(GH_Canvas canvas)
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
                else if (shownickname)
                {
                    name = comp.NickName == string.Empty ? Instances.ComponentServer.EmitObjectProxy(comp.ComponentGuid).Desc.NickName : comp.NickName;
                }
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

                canvas.Graphics.DrawString(
                    name,
                    font, brush,
                    x, y, alignment);
            }
        }

    }

    public class GH_LabelSettingsUI : IGH_SettingFrontend
    {
        public string Category => "Widgets";
        public string Name => "Label";
        public IEnumerable<string> Keywords => new string[]
        {
            "Font",
        };

        public Control SettingsUI()
        {
            return new GH_LabelSettingFrontEnd();
        }
        public class GH_LabelSettingFrontEnd : UserControl
        {
            private GH_FontControl fontpicker;
            private TextBox input;
            private TableLayoutPanel tableLayoutPanel;
            private GH_ColourSwatchControl colour;
            private Label colourLabel, nickLabel;
            private CheckBox nickCheck;
            public GH_LabelSettingFrontEnd()
            {
                tableLayoutPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(12, 12, 12, 12),
                    Size = new Size(553, 5),
                    ColumnCount = 4,
                    RowCount = 2,
                };

                colour = new GH_ColourSwatchControl()
                {
                    Colour = LabelWidget.brush.Color,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                colour.ColourChanged += (s, e) =>
                {
                    LabelWidget.brush.Color = colour.Colour;
                    Instances.ActiveCanvas.Refresh();
                    GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                    settings.SetValue("Colour", colour.Colour);
                    settings.WritePersistentSettings();
                };
                colourLabel = new Label()
                {
                    Text = "Label Colour",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                nickCheck = new CheckBox()
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0),
                    AutoSize = true,
                    Margin = new Padding(0),
                    Checked = LabelWidget.shownickname,
                    Size = new Size(20, 20),
                };
                nickCheck.CheckedChanged += (s, e) =>
                {
                    LabelWidget.shownickname = nickCheck.Checked;
                    Instances.ActiveCanvas.Refresh();
                    GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                    settings.SetValue("Nickname", nickCheck.Checked);
                    settings.WritePersistentSettings();
                };
                nickLabel = new Label
                {
                    Text = "Use Nickname",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                fontpicker = new GH_FontControl
                {
                    Font = LabelWidget.font,
                    Name = "FontPicker",
                    Size = new Size(553, 75),
                    BorderStyle = BorderStyle.None,
                    Dock = DockStyle.Fill,
                };
                fontpicker.FontChanged += (s, e) =>
                {
                    LabelWidget.font = fontpicker.Font;
                    Instances.ActiveCanvas.Refresh();
                    GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                    settings.SetValue("Font", string.Join("|", new string[] { LabelWidget.font.FontFamily.Name, LabelWidget.font.SizeInPoints.ToString(), ((int)LabelWidget.font.Style).ToString() }));
                    settings.WritePersistentSettings();
                };
                input = new TextBox()
                {
                    Text = string.Join(",", LabelWidget.Exclude),
                    Name = "textBox1",
                    Size = new Size(553, 5),
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0, 6, 0, 6),
                };
                input.TextChanged += (s, e) =>
                {
                    GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                    settings.SetValue("Exclude", input.Text);
                    settings.WritePersistentSettings();
                    LabelWidget.Exclude = input.Text.Split(',').ToList();
                };

                tableLayoutPanel.SuspendLayout();
                SuspendLayout();

                tableLayoutPanel.Controls.Add(colour, 0, 0);
                tableLayoutPanel.Controls.Add(colourLabel, 1, 0);
                tableLayoutPanel.Controls.Add(nickCheck, 2, 0);
                tableLayoutPanel.Controls.Add(nickLabel, 3, 0);
                tableLayoutPanel.Controls.Add(fontpicker, 0, 1);
                tableLayoutPanel.SetColumnSpan(fontpicker, 4);
                tableLayoutPanel.Controls.Add(input, 0, 2);
                tableLayoutPanel.SetColumnSpan(input, 4);

                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
                tableLayoutPanel.RowStyles.Add(new RowStyle());
                tableLayoutPanel.RowStyles.Add(new RowStyle());

                this.AutoScaleDimensions = new SizeF(6f, 13f);
                this.AutoScaleMode = AutoScaleMode.Font;
                this.AutoSize = true;
                this.Controls.Add(tableLayoutPanel);
                this.Margin = new Padding(12, 12, 12, 12);

                tableLayoutPanel.ResumeLayout(true);
                this.ResumeLayout(false);
            }

        }
    }

}
