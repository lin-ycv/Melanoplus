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
            Grasshopper.Instances.CanvasCreated += LoadQuickButtons;
            var server = Grasshopper.Instances.ComponentServer;
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
            Grasshopper.Instances.CanvasCreated -= LoadQuickButtons;
            var editor = Grasshopper.Instances.DocumentEditor;
            //editor.SuspendLayout();
            ((ToolStrip)editor.Controls[0].Controls[1]).Items.AddRange(QuickButtons);
            //editor.ResumeLayout();
        }
        private ToolStripItem[] QuickButtons
        {
            get {
                List<ToolStripItem> buttons = new List<ToolStripItem>(){
                    new ToolStripButton("Create Snippet", Properties.Resources.SnippetBuilder, SaveSnippetClick){
                        Name = "Melanoplus_Snippet",
                        DisplayStyle = ToolStripItemDisplayStyle.Image,
                    },
                    new ToolStripButton("Clean Canvas", Properties.Resources.CleanCanvas, CleanCanvasClick){
                        Name = "Melanoplus_CleanCanvas",
                        DisplayStyle = ToolStripItemDisplayStyle.Image,
                    },
                };
                return buttons.ToArray();
            }
        }
        private void SaveSnippetClick(object s, EventArgs e)
        {
            Snippet.Save(Grasshopper.Instances.ActiveCanvas.Document);
        }
        private void CleanCanvasClick(object s, EventArgs e)
        {
            CleanCanvas.Clean(Grasshopper.Instances.ActiveCanvas.Document);
        }
    }
}
