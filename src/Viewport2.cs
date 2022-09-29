using Grasshopper.Kernel;
using Grasshopper;
using Rhino.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rhino.Display;
using System.Drawing;
using System.Drawing.Drawing2D;
using Grasshopper.GUI.Widgets;
using Grasshopper.GUI.Canvas;
using Rhino;
using System.Timers;
using Grasshopper.GUI;

namespace Melanoplus
{
    public class Viewport2 : GH_CanvasWidget_FixedObject
    {
        public override string Name => "Peek Viewport";
        public override string Description => "Peek at active Rhino viewport";
        public override Bitmap Icon_24x24 => Properties.Resources.viewportRhino;
        public override bool Visible 
        { 
            get => enabled; 
            set             
            { 
                enabled = value;
                aTimer.Elapsed -= Sync;
                if (value)
                {
                    aTimer.Start();
                    aTimer.Elapsed += Sync;
                }
                else
                    aTimer.Stop();              
            } 
        }

        public override SizeF Ratio
        {
            get => ratio;
            set
            {
                value.Width = value.Width > 1f ? 1f : value.Width < 0f ? 0F : value.Width;
                value.Height = value.Height > 1f ? 1f : value.Height < 0f ? 0F : value.Height;
                if (!(ratio == value))
                    ratio = value;
            }
        }
        private static SizeF ratio = new SizeF(Convert.ToSingle(1), Convert.ToSingle(1));
        public override Size Size => new Size(Global_Proc.UiAdjust(200), Global_Proc.UiAdjust(150));

        public override int Padding => Global_Proc.UiAdjust(10);
        private static bool enabled = false, refresh = false;
        private static System.Timers.Timer aTimer = new System.Timers.Timer() { AutoReset = true, Interval = 250};
        private static Bitmap bitmap = new Bitmap(1, 1);
        private static Size reSize = new Size(Global_Proc.UiAdjust(200), Global_Proc.UiAdjust(150));

        public Viewport2()       {       }
        public override bool Contains(Point pt_control, PointF pt_canvas)
        {
            if (base.Owner.Document == null)
            {
                return false;
            }
            PointF pointF = CanvasLocation(base.Owner.Viewport);
            return Math.Pow(pt_canvas.X - pointF.X, 2.0) + Math.Pow(pt_canvas.Y - pointF.Y, 2.0) < Math.Pow((float)100 * base.Owner.Viewport.ZoomInverse, 2.0);
        }

        protected override void Render_Internal(GH_Canvas canvas, Point controlAnchor, PointF canvasAnchor, Rectangle controlFrame, RectangleF canvasFrame)
        {
            if (canvas.Document != null)
            {
                Graphics graphics = canvas.Graphics;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                Matrix transform = graphics.Transform;
                graphics.ResetTransform();

                if (refresh)
                {
                    bitmap = RhinoDoc.ActiveDoc.Views.ActiveView.DisplayPipeline.FrameBuffer;
                    refresh = false;
                    double aspect = (bitmap.Height*1.0)/(bitmap.Width*1.0);
                    reSize = new Size(
                        Global_Proc.UiAdjust(Size.Width), 
                        Global_Proc.UiAdjust((int)Math.Round(Size.Height*aspect)));
                    //int maxWidth = Math.Min(Size.Width, bitmap.Width),
                    //    maxHeight = Math.Min(Size.Width, bitmap.Height);
                    //decimal rnd = Math.Min(maxWidth / (decimal)bitmap.Width, maxHeight / (decimal)bitmap.Height);
                    //reSize = new Size((int)Math.Round(Size.Width * rnd), (int)Math.Round(Size.Height * rnd));
                }

                graphics.DrawImage(bitmap, controlFrame);
                
                //var bmp = new Bitmap(bitmap, reSize);

                //graphics.DrawImage(bmp, 
                //    new RectangleF(
                //        new Point(
                //            controlFrame.X,
                //            controlFrame.Y+(controlFrame.Height-bmp.Height)/2),
                //        reSize));

                //TextureBrush brush = new TextureBrush(bitmap ?? new Bitmap(1, 1), WrapMode.Clamp);

                //DrawObjectMarkers(graphics, controlFrame);
                //DrawCompassDiscBackground(graphics, controlFrame);
                //DrawCompassDiscNeedle(graphics, controlFrame, canvas.Viewport.Target);
                //DrawCompassDiscBorder(graphics, controlFrame);
                graphics.Transform = transform;
            }
        }

        private void Sync(object sender, ElapsedEventArgs e)
        {
            refresh=true;
        }

        public override GH_ObjectResponse RespondToMouseDown(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (sender.Document == null)
            {
                return GH_ObjectResponse.Ignore;
            }
            if (!Contains(e.ControlLocation, e.CanvasLocation))
            {
                return GH_ObjectResponse.Ignore;
            }
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Instances.CursorServer.AttachCursor(sender, "GH_HandClosed");
                    return GH_ObjectResponse.Capture;
                case MouseButtons.Right:
                    return base.RespondToMouseDown(sender, e);
                default:
                    return base.RespondToMouseDown(sender, e);
            }
        }

        public override GH_ObjectResponse RespondToMouseMove(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (sender.Document == null)
            {
                return GH_ObjectResponse.Ignore;
            }
            switch (e.Button)
            {
                case MouseButtons.Left:
                    SetNewRatio(sender.Viewport, e.CanvasLocation);
                    sender.Refresh();
                    return GH_ObjectResponse.Ignore;
                case MouseButtons.None:
                    Instances.CursorServer.AttachCursor(sender, "GH_HandOpen");
                    return GH_ObjectResponse.Handled;
                default:
                    return GH_ObjectResponse.Ignore;
            }
        }

