using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Melanoplus
{
    public class Cluster
    {
        internal static void Un(GH_Document document)
        {
            foreach (var c in document.SelectedObjects().Where(c=>c is GH_Cluster))
            {
                    typeof(GH_Cluster).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[0].SetValue(c, new byte[0]);
            }

        }
    }
}
