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
        private static List<ToolStripMenuItem> MenuEntryAllowShortcut = new List<ToolStripMenuItem>() { };
        public override GH_LoadingInstruction PriorityLoad()
        {
            GH_Canvas.WidgetListCreated += AddWidget;
            Instances.CanvasCreated += LoadQuickButtons;
            Instances.CanvasCreated += LoadMenuOptions;
            GH_DocumentEditor.AggregateShortcutMenuItems += AggregateShortcutMenuItems;
            /*var server = Instances.ComponentServer;
            server.AddCategoryShortName("Melanoplus", "Plus");
            server.AddCategorySymbolName("Melanoplus", '➕');
            server.AddCategoryIcon("Melanoplus", Properties.Resources.MelanoplusSimple16);*/
            return GH_LoadingInstruction.Proceed;
        }

        private void AddWidget(object s, GH_CanvasWidgetListEventArgs e)
        {
            GH_Canvas.WidgetListCreated -= AddWidget;
            var c = (GH_Canvas)s;            
            e.AddWidget(new LabelWidget());
            LabelWidget.CanvasCreated(c);
            e.AddWidget(new WiresWidget());
            WiresWidget.CanvasCreated(c);
        }

        private void LoadQuickButtons(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= LoadQuickButtons;
            ToolStripItemCollection items = ((ToolStrip)(Instances.DocumentEditor).Controls[0].Controls[1]).Items;
            items.Add(new ToolStripButton("Create Snippet", Properties.Resources.SnippetBuilder, (s, e) => Snippet.Save(canvas.Document))
            {
                Name = "Melanoplus_Snippet",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
            });
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
                var cleancanvas = new ToolStripMenuItem("Clean Canvas", Properties.Resources.CleanCanvas, (s, e) => CleanCanvas.Clean(canvas.Document), "mnuCleanCanvas");
                mnu.DropDownItems.Insert(index, cleancanvas);
                MenuEntryAllowShortcut.Add(cleancanvas);


                mnu = (ToolStripMenuItem)editor.MainMenuStrip.Items["mnuEdit"];
                index = mnu.DropDownItems.IndexOfKey("mnuClusterSelection") + 1;
                var cluster = new ToolStripMenuItem("Unlock", Properties.Resources.unlock, (s, e) => Cluster.Un(canvas.Document), "mnuUnlockCluster") { ShortcutKeys = System.Windows.Forms.Keys.U | System.Windows.Forms.Keys.Control };
                mnu.DropDownItems.Insert(index, cluster);
                MenuEntryAllowShortcut.Add(cluster);

                editor.ResumeLayout();
                mnu.DropDownOpening += (s, e) => mnu.DropDownItems["mnuUnlockCluster"].Visible = canvas.IsDocument && canvas.Document.SelectedObjects().Any(o => o is GH_Cluster);
            }
        }
        private static void AggregateShortcutMenuItems(object sender, GH_MenuShortcutEventArgs e)
        {
            GH_DocumentEditor.AggregateShortcutMenuItems -= AggregateShortcutMenuItems;
            MenuEntryAllowShortcut.ForEach(e.AppendItem);
        }
    }
}
