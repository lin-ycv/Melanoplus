namespace Melanoplus.Widgets
{
    public class LabelWidget : GH_Widget
    {
        public override bool Visible
        {
            get;
            set;
        }

        public override string Name => "Label";

        public override string Description => "Display component name/nickname above component\r\nDouble click to toggle between Fullname and Nickname\r\n.\r\nAdditional settings in Grasshopper Preferences";

        public override Bitmap Icon_24x24 => Properties.Resources.Label;
        private static readonly StringFormat alignment = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Far };
        internal static Color Color;
        internal static bool Nickname, CustomNickname;
        internal static List<string> Exclude;
        internal static Font Font = GH_FontServer.StandardItalic;
        private readonly List<RectangleF> _labels;

        public LabelWidget() : base()
        {
            TypeConverter converter = TypeDescriptor.GetConverter(typeof(Font));
            string defaultExlusion = "Relay,Panel,Button,Boolean Toggle,Number Slider,Sketch,Scribble",
                defaultString = converter.ConvertToInvariantString(GH_FontServer.StandardItalic);
            GH_SettingsServer settings = new("Melanoplus", true);
            Color = settings.GetValue("LabelWidget.Color", Color.Black);
            Visible = settings.GetValue("LabelWidget.Enabled", false);
            Nickname = settings.GetValue("LabelWidget.NickName", false);
            CustomNickname = settings.GetValue("LabelWidget.CustomNickName", false);
            Exclude =
                [
                    ..settings.GetValue("LabelWidget.Exclude", defaultExlusion).Split(','),
                ];
            Font = (Font)converter.ConvertFromString(settings.GetValue("LabelWidget.Font", defaultString));
            _labels = [];
            RhinoApp.Closing += (s, e) =>
            {
                bool save = false;
                if (Color != settings.GetValue("LabelWidget.Color", Color.Black))
                    settings.SetValue("LabelWidget.Color", Color); save = true;
                if (Visible != settings.GetValue("LabelWidget.Enabled", false))
                    settings.SetValue("LabelWidget.Enabled", Visible); save = true;
                if (Nickname != settings.GetValue("LabelWidget.NickName", false))
                    settings.SetValue("LabelWidget.NickName", Nickname); save = true;
                if (CustomNickname != settings.GetValue("LabelWidget.CustomNickName", false))
                    settings.SetValue("LabelWidget.CustomNickName", CustomNickname); save = true;
                if (Exclude != settings.GetValue("LabelWidget.Exclude", defaultExlusion).Split(',').ToList())
#if NET7_0
                    settings.SetValue("LabelWidget.Exclude", string.Join(',', Exclude)); save = true;
#else
                    settings.SetValue("LabelWidget.Exclude", string.Join(",", Exclude)); save = true;
#endif
                if (Font != (Font)converter.ConvertFromString(settings.GetValue("LabelWidget.Font", defaultString)))
                    settings.SetValue("LabelWidget.Font", converter.ConvertToInvariantString(Font)); save = true;
                if (save)
                    settings.WritePersistentSettings();
            };
        }
        
        //public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        //{
        //    Nickname = !Nickname;
        //    Instances.RedrawCanvas();
        //    return GH_ObjectResponse.Handled;
        //}
        public override bool Contains(Point pt_control, PointF pt_canvas)
        {
            return false;
            //foreach (RectangleF label in _labels)
            //{
            //    if (label.Contains(pt_canvas))
            //    {
            //        return true;
            //    }
            //}
            //return false;
        }

        public override void Render(GH_Canvas Canvas)
        {
            _labels.Clear();
            GH_Document document = Canvas.Document;
            GH_Viewport viewport = Canvas.Viewport;
            if (viewport.Zoom <= GH_Viewport.ZoomDefault * 0.5f
                || document == null
                || GH_Canvas.ZoomFadeLow == 0)
            {
                return;
            }

            using SolidBrush solidBrush = new(Color);
            foreach (IGH_DocumentObject component in document.Objects)
            {
                RectangleF bounds = component.Attributes.Bounds;
                if (!viewport.IsVisible(ref bounds, 50f)
                    || Exclude.Contains(component.Name)
                    || component is GH_Group)
                {
                    continue;
                }

                float x = bounds.X + 0.5f * bounds.Width;
                float y = bounds.Y;
                string name = Nickname ?
                    ObjectProxyNickName(component, CustomNickname) :
                    component.Name;
                Size nameSize = GH_FontServer.MeasureString(name, Font);
                if (nameSize.Width > bounds.Width * 1.25f)
                {
                    int limit = name.Length / (int)Math.Round(nameSize.Width / bounds.Width)
                        , count = 0;
                    string[] words = name.Split([' ', '_']);
                    StringBuilder sb = new();
                    foreach (string word in words)
                    {
                        if (count > limit)
                        {
                            sb.Append(Environment.NewLine);
                            count = 0;
                        }
                        else
                        {
                            sb.Append(' ');
                        }
                        sb.Append(word);
                        count += word.Length + 1;
                    }
                    name = sb.ToString().Trim();
                }
                Canvas.Graphics.DrawString(name, Font, solidBrush, x, y, alignment);
                RectangleF labelBounds = new(
                    bounds.X + 0.25f * bounds.Width,
                    y - nameSize.Height,
                    bounds.Width * 0.5f,
                    nameSize.Height);
                _labels.Add(labelBounds);
            }
        }

        private static string ObjectProxyNickName(IGH_DocumentObject component, bool CustomNickname)
        {
            string nickname = component.NickName;
            if (!CustomNickname)
            {
                IGH_ObjectProxy pxy = Instances.ComponentServer.EmitObjectProxy(component.ComponentGuid);
                if (pxy == null)
                    nickname = component.Name;
                else
                    nickname = pxy.Desc.NickName;
            }
            return nickname;
        }

    }
}

// REF: https://discourse.mcneel.com/t/c-custom-widgets/56777/3