namespace Melanoplus.Utils
{
    public class GroupPresets
    {
        internal static readonly ED.Color[] Colours = LoadColours();
        internal static int[] Style;
        internal static string[] Name;

        private static ED.Color[] LoadColours()
        {
            GH_SettingsServer settings = new("Melanoplus", true);
            ED.Color ec = ED.Colors.Silver;
            ED.Color[] defaults = [ec, ec, ec, ec, ec, ec, ec, ec, ec, ec];
            string values = settings.GetValue("GroupPresets.Colours", null);

            if (values != null)
                defaults = [.. values.Split(',').Select(s => { _ = ED.Color.TryParse(s, out ED.Color c); return c; })];

            string styles = settings.GetValue("GroupPresets.Styles", null);
            if (styles == null)
            {
                Style = new int[defaults.Length];
                for (int i = 0; i < Style.Length; i++)
                    Style[i] = 1;
            }
            else
                Style = [.. styles.Split(',').Select(s => int.Parse(s))];

            string names = settings.GetValue("GroupPresets.names", null);
            if (names == null)
            {
                Name = new string[defaults.Length];
                for (int i = 0; i < Name.Length; i++)
                    Name[i] = string.Empty;
            }
            else
                Name = [.. names.Split(',').Select(n => n ?? string.Empty)];

            return defaults;
        }

        public static void Config()
        {
            GH_SettingsServer settings = new("Melanoplus", true);
            EF.FloatingForm form = new()
            {
                ShowInTaskbar = true,
                Owner = Instances.EtoDocumentEditor,
                Title = "Configure Group Presets",
                MinimumSize = new(400, 290),
                Topmost = true,
            };
            form.Closed += (s, e) =>
            {
                if (Colours.Count(c => c == ED.Color.FromArgb(255, 255, 255, 255)) != 10)
                {
                    settings.SetValue("GroupPresets.Colours", string.Join(",", Colours.Select(c => c.ToString())));
                    settings.SetValue("GroupPresets.Names", string.Join(",", Name.Select(n => n.ToString())));
                    settings.SetValue("GroupPresets.Styles", string.Join(",", Style.Select(s => s.ToString())));
                }
                settings.WritePersistentSettings();
            };
            Reflection.R8.UseRhinoStyle(form);

            EF.DynamicLayout layout = new();

            for (int i = 0; i < Colours.Length; i++)
            {
                EF.TextBox textbox = new()
                {
                    Text = Name[i],
                    ToolTip = "Set default name for group " + i,
                    ID = i.ToString(),
                };
                textbox.TextChanged += (s, e) =>
                {
                    Name[int.Parse(textbox.ID)] = textbox.Text;
                };
                EF.Button button = new()
                {
                    Text = "Group " + i,
                    TextColor = GetContrastColor(Colours[i]),
                    ToolTip = "Change the colour of group " + i,
                    BackgroundColor = Colours[i],
                    ID = i.ToString(),
                };
                button.MouseUp += (s, e) =>
                {
                    EF.ColorDialog picker = new()
                    {
                        AllowAlpha = true,
                        Color = Colours[int.Parse(button.ID)],
                    };
                    form.Topmost = false;
                    if (picker.ShowDialog(layout) == EF.DialogResult.Ok)
                    {
                        int id = int.Parse(button.ID);
                        Colours[id] = picker.Color;
                        if (picker.Color.A == 0)
                            Colours[id].A = 1f;
                        button.BackgroundColor = Colours[id];
                        button.TextColor = GetContrastColor(Colours[id]);
                    }
                    form.Topmost = true;
                };
                EF.DropDown drop = new()
                {
                    DataStore = Enum.GetNames(typeof(GH_GroupBorder)),
                    SelectedIndex = Style[i],
                    ID = i.ToString(),
                };
                drop.SelectedIndexChanged += (s, e) =>
                {
                    Style[int.Parse(drop.ID)] = drop.SelectedIndex;
                };
                layout.AddRow(drop, button, textbox);
            }
            EF.Button reset = new()
            {
                Text = "Reset",
            };
            reset.Click += (s, e) =>
            {
                settings.DeleteValue("GroupPresets.Colours");
                settings.DeleteValue("GroupPresets.Names");
                settings.DeleteValue("GroupPresets.Styles");
                for (int i = 0; i < Colours.Length; i++)
                    Colours[i] = ED.Color.FromArgb(255, 255, 255, 255);
                form.Close();
                Config();
            };
            layout.Add(reset);

            form.Content = layout;
            form.Show();

            static ED.Color GetContrastColor(ED.Color backgroundColor)
            {
                double luminance = (0.299 * backgroundColor.R + 0.587 * backgroundColor.G + 0.114 * backgroundColor.B);
                return luminance > 0.4 ? ED.Colors.Black : ED.Colors.White;
            }
        }

        public static void Run(int i)
        {
            GH_Document ghdoc = Instances.ActiveCanvas.Document;
            if (ghdoc == null || ghdoc.SelectedCount == 0)
                return;

            GH_Group grp = new()
            {
                NickName = Name[i],
                Border = (GH_GroupBorder)Style[i],
                Colour = Colours[i].ToSD(),
            };
            foreach (var obj in ghdoc.SelectedObjects())
                if (obj is not GH_Group)
                    grp.AddObject(obj.InstanceGuid);
            ghdoc.AddObject(grp, true);
        }
    }
}
