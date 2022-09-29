﻿using Grasshopper.Kernel;
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
using Rhino.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime;
//REF: https://github.com/mcneel/GhViewportWidget

namespace Melanoplus
{
    public class ViewportWidget : GH_Widget
    {
        public override string Name => "Rhino Viewport";
        public override string Description => "Rhino viewport inside Grasshopper";
        public override Bitmap Icon_24x24 => Properties.Resources.viewportRhino;
        public override bool Visible
        {
            get => enabled;
            set
            {
                enabled = value;
                if (value)
                {
                    if (_viewportControlPanel == null)
                        CreateView();
                    Instances.ActiveCanvas.Controls.Add(_viewportControlPanel);
                    if (_viewportControlPanel.Size.Width >= Instances.ActiveCanvas.Width
                        || _viewportControlPanel.Size.Height >= Instances.ActiveCanvas.Height)
                        _viewportControlPanel.Size = new Size(Instances.ActiveCanvas.Width / 2, Instances.ActiveCanvas.Height / 2);
                    _viewportControlPanel.Show();
                    settings.SetValue("enabled", true);
                    settings.WritePersistentSettings();
                }
                else
                {
                    Instances.ActiveCanvas.Controls.Remove(_viewportControlPanel);
                    _viewportControlPanel = null;
                    settings.SetValue("enabled", false);
                    settings.WritePersistentSettings();
                }
            }
        }
        static Panel _viewportControlPanel;
        static RhinoWindows.Forms.Controls.ViewportControl ctrl;
        static GH_SettingsServer settings = new GH_SettingsServer("melanoplus_viewport", true);
        static bool enabled = settings.GetValue("enabled", false);

        void CreateView()
        {
            _viewportControlPanel = new ViewportContainerPanel()
            {
                Size = settings.GetValue("size", new Size(Global_Proc.UiAdjust(400), Global_Proc.UiAdjust(300))),
                MinimumSize = new Size(Global_Proc.UiAdjust(50), Global_Proc.UiAdjust(50)),
                Padding = new Padding(Global_Proc.UiAdjust(10)),
                BorderStyle = BorderStyle.FixedSingle,
                Location = settings.GetValue("anchor", new Point(0, 0)),
            };
            ctrl = new ViewportControl()
            {
                Dock = DockStyle.Fill,
            };
            _viewportControlPanel.Controls.Add(ctrl);
        }

        public ViewportWidget()
        {
            if (enabled && _viewportControlPanel == null)
                CreateView();            
        }
        internal static void CanvasCreated(GH_Canvas canvas)
        {
            if (enabled)
            {
                canvas.Controls.Add(_viewportControlPanel);
                _viewportControlPanel.Visible = true;
            }
        }

        public override void Render(GH_Canvas Canvas) { }

        public override bool Contains(Point pt_control, PointF pt_canvas) => false;

        /// <summary>
        /// Panel with a "re-sizable" border that contains a viewport control
        /// </summary>
        class ViewportContainerPanel : Panel
        {
            System.Drawing.Point LeftMouseDownLocation { get; set; }
            System.Drawing.Size LeftMouseDownSize { get; set; }
            enum Mode
            {
                None,
                SizeWE,
                SizeNS,
                SizeNWSE,
                SizeNESW,
                Move
            }
            Mode _mode;
            public override Cursor Cursor
            {
                get
                {
                    var location = PointToClient(MousePosition);
                    switch (ComputeMode(location))
                    {
                        case Mode.None:
                            return Cursors.Default;
                        case Mode.Move:
                            return Cursors.SizeAll;
                        case Mode.SizeNESW:
                            return Cursors.SizeNESW;
                        case Mode.SizeNS:
                            return Cursors.SizeNS;
                        case Mode.SizeWE:
                            return Cursors.SizeWE;
                        case Mode.SizeNWSE:
                            return Cursors.SizeNWSE;
                    }
                    return base.Cursor;
                }
                set => base.Cursor = value;
            }

            Mode ComputeMode(System.Drawing.Point location)
            {
                if (location.X > (Width - Padding.Right))
                    return location.Y > (Height - Padding.Bottom) ? Mode.SizeNWSE : Mode.SizeWE;
                if (location.Y > (Height - Padding.Bottom))
                    return Mode.SizeNS;
                if (location.X < Padding.Left || location.Y < Padding.Top)
                    return Mode.Move;
                return Mode.None;
            }

