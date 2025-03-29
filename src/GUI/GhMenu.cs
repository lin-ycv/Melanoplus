using Melanoplus.Utils;

namespace Melanoplus.GUI
{
    internal class GhMenu
    {
        internal static readonly List<ToolStripMenuItem> MenuEntryAllowShortcut = [];
        private readonly ToolStripMenuItem _menu = new()
        {
            Name = "Melanoplus",
            ToolTipText = "Melanoplus",
            Text = "+",
        };
        internal void AddMenu()
        {
            GH_DocumentEditor editor = Instances.DocumentEditor;
            if (editor == null)
            {
                RhinoApp.WriteLine("Melanoplus Error: Document editor is null");
                return;
            }
            MenuStrip menuStrip = editor.MainMenuStrip;
            menuStrip.SuspendLayout();
            BuildMainMenu();
            menuStrip.Items.Add(_menu);
            menuStrip.ResumeLayout(false);
        }
        private void BuildMainMenu()
        {
            _menu.DropDownItems.Clear();
            CopyWires copy = new();
            BreakWires breakW = new();
            ToolStripMenuItem[] menuOptions =
                [
                    new("Copy Wires",
                        Properties.Resources.CopyWire,
                        (sender, e) => copy.Enabled = ((ToolStripMenuItem)sender).Checked,
                        "mnuCopyWires")
                    {
                        ToolTipText = "1. Start a rewire event using Ctrl+Shift+LMB\r\n2. Drag mouse to destination input/output\r\n3. Instead of letting go of LMB at the destination, click Alt copy the wires\r\n4. Repeat 2, 3 to copy to multiple destinations\r\n*Move cursor away from input/output before letting go of LMB\r\n\r\nCtrl+Shift+LMB[+Alt]",
                        CheckOnClick = true,
                        Checked = copy.Enabled,
                    },
                    new("Break Wires",
                        Properties.Resources.BreakWire,
                        (sender, e) => breakW.Enabled = ((ToolStripMenuItem)sender).Checked,
                        "mnuBreakWires")
                    {
                        ToolTipText = "Hold CTRL+SHIFT and drag LMB to break wires that are selected",
                        CheckOnClick = true,
                        Checked = breakW.Enabled,
                    },
                    new("Clean Canvas",
                        Properties.Resources.CleanCanvas,
                        (sender, e) => CleanCanvas.Clean(Instances.ActiveCanvas.Document),
                        "mnuCleanCanvas")
                    {
                        ShortcutKeys = (Keys.Control | Keys.Shift) | Keys.C,
                        ToolTipText = "Removes placeholder components and/or transparent groups",
                    },
                    new("Rename Group",
                        null,
                        (sender, e) => GroupName.Rename(),
                        "mnuRenameGroup")
                    {
                        ShortcutKeys = (Keys.Control | Keys.Shift) | Keys.R,
                        ToolTipText = "Rename selected GH_Group(s)",
                    },
                    new("Create Snippet",
                        null,
                        (sender, e) => Snippet.Save(Instances.ActiveCanvas.Document),
                        "mnuCreateSnippet")
                    {
                        ToolTipText = "Create a snippet (multi-component user object) from selected components",
                    },
                    new("Unlock",
                        Properties.Resources.Unlock,
                        (sender, e) => Cluster.Un(Instances.ActiveCanvas.Document),
                        "mnuUnlock")
                    {
                        ShortcutKeys = Keys.Control | Keys.U,
                        //Visible = false,
                    },
                    new("Extract Icon",
                        null,
                        (sender, e) => GetIcon.Save(Instances.ActiveCanvas.Document),
                        "mnuIcon")
                    {
                        ToolTipText = "Extract the icon of selected component",
                    },
                ];
            _menu.DropDownItems.AddRange(menuOptions);
            int index = _menu.DropDownItems.IndexOfKey("mnuCreateSnippet");
            _menu.DropDownItems.Insert(index, new ToolStripSeparator());
            MenuEntryAllowShortcut.AddRange(menuOptions);
            ToolStripMenuItem presets = new("Group Presets", null, null, "mnuGroupPresets") { };
            presets.DropDownItems.AddRange(BuildGroupPresets());
            presets.DropDownItems.Insert(1, new ToolStripSeparator());
            _menu.DropDownItems.Insert(index, presets);
            index = _menu.DropDownItems.IndexOfKey("mnuCleanCanvas");
            _menu.DropDownItems.Insert(index, new ToolStripSeparator());
            //_menu.DropDownOpening += (s, e) => _menu.DropDownItems["mnuUnlock"].Visible = Instances.ActiveCanvas.IsDocument && Instances.ActiveCanvas.Document.SelectedObjects().Any(o => o is GH_Cluster);
        }
        private static ToolStripItem[] BuildGroupPresets()
        {
            ToolStripMenuItem[] options = new ToolStripMenuItem[11];
            options[0] = new("Edit Presets", null, (s, e) => GroupPresets.Config());
            for (int i = 0; i < GroupPresets.Colours.Length; i++)
            {
                ToolStripMenuItem preset = new(
                    $"Group {i}",
                    null,
                    (s, e) => GroupPresets.Run((int)(s as ToolStripMenuItem).Tag),
                    $"mnuGPreset{i}")
                {
                    Tag = i,
                    ToolTipText = $"Style: {(GH_GroupBorder)GroupPresets.Style[i]}\r\nColour: {GroupPresets.Colours[i]}\r\nName: {GroupPresets.Name[i]} \r\n\r\nShortcut for preset can be set in Grasshopper Preferences",
                };
                options[i + 1] = preset;
                MenuEntryAllowShortcut.Add(preset);
            }
            return options;
        }
    }
}
