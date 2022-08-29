using Grasshopper;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.GUI.SettingsControls;
using Grasshopper.Kernel;
using Rhino.NodeInCode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Melanoplus
{
    public class ColorCanvas : IGH_SettingFrontend
    {
        public string Category => "Palette";

        public string Name => "Canvas";

        public IEnumerable<string> Keywords => new string[1] { "Background" };

        public Control SettingsUI() => new GH_ColorCanvasSettinFrontEnd();
        //new GH_GenericCapsulePaletteSettings() {};

        public class GH_ColorCanvasSettinFrontEnd : UserControl
        {
            private IContainer components;
            private GH_ColourSwatchControl swatchGrid, swatchBack, swatchEdge, swatchShade, swatchMono;
            private ToolTip ToolTip;
            public GH_ColorCanvasSettinFrontEnd()
            {
                components = new Container();
                ToolTip = new ToolTip(components);
                swatchBack = new GH_ColourSwatchControl() 
                {
                    AllowDrop = true,
                    Colour = GH_Skin.canvas_back,
                    Location = new Point(0, 0),
                    Name = "swatchBack",
                    Size = new Size(20, 20),
                    TabIndex = 0,
                };
                swatchBack.ColourChanged += (s, e) => { GH_Skin.canvas_back = swatchBack.Colour; Instances.ActiveCanvas.Refresh(); };
                swatchEdge = new GH_ColourSwatchControl()
                {
                    //AllowDrop = true,
                    Colour = GH_Skin.canvas_edge,
                    Location = new Point(19, 0),
                    Name = "swatchEdge",
                    Size = new Size(20, 20),
                    //TabIndex = 1,
                };
                SuspendLayout();
                ToolTip.SetToolTip(swatchBack, "Set the colour used to draw Background");
                ToolTip.SetToolTip(swatchEdge, "Set the colour used to draw Edges");
                base.AutoScaleDimensions = new SizeF(6f, 13f);
                base.AutoScaleMode = AutoScaleMode.Font;
                base.Controls.Add(swatchBack);
                base.Controls.Add(swatchEdge);
                base.Name = "GH_GenericCapsulePaletteSettings";
                base.Size = new Size(293, 58);
                ResumeLayout(false);
            }

                //GH_ColourPicker

                //Defaults
                //GH_Skin.canvas_grid = Color.FromArgb(30, 0, 0, 0);
                //GH_Skin.canvas_back = Color.FromArgb(255, 212, 208, 200);
                //GH_Skin.canvas_edge = Color.FromArgb(255, 0, 0, 0);
                //GH_Skin.canvas_shade = Color.FromArgb(80, 0, 0, 0);
                //GH_Skin.canvas_grid_col = 150;
                //GH_Skin.canvas_grid_row = 50;
                //GH_Skin.canvas_mono = false;
                //GH_Skin.canvas_mono_color = Color.FromArgb(255, 255, 255, 255);
                //GH_Skin.canvas_shade_size = 30;
        }
    
    }
}
