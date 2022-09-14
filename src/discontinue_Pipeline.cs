using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
using Grasshopper.Kernel.Types;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Melanoplus
{
    public class discontinue_Pipeline : GH_Component
    {
        //Use Human plugin
        public override Guid ComponentGuid => new Guid("{0E750FE4-C32B-4411-B9C3-A2AAEC7CF1A1}");
        public override GH_Exposure Exposure => GH_Exposure.hidden; //GH_Exposure.quarternary | GH_Exposure.obscure;
        protected override Bitmap Icon => base.Icon;

        readonly Array ObjTypes = Enum.GetNames(typeof(ObjectType));

        public discontinue_Pipeline() : base("Pipeline", "Get", 
            "Pipeline to get objects from Rhino", 
            "Params", "Input") { }
        
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type", "T", "Type of data to get", GH_ParamAccess.item);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Attributes", null, null, GH_ParamAccess.list);
            pManager.AddGenericParameter("Geometry", null, null, GH_ParamAccess.list);
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string selected = "";
            DA.GetData("Type",ref selected);
            if (selected == "None") return;
            RhinoObject[] rObj = Rhino.RhinoDoc.ActiveDoc.Objects.FindByObjectType((ObjectType)Enum.Parse(typeof(ObjectType), selected)); //.Where(t => t.ObjectType == ObjectType.Annotation).Cast<TextObject>().ToList();
            List<IGH_Goo> refObj = new List<IGH_Goo>();
            foreach(var o in rObj)
                refObj.Add(GH_Convert.ObjRefToGeometry(new ObjRef(o)));
            DA.SetDataList(0, rObj.Select(o => o.Attributes));
            DA.SetDataList(1, refObj);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            if (this.Params.Input[0].SourceCount == 0)
            {
                var picker = new GH_ValueList();
                picker.CreateAttributes();
                picker.Attributes.Pivot = new PointF(
                    this.Attributes.Pivot.X - this.Attributes.Bounds.Width - picker.Attributes.Bounds.Width,
                    this.Attributes.Pivot.Y - picker.Attributes.Bounds.Height
                    );
                document.AddObject(picker, false);
                this.Params.Input[0].AddSource(picker);
                picker.ListItems.Clear();
                foreach (string ot in ObjTypes)
                {
                    picker.ListItems.Add(new GH_ValueListItem(ot, $"\"{ot}\""));
                }
                picker.ListItems[0].Selected = true;
            }
        }
    }
}
