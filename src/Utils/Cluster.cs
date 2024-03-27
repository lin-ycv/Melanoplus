namespace Melanoplus.Utils
{
    public class Cluster
    {
        internal static void Un(GH_Document document)
        {
            if (document == null) return;
            foreach (var c in document.SelectedObjects().Where(c => c is GH_Cluster))
            {
                typeof(GH_Cluster).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)[0].SetValue(c, Array.Empty<byte>());
            }
        }
    }
}
