using System.Collections.Concurrent;

namespace Melanoplus.Utils
{
    internal class BreakWires
    {
        internal BreakWires()
        {
            Enabled = new GH_SettingsServer("Melanoplus", true).GetValue("BreakWires", false);
            Instances.CanvasCreated += Handler;
            RhinoApp.Closing += (s, e) =>
            {
                GH_SettingsServer settings = new("Melanoplus", true);
                if (settings.GetValue("BreakWires", false) != Enabled)
                {
                    settings.SetValue("BreakWires", Enabled);
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
        PointF _start = PointF.Empty;

        private void Handler(object sender)
        {
            if (sender is not GH_Canvas canvas)
                return;

            if (Enabled)
                canvas.MouseMove += Trigger;
            else
                canvas.MouseMove -= Trigger;
        }

        private void Trigger(object sender, MouseEventArgs e)
        {
            GH_Canvas canvas = Instances.ActiveCanvas;
            if (canvas.ActiveInteraction is GH_WindowSelectInteraction
                && e.Button == MouseButtons.Left
                && Control.ModifierKeys == (Keys.Control | Keys.Shift)
                && !Control.ModifierKeys.HasFlag(Keys.Alt))
            {
                Instances.CursorServer.AttachCursor(canvas, "GH_RemoveWire");
                if (_start == PointF.Empty)
                {
                    _start = canvas.CursorCanvasPosition;
                    canvas.MouseUp += Fin;
                }
            }
        }
        private void Fin(object sender, MouseEventArgs e)
        {
            if (_start == PointF.Empty)
                return;

            GH_Canvas canvas = Instances.ActiveCanvas;
            canvas.MouseUp -= Fin;
            PointF start = _start,
                end = canvas.CursorCanvasPosition;
            _start = PointF.Empty;

            var removeList = Search(canvas, start, end);
            if (removeList.IsEmpty)
                return;

            GH_UndoRecord record = new("Break Wires");
            foreach (var kvp in removeList)
                foreach (var kvp2 in kvp.Value)
                {
                    record.AddAction(new GH_WireAction(kvp.Key));
                    kvp.Key.RemoveSource(kvp2.Key);
                }

            canvas.Document.UndoServer.PushUndoRecord(record);
            canvas.Document.SelectedObjects().Clear();
            canvas.Document.NewSolution(false);
        }
        private static ConcurrentDictionary<IGH_Param, ConcurrentDictionary<IGH_Param, byte>> Search(GH_Canvas canvas, PointF p1, PointF p2)
        {
            ConcurrentDictionary<IGH_Param, ConcurrentDictionary<IGH_Param, byte>> ts = new();

            float xmin = Math.Min(p1.X, p2.X),
                xmax = Math.Max(p1.X, p2.X),
                ymin = Math.Min(p1.Y, p2.Y),
                ymax = Math.Max(p1.Y, p2.Y),
                stepx = (xmax - xmin) / (int)(xmax - xmin),
                stepy = (ymax - ymin) / (int)(ymax - ymin),
                xx = xmax - xmin,
                yy = ymax - ymin;
            bool xy = xx < yy;

            if (xy)
                Parallel.For(0, (int)xx, x =>
                {
                    for (int y = 0; y < (int)yy; y++)
                        detect(x, y);
                });
            else
                Parallel.For(0, (int)yy, y =>
                {
                    for (int x = 0; x < (int)xx; x++)
                        detect(x, y);
                });

            void detect(int x, int y)
            {
                PointF p = new(xmin + x * stepx, ymin + y * stepy);
                IGH_Param source = null, target = null;
                if (canvas.Document.FindWireAt(p, 5f, ref source, ref target))
                    ts.GetOrAdd(target, _ => new ConcurrentDictionary<IGH_Param, byte>())[source] = 0;
            }

            return ts;
        }
    }
}