        public override GH_ObjectResponse RespondToMouseUp(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            return GH_ObjectResponse.Release;
        }
        private void SetNewRatio(GH_Viewport viewport, PointF canvasPoint)
        {
            PointF pointF = base.Owner.Viewport.ProjectPoint(canvasPoint);
            float width = pointF.X / (float)viewport.Width;
            float height = pointF.Y / (float)viewport.Height;
            Ratio = new SizeF(width, height);
        }


        //    Eto.Forms.UITimer _timer;
        //    static Panel _viewportControlPanel;
        //    static RhinoWindows.Forms.Controls.ViewportControl ctrl;
        //    static PictureBox lockedPic1;
        //    static PictureBox lockedPic2;
        //    public static ToolStripMenuItem viewportMenuItem;
        //    public static GH_SettingsServer settings = new GH_SettingsServer("GhViewport2", true);

        //    public void AddToMenu()
        //    {
        //        if (_timer != null)
        //            return;
        //        _timer = new Eto.Forms.UITimer();
        //        _timer.Interval = 1;
        //        _timer.Elapsed += SetupMenu;
        //        _timer.Start();
        //    }

        //    public void SetupMenu(object sender, EventArgs e)
        //    {
        //        if (!settings.ConstainsEntry("state"))
        //        { DefaultSettings(); }
        //        bool state = settings.GetValue("state", false);
        //        int width = settings.GetValue("width", 400);
        //        int height = settings.GetValue("height", 300);
        //        string dock = settings.GetValue("dock", "topleft");

        //        var editor = Instances.DocumentEditor;
        //        if (null == editor || editor.Handle == IntPtr.Zero)
        //            return;

        //        var controls = editor.Controls;
        //        if (null == controls || controls.Count == 0)
        //            return;

        //        _timer.Stop();
        //        foreach (var ctrl2 in controls)
        //        {
        //            var menu = ctrl2 as Grasshopper.GUI.GH_MenuStrip;
        //            if (menu == null)
        //                continue;
        //            for (int i = 0; i < menu.Items.Count; i++)
        //            {
        //                var menuitem = menu.Items[i] as ToolStripMenuItem;
        //                if (menuitem != null && menuitem.Text == "Display")
        //                {
        //                    for (int j = 0; j < menuitem.DropDownItems.Count; j++)
        //                    {
        //                        if (menuitem.DropDownItems[j].Text.StartsWith("canvas widgets", StringComparison.OrdinalIgnoreCase))
        //                        {
        //                            viewportMenuItem = new ToolStripMenuItem("Canvas Viewport", Properties.Resources.viewportRhino, new EventHandler(OnToggle));
        //                            viewportMenuItem.ToolTipText = "Opens a docked window in Grasshopper that displays Rhino viewports.\r\n - Use the right-click menu to change display modes, views, and other settings.";
        //                            viewportMenuItem.CheckOnClick = true;
        //                            viewportMenuItem.Checked = state;

        //                            if (state)
        //                            {
        //                                if (_viewportControlPanel == null)
        //                                {
        //                                    _viewportControlPanel = new ViewportContainerPanel();
        //                                    _viewportControlPanel.Size = new Size(width, height);
        //                                    _viewportControlPanel.MinimumSize = new Size(50, 50);
        //                                    _viewportControlPanel.Padding = new Padding(10);
        //                                    ctrl = new Viewport2Control();
        //                                    ctrl.Dock = DockStyle.Fill;
        //                                    _viewportControlPanel.BorderStyle = BorderStyle.FixedSingle;
        //                                    UpdateViewport(false);
        //                                    _viewportControlPanel.Controls.Add(ctrl);
        //                                    _viewportControlPanel.Location = new Point(0, 0);
        //                                    Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
        //                                    if (dock == "topleft")
        //                                    { Dock(AnchorStyles.Top | AnchorStyles.Left); }
        //                                    if (dock == "bottomleft")
        //                                    { Dock(AnchorStyles.Bottom | AnchorStyles.Left); }
        //                                    if (dock == "bottomright")
        //                                    { Dock(AnchorStyles.Bottom | AnchorStyles.Right); }
        //                                    if (dock == "topright")
        //                                    { Dock(AnchorStyles.Top | AnchorStyles.Right); }
        //                                }
        //                                _viewportControlPanel.Show();
        //                                settings.SetValue("state", true);
        //                                settings.WritePersistentSettings();
        //                            }
        //                            else
        //                            {
        //                                if (_viewportControlPanel != null && _viewportControlPanel.Visible)
        //                                {
        //                                    _viewportControlPanel.Hide();
        //                                    settings.SetValue("state", false);
        //                                    settings.WritePersistentSettings();
        //                                }
        //                            }
        //                            viewportMenuItem.CheckedChanged += ViewportMenuItem_CheckedChanged;
        //                            var canvasWidgets = menuitem.DropDownItems[j] as ToolStripMenuItem;
        //                            if (canvasWidgets != null)
        //                            {
        //                                canvasWidgets.DropDownOpening += (s, args) =>
        //                                    canvasWidgets.DropDownItems.Insert(0, viewportMenuItem);
        //                            }
        //                            break;
        //                        }
        //                    }
        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    /// <summary>
        //    /// Panel with a "re-sizable" border that contains a viewport control
        //    /// </summary>
        //    class ViewportContainerPanel : Panel
        //    {
        //        public override Cursor Cursor
        //        {
        //            get
        //            {
        //                var location = PointToClient(Control.MousePosition);
        //                var mode = ComputeMode(location);
        //                switch (mode)
        //                {
        //                    case Mode.None:
        //                        return Cursors.Default;
        //                    case Mode.Move:
        //                        return Cursors.SizeAll;
        //                    case Mode.SizeNESW:
        //                        return Cursors.SizeNESW;
        //                    case Mode.SizeNS:
        //                        return Cursors.SizeNS;
        //                    case Mode.SizeWE:
        //                        return Cursors.SizeWE;
        //                    case Mode.SizeNWSE:
        //                        return Cursors.SizeNWSE;
        //                }
        //                return base.Cursor;
        //            }
        //            set => base.Cursor = value;
        //        }

