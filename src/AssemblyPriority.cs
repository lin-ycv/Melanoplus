namespace Melanoplus
{
    public class AssemblyPriority : GH_AssemblyPriority
    {
        public AssemblyPriority() { }

        public override GH_LoadingInstruction PriorityLoad()
        {
            try
            {
                GH_Canvas.WidgetListCreated += new GH_Canvas.WidgetListCreatedEventHandler(AddWidget);
                Instances.CanvasCreated += AddMenu;
                GH_DocumentEditor.AggregateShortcutMenuItems += new GH_DocumentEditor.AggregateShortcutMenuItemsEventHandler(Shorcuts);

                var server = Instances.ComponentServer;
                server.AddCategoryShortName("Melanoplus", "Plus");
                server.AddCategorySymbolName("Melanoplus", '➕');
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
            Widgets.LabelWidget label = new();
            e.AddWidget(label);
        }

        private void AddMenu(GH_Canvas canvas)
        {
            GUI.GhMenu menu = new();
            menu.AddMenu();
        }

        private void Shorcuts(object sender, GH_MenuShortcutEventArgs e)
        {
            GUI.GhMenu.MenuEntryAllowShortcut.ForEach(e.AppendItem);
        }
    }
}
