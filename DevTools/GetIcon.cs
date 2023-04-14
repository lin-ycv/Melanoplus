using Eto.Drawing;
using Eto.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Rhino;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus.DevTools
{
    public class GetIcon
    {
        public static void Save(GH_Document document)
        {
            if (document == null || document.SelectedCount == 0) return;
            try
            {
                var selected = document.SelectedObjects();
                var path = document.FilePath == null ? Environment.GetFolderPath(Environment.SpecialFolder.Desktop): "";
                foreach (var obj in selected)
                {
                    if (obj is GH_DocumentObject comp)
                    {
                        comp.Icon_24x24.Save(Path.Combine(path, comp.NickName + ".png"), System.Drawing.Imaging.ImageFormat.Png);
                        RhinoApp.WriteLine("Saved {0}.png to {1}", comp.NickName, path==""? Directory.GetCurrentDirectory():path);
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