        //        Mode ComputeMode(Point location)
        //        {
        //            var dock = Anchor;
        //            switch (Anchor)
        //            {
        //                case (AnchorStyles.Left | AnchorStyles.Top):
        //                    {
        //                        if (location.X > (Width - Padding.Right))
        //                            return location.Y > (Height - Padding.Bottom) ? Mode.SizeNWSE : Mode.SizeWE;
        //                        if (location.Y > (Height - Padding.Bottom))
        //                            return Mode.SizeNS;
        //                        if (location.X < Padding.Left || location.Y < Padding.Top)
        //                            return Mode.None; //moving is little weird the way this is set up, don't support for now
        //                        return Mode.None;
        //                    }
        //                case (AnchorStyles.Left | AnchorStyles.Bottom):
        //                    {
        //                        if (location.X > (Width - Padding.Right))
        //                            return location.Y < Padding.Top ? Mode.SizeNESW : Mode.SizeWE;
        //                        if (location.Y < Padding.Top)
        //                            return Mode.SizeNS;
        //                        if (location.X < Padding.Left || location.Y > (Height - Padding.Bottom))
        //                            return Mode.None; //moving is little weird the way this is set up, don't support for now
        //                        return Mode.None;

        //                    }
        //                case (AnchorStyles.Right | AnchorStyles.Top):
        //                    {
        //                        if (location.X < Padding.Left)
        //                            return location.Y > (Height - Padding.Bottom) ? Mode.SizeNESW : Mode.SizeWE;
        //                        if (location.Y > (Height - Padding.Bottom))
        //                            return Mode.SizeNS;
        //                        if (location.X > (Width - Padding.Right) || location.Y < Padding.Top)
        //                            return Mode.None; //moving is little weird the way this is set up, don't support for now
        //                        return Mode.None;
        //                    }
        //                case (AnchorStyles.Right | AnchorStyles.Bottom):
        //                    {
        //                        if (location.X < Padding.Left)
        //                            return location.Y < Padding.Top ? Mode.SizeNWSE : Mode.SizeWE;
        //                        if (location.Y < Padding.Top)
        //                            return Mode.SizeNS;
        //                        if (location.X > (Width - Padding.Right) || location.Y > (Height - Padding.Bottom))
        //                            return Mode.None; //moving is little weird the way this is set up, don't support for now
        //                        return Mode.None;
        //                    }
        //            }

        //            return Mode.None;
        //        }

        //        Point LeftMouseDownLocation { get; set; }
        //        Size LeftMouseDownSize { get; set; }

        //        enum Mode
        //        {
        //            None,
        //            SizeWE,
        //            SizeNS,
        //            SizeNWSE,
        //            SizeNESW,
        //            Move
        //        }
        //        Mode _mode;
        //        protected override void OnMouseDown(MouseEventArgs e)
        //        {
        //            _mode = Mode.None;
        //            if (e.Button == MouseButtons.Left)
        //            {
        //                _mode = ComputeMode(e.Location);
        //                LeftMouseDownLocation = e.Location;
        //                LeftMouseDownSize = Size;
        //            }
        //            base.OnMouseDown(e);
        //        }
        //        protected override void OnMouseMove(MouseEventArgs e)
        //        {
        //            if (_mode != Mode.None)
        //            {
        //                int x = Location.X;
        //                int y = Location.Y;
        //                int width = Width;
        //                int height = Height;

        //                int deltaX = e.X - LeftMouseDownLocation.X;
        //                int deltaY = e.Y - LeftMouseDownLocation.Y;
        //                if (_mode == Mode.SizeNESW || _mode == Mode.SizeNS || _mode == Mode.SizeNWSE)
        //                {
        //                    if ((Anchor & AnchorStyles.Top) == AnchorStyles.Top)
        //                        height = LeftMouseDownSize.Height + deltaY;
        //                    if ((Anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
        //                    {
        //                        var pt = new Point(Location.X, Location.Y + deltaY);
        //                        height = Height - (pt.Y - Location.Y);
        //                        y = Location.Y + deltaY;
        //                    }
        //                }
        //                if (_mode == Mode.SizeNESW || _mode == Mode.SizeWE || _mode == Mode.SizeNWSE)
        //                {
        //                    if ((Anchor & AnchorStyles.Left) == AnchorStyles.Left)
        //                        width = LeftMouseDownSize.Width + deltaX;
        //                    if ((Anchor & AnchorStyles.Right) == AnchorStyles.Right)
        //                    {
        //                        var pt = new Point(Location.X + deltaX, Location.Y);
        //                        width = Width - (pt.X - Location.X);
        //                        x = Location.X + deltaX;
        //                    }
        //                }
        //                SetBounds(x, y, width, height);
        //                settings.SetValue("width", width);
        //                settings.SetValue("height", height);
        //                settings.WritePersistentSettings();
        //            }
        //            base.OnMouseMove(e);
        //        }
        //        protected override void OnMouseUp(MouseEventArgs e)
        //        {
        //            _mode = Mode.None;
        //            base.OnMouseUp(e);
        //        }
        //    }

