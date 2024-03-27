namespace Melanoplus.Components
{
    public class RoundToDecimal : GH_Component
    {
        public override Guid ComponentGuid => new("{EFD86A03-6D94-48E8-9E80-382FBE65154C}");
        public override GH_Exposure Exposure => GH_Exposure.secondary;
        protected override Bitmap Icon => Properties.Resources.RoundToDecimal;
        public RoundToDecimal() : base(
            "Round (Decimal)", "Round",
            "Round to a specific decimal place (default: Rhino tolerance)",
            "Melanoplus", "Data")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Numbers", "x", "Number(s) to round", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Decimals", "d", "Number of decimals to keep", GH_ParamAccess.item, BitConverter.GetBytes(decimal.GetBits((decimal)DocumentTolerance())[3])[2]);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Result", "R", "", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<double> numbers = [];
            int decimals = 0;
            DA.GetDataList(0, numbers);
            DA.GetData(1, ref decimals);

            DA.SetDataList(0, numbers.Select(n => Math.Round(n, decimals)));
        }
    }
}
