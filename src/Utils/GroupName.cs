namespace Melanoplus.Utils
{
    public class GroupName
    {
        internal static void Rename()
        {
            GH_Document document = Instances.ActiveCanvas.Document;
            if(document == null) return;
            var selected = document.SelectedObjects();
            var groups = selected.Where(o => o is GH_Group);
            if (!selected.Any() || !groups.Any())
            {
                EF.MessageBox.Show("No GH_Group Selected", "Rename Group", EF.MessageBoxButtons.OK, EF.MessageBoxType.Information);
                return;
            }
            EF.Dialog form = new InputForm();
            form.ShowModal(Instances.EtoDocumentEditor);
            string name = ((InputForm)form).GetText();
            if (((InputForm)form).Result != DialogResult.OK) return;
            GH_UndoRecord record = new("Rename Groups");
            foreach (GH_Group group in groups.Cast<GH_Group>())
            {
                GH_NickNameAction action = new(group);
                group.NickName = name;
                record.AddAction(action);
            }
            Instances.ActiveCanvas.Document.UndoServer.PushUndoRecord(record);
            Instances.ActiveCanvas.Refresh();
        }
    }

    public class InputForm : EF.Dialog<DialogResult>
    {
        private readonly EF.TextArea _textArea;
        public InputForm()
        {
            Title = "Rename GH_Group: Multiline Name";
            Padding = new(10);
            Result = DialogResult.Cancel;
            MinimumSize = new ED.Size(300, 150);
            WindowStyle = EF.WindowStyle.Utility;

            _textArea = new()
            {
                AcceptsTab = false,
                TextAlignment = EF.TextAlignment.Center,
                ToolTip = "Enter the new (multiline) name for the selected GH_Group(s)",
                Wrap = true,
                Height = 100,
            };
            EF.Button confirm = new((sender, e) => Close(DialogResult.OK))
            {
                Text = "OK",
            };
            EF.DynamicLayout layout = new()
            {
                Spacing = new ED.Size(5, 5),
            };
            layout.AddRow(_textArea);
            layout.AddRow(null);
            layout.AddRow(confirm);

            Content = layout;
            Reflection.R8.UseRhinoStyle(this);
        }

        internal string GetText()
        {
            return _textArea.Text;
        }
    }
}
