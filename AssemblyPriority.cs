using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
            e.AddWidget(new WiresWidget());
            WiresWidget.CanvasCreated(c);
            e.AddWidget(new ViewportWidget());
            ViewportWidget.CanvasCreated(c);
            e.AddWidget(new LabelWidget());
            LabelWidget.CanvasCreated(c);
        }

        private void LoadQuickButtons(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= LoadQuickButtons;
            ToolStripItemCollection items = ((ToolStrip)(Instances.DocumentEditor).Controls[0].Controls[1]).Items;
            ((ToolStrip)(Instances.DocumentEditor).Controls[0].Controls[1]).SuspendLayout();
            string base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAACXBIWXMAAAsTAAALEwEAmpwYAAAE7mlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNy4xLWMwMDAgNzkuZWRhMmIzZiwgMjAyMS8xMS8xNC0xMjozMDo0MiAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczpkYz0iaHR0cDovL3B1cmwub3JnL2RjL2VsZW1lbnRzLzEuMS8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdEV2dD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlRXZlbnQjIiB4bWxuczpwaG90b3Nob3A9Imh0dHA6Ly9ucy5hZG9iZS5jb20vcGhvdG9zaG9wLzEuMC8iIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIDIzLjEgKFdpbmRvd3MpIiB4bXA6Q3JlYXRlRGF0ZT0iMjAyMi0wOC0yMFQxNToxOTo0NiswODowMCIgeG1wOk1ldGFkYXRhRGF0ZT0iMjAyMi0wOC0yMFQxNToxOTo0NiswODowMCIgeG1wOk1vZGlmeURhdGU9IjIwMjItMDgtMjBUMTU6MTk6NDYrMDg6MDAiIGRjOmZvcm1hdD0iaW1hZ2UvcG5nIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOmQxODNhYjZhLTEwZTYtOWU0ZC1hNGRjLTJkMWViOGEyMjgzMyIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDpkMTgzYWI2YS0xMGU2LTll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";
            Image image = (Image)new ImageConverter().ConvertFrom(Convert.FromBase64String(base64));
            items.Add(new ToolStripButton("Create Snippet", image/*Properties.Resources.SnippetBuilder*/, (s, e) => Snippet.Save(canvas.Document))
            {
                Name = "Melanoplus_Snippet",
                DisplayStyle = ToolStripItemDisplayStyle.Image,
            });
            ((ToolStrip)(Instances.DocumentEditor).Controls[0].Controls[1]).ResumeLayout();
        }

        private void LoadMenuOptions(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= LoadMenuOptions;
            var editor = Instances.DocumentEditor;
            if (editor != null)
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
