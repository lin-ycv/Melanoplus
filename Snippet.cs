using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Special;
using Rhino;

namespace Melanoplus
{
    public class Snippet : GH_ButtonObject
    {
        public override string Name => "Component Snippet";
        public override string NickName => "Snippet";
        public override string Description => "Save selected components as a user object";
        public override string Category => "Melanoplus";
        public override string SubCategory => "Tools";
        protected override Bitmap Icon => Properties.Resources.SnippetBuilder;
        public override GH_Exposure Exposure => GH_Exposure.hidden;
        public override Guid ComponentGuid => new Guid("{D839EB25-E45A-4FC8-8E5F-1B1C6C80807C}");

        public Snippet():base(){}
        public override void CreateAttributes()
        {
            m_attributes = new SnippetAttributes(this);
        }

        internal static void Save(GH_Document document)
        {
            if (document == null) return;
            try
            {
                var objIDs = document.SelectedObjects().Select(o => o.InstanceGuid).ToArray();
                if (objIDs.Length == 0) return;
                GH_DocumentIO docIO = new GH_DocumentIO(document);
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
            GH_Archive archive = new GH_Archive();
            archive.Deserialize_Xml(snip);
            return Convert.ToBase64String(archive.Serialize_Binary());
        }
        private static string XMLFromSnip(string snip)
        {
            GH_Archive archive = new GH_Archive();
            archive.Deserialize_Binary(Convert.FromBase64String(snip));
            string XML = archive.Serialize_Xml();
            return XML;
        }
        private static void Paste(GH_Document GrasshopperDocument)
        {

            //set up a GH documentIO object
            GH_DocumentIO documentIO = new GH_DocumentIO();
            //paste into the virtual documentIO
            documentIO.Paste(GH_ClipboardType.System);

            float smallestX = float.MaxValue;
            float smallestY = float.MaxValue;
            origin(documentIO.Document.Objects, ref smallestX, ref smallestY);

            float selectedX = float.MaxValue;
            float selectedY = float.MaxValue;
            origin(GrasshopperDocument.SelectedObjects(), ref selectedX, ref selectedY);

            void origin(IList<IGH_DocumentObject> objects,ref float x, ref float y)
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

            //clean up
            documentIO.Document.SelectAll();
            documentIO.Document.ExpireSolution();
            documentIO.Document.MutateAllIds();
            IEnumerable<IGH_DocumentObject> objs = documentIO.Document.Objects;
            GrasshopperDocument.DeselectAll();
            GrasshopperDocument.MergeDocument(documentIO.Document);
            GrasshopperDocument.UndoUtil.RecordAddObjectEvent("Paste", objs);
            GrasshopperDocument.ScheduleSolution(10);
        }

        readonly static string scriptSnip = "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";

        public class SnippetAttributes : GH_Attributes<Snippet>
        {
            public SnippetAttributes(Snippet owner) : base(owner)
            {
            }

            public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
            {
                Save(sender.Document);
                return base.RespondToMouseDown(sender, e);
            }
        }
    }
}
