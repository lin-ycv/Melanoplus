using Grasshopper.Kernel;
using Grasshopper.Kernel.Special;
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
    public class WIP_Pipeline : GH_Component
    {
        public WIP_Pipeline() : base("Pipeline", "Get", "Pipeline to get objects from Rhino", "Params", "Input") { }
        public override GH_Exposure Exposure => GH_Exposure.hidden; //GH_Exposure.quarternary | GH_Exposure.obscure;
        public override Guid ComponentGuid => new Guid("{0E750FE4-C32B-4411-B9C3-A2AAEC7CF1A1}");
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

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Type", "T", "Type of data to get", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Attributes", null, null, GH_ParamAccess.list);
            pManager.AddGenericParameter("Geometry", null, null, GH_ParamAccess.list);
        }

        readonly Array ObjTypes = Enum.GetNames(typeof(ObjectType));
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Doesn't work with object details component rhino object->ref??
            string selected = "";
            DA.GetData("Type",ref selected);
            RhinoObject[] rObj = Rhino.RhinoDoc.ActiveDoc.Objects.FindByObjectType((ObjectType)Enum.Parse(typeof(ObjectType), selected)); //.Where(t => t.ObjectType == ObjectType.Annotation).Cast<TextObject>().ToList();
            DA.SetDataList(0, rObj.Select(o => o.Attributes));
            DA.SetDataList(1, rObj.Select(o => o.Geometry));
        }
    }
}