        //    public static void ViewportMenuItem_CheckedChanged(object sender, EventArgs e)
        //    {
        //        var v = Rhino.RhinoApp.Version;
        //        if (v.Major < 6 || (v.Major == 6 && v.Minor < 3))
        //        {
        //            // The viewport control does not work very well pre 6.3
        //            Rhino.UI.Dialogs.ShowMessage("Canvas viewport requires Rhino 6.3 or greater version", "New Version Required");
        //            return;
        //        }
        //        int width = settings.GetValue("width", 400);
        //        int height = settings.GetValue("height", 300);
        //        string dock = settings.GetValue("dock", "topleft");

        //        var menuitem = sender as ToolStripMenuItem;
        //        if (menuitem != null)
        //        {
        //            if (menuitem.Checked)
        //            {
        //                if (_viewportControlPanel == null)
        //                {
        //                    _viewportControlPanel = new ViewportContainerPanel();
        //                    _viewportControlPanel.Size = new Size(width, height);
        //                    _viewportControlPanel.MinimumSize = new Size(50, 50);
        //                    _viewportControlPanel.Padding = new Padding(10);
        //                    ctrl = new Viewport2Control();
        //                    ctrl.Dock = DockStyle.Fill;
        //                    _viewportControlPanel.BorderStyle = BorderStyle.FixedSingle;
        //                    UpdateViewport(false);
        //                    _viewportControlPanel.Controls.Add(ctrl);
        //                    _viewportControlPanel.Location = new Point(0, 0);
        //                    Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
        //                    if (dock == "topleft")
        //                    { Dock(AnchorStyles.Top | AnchorStyles.Left); }
        //                    if (dock == "bottomleft")
        //                    { Dock(AnchorStyles.Bottom | AnchorStyles.Left); }
        //                    if (dock == "bottomright")
        //                    { Dock(AnchorStyles.Bottom | AnchorStyles.Right); }
        //                    if (dock == "topright")
        //                    { Dock(AnchorStyles.Top | AnchorStyles.Right); }
        //                }
        //                _viewportControlPanel.Show();
        //                settings.SetValue("state", true);
        //                settings.WritePersistentSettings();
        //            }
        //            else
        //            {
        //                if (_viewportControlPanel != null && _viewportControlPanel.Visible)
        //                {
        //                    _viewportControlPanel.Hide();
        //                    settings.SetValue("state", false);
        //                    settings.WritePersistentSettings();
        //                }
        //            }
        //        }
        //    }

        //    public static void UpdateViewport(bool flag)
        //    {
        //        if (flag)
        //        {
        //            ctrl.Controls.Remove(lockedPic1);
        //            ctrl.Controls.Remove(lockedPic2);
        //        }
        //        bool icontoggle = settings.GetValue("icontoggle", false);
        //        if (!icontoggle)
        //        {
        //            string dockicons = settings.GetValue("dockicons", "topleft");
        //            string iconstyle = settings.GetValue("iconstyle", "colored");
        //            bool iconoffset = settings.GetValue("iconoffset", false);
        //            bool locked1 = settings.GetValue("locked1", false);
        //            lockedPic1 = new PictureBox();
        //            bool locked2 = settings.GetValue("locked2", false);
        //            lockedPic2 = new PictureBox();
        //            if (locked1)
        //            { if (iconstyle == "colored") { lockedPic1.Image = null; } else { lockedPic1.Region = CreateRegion(null); lockedPic1.Image = null; } }
        //            else
        //            { if (iconstyle == "colored") { lockedPic1.Image = null; } else { lockedPic1.Region = CreateRegion(null); lockedPic1.Image = null; } }
        //            lockedPic1.Size = new Size(16, 16);
        //            int offset = -3;
        //            if (iconoffset)
        //            { offset = -14; }
        //            if (dockicons == "topleft")
        //            { DockIcons(lockedPic1, AnchorStyles.Top | AnchorStyles.Left, 3, 3); }
        //            if (dockicons == "bottomleft")
        //            { DockIcons(lockedPic1, AnchorStyles.Bottom | AnchorStyles.Left, offset, 3); }
        //            if (dockicons == "bottomright")
        //            { DockIcons(lockedPic1, AnchorStyles.Bottom | AnchorStyles.Right, offset, -22); }
        //            if (dockicons == "topright")
        //            { DockIcons(lockedPic1, AnchorStyles.Top | AnchorStyles.Right, 3, -22); }
        //            ctrl.Controls.Add(lockedPic1);
        //            if (locked2)
        //            { if (iconstyle == "colored") { lockedPic2.Image = null; } else { lockedPic2.Region = CreateRegion(null); lockedPic2.Image = null; } }
        //            else
        //            { if (iconstyle == "colored") { lockedPic2.Image = null; } else { lockedPic2.Region = CreateRegion(null); lockedPic2.Image = null; } }
        //            lockedPic2.Size = new Size(16, 16);
        //            if (dockicons == "topleft")
        //            { DockIcons(lockedPic2, AnchorStyles.Top | AnchorStyles.Left, 3, 22); }
        //            if (dockicons == "bottomleft")
        //            { DockIcons(lockedPic2, AnchorStyles.Bottom | AnchorStyles.Left, offset, 22); }
        //            if (dockicons == "bottomright")
        //            { DockIcons(lockedPic2, AnchorStyles.Bottom | AnchorStyles.Right, offset, -3); }
        //            if (dockicons == "topright")
        //            { DockIcons(lockedPic2, AnchorStyles.Top | AnchorStyles.Right, 3, -3); }
        //            ctrl.Controls.Add(lockedPic2);
        //        }
        //    }

