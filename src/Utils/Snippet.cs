﻿namespace Melanoplus.Utils
{
    public class Snippet
    {
        readonly static string scriptSnip = "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";
        internal static void Save(GH_Document document)
        {
            if (document == null || document.SelectedObjects().Count == 0) return;
            try
            {
                Guid[] objIDs = document.SelectedObjects().Select(o => o.InstanceGuid).ToArray();
                if (objIDs.Length == 0) return;
                GH_DocumentIO docIO = new(document);
                docIO.Copy(GH_ClipboardType.System, objIDs);
                string binary = BinaryFromSnip(Clipboard.GetText());
                docIO.ClearClipboard(GH_ClipboardType.System);
                Clipboard.SetText(XMLFromSnip(scriptSnip).Replace("REPLACETHISWITHSNIP", binary));
                Paste(document);
            }
            catch (Exception er)
            {
                RhinoApp.WriteLine(er.Message);
            }
        }
        private static string BinaryFromSnip(string snip)
        {
            GH_Archive archive = new();
            archive.Deserialize_Xml(snip);
            return Convert.ToBase64String(archive.Serialize_Binary());
        }
        private static string XMLFromSnip(string snip)
        {
            GH_Archive archive = new();
            archive.Deserialize_Binary(Convert.FromBase64String(snip));
            string XML = archive.Serialize_Xml();
            return XML;
        }
        private static void Paste(GH_Document GrasshopperDocument)
        {

            // set up a GH documentIO object
            GH_DocumentIO documentIO = new();
            // paste into the virtual documentIO
            documentIO.Paste(GH_ClipboardType.System);

            float smallestX = float.MaxValue;
            float smallestY = float.MaxValue;
            origin(documentIO.Document.Objects, ref smallestX, ref smallestY);

            float selectedX = float.MaxValue;
            float selectedY = float.MaxValue;
            origin(GrasshopperDocument.SelectedObjects(), ref selectedX, ref selectedY);

            static void origin(IList<IGH_DocumentObject> objects, ref float x, ref float y)
            {
                foreach (IGH_DocumentObject obj in objects)
                {
                    var pivot = obj.Attributes.Pivot;
                    if (pivot.X < x)
                    {
                        x = pivot.X;
                        y = pivot.Y;
                    }
                }
            }

            documentIO.Document.TranslateObjects(new Size((int)(selectedX - smallestX), (int)(selectedY - smallestY)), false);

            // clean up
            documentIO.Document.SelectAll();
            documentIO.Document.ExpireSolution();
            documentIO.Document.MutateAllIds();
            IEnumerable<IGH_DocumentObject> objs = documentIO.Document.Objects;
            GrasshopperDocument.DeselectAll();
            GrasshopperDocument.MergeDocument(documentIO.Document);
            GrasshopperDocument.UndoUtil.RecordAddObjectEvent("Paste", objs);
            GrasshopperDocument.ScheduleSolution(10);
            List<IGH_DocumentObject> list = GrasshopperDocument.SelectedObjects();
            if (list.Count == 1)
            {
                GrasshopperDocument.Enabled = false;
                ((GH_Component)list[0]).Locked = false;
                GH_ComponentServer.NewUserObject(list[0]);
                GrasshopperDocument.RemoveObject(list[0], true);
                GrasshopperDocument.Enabled = true;
            }
        }
    }
}

// REF: https://www.grasshopper3d.com/profiles/blogs/create-snippets-with-grasshopper