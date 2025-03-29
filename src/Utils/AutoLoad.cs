namespace Melanoplus.Utils
{
    internal static class AutoLoad
    {
        internal static bool Enabled = new GH_SettingsServer("Melanoplus", true).GetValue("AutoLoad", false);

        internal static void Handler(GH_DocumentServer sender, GH_Document doc)
        {
            if (Enabled && doc.IsFilePathDefined)
            {
                string path = Path.GetDirectoryName(doc.FilePath),
                    name = Path.GetFileNameWithoutExtension(doc.FilePath),
                    rhino = Path.Combine(path, $"{name}.3dm");
                if (!File.Exists(rhino)) 
                    return;
                if (RhinoDoc.ActiveDoc.Path == rhino
                    || (RhinoDoc.ActiveDoc.Name != null
                        && EF.DialogResult.No == EF.MessageBox.Show("A 3DM file is already open, save and close this file to load the new one?", "AutoLoad - Melanoplus", EF.MessageBoxButtons.YesNo, EF.MessageBoxType.Question)))
                    return;
                RhinoDoc.Open(rhino, out _);
            }
        }
    }
}