        //    private static Region CreateRegion(Bitmap maskImage)
        //    {
        //        Color mask = maskImage.GetPixel(0, 0);
        //        GraphicsPath graphicsPath = new GraphicsPath();
        //        for (int x = 0; x < maskImage.Width; x++)
        //        {
        //            for (int y = 0; y < maskImage.Height; y++)
        //            {
        //                if (!maskImage.GetPixel(x, y).Equals(mask))
        //                {
        //                    graphicsPath.AddRectangle(new Rectangle(x, y, 1, 1));
        //                }
        //            }
        //        }

        //        return new Region(graphicsPath);
        //    }

        //    private void OnToggle(object sender, EventArgs e)
        //    {
        //        if (viewportMenuItem.Checked)
        //        { settings.SetValue("state", true); settings.WritePersistentSettings(); }
        //        else
        //        { settings.SetValue("state", false); settings.WritePersistentSettings(); }
        //    }
        //    public static void Dock(AnchorStyles anchor)
        //    {
        //        DockPanel(_viewportControlPanel, anchor);
        //    }
        //    public static void DockPanel(Control ctrl, AnchorStyles anchor)
        //    {
        //        if (ctrl == null)
        //            return;
        //        var canvas = Instances.ActiveCanvas;
        //        var canvasSize = canvas.ClientSize;
        //        int xEnd = 0;
        //        if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
        //            xEnd = canvasSize.Width - ctrl.Width;
        //        int yEnd = 0;
        //        if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
        //            yEnd = canvasSize.Height - ctrl.Height;

        //        ctrl.Location = new Point(xEnd, yEnd);
        //        ctrl.Anchor = anchor;
        //    }
        //    public static void DockIcons(Control ctrl2, AnchorStyles anchor, int y, int x)
        //    {
        //        if (ctrl2 == null)
        //            return;
        //        var canvas = ctrl;
        //        var canvasSize = canvas.ClientSize;
        //        int xEnd = 0;
        //        if ((anchor & AnchorStyles.Right) == AnchorStyles.Right)
        //            xEnd = canvasSize.Width - ctrl2.Width;
        //        int yEnd = 0;
        //        if ((anchor & AnchorStyles.Bottom) == AnchorStyles.Bottom)
        //            yEnd = canvasSize.Height - ctrl2.Height;

        //        ctrl2.Location = new Point(xEnd + x, yEnd + y);
        //        ctrl2.Anchor = anchor;
        //    }
        //    public static void DefaultSettings()
        //    {
        //        settings.SetValue("state", false);
        //        settings.SetValue("width", 400);
        //        settings.SetValue("height", 300);
        //        settings.SetValue("dock", "topleft");
        //        settings.SetValue("locked1", false);
        //        settings.SetValue("locked2", false);
        //        settings.SetValue("icontoggle", false);
        //        settings.SetValue("iconoffset", false);
        //        settings.SetValue("dockicons", "topleft");
        //        settings.SetValue("iconstyle", "colored");
        //        settings.SetValue("view", "Perspective");
        //        settings.SetValue("displaymode", "Wireframe");
        //        settings.SetValue("gridtoggle", true);
        //        settings.SetValue("axestoggle", true);
        //        settings.SetValue("worldtoggle", true);
        //        settings.WritePersistentSettings();
        //    }
        //}

        //class Viewport2Control : RhinoWindows.Forms.Controls.ViewportControl
        //{
        //    public Viewport2Control()
        //    {
        //        // stupid hack to get GH to draw preview geometry in this control
        //        this.Viewport.Name = "GH_HACK";

        //        bool gridtoggle = Viewport2.settings.GetValue("gridtoggle", true);
        //        bool axestoggle = Viewport2.settings.GetValue("axestoggle", true);
        //        bool worldtoggle = Viewport2.settings.GetValue("worldtoggle", true);
        //        Viewport.DisplayMode = DisplayModeDescription.FindByName(Viewport2.settings.GetValue("displaymode", "Wireframe"));
        //        Viewport.ConstructionGridVisible = gridtoggle;
        //        Viewport.ConstructionAxesVisible = axestoggle;
        //        Viewport.WorldAxesVisible = worldtoggle;

        //        ViewReset();
        //        CamFetcher();
        //    }

        //    System.Drawing.Point RightMouseDownLocation { get; set; }
        //    protected override void OnMouseDown(MouseEventArgs e)
        //    {
        //        if (e.Button == MouseButtons.Right)
        //        {
        //            RightMouseDownLocation = e.Location;
        //        }
        //        else
        //        { RightMouseDownLocation = System.Drawing.Point.Empty; }
        //        base.OnMouseDown(e);
        //    }
        //    protected override void OnMouseMove(MouseEventArgs e)
        //    {
        //        if (e.Button == MouseButtons.Right)
        //        {
        //            bool locked1 = Viewport2.settings.GetValue("locked1", false);
        //            if (!locked1)
        //            {
        //                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
        //                if (vec.Length > 10)
        //                { RightMouseDownLocation = System.Drawing.Point.Empty; }
        //                base.OnMouseMove(e);
        //            }
        //        }
        //        else
        //        {
        //            bool locked2 = Viewport2.settings.GetValue("locked2", false);
        //            if (!locked2)
        //            {
        //                RightMouseDownLocation = System.Drawing.Point.Empty;
        //                base.OnMouseMove(e);
        //            }
        //        }
        //    }
        //    protected override void OnMouseUp(MouseEventArgs e)
        //    {
        //        base.OnMouseUp(e);
        //        if (e.Button == MouseButtons.Right)
        //        {
        //            var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
        //            if (vec.Length < 10)
        //            { ShowContextMenu(e.Location); }
        //        }
        //        RightMouseDownLocation = System.Drawing.Point.Empty;
        //    }

