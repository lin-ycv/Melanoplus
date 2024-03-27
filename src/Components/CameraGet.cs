namespace Melanoplus.Components
{
    public class CameraGet : GH_Component
    {
        public override Guid ComponentGuid => new("{80C689F8-9C9F-4CFB-8FAD-DA15578E7460}");
        protected override Bitmap Icon => Properties.Resources.CameraOutline;
        public CameraGet() : base("Get Camera", "Camera",
            "Get Rhino camera properties.",
            "Melanoplus", "Display")
        { }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("⠀", null, "Something to expire this component", GH_ParamAccess.item);
            pManager[0].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Camera", null, "Get viewport camera location.", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", null, "Get camera target point.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Up direction", "Up", "Get up direction of camera", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens length", "Lens", "35mm equivalent focal lens", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            RG.Point3d camera = vp.CameraLocation, target = vp.CameraTarget;
            RG.Vector3d up = vp.CameraUp;
            double lens = vp.Camera35mmLensLength;

            DA.SetData(0, camera);
            DA.SetData(1, target);
            DA.SetData(2, up);
            DA.SetData(3, lens);
        }
        public override void CreateAttributes()
        {
            m_attributes = new CamGetAttributes(this);
        }
    }
    public class CamGetAttributes(IGH_Component component) : GH_ComponentAttributes(component)
    {
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            base.Owner.ExpireSolution(true);
            return GH_ObjectResponse.Handled;
        }
    }
}
