using Melanoplus.Widgets;

namespace Melanoplus.GUI
{
    public class LabelWidgetSettingsUI : IGH_SettingFrontend
    {
        public string Category => "Widgets";
        public string Name => "Label (Melanoplus)";
        public IEnumerable<string> Keywords =>
        [
            "Widget", "Label", "Font",
        ];

        public Control SettingsUI()
        {
            return new GH_LabelWidgetFrontEnd();
        }
    }
    public class GH_LabelWidgetFrontEnd : UserControl
    {
        internal TableLayoutPanel LayoutPanel;
        internal GH_Label Icon;
        internal CheckBox Nickname, CustomNickname;
        internal GH_ColourSwatchControl ColorPicker;
        internal Label ColorLabel;
        internal TextBox Exclude;
        internal GH_FontControl FontPicker;
        internal Button ButtonSelected;
        private Size _container = new(514, 242);
        public GH_LabelWidgetFrontEnd()
        {
            LayoutPanel = new()
            {
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Margin = new Padding(24, 23, 24, 23),
                Name = "LayoutPanel",
                Size = _container,
                ColumnCount = 2,
                RowCount = 6,
            };
            Icon = new()
            {
                Dock = DockStyle.Fill,
                Image = Properties.Resources.Label,
                ImageAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 0),
                Margin = new Padding(0),
                Name = "Icon",
                Size = new Size(32, 32),
                Text = null,
            };
            Nickname = new()
            {
                Checked = LabelWidget.Nickname,
                Dock = DockStyle.Fill,
                Location = new Point(38, 0),
                Margin = new Padding(6, 0, 0, 0),
                Name = "Nickname",
                Size = new Size(231, 32),
                Text = "Nickname instead of Full Name",
                UseVisualStyleBackColor = true,
            };
            Nickname.CheckedChanged += (sender, e) =>
            {
                LabelWidget.Nickname = ((CheckBox)sender).Checked;
                Instances.ActiveCanvas.Refresh();
                CustomNickname.Enabled = LabelWidget.Nickname;
            };
            CustomNickname = new()
            {
                Checked = LabelWidget.CustomNickname,
                Dock = DockStyle.Fill,
                Enabled = LabelWidget.Nickname,
                Location = new Point(38, 0),
                Margin = new Padding(6, 0, 0, 0),
                Name = "CustomNickname",
                Size = new Size(231, 20),
                Text = "User nickname instead of official nickname",
                UseVisualStyleBackColor = true,
            };
            CustomNickname.CheckedChanged += (sender, e) =>
            {
                LabelWidget.CustomNickname = ((CheckBox)sender).Checked;
                Instances.ActiveCanvas.Refresh();
            };
            ColorPicker = new()
            {
                AllowDrop = true,
                Colour = LabelWidget.Color,
                Location = new Point(0, 0),
                Margin = new Padding(6, 0, 6, 0),
                Name = "ColorPicker",
                Size = new Size(20, 20),
            };
            ColorPicker.ColourChanged += (sender, e) =>
            {
                LabelWidget.Color = ((GH_ColourSwatchControl)sender).Colour;
                Instances.ActiveCanvas.Refresh();
            };
            ColorLabel = new()
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 3, 6, 0),
                Name = "ColorLabel",
                Size = new Size(231, 20),
                Text = "Label Color",
            };
            FontPicker = new()
            {
                AllowFamilyOverride = true,
                AllowSizeOverride = true,
                AllowStyleOverride = true,
                AutoSize = false,
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Font = LabelWidget.Font,
                Location = new Point(0, 0),
                Margin = new Padding(6, 6, 6, 0),
                Name = "FontPicker",
                Size = new Size(514, 75),
            };
            FontPicker.FontChanged += (sender, e) =>
            {
                LabelWidget.Font = ((GH_FontControl)sender).Font;
                Instances.ActiveCanvas.Refresh();
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            };
            Exclude = new()
            {
                AcceptsReturn = false,
                AcceptsTab = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(6, 0, 6, 6),
                Multiline = true,
                Name = "Exclude",
                ScrollBars = ScrollBars.Vertical,
                Size = new Size(514, 36),
                Text = string.Join(",", LabelWidget.Exclude),
#if NET7_0_OR_GREATER
                PlaceholderText = "Comma separated list of component names to exclude from labeling",
#endif
            };
            Exclude.TextChanged += (sender, e) =>
            {
                string text = ((TextBox)sender).Text;
                LabelWidget.Exclude =
                    [
#if NET7_0_OR_GREATER
                        .. text.Split(',',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
#else
                        .. text.Split([','],StringSplitOptions.RemoveEmptyEntries).Select(s=>s.Trim())
#endif
                    ];
                Instances.ActiveCanvas.Refresh();
            };
            ButtonSelected = new()
            {
                Text = "Add selected components to exclusion",
                Height = 20,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 6),
                Padding = new Padding(0),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
            };
            ButtonSelected.Click += (s, e) =>
            {
                GH_Document doc = Instances.ActiveCanvas.Document;
                foreach (IGH_DocumentObject obj in doc.SelectedObjects())
                {
                    if (!LabelWidget.Exclude.Contains(obj.Name))
                    {
                        Exclude.Text += $",{obj.Name}";
                    }
                }
            };
            Exclude.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab)
                {
                    e.SuppressKeyPress = true;
                    e.Handled = true;
                }
            };


            LayoutPanel.SuspendLayout();
            SuspendLayout();

            LayoutPanel.ColumnStyles.Add(new ColumnStyle());
            LayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle());
            LayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            LayoutPanel.Controls.Add(Icon, 0, 0);
            LayoutPanel.Controls.Add(Nickname, 1, 0);
            LayoutPanel.Controls.Add(CustomNickname, 1, 1);
            LayoutPanel.Controls.Add(ColorPicker, 0, 2);
            LayoutPanel.Controls.Add(ColorLabel, 1, 2);
            LayoutPanel.Controls.Add(FontPicker, 0, 3);
            LayoutPanel.SetColumnSpan(FontPicker, 2);
            LayoutPanel.Controls.Add(Exclude, 0, 4);
            LayoutPanel.SetColumnSpan(Exclude, 2);
            LayoutPanel.Controls.Add(ButtonSelected, 1, 5);


            AutoScaleDimensions = new SizeF(6f, 13f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(LayoutPanel);
            Margin = new Padding(24, 23, 24, 23);
            Name = "GH_LabelWidgetFrontEnd";
            Size = _container;
            LayoutPanel.ResumeLayout(false);
            base.ResumeLayout(false);
        }
    }
}