        //    void ShowContextMenu(System.Drawing.Point location)
        //    {
        //        var contextMenu = new ContextMenu();

        //        var lockMenuMain = new MenuItem("Locking Options");

        //        var lockMenu1 = new MenuItem("Lock Rotation");
        //        bool locked1 = Viewport2.settings.GetValue("locked1", false);
        //        lockMenu1.Click += (s, e) => { locked1 = !locked1; Viewport2.settings.SetValue("locked1", locked1); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        lockMenu1.Checked = locked1;
        //        lockMenuMain.MenuItems.Add(lockMenu1);
        //        contextMenu.MenuItems.Add(lockMenuMain);

        //        var lockMenu2 = new MenuItem("Lock Dragging");
        //        bool locked2 = Viewport2.settings.GetValue("locked2", false);
        //        lockMenu2.Click += (s, e) => { locked2 = !locked2; Viewport2.settings.SetValue("locked2", locked2); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        lockMenu2.Checked = locked2;
        //        lockMenuMain.MenuItems.Add(lockMenu2);
        //        contextMenu.MenuItems.Add(lockMenuMain);

        //        var iconToggle = new MenuItem("Disable Lock Icons");
        //        bool icontoggle = Viewport2.settings.GetValue("icontoggle", false);
        //        iconToggle.Click += (s, e) => { icontoggle = !icontoggle; Viewport2.settings.SetValue("icontoggle", icontoggle); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        iconToggle.Checked = icontoggle;
        //        lockMenuMain.MenuItems.Add(iconToggle);

        //        lockMenuMain.MenuItems.Add("-");

        //        var dockiconsMenu = new MenuItem("Dock Icons");
        //        string dockicons = Viewport2.settings.GetValue("dockicons", "topleft");
        //        var dockiconsMenuItem = new MenuItem("Top Left");
        //        dockiconsMenuItem.RadioCheck = true;
        //        dockiconsMenuItem.Click += (s, e) => { dockicons = "topleft"; Viewport2.settings.SetValue("dockicons", dockicons); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        dockiconsMenuItem.Checked = (dockicons == "topleft");
        //        dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
        //        dockiconsMenuItem = new MenuItem("Top Right");
        //        dockiconsMenuItem.RadioCheck = true;
        //        dockiconsMenuItem.Click += (s, e) => { dockicons = "topright"; Viewport2.settings.SetValue("dockicons", dockicons); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        dockiconsMenuItem.Checked = (dockicons == "topright");
        //        dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
        //        dockiconsMenuItem = new MenuItem("Bottom Left");
        //        dockiconsMenuItem.RadioCheck = true;
        //        dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomleft"; Viewport2.settings.SetValue("dockicons", dockicons); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        dockiconsMenuItem.Checked = (dockicons == "bottomleft");
        //        dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
        //        dockiconsMenuItem = new MenuItem("Bottom Right");
        //        dockiconsMenuItem.RadioCheck = true;
        //        dockiconsMenuItem.Click += (s, e) => { dockicons = "bottomright"; Viewport2.settings.SetValue("dockicons", dockicons); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        dockiconsMenuItem.Checked = (dockicons == "bottomright");
        //        dockiconsMenu.MenuItems.Add(dockiconsMenuItem);
        //        lockMenuMain.MenuItems.Add(dockiconsMenu);

        //        var styleMenu = new MenuItem("Icon Style");
        //        string iconstyle = Viewport2.settings.GetValue("iconstyle", "colored");

        //        var styleMenuItem = new MenuItem("Colored");
        //        styleMenuItem.RadioCheck = true;
        //        styleMenuItem.Click += (s, e) => { iconstyle = "colored"; Viewport2.settings.SetValue("iconstyle", iconstyle); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        styleMenuItem.Checked = (iconstyle == "colored");
        //        styleMenu.MenuItems.Add(styleMenuItem);
        //        styleMenuItem = new MenuItem("Simple");
        //        styleMenuItem.RadioCheck = true;
        //        styleMenuItem.Click += (s, e) => { iconstyle = "simple"; Viewport2.settings.SetValue("iconstyle", iconstyle); Viewport2.settings.WritePersistentSettings(); Viewport2.UpdateViewport(true); };
        //        styleMenuItem.Checked = (iconstyle == "simple");
        //        styleMenu.MenuItems.Add(styleMenuItem);
        //        lockMenuMain.MenuItems.Add(styleMenu);

        //        contextMenu.MenuItems.Add(lockMenuMain);

        //        var displayModeMenu = new MenuItem("Display Mode");
        //        var displayModeName = DisplayModeDescription.FindByName(Viewport2.settings.GetValue("displaymode", "Wireframe"));
        //        Guid displaymode = displayModeName.Id;
        //        var modes = DisplayModeDescription.GetDisplayModes();
        //        var currentModeId = displaymode;
        //        if (Viewport.DisplayMode != null)
        //            currentModeId = Viewport.DisplayMode.Id;

