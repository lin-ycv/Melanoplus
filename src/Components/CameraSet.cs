namespace Melanoplus.Components
{
    public class CameraSet : GH_Component
    {
        public override Guid ComponentGuid => new("{37C3E9E6-D219-4D02-AF3A-04FFF171AD93}");
        protected override Bitmap Icon => Properties.Resources.CameraFill;
        public CameraSet() : base("Set Camera", "Camera",
            "Set Rhino camera properties.",
            "Melanoplus", "Display")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Camera", null, "Get viewport camera location.", GH_ParamAccess.item);
            pManager.AddPointParameter("Target", null, "Get camera target point.", GH_ParamAccess.item);
            pManager.AddVectorParameter("Up direction", "Up", "Get up direction of camera", GH_ParamAccess.item);
            pManager.AddNumberParameter("Lens length", "Lens", "35mm equivalent focal lens", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            RG.Point3d camera = new(), target = new();
            RG.Vector3d up = new();
            double lens = new();

            DA.GetData(0, ref camera);
            DA.GetData(1, ref target);
            DA.GetData(2, ref up);
            DA.GetData(3, ref lens);

            RhinoViewport vp = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport;
            vp.SetCameraLocations(target, camera);
            vp.CameraUp = up;
            vp.Camera35mmLensLength = lens;
        }
        public override void CreateAttributes()
        {
            m_attributes = new CamGetAttributes(this);
        }
    }
}
