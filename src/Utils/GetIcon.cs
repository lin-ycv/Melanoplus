namespace Melanoplus.Utils
{
    public class GetIcon
    {
        public static void Save(GH_Document document)
        {
            if (document == null || document.SelectedCount == 0) return;
            try
            {
                var selected = document.SelectedObjects();
                var path = document.FilePath == null ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop) : "";
                foreach (var obj in selected)
                {
                    if (obj is GH_DocumentObject comp)
                    {
                        comp.Icon_24x24.Save(Path.Combine(path, comp.Name + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                        RhinoApp.WriteLine("Saved {0}.png to {1}", comp.Name, path == "" ? Directory.GetCurrentDirectory() : path);
                    }
                }
            }
            catch (Exception er)
            {
                RhinoApp.WriteLine(er.Message);
            }
        }

    }
}