        //        foreach (var mode in modes)
        //        {
        //            var modeMenuItem = new MenuItem(mode.LocalName);
        //            modeMenuItem.RadioCheck = true;
        //            modeMenuItem.Checked = (currentModeId == mode.Id);
        //            modeMenuItem.Click += (s, e) =>
        //            {
        //                Viewport.DisplayMode = mode;
        //                if (Viewport.DisplayMode.LocalName == "V-Ray Interactive" || Viewport.DisplayMode.LocalName == "Raytraced")
        //                { Viewport2.settings.SetValue("iconoffset", true); }
        //                else
        //                { Viewport2.settings.SetValue("iconoffset", false); }
        //                Viewport2.settings.SetValue("displaymode", mode.LocalName);
        //                Viewport2.settings.WritePersistentSettings();
        //                Viewport2.UpdateViewport(true);
        //                Invalidate();
        //            };
        //            displayModeMenu.MenuItems.Add(modeMenuItem);
        //            displayModeMenu.Tag = mode.Id;
        //        }
        //        contextMenu.MenuItems.Add(displayModeMenu);

        //        var viewMenu = new MenuItem("Set View");
        //        string view = Viewport2.settings.GetValue("view", "Perspective");
        //        var viewMenuItem = new MenuItem("Top");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Top, "Top", true); view = "Top"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Top");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Bottom");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Bottom, "Bottom", true); view = "Bottom"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Bottom");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Left");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Left, "Left", true); view = "Left"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Left");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Right");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Right, "Right", true); view = "Right"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Right");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Front");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Front, "Front", true); view = "Front"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Front");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Back");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Back, "Back", true); view = "Back"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Back");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("Perspective");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true); view = "Perspective"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "Perspective");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        viewMenuItem = new MenuItem("TwoPointPerspective");
        //        viewMenuItem.RadioCheck = true;
        //        viewMenuItem.Click += (s, e) => { Viewport.SetProjection(DefinedViewportProjection.TwoPointPerspective, "TwoPointPerspective", true); view = "TwoPointPerspective"; Viewport2.settings.SetValue("view", view); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        viewMenuItem.Checked = (view == "TwoPointPerspective");
        //        viewMenu.MenuItems.Add(viewMenuItem);
        //        contextMenu.MenuItems.Add(viewMenu);

        //        string dock = Viewport2.settings.GetValue("dock", "topleft");
        //        var dockMenu = new MenuItem("Dock");
        //        var mnu = new MenuItem("Top Left");
        //        mnu.RadioCheck = true;
        //        mnu.Checked = (dock == "topleft");
        //        mnu.Click += (s, args) => { Viewport2.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Left); dock = "topleft"; Viewport2.settings.SetValue("dock", dock); Viewport2.settings.WritePersistentSettings(); };
        //        dockMenu.MenuItems.Add(mnu);
        //        mnu = new MenuItem("Top Right");
        //        mnu.Checked = (dock == "topright");
        //        mnu.RadioCheck = true;
        //        mnu.Click += (s, args) => { Viewport2.DockPanel(Parent, AnchorStyles.Top | AnchorStyles.Right); dock = "topright"; Viewport2.settings.SetValue("dock", dock); Viewport2.settings.WritePersistentSettings(); };
        //        dockMenu.MenuItems.Add(mnu);
        //        mnu = new MenuItem("Bottom Left");
        //        mnu.Checked = (dock == "bottomleft");
        //        mnu.RadioCheck = true;
        //        mnu.Click += (s, args) => { Viewport2.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Left); dock = "bottomleft"; Viewport2.settings.SetValue("dock", dock); Viewport2.settings.WritePersistentSettings(); };
        //        dockMenu.MenuItems.Add(mnu);
        //        mnu = new MenuItem("Bottom Right");
        //        mnu.Checked = (dock == "bottomright");
        //        mnu.RadioCheck = true;
        //        mnu.Click += (s, args) => { Viewport2.DockPanel(Parent, AnchorStyles.Bottom | AnchorStyles.Right); dock = "bottomright"; Viewport2.settings.SetValue("dock", dock); Viewport2.settings.WritePersistentSettings(); };
        //        dockMenu.MenuItems.Add(mnu);
        //        contextMenu.MenuItems.Add(dockMenu);
        //        dockMenu.Popup += (s, args) =>
        //        {
        //            var anchor = this.Parent.Anchor;
        //            dockMenu.MenuItems[0].Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Left));
        //            dockMenu.MenuItems[1].Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Left));
        //            dockMenu.MenuItems[2].Checked = (anchor == (AnchorStyles.Top | AnchorStyles.Right));
        //            dockMenu.MenuItems[3].Checked = (anchor == (AnchorStyles.Bottom | AnchorStyles.Right));
        //        };

        //        contextMenu.MenuItems.Add("-");

        //        bool gridtoggle = Viewport2.settings.GetValue("gridtoggle", true);
        //        var grid = new MenuItem("Toggle Grid");
        //        grid.Checked = (gridtoggle);
        //        grid.Click += (s, args) => { Viewport.ConstructionGridVisible = !gridtoggle; gridtoggle = !gridtoggle; Viewport2.settings.SetValue("gridtoggle", gridtoggle); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        contextMenu.MenuItems.Add(grid);

