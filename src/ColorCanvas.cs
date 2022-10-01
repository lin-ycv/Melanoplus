using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.SettingsControls;
using Grasshopper.Kernel;
using Rhino.NodeInCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus.Settings
{
    public class ColorCanvas : IGH_SettingFrontend
    {
        public string Category => "Palette";
        public string Name => "Canvas Colours";
        public IEnumerable<string> Keywords => new string[1] { "Background" };
        public Control SettingsUI() => new GH_ColorCanvasSettinFrontEnd();
        public class GH_ColorCanvasSettinFrontEnd : UserControl
        {
            private TableLayoutPanel tableLayoutPanel;
            private IContainer components;
            private GH_ColourSwatchControl swatchGrid, swatchBack, swatchEdge, swatchShade, swatchMono;
            private Label labelBack, labelEdge, labelShade, labelGrid, labelMono, checkLabel;
            private ToolTip ToolTip;
            private CheckBox checkMono;
            private Button reset;
            public GH_ColorCanvasSettinFrontEnd()
            {
                tableLayoutPanel = new TableLayoutPanel()
                {
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                    ColumnCount = 6,
                    RowCount = 3,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                };
                components = new Container();
                ToolTip = new ToolTip(components);
                swatchEdge = new GH_ColourSwatchControl()
                {
                    Colour = GH_Skin.canvas_edge,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                swatchEdge.ColourChanged += (s, e) => { GH_Skin.canvas_edge = swatchEdge.Colour; Instances.ActiveCanvas.Refresh(); };
                labelEdge = new Label()
                {
                    Text = "Edge",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                swatchBack = new GH_ColourSwatchControl()
                {
                    Colour = GH_Skin.canvas_back,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                swatchBack.ColourChanged += (s, e) => { GH_Skin.canvas_back = swatchBack.Colour; Instances.ActiveCanvas.Refresh(); };
                labelBack = new Label()
                {
                    Text = "Fill",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                swatchShade = new GH_ColourSwatchControl()
                {
                    Colour = GH_Skin.canvas_shade,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                swatchShade.ColourChanged += (s, e) => { GH_Skin.canvas_shade = swatchShade.Colour; Instances.ActiveCanvas.Refresh(); };
                labelShade = new Label()
                {
                    Text = "Shade",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                swatchGrid = new GH_ColourSwatchControl()
                {
                    Colour = GH_Skin.canvas_grid,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                swatchGrid.ColourChanged += (s, e) => { GH_Skin.canvas_grid = swatchGrid.Colour; Instances.ActiveCanvas.Refresh(); };
                labelGrid = new Label()
                {
                    Text = "Grid",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                checkMono = new CheckBox()
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(0),
                    AutoSize = true,
                    Margin = new Padding(0),
                    Checked = GH_Skin.canvas_mono,
                    Size = new Size(20, 20),
                };
                checkMono.CheckedChanged += (s, e) => { GH_Skin.canvas_mono = checkMono.Checked; Instances.ActiveCanvas.Refresh(); };
                checkLabel = new Label()
                {
                    Text = "Mono",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                swatchMono = new GH_ColourSwatchControl()
                {
                    Colour = GH_Skin.canvas_mono_color,
                    Size = new Size(20, 20),
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                swatchMono.ColourChanged += (s, e) => { GH_Skin.canvas_mono_color = swatchMono.Colour; Instances.ActiveCanvas.Refresh(); };
                labelMono = new Label()
                {
                    Text = "Colour",
                    Font = SystemFonts.DefaultFont,
                    Enabled = false,
                    AutoSize = true,
                    Margin = new Padding(3, 0, 40, 0),
                };
                reset = new Button()
                {
                    Text = "Reset to Default values",
                    Height = 20,
                    FlatStyle = FlatStyle.Flat,
                    Margin = new Padding(0,12,0,12),
                    Padding = new Padding(0),
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowOnly,
                };
                reset.Click += (s, e) =>
                {
                    swatchGrid.Colour = Color.FromArgb(30, 0, 0, 0);
                    swatchBack.Colour = Color.FromArgb(255, 212, 208, 200);
                    swatchEdge.Colour = Color.FromArgb(255, 0, 0, 0);
                    swatchShade.Colour = Color.FromArgb(80, 0, 0, 0);
                    swatchMono.Colour = Color.FromArgb(255, 255, 255, 255);
                    checkMono.Checked = false;
                    GH_Skin.canvas_shade_size = 30;
                    GH_Skin.canvas_grid_col = 150;
                    GH_Skin.canvas_grid_row = 50;
                    Instances.ActiveCanvas.Refresh();
                };

                tableLayoutPanel.SuspendLayout();
                SuspendLayout();
                ToolTip.SetToolTip(swatchBack, "Set the colour used to draw Background");
                ToolTip.SetToolTip(swatchEdge, "Set the colour used to draw Edges");
                ToolTip.SetToolTip(swatchGrid, "Set the colour used to draw Grids");
                ToolTip.SetToolTip(swatchShade, "Set the colour used to draw Shades");
                ToolTip.SetToolTip(swatchMono, "Set the colour used for Monocolour");
                ToolTip.SetToolTip(checkMono, "Set Monocolour");
                ToolTip.SetToolTip(checkLabel, "Set Monocolour");
                ToolTip.SetToolTip(reset, "Return canvas colour settings to Default values");

                tableLayoutPanel.Controls.Add(swatchEdge, 0, 0);
                tableLayoutPanel.Controls.Add(labelEdge, 1, 0);
                tableLayoutPanel.Controls.Add(swatchBack, 0, 1);
                tableLayoutPanel.Controls.Add(labelBack, 1, 1);
                tableLayoutPanel.Controls.Add(swatchShade, 2, 0);
                tableLayoutPanel.Controls.Add(labelShade, 3, 0);
                tableLayoutPanel.Controls.Add(swatchGrid, 2, 1);
                tableLayoutPanel.Controls.Add(labelGrid, 3, 1);
                tableLayoutPanel.Controls.Add(checkMono, 4, 0);
                tableLayoutPanel.Controls.Add(checkLabel, 5, 0);
                tableLayoutPanel.Controls.Add(swatchMono, 4, 1);
                tableLayoutPanel.Controls.Add(labelMono, 5, 1);
                tableLayoutPanel.Controls.Add(reset, 2, 2);
                tableLayoutPanel.SetColumnSpan(reset, 4);

                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.ColumnStyles.Add(new ColumnStyle());
                tableLayoutPanel.RowStyles.Add(new RowStyle());
                tableLayoutPanel.RowStyles.Add(new RowStyle());
                base.AutoScaleDimensions = new SizeF(6f, 13f);
                base.AutoScaleMode = AutoScaleMode.Font;
                base.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                base.Controls.Add(tableLayoutPanel);
                base.Size = new Size(293, 90);
                tableLayoutPanel.ResumeLayout(false);
                tableLayoutPanel.PerformLayout();
                ResumeLayout(false);
            }
        }

    }
}
