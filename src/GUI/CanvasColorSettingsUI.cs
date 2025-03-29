namespace Melanoplus.GUI
{
    public class CanvasColorSettingsUI : IGH_SettingFrontend
    {
        public string Category => "Palette";

        public string Name => "Canvas Colours (Melanoplus)";

        public IEnumerable<string> Keywords =>
        [
            "Canvas", "Colour",
        ];
        public Control SettingsUI()
        {
            return new GH_CanvasColorFrontEnd();
        }
    }
    public class GH_CanvasColorFrontEnd : UserControl
    {
        internal TableLayoutPanel LayoutPanel;
        internal TableLayoutPanel[] SwatchLabelLayout;
        internal GH_ColourSwatchControl SwatchGrid, SwatchBack, SwatchEdge, SwatchShade, SwatchMono;
        internal Label LabelBack, LabelEdge, LabelShade, LabelGrid, LabelMono;
        internal CheckBox CheckMono;
        internal Button DarkMode, ButtonReset;

        public GH_CanvasColorFrontEnd()
        {
            LayoutPanel = new()
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Name = "LayoutPanel",
                ColumnCount = 2,
                RowCount = 4,
            };
            SwatchLabelLayout = [new(), new(), new(), new(), new(),];
            for (int i = 0; i < SwatchLabelLayout.Length; i++)
            {
                SwatchLabelLayout[i].ColumnCount = 2;
                SwatchLabelLayout[i].RowCount = 1;
                SwatchLabelLayout[i].Dock = DockStyle.Fill;
                SwatchLabelLayout[i].Location = new Point(0, 0);
                SwatchLabelLayout[i].Margin = new Padding(0);
                SwatchLabelLayout[i].Size = new Size(100, 20);
                SwatchLabelLayout[i].ColumnStyles.Add(new ColumnStyle());
                SwatchLabelLayout[i].ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
                SwatchLabelLayout[i].RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            }

            LayoutPanel.SuspendLayout();
            SuspendLayout();
            LayoutPanel.ColumnStyles.Add(new ColumnStyle());
            LayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Row 1 Column 1
            CheckMono = new()
            {
                Checked = GH_Skin.canvas_mono,
                Dock = DockStyle.Fill,
                Location = new Point(38, 0),
                Margin = new Padding(6, 0, 0, 0),
                Size = new Size(100, 20),
                Text = "Mono Colour",
                UseVisualStyleBackColor = true,
            };
            CheckMono.CheckedChanged += (s, e) =>
            {
                ToggleMono(CheckMono.Checked);
                Instances.ActiveCanvas.Refresh();
            };
            LayoutPanel.Controls.Add(CheckMono, 0, 0);
            // Row 1 Column 2
            SwatchMono = new()
            {
                Colour = GH_Skin.canvas_mono_color,
                Enabled = GH_Skin.canvas_mono,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Size = new Size(20, 20),
            };
            SwatchMono.ColourChanged += (s, e) =>
            {
                GH_Skin.canvas_mono_color = SwatchMono.Colour;
                Instances.ActiveCanvas.Refresh();
            };
            LabelMono = new()
            {
                Dock = DockStyle.Fill,
                Enabled = GH_Skin.canvas_mono,
                Margin = new Padding(3, 3, 0, 0),
                Size = new Size(231, 20),
                Text = "Mono Colour",
            };
            SwatchLabelLayout[0].Controls.Add(SwatchMono, 0, 0);
            SwatchLabelLayout[0].Controls.Add(LabelMono, 1, 0);
            LayoutPanel.Controls.Add(SwatchLabelLayout[0], 1, 0);

            // Row 2 Column 1
            SwatchBack = new()
            {
                Colour = GH_Skin.canvas_back,
                Enabled = !GH_Skin.canvas_mono,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Size = new Size(20, 20),
            };
            SwatchBack.ColourChanged += (s, e) =>
            {
                GH_Skin.canvas_back = SwatchBack.Colour;
                Instances.ActiveCanvas.Refresh();
            };
            LabelBack = new()
            {
                Dock = DockStyle.Fill,
                Enabled = !GH_Skin.canvas_mono,
                Margin = new Padding(3, 3, 0, 0),
                Size = new Size(80, 20),
                Text = "Fill",
            };
            SwatchLabelLayout[1].Controls.Add(SwatchBack, 0, 0);
            SwatchLabelLayout[1].Controls.Add(LabelBack, 1, 0);
            LayoutPanel.Controls.Add(SwatchLabelLayout[1], 0, 1);
            // Row2  Column 2
            SwatchEdge = new()
            {
                Colour = GH_Skin.canvas_edge,
                Enabled = !GH_Skin.canvas_mono,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Size = new Size(20, 20),
            };
            SwatchEdge.ColourChanged += (s, e) =>
            {
                GH_Skin.canvas_edge = SwatchEdge.Colour;
                Instances.ActiveCanvas.Refresh();
            };
            LabelEdge = new()
            {
                Dock = DockStyle.Fill,
                Enabled = !GH_Skin.canvas_mono,
                Margin = new Padding(3, 3, 0, 0),
                Size = new Size(80, 20),
                Text = "Edge",
            };
            SwatchLabelLayout[2].Controls.Add(SwatchEdge, 0, 0);
            SwatchLabelLayout[2].Controls.Add(LabelEdge, 1, 0);
            LayoutPanel.Controls.Add(SwatchLabelLayout[2], 1, 1);

            // Row 3 Column 1
            SwatchShade = new()
            {
                Colour = GH_Skin.canvas_shade,
                Enabled = !GH_Skin.canvas_mono,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Size = new Size(20, 20),
            };
            SwatchShade.ColourChanged += (s, e) =>
            {
                GH_Skin.canvas_shade = SwatchShade.Colour;
                Instances.ActiveCanvas.Refresh();
            };
            LabelShade = new()
            {
                Dock = DockStyle.Fill,
                Enabled = !GH_Skin.canvas_mono,
                Margin = new Padding(3, 3, 0, 0),
                Size = new Size(80, 20),
                Text = "Shade",
            };
            SwatchLabelLayout[3].Controls.Add(SwatchShade, 0, 0);
            SwatchLabelLayout[3].Controls.Add(LabelShade, 1, 0);
            LayoutPanel.Controls.Add(SwatchLabelLayout[3], 0, 2);
            // Row 3 Column 2
            SwatchGrid = new()
            {
                Colour = GH_Skin.canvas_grid,
                Enabled = !GH_Skin.canvas_mono,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Size = new Size(20, 20),
            };
            SwatchGrid.ColourChanged += (s, e) =>
            {
                GH_Skin.canvas_grid = SwatchGrid.Colour;
                Instances.ActiveCanvas.Refresh();
            };
            LabelGrid = new()
            {
                Dock = DockStyle.Fill,
                Enabled = !GH_Skin.canvas_mono,
                Margin = new Padding(3, 3, 0, 0),
                Size = new Size(80, 20),
                Text = "Grid",
            };
            SwatchLabelLayout[4].Controls.Add(SwatchGrid, 0, 0);
            SwatchLabelLayout[4].Controls.Add(LabelGrid, 1, 0);
            LayoutPanel.Controls.Add(SwatchLabelLayout[4], 1, 2);

            // Row 4
            DarkMode = new()
            {
                Text = "Dark Mode",
                Height = 20,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 12, 0, 0),
                Padding = new Padding(0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
            };
            DarkMode.Click += (s, e) =>
            {
                SwatchGrid.Colour = Color.FromArgb(30, 255, 255, 255);
                SwatchBack.Colour = Color.FromArgb(255, 46, 46, 46);
                SwatchEdge.Colour = Color.FromArgb(255, 0, 0, 0);
                SwatchShade.Colour = Color.FromArgb(80, 0, 0, 0);
                SwatchMono.Colour = Color.FromArgb(255, 255, 255, 255);
                CheckMono.Checked = false;
                GH_Skin.canvas_shade_size = 30;
                GH_Skin.canvas_grid_col = 150;
                GH_Skin.canvas_grid_row = 50;
                GH_Skin.wire_default = Color.Gray;
                GH_Skin.wire_selected_b = Color.DarkGray;
                GH_Skin.palette_normal_standard.Fill = Color.DarkGray;
                GH_Skin.palette_hidden_standard.Fill = Color.FromArgb(255, 104, 104, 104);
                GH_Skin.palette_locked_standard.Fill = Color.FromArgb(255, 80, 80, 80);
                Instances.ActiveCanvas.Refresh();
            };
            LayoutPanel.Controls.Add(DarkMode, 0, 3);
            LayoutPanel.SetColumnSpan(DarkMode, 4);

            // Row 5
            ButtonReset = new()
            {
                Text = "Reset to Default values",
                Height = 20,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 12),
                Padding = new Padding(0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
            };
            ButtonReset.Click += (s, e) =>
            {
                SwatchGrid.Colour = Color.FromArgb(30, 0, 0, 0);
                SwatchBack.Colour = Color.FromArgb(255, 212, 208, 200);
                SwatchEdge.Colour = Color.FromArgb(255, 0, 0, 0);
                SwatchShade.Colour = Color.FromArgb(80, 0, 0, 0);
                SwatchMono.Colour = Color.FromArgb(255, 255, 255, 255);
                CheckMono.Checked = false;
                GH_Skin.canvas_shade_size = 30;
                GH_Skin.canvas_grid_col = 150;
                GH_Skin.canvas_grid_row = 50;
                GH_Skin.wire_default = Color.Black;
                GH_Skin.wire_selected_b = Color.FromArgb(50, 0, 0, 0);
                GH_Skin.palette_normal_standard.Fill = Color.FromArgb(255, 200, 200, 200);
                GH_Skin.palette_normal_standard.Text = Color.Black;
                GH_Skin.palette_locked_standard.Fill = Color.FromArgb(255, 120, 120, 120);
                GH_Skin.palette_locked_standard.Text = Color.FromArgb(255, 70, 70, 70);
                GH_Skin.palette_hidden_standard.Fill = Color.FromArgb(255, 140, 140, 155);
                GH_Skin.palette_hidden_standard.Text = Color.FromArgb(255, 0, 0, 0);
                Instances.ActiveCanvas.Refresh();
            };
            LayoutPanel.Controls.Add(ButtonReset, 0, 4);
            LayoutPanel.SetColumnSpan(ButtonReset, 4);

            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LayoutPanel);
            Margin = new Padding(24, 23, 24, 23);
            Name = "GH_LabelWidgetFrontEnd";
            Size = new Size(514, 134);
            LayoutPanel.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void ToggleMono(bool value)
        {
            GH_Skin.canvas_mono = value;
            SwatchMono.Enabled = value;
            LabelMono.Enabled = value;
            SwatchBack.Enabled = !value;
            LabelBack.Enabled = !value;
            SwatchEdge.Enabled = !value;
            LabelEdge.Enabled = !value;
            SwatchShade.Enabled = !value;
            LabelShade.Enabled = !value;
            SwatchGrid.Enabled = !value;
            LabelGrid.Enabled = !value;
        }
    }
}