        //        bool axestoggle = Viewport2.settings.GetValue("axestoggle", true);
        //        var gridaxes = new MenuItem("Toggle Grid Axes");
        //        gridaxes.Checked = (axestoggle);
        //        gridaxes.Click += (s, args) => { Viewport.ConstructionAxesVisible = !axestoggle; axestoggle = !axestoggle; Viewport2.settings.SetValue("axestoggle", axestoggle); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        contextMenu.MenuItems.Add(gridaxes);

        //        bool worldtoggle = Viewport2.settings.GetValue("worldtoggle", true);
        //        var worldaxes = new MenuItem("Toggle World Axes");
        //        worldaxes.Checked = (worldtoggle);
        //        worldaxes.Click += (s, args) => { Viewport.WorldAxesVisible = !worldtoggle; worldtoggle = !worldtoggle; Viewport2.settings.SetValue("worldtoggle", worldtoggle); Viewport2.settings.WritePersistentSettings(); Invalidate(); };
        //        contextMenu.MenuItems.Add(worldaxes);

        //        contextMenu.MenuItems.Add("Reset Camera", (s, e) =>
        //        {
        //            Viewport.SetCameraLocation(camLocation, true);
        //            Viewport.SetCameraTarget(camTarget, true);
        //            Refresh();
        //        });
        //        contextMenu.MenuItems.Add("Reset View", (s, e) =>
        //        {
        //            ViewReset();
        //            Viewport.SetCameraLocation(camLocation, true);
        //            Viewport.SetCameraTarget(camTarget, true);
        //            Viewport.Camera35mmLensLength = 50;
        //            Viewport.ZoomExtents();
        //            Refresh();
        //        });
        //        contextMenu.MenuItems.Add("Zoom Extents", (s, e) =>
        //        {
        //            Viewport.Camera35mmLensLength = 50;
        //            Viewport.ZoomExtents();
        //            Refresh();
        //        });

        //        contextMenu.MenuItems.Add("-");

        //        contextMenu.MenuItems.Add("Restore Defaults", (s, e) =>
        //        {
        //            Viewport2.settings.SetValue("width", 400);
        //            Viewport2.settings.SetValue("height", 300);
        //            Viewport2.settings.SetValue("dock", "topleft");
        //            Viewport2.settings.SetValue("locked1", false);
        //            Viewport2.settings.SetValue("locked2", false);
        //            Viewport2.settings.SetValue("icontoggle", false);
        //            Viewport2.settings.SetValue("iconoffset", false);
        //            Viewport2.settings.SetValue("dockicons", "topleft");
        //            Viewport2.settings.SetValue("iconstyle", "colored");
        //            Viewport2.settings.SetValue("view", "Perspective");
        //            Viewport2.settings.SetValue("displaymode", "Wireframe");
        //            Viewport2.settings.SetValue("gridtoggle", true);
        //            Viewport2.settings.SetValue("axestoggle", true);
        //            Viewport2.settings.SetValue("worldtoggle", true);
        //            Viewport2.settings.WritePersistentSettings();
        //            Viewport2.UpdateViewport(true);
        //            Viewport.DisplayMode = DisplayModeDescription.FindByName("Wireframe");
        //            Viewport.WorldAxesVisible = true;
        //            Viewport.ConstructionAxesVisible = true;
        //            Viewport.ConstructionGridVisible = true;
        //            Parent.Width = 400;
        //            Parent.Height = 300;
        //            Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true);
        //            ViewReset();
        //            Viewport.SetCameraLocation(camLocation, true);
        //            Viewport.SetCameraTarget(camTarget, true);
        //            Viewport.Camera35mmLensLength = 50;
        //            Viewport.ZoomExtents();
        //            Viewport2.DockPanel(Parent, base.Anchor);
        //            Invalidate();
        //        });
        //        contextMenu.MenuItems.Add("Close Viewport", (s, e) =>
        //        {
        //            Viewport2.viewportMenuItem.Checked = false;
        //            Viewport2.settings.SetValue("state", false);
        //            Viewport2.settings.WritePersistentSettings();
        //            Viewport2.viewportMenuItem.CheckedChanged += Viewport2.ViewportMenuItem_CheckedChanged;
        //        });
        //        contextMenu.Show(this, location);
        //    }

        //    public Rhino.Geometry.Point3d camLocation;
        //    public Rhino.Geometry.Point3d camTarget;
        //    public void CamFetcher()
        //    {
        //        camLocation = Viewport.CameraLocation;
        //        camTarget = Viewport.CameraTarget;
        //    }

        //    public void ViewReset()
        //    {
        //        string view = Viewport2.settings.GetValue("view", "Perspective");
        //        if (view == "Top")
        //        { Viewport.SetProjection(DefinedViewportProjection.Top, "Top", true); }
        //        if (view == "Bottom")
        //        { Viewport.SetProjection(DefinedViewportProjection.Bottom, "Bottom", true); }
        //        if (view == "Left")
        //        { Viewport.SetProjection(DefinedViewportProjection.Left, "Left", true); }
        //        if (view == "Right")
        //        { Viewport.SetProjection(DefinedViewportProjection.Right, "Right", true); }
        //        if (view == "Front")
        //        { Viewport.SetProjection(DefinedViewportProjection.Front, "Front", true); }
        //        if (view == "Back")
        //        { Viewport.SetProjection(DefinedViewportProjection.Back, "Back", true); }
        //        if (view == "Perspective")
        //        { Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true); }
        //        if (view == "TwoPointPerspective")
        //        { Viewport.SetProjection(DefinedViewportProjection.TwoPointPerspective, "TwoPointPerspective", true); }
        //    }
    }
}
