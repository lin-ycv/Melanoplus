using Grasshopper;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus
{
    public class AssemblyPriority : GH_AssemblyPriority
    {
        public override GH_LoadingInstruction PriorityLoad()
        {
            GH_Canvas.WidgetListCreated += new GH_Canvas.WidgetListCreatedEventHandler(AddWidget);
            Instances.CanvasCreated += LoadQuickButtons;
            Instances.CanvasCreated += LoadMenuOptions;
            var server = Instances.ComponentServer;
            server.AddCategoryShortName("Melanoplus", "Plus");
            server.AddCategorySymbolName("Melanoplus", '➕');
            server.AddCategoryIcon("Melanoplus", Properties.Resources.MelanoplusSimple16);
            return GH_LoadingInstruction.Proceed;
        }

        private void AddWidget(object s, GH_CanvasWidgetListEventArgs e)
        {
            LabelWidget labelWidget = new LabelWidget();
            e.AddWidget(labelWidget);
        }

        private void LoadQuickButtons(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= LoadQuickButtons;
            var editor = Instances.DocumentEditor;
            //editor.SuspendLayout();
            ((ToolStrip)editor.Controls[0].Controls[1]).Items.AddRange(QuickButtons);
            //editor.ResumeLayout();
        }
        private ToolStripItem[] QuickButtons
        {
            get {
                List<ToolStripItem> buttons = new List<ToolStripItem>(){
                    new ToolStripButton("Create Snippet", Properties.Resources.SnippetBuilder, (s,e) => Snippet.Save(Instances.ActiveCanvas.Document)){
                        Name = "Melanoplus_Snippet",
                        DisplayStyle = ToolStripItemDisplayStyle.Image,
                    },
                    //move cleancanvas to solution menu
                    new ToolStripButton("Clean Canvas", Properties.Resources.CleanCanvas, (s,e) => CleanCanvas.Clean(Instances.ActiveCanvas.Document)){
                        Name = "Melanoplus_CleanCanvas",
                        DisplayStyle = ToolStripItemDisplayStyle.Image,
                    },
                };
                return buttons.ToArray();
            }
        }

        private void LoadMenuOptions(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= LoadMenuOptions;
            var editor = Instances.DocumentEditor;
            if(editor != null)
            {
                var mnuEdit = ((ToolStripMenuItem)editor.MainMenuStrip.Items["mnuEdit"]);
                mnuEdit.MouseHover += (s, e) => mnuEdit.DropDownItems["mnuUnlockCluster"].Visible = (Instances.ActiveCanvas.Document!= null) && (Instances.ActiveCanvas.Document.SelectedCount > 0);
                var index = mnuEdit.DropDownItems.IndexOfKey("mnuClusterSelection")+1;
                mnuEdit.DropDownItems.Insert(index, new ToolStripMenuItem("Unlock", Properties.Resources.unlock, (s, e) => Cluster.Un(Instances.ActiveCanvas.Document), "mnuUnlockCluster"){ Visible = false });
            }
        }
    }
}
