using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
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
            ((ToolStrip)editor.Controls[0].Controls[1]).Items.AddRange(QuickButtons);
        }
        private ToolStripItem[] QuickButtons
        {
            get {
                List<ToolStripItem> buttons = new List<ToolStripItem>(){
                    new ToolStripButton("Create Snippet", Properties.Resources.SnippetBuilder, (s,e) => Snippet.Save(Instances.ActiveCanvas.Document)){
                        Name = "Melanoplus_Snippet",
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
                editor.SuspendLayout();
                
                var mnu = (ToolStripMenuItem)editor.MainMenuStrip.Items["mnuDisplay"];
                var index = mnu.DropDownItems.IndexOfKey("mnuFullNames") + 1;
                mnu.DropDownItems.Insert(index, new ToolStripMenuItem("Clean Canvas", Properties.Resources.CleanCanvas, (s, e) => CleanCanvas.Clean(canvas.Document), "mnuCleanCanvas") { ShortcutKeys = Keys.None });

                mnu = (ToolStripMenuItem)editor.MainMenuStrip.Items["mnuEdit"];
                index = mnu.DropDownItems.IndexOfKey("mnuClusterSelection") + 1;
                mnu.DropDownItems.Insert(index, new ToolStripMenuItem("Unlock", Properties.Resources.unlock, (s, e) => Cluster.Un(canvas.Document), "mnuUnlockCluster") { Visible = true, Enabled = true, ShortcutKeys = System.Windows.Forms.Keys.U | System.Windows.Forms.Keys.Control });
                
                editor.ResumeLayout();
                mnu.DropDownOpening += (s, e) => mnu.DropDownItems["mnuUnlockCluster"].Visible = canvas.IsDocument && canvas.Document.SelectedObjects().Any(o => o is GH_Cluster);
            }
        }
    }
}
