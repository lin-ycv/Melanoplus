namespace Melanoplus
{
    public class AssemblyPriority : GH_AssemblyPriority
    {
        public AssemblyPriority() { }

        public override GH_LoadingInstruction PriorityLoad()
        {
            try
            {
                GH_Canvas.WidgetListCreated += AddWidget;
                Instances.CanvasCreated += CanvasCreated;
                GH_DocumentEditor.AggregateShortcutMenuItems += Shorcuts;

                var server = Instances.ComponentServer;
                server.AddCategoryShortName("Melanoplus", "Plus");
                server.AddCategorySymbolName("Melanoplus", '+');
                server.AddCategoryIcon("Melanoplus", Properties.Resources.MelanoplusSimple16);
            }
            catch (Exception e)
            {
                RhinoApp.WriteLine($"Melanoplus Error: {e.Message}");
            }
            return GH_LoadingInstruction.Proceed;
        }

        private void AddWidget(object sender, GH_CanvasWidgetListEventArgs e)
        {
            e.AddWidget(new Widgets.LabelWidget());
        }

        private void CanvasCreated(GH_Canvas canvas)
        {
            Instances.DocumentServer.DocumentAdded += Utils.AutoLoad.Handler;
            GUI.GhMenu menu = new();
            menu.AddMenu();
        }

        private void Shorcuts(object sender, GH_MenuShortcutEventArgs e)
        {
            GUI.GhMenu.MenuEntryAllowShortcut.ForEach(e.AppendItem);
        }
    }
}
