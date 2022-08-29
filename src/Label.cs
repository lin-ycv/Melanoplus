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
        public static List<string> Exclude = new List<string>();
        private bool enabled = false;

        public LabelWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
            var cvt = new FontConverter();
            if (settings.Count > 0)
            {
                enabled = settings.GetValue("Enabled", false);
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
            Instances.CanvasCreated += CanvasCreated;
        }

        private void CanvasCreated(GH_Canvas canvas)
        {
            Instances.ActiveCanvas.DocumentChanged += DocumentChanged;
        }
        private void DocumentChanged(GH_Canvas canvas, GH_CanvasDocumentChangedEventArgs e)
        {
            Handler(enabled);
        }

        public override bool Visible
        {
            get { return enabled; }
            set
            {
                Handler(value);
                enabled = value;
                GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                settings.SetValue("Enabled", enabled);
                settings.WritePersistentSettings();
            }
        }
        private void Handler(bool value)
        {
            Instances.ActiveCanvas.CanvasPrePaintObjects -= Label;
            if (value == true)
            {
                Instances.ActiveCanvas.CanvasPrePaintObjects += Label;
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
        public string Name => "Label";
        public IEnumerable<string> Keywords => new string[2]
        {
            "Font",
            "Exclusion",
        }; 

        public System.Windows.Forms.Control SettingsUI()
        {
            return new GH_LabelSettingFrontEnd();
        }

        System.Windows.Forms.Control IGH_SettingFrontend.SettingsUI()
        {
            return this.SettingsUI();
        }
        public class GH_LabelSettingFrontEnd : UserControl
        {
            private GH_FontControl fontpicker;
            private System.Windows.Forms.TextBox input;
            private TableLayoutPanel tableLayoutPanel;
            public GH_LabelSettingFrontEnd()
            {
                InitializeComponent();
            }
            private void InitializeComponent()
            {
                tableLayoutPanel = new TableLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(12, 12, 12, 12),
                    Size = new Size(553, 5),
                    ColumnCount = 1,
                    RowCount = 2,
                };

                fontpicker = new GH_FontControl
                {
                    Font = LabelWidget.font,
                    Name = "FontPicker",
                    Size = new Size(553, 75),
                    BorderStyle = BorderStyle.None,
                    Dock = DockStyle.Fill,
                    TabIndex = 0,
                };
                fontpicker.FontChanged += (s, e) =>
                {
                    LabelWidget.font = fontpicker.Font;
                    Instances.ActiveCanvas.Refresh();
                    GH_SettingsServer settings = new GH_SettingsServer("melanoplus_label", true);
                    settings.SetValue("Font", new FontConverter().ConvertToInvariantString(LabelWidget.font));
                    settings.WritePersistentSettings();
                };

                input = new System.Windows.Forms.TextBox()
                {
                    Location = new Point(3,80),
                    Text = string.Join(",", LabelWidget.Exclude),
                    Name = "textBox1",
                    Size = new Size(553, 20),
                    Dock = DockStyle.Fill,
                    TabIndex = 1,
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


                tableLayoutPanel.Controls.Add(fontpicker);
                tableLayoutPanel.Controls.Add(input);

                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
                tableLayoutPanel.RowStyles.Add(new RowStyle());
                tableLayoutPanel.RowStyles.Add(new RowStyle());


                this.AutoScaleDimensions = new SizeF(6f, 13f);
                this.AutoScaleMode = AutoScaleMode.Font;
                this.AutoSize = true;
                this.Controls.Add(tableLayoutPanel);
                this.Margin = new Padding(12, 12, 12, 12);

                tableLayoutPanel.ResumeLayout(false);
                tableLayoutPanel.PerformLayout();
                this.ResumeLayout(false);

            }
            
        }
    }

}
