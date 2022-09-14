using Rhino.DocObjects;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Melanoplus
{
    public class discontinue_AttributeViewer : GH_Component
    {
        //Use Human plugin
        public override Guid ComponentGuid => new Guid("{8B143790-0397-4A63-9CAF-0CD172363C79}");
        public override GH_Exposure Exposure => GH_Exposure.hidden; //GH_Exposure.quarternary | GH_Exposure.obscure;
        protected override Bitmap Icon => base.Icon;

        //ObjectAttributes attributes = new ObjectAttributes();

        public discontinue_AttributeViewer() : base("Attribute Viewer", "Attribute",
                "Exposes attribute properties",
                "Params", "input")
        { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Attribute", "A", "Attribute to expose", GH_ParamAccess.item);

        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            dynamic att = null;
            DA.GetData(0, ref att);
            PropertyInfo[] props = att.Value.GetType().GetProperties();
            if (Params.Output.Count == 0)
            {
                if (att.Value.GetType().GetProperty("Count") == null)
                {
                    foreach (PropertyInfo prop in props.Where(p => p.CanRead))
                    {
                        Params.Output.Add(new Param_GenericObject { Name = prop.Name, NickName = prop.Name, Access = GH_ParamAccess.item });
                    }
                }
                else
                {
                    Params.Output.Add(new Param_GenericObject { Name = att.Value.GetType().Name, NickName = att.Value.GetType().Name, Access = GH_ParamAccess.list });
                }
                Params.OnParametersChanged();
                OnPingDocument().ScheduleSolution(5, (s) => ExpireSolution(true));
            }
            else
            {
                if (att.Value.GetType().GetProperty("Count") == null)
                {
                    foreach (PropertyInfo prop in props.Where(p => p.CanRead))
                    {
                        if (att.Value != null)
                            DA.SetData(prop.Name, prop.GetValue(att.Value));
                    }
                }
                else
                {
                    List<dynamic> output = new List<dynamic>();
                    for (int i = 0; i < att.Value.Count; i++)
                        if (att.Value[i] != null)
                            output.Add(att.Value[i]);

                    DA.SetDataList(att.Value.GetType().Name, output);

                }
            }
        }
    }
}
