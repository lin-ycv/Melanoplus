//using Eto.Forms;
using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.Canvas.Interaction;
using Grasshopper.GUI.Widgets;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus
{
    public class WiresWidget : GH_Widget
    {
        public override string Name => "Copy Wires";
        public override string Description => "Copy all wires from one component to another.";
        public override Bitmap Icon_24x24 => Properties.Resources.Copy_Wire;
        public override bool Visible
        {
            get => enabled;
            set
            {
                enabled = value;
                Handler(value);
                GH_SettingsServer settings = new GH_SettingsServer("grasshopper_kernel", true);
                settings.SetValue("Widget.Melanoplus.cWires", enabled);
                settings.WritePersistentSettings();
            }
        }

        private static bool enabled = false;

        public WiresWidget()
        {
            GH_SettingsServer settings = new GH_SettingsServer("grasshopper_kernel", true);
            enabled = settings.GetValue("Widget.Melanoplus.cWires", false);
        }
        public override bool Contains(Point pt_control, PointF pt_canvas) => false;
        public override void Render(GH_Canvas Canvas) { }

        internal static void CanvasCreated(GH_Canvas canvas)
        {
            Instances.CanvasCreated -= CanvasCreated;
            Handler(enabled);
        }
        private static void Handler(bool value)
        {
            Instances.ActiveCanvas.KeyDown -= ObjLMB;
            if (value)
                Instances.ActiveCanvas.KeyDown += ObjLMB;
        }
        private static void ObjLMB(object sender, KeyEventArgs e)
        {
            if (sender is GH_Canvas canvas
                && canvas.ActiveInteraction is GH_RewireInteraction
                && (Control.ModifierKeys & Keys.Alt) != 0)
            {
                Type type = typeof(GH_RewireInteraction);
                if (type
                    .GetField("m_input", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(canvas.ActiveInteraction) is bool fromInput
                    && type.GetField("m_source", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(canvas.ActiveInteraction) is IGH_Param from
                        && type.GetField("m_target", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(canvas.ActiveInteraction) is IGH_Param target)
                {
                    if (fromInput)
                        foreach (IGH_Param s in from.Sources)
                            target.AddSource(s);
                    else
                        foreach (IGH_Param r in from.Recipients)
                            r.AddSource(target);
                    canvas.Document.NewSolution(false);
                }
            }
        }
    }

    public class GH_cWireSettingsUI : IGH_SettingFrontend
    {
        public string Category => "Widgets";
        public string Name => "Copy Wires";
        public IEnumerable<string> Keywords => new string[1]
        {
            "Instructions",
        };

        public Control SettingsUI()
        {
            var ins = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                MinimumSize = new Size(125, 50),
                BackColor = Color.Transparent,
                FlatStyle = FlatStyle.Flat,
                Image = Properties.Resources.Copy_Wire,
                ImageAlign = ContentAlignment.MiddleLeft,
                Text = "During rewire, press Alt to copy the wires.\r\n[ Ctrl + Shift + LMB + Alt]",
                TextAlign = ContentAlignment.MiddleLeft,
                TextImageRelation = TextImageRelation.ImageBeforeText,
            };
            ins.FlatAppearance.BorderSize = 0;
            ins.FlatAppearance.MouseDownBackColor = Color.Transparent;
            ins.FlatAppearance.MouseOverBackColor = Color.Transparent;
            return ins;
        }
    }
}
