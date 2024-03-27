namespace Melanoplus.Utils
{
    public class CleanCanvas
    {
        internal static void Clean(GH_Document document)
        {
            if (document == null || document.ObjectCount == 0) return;
            try
            {
                IEnumerable<IGH_DocumentObject> objs = document.Objects.Where(o => o.ToString() == "Grasshopper.Kernel.Components.GH_PlaceholderComponent");
                IEnumerable<IGH_DocumentObject> group = document.Objects.Where(o => o is GH_Group g && g.Colour.A == 0);

                if (objs.Any())
                {
                    EF.DialogResult answer = EF.MessageBox.Show("Remove Placholder components?\r\nThis is a non-reversible process.", "Clean Canvs", EF.MessageBoxButtons.YesNoCancel, EF.MessageBoxType.Question);
                    if (answer == EF.DialogResult.No)
                        objs = Enumerable.Empty<IGH_DocumentObject>();
                    else if (answer == EF.DialogResult.Cancel)
                        return;
                }

                if (group.Any() &&
                    EF.MessageBox.Show("Remove transparent groups?", "Clean Canvs", EF.MessageBoxButtons.YesNo, EF.MessageBoxType.Question, EF.MessageBoxDefaultButton.Yes) == EF.DialogResult.Yes)
                {
                    objs = objs.Concat(group);
                    document.UndoUtil.RecordRemoveObjectEvent("Clean transparent groups", group);
                }

                document.RemoveObjects(objs.ToList(), false);
                Instances.ActiveCanvas.Refresh();
            }
            catch (Exception er)
            {
                RhinoApp.WriteLine(er.Message);
            }
        }

    }
}