            protected override void OnMouseDown(MouseEventArgs e)
            {
                _mode = Mode.None;
                if (e.Button == MouseButtons.Left)
                {
                    _mode = ComputeMode(e.Location);
                    LeftMouseDownLocation = e.Location;
                    LeftMouseDownSize = Size;
                }
                base.OnMouseDown(e);
            }
            protected override void OnMouseMove(MouseEventArgs e)
            {
                if (_mode != Mode.None)
                {
                    int x = Location.X;
                    int y = Location.Y;
                    int width = Width;
                    int height = Height;

                    int deltaX = e.X - LeftMouseDownLocation.X;
                    int deltaY = e.Y - LeftMouseDownLocation.Y;
                    switch (_mode)
                    {
                        case Mode.SizeNS:
                        case Mode.SizeNESW:
                        case Mode.SizeNWSE:
                            height = LeftMouseDownSize.Height + deltaY;
                            if (Location.Y + height > Instances.ActiveCanvas.Height)
                                height = Instances.ActiveCanvas.Height - Location.Y;
                            goto case Mode.SizeWE;
                        case Mode.SizeWE:
                            width = LeftMouseDownSize.Width + deltaX;
                            if (Location.X + width > Instances.ActiveCanvas.Width)
                                width = Instances.ActiveCanvas.Width - Location.X;
                            SetBounds(x, y, width, height);
                            break;
                        case Mode.Move:
                            Location = new Point(Location.X + deltaX, Location.Y + deltaY);
                            break;
                    }
                }
                base.OnMouseMove(e);
            }
            protected override void OnMouseUp(MouseEventArgs e)
            {
                if (_mode == Mode.Move && e.Button == MouseButtons.Left)
                {
                    int x = Location.X >= Instances.ActiveCanvas.Width ?
                                Instances.ActiveCanvas.Width - Width - Padding.Right :
                                Location.X <= 0 ? 10 : Location.X,
                        y = Location.Y >= Instances.ActiveCanvas.Height ?
                                Instances.ActiveCanvas.Height - Height - Padding.Bottom :
                                Location.Y <= 0 ? 10 : Location.Y;
                    SetBounds(x, y, Width, Height);
                }
                _mode = Mode.None;
                settings.SetValue("size", Size);
                settings.SetValue("anchor", Bounds.Location);
                base.OnMouseUp(e);
            }
        }
    }

    class ViewportControl : RhinoWindows.Forms.Controls.ViewportControl
    {
        System.Drawing.Point RightMouseDownLocation { get; set; }
        public ViewportControl()
        {
            // stupid hack to get GH to draw preview geometry in this control
            this.Viewport.Name = "GH_HACK";

            Viewport.DisplayMode = DisplayModeDescription.FindByName("Shaded");
            Viewport.ConstructionGridVisible = true;
            Viewport.ConstructionAxesVisible = true;
            Viewport.WorldAxesVisible = true;
            Viewport.SetProjection(DefinedViewportProjection.Perspective, "Perspective", true);

        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            Rhino.Geometry.Vector3d vec = Viewport.CameraDirection;
            Viewport.SetCameraLocation(Viewport.CameraLocation + vec * e.Delta,false);
            Refresh();
            //base.OnMouseWheel(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                RightMouseDownLocation = e.Location;
            else
                RightMouseDownLocation = System.Drawing.Point.Empty;
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var vec = new Rhino.Geometry.Vector2d(e.X - RightMouseDownLocation.X, e.Y - RightMouseDownLocation.Y);
                if (vec.Length > 10)
                    RightMouseDownLocation = System.Drawing.Point.Empty;
            }
            else
                RightMouseDownLocation = System.Drawing.Point.Empty;
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Right && RightMouseDownLocation != System.Drawing.Point.Empty)
                ShowContextMenu(e.Location);
            RightMouseDownLocation = System.Drawing.Point.Empty;
        }
        void ShowContextMenu(System.Drawing.Point location)
        {
            var contextMenu = new ContextMenu();

            var displayModeMenu = new MenuItem("Display Mode");
            var modes = Rhino.Display.DisplayModeDescription.GetDisplayModes();
            var currentModeId = Guid.Empty;
            if (Viewport.DisplayMode != null)
                currentModeId = Viewport.DisplayMode.Id;

            foreach (var mode in modes)
            {
                var modeMenuItem = new MenuItem(mode.LocalName);
                modeMenuItem.RadioCheck = true;
                modeMenuItem.Checked = (currentModeId == mode.Id);
                modeMenuItem.Click += (s, e) =>
                {
                    Viewport.DisplayMode = mode;
                    Invalidate();
                };
                displayModeMenu.MenuItems.Add(modeMenuItem);
                displayModeMenu.Tag = mode.Id;
            }
            contextMenu.MenuItems.Add(displayModeMenu);

            contextMenu.MenuItems.Add("Zoom Extents", (s, e) =>
            {
                Viewport.Camera35mmLensLength = 50;
                Viewport.ZoomExtents();
                Refresh();
            });
            contextMenu.MenuItems.Add("Hide", (s, e) =>
            {
                this.Parent.Hide();
            });
            contextMenu.Show(this, location);

        }
    }
}
