namespace Melanoplus.GUI
{
    public class AutoLoadSettings : IGH_SettingFrontend
    {
        public string Category => "Files";
        public string Name => "AutoLoad (Melanoplus)";
        public IEnumerable<string> Keywords =>
        [
            "AutoLoad", "3dm", "File",
        ];

        public Control SettingsUI()
        {
            return new GH_AutoLoadSettings();
        }
    }

    internal class GH_AutoLoadSettings : UserControl
    {
        readonly ToolTip _toolTip = new();
        internal CheckBox Enable;
        public GH_AutoLoadSettings()
        {
            Enable = new()
            {
                Checked = Utils.AutoLoad.Enabled,
                Dock = DockStyle.Fill,
                Location = new Point(0, 0),
                Margin = new Padding(24, 23, 24, 23),
                Name = "Enable",
                Size = new Size(231, 32),
                Text = "Enable AutoLoad",
                UseVisualStyleBackColor = true,
            };
            Enable.CheckedChanged += (s, e) =>
            {
                Utils.AutoLoad.Enabled = ((CheckBox)s).Checked;
                var settings = new GH_SettingsServer("Melanoplus", true);
                settings.SetValue("AutoLoad", Utils.AutoLoad.Enabled);
                settings.WritePersistentSettings();
            };
            _toolTip.SetToolTip(Enable, "Autoload respective 3dm file if it exists in the same folder as the gh file");
            SuspendLayout();
            AutoScaleDimensions = new SizeF(100f, 100f);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(Enable);
            Margin = new Padding(24, 23, 24, 23);
            Name = "GH_AutoLoadFrontEnd";
            base.ResumeLayout(false);
        }
    }
}
