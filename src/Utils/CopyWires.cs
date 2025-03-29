namespace Melanoplus.Utils
{
    internal class CopyWires
    {
        internal CopyWires()
        {
            Enabled = new GH_SettingsServer("Melanoplus", true).GetValue("CopyWires", false);
            Instances.CanvasCreated += Handler;
            RhinoApp.Closing += (s, e) =>
            {
                GH_SettingsServer settings = new("Melanoplus", true);
                if (settings.GetValue("CopyWires", false) != Enabled)
                {
                    settings.SetValue("CopyWires", Enabled);
                    settings.WritePersistentSettings();
                }
            };
        }

        public bool Enabled
        {
            get => _enable;
            set
            {
                _enable = value;
                Handler(Instances.ActiveCanvas);
            }
        }
        private bool _enable;

        private void Handler(object sender)
        {
            if (sender is not GH_Canvas canvas)
                return;

            if (Enabled)
                canvas.KeyDown += Copy;
            else
                canvas.KeyDown -= Copy;
        }

        private void Copy(object sender, KeyEventArgs e)
        {
            GH_Canvas canvas = Instances.ActiveCanvas;

            if (canvas.ActiveInteraction is GH_RewireInteraction
                && e.Alt)
            {
                Instances.CursorServer.AttachCursor(canvas, "GH_AddWire");
                Type type = typeof(GH_RewireInteraction);
                if (type
                    .GetField("m_input", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(canvas.ActiveInteraction) is bool fromInput
                    && type.GetField("m_source", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(canvas.ActiveInteraction) is IGH_Param from
                        && type.GetField("m_target", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(canvas.ActiveInteraction) is IGH_Param target)
                {
                    GH_UndoRecord record = new("Copy Wires");
                    if (fromInput)
                    {
                        GH_WireAction action = new(target);
                        foreach (IGH_Param s in from.Sources)
                            target.AddSource(s);
                        record.AddAction(action);
                    }
                    else
                        foreach (IGH_Param r in from.Recipients)
                        {
                            GH_WireAction action = new(r);
                            r.AddSource(target);
                            record.AddAction(action);
                        }
                    canvas.Document.UndoServer.PushUndoRecord(record);

                    canvas.Document.NewSolution(false);
                }
            }
        }
    }
}

// REF: https://discourse.mcneel.com/t/wire-event/69005/4