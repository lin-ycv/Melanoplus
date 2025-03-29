using Grasshopper.Kernel.Parameters;

namespace Melanoplus.Components
{
#if NET7_0_OR_GREATER
    public class TextOutline : IOComponents.ZuiComponent
#else
    public class TextOutline : GH_Component, IGH_VariableParameterComponent
#endif
    {
        public override Guid ComponentGuid => new("{7A4DF24D-8664-4627-B756-65A732708152}");
        protected override Bitmap Icon => Properties.Resources.TextOutline;
        public TextOutline() : base("Text Outline", "TraceTxt",
            "Creates curve outlines of text.",
            "Melanoplus", "Curves")
        { }
#if NET7_0_OR_GREATER
        protected override ParamDefinition[] Inputs => _inputs;
        private readonly ParamDefinition[] _inputs =
        {
            new(new Param_String(){Name = "Text", NickName ="Txt", Description ="The text to convert to outline",  Access =GH_ParamAccess.item}, ParamRelevance.Binding),
            new(new Param_Plane(){Name = "plane", NickName ="pln", Description ="Location of text outline",  Access = GH_ParamAccess.item, Optional = true}, ParamRelevance.Secondary),
            new(new Param_String(){Name = "font", NickName ="ttf", Description ="TrueType front to use",  Access =GH_ParamAccess.item , Optional=true}, ParamRelevance.Secondary),
            new(new Param_Number(){Name = "height", NickName ="h", Description ="Height of text",  Access =GH_ParamAccess.item, Optional=true}, ParamRelevance.Secondary),
            new(new Param_Integer(){Name = "style", NickName ="s", Description ="Font style",  Access =GH_ParamAccess.item, Optional=true}, ParamRelevance.Tertiary),
            new(new Param_Number(){Name = "scale", NickName ="ls", Description ="Display lower case as small upper case when a scale is provided",  Access =GH_ParamAccess.item, Optional=true}, ParamRelevance.Tertiary)
        };

        protected override ParamDefinition[] Outputs => _outputs;
        private readonly ParamDefinition[] _outputs =
        {
            new(new Param_Curve(){Name = "Curves", NickName ="C", Description ="The resulting curves",  Access =GH_ParamAccess.list}, ParamRelevance.Binding)
        };
#else
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Text", "Txt", "The text to convert to outline", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curves", "C", "The resulting curves", GH_ParamAccess.list);
        }
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index >= 1 && index <= 5)
                return true;
            return false;
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && index >= 1 && index <= 5)
                return true;
            return false;
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            switch (index)
            {
                case 1:
                    if(Params.Input.Any(p=>p.Name == "plane")) goto default;
                    Param_Plane param1 = new() { Name = "plane", NickName = "pln", Description = "Location of text outline", Access = GH_ParamAccess.item, Optional = true };
                    param1.SetPersistentData(new GH_Plane(RG.Plane.WorldXY));
                    return param1;
                case 2:
                    if(Params.Input.Any(p=>p.Name == "font")) goto case 3;
                    Param_String param2 = new() { Name = "font", NickName = "ttf", Description = "TrueType front to use", Access = GH_ParamAccess.item, Optional = true };
                    param2.SetPersistentData(new GH_String("Arial"));
                    return param2;
                case 3:
                    if(Params.Input.Any(p=>p.Name == "height")) goto case 4;
                    Param_Number param3 = new() { Name = "height", NickName = "h", Description = "Height of text", Access = GH_ParamAccess.item, Optional = true };
                    param3.SetPersistentData(new GH_Number(12));
                    return param3;
                case 4:
                    if(Params.Input.Any(p=>p.Name == "style")) goto case 5;
                    Param_Integer param4 = new() { Name = "style", NickName = "s", Description = "Font style", Access = GH_ParamAccess.item, Optional = true };
                    param4.SetPersistentData(new GH_Integer(0));
                    return param4;
                case 5:
                    if(Params.Input.Any(p=>p.Name == "scale")) goto default;
                    Param_Number param5 = new() { Name = "scale", NickName = "ls", Description = "Display lower case as small upper case when a scale is provided", Access = GH_ParamAccess.item, Optional = true };
                    param5.SetPersistentData(new GH_Number(double.NaN));
                    return param5;
                default:
                    return null;
            }
        }

        public bool DestroyParameter(GH_ParameterSide side, int index) => true;

        public void VariableParameterMaintenance()
        {
        }
#endif

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string text = "", font = "";
            double height = 12, lowerScale = double.NaN, tolerance = DocumentTolerance();
            int style = 0;
            RG.Plane plane = RG.Plane.WorldXY;
            DA.GetData(0, ref text);
#if NET7_0_OR_GREATER
            TryGetData<RG.Plane>(DA, "plane", out var _plane);
            TryGetData<string>(DA, "font", out var _font);
            TryGetData<double>(DA, "height", out var _height);
            TryGetData<int>(DA, "style", out var _style);
            TryGetData<double>(DA, "scale", out var _lowerScale);
            plane = _plane ?? plane;
            font = _font ?? font;
            height = _height ?? height;
            style = _style ?? style;
            lowerScale = _lowerScale ?? lowerScale;
#else           
            if(Params.Input.Any(p => p.Name == "plane"))DA.GetData("plane", ref plane);
            if(Params.Input.Any(p => p.Name == "font"))DA.GetData("font", ref font);
            if(Params.Input.Any(p => p.Name == "height"))DA.GetData("height", ref height);
            if(Params.Input.Any(p => p.Name == "style"))DA.GetData("style", ref style);
            if(Params.Input.Any(p => p.Name == "scale"))DA.GetData("scale", ref lowerScale);
#endif
            DA.SetDataList(0, RG.Curve.CreateTextOutlines(text, font, height, style, true, plane, lowerScale, tolerance));
        }
        public override void AppendAdditionalMenuItems(ToolStripDropDown menu)
        {
            Menu_AppendSeparator(menu);
            Menu_AppendItem(menu, "List usable fonts", ListFonts);
            base.AppendAdditionalMenuItems(menu);
        }
        private void ListFonts(object sender, EventArgs e)
        {
            // Value list is laggy with too many items
            //GH_ValueList list = new();
            //list.CreateAttributes();
            //list.Attributes.Pivot = new PointF(
            //            Attributes.Pivot.X - list.Attributes.Bounds.Width - Attributes.Bounds.Width / 2,
            //            Attributes.Pivot.Y - list.Attributes.Bounds.Height);
            //list.ListItems.Clear();
            //list.ListItems.AddRange(FontFamily.Families.Select(i => new GH_ValueListItem(i.Name, $"\"{i.Name}\"")));
            //if (list.ListItems.Count > 0)
            //    list.ListItems[0].Selected = true;
            //Instances.ActiveCanvas.Document.AddObject(list, true);

            GH_Panel panel = new();
            panel.CreateAttributes();
            panel.Attributes.Pivot = new PointF(
                this.Attributes.Pivot.X - this.Attributes.Bounds.Width - panel.Attributes.Bounds.Width,
                this.Attributes.Pivot.Y - panel.Attributes.Bounds.Height
                );
            panel.UserText = string.Join("\r\n", FontFamily.Families.Select(f => f.Name).ToArray());
            panel.Properties.Multiline = false;
            panel.NickName = "Usable fonts";
            Instances.ActiveCanvas.Document.AddObject(panel, true);
        }
    }
}