using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Melanoplus.Component
{
    public class Fuse : GH_Component
    {
        //Unable to cancel current working component, can only abort after, effectiveness limited.
        public override Guid ComponentGuid => new Guid("{ABB71BD7-CE6C-4207-8B7A-80F20EB3AD2B}");
        public override GH_Exposure Exposure => GH_Exposure.hidden; //GH_Exposure.secondary;
        protected override Bitmap Icon => base.Icon;//Properties.Resources.TextOutline;
        public Fuse() : base("Fuse", "Fuse",
            "Attempts to abort solution if it takes too long.",
            "Params", "Util")
        { }

        static int duration = 500;
        static System.Timers.Timer t = new System.Timers.Timer(duration);

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Sec",null,"Theshold for abort", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.GetData(0, ref duration);
        }

        public override void AddedToDocument(GH_Document document)
        {
            base.AddedToDocument(document);
            document.SolutionStart -= fuse;
            document.SolutionEnd -= fuseE;
            document.SolutionStart += fuse;
            document.SolutionEnd += fuseE;
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            document.SolutionStart -= fuse;
            document.SolutionEnd -= fuseE;
        }

        private void fuse(Object sender, GH_SolutionEventArgs e)
        {
            t.Elapsed -= abort;
            t.Elapsed += abort;
            t.AutoReset = false;
            t.Interval = duration;
            t.Stop();
            if(!this.Locked)
                t.Start();

        }
        private void fuseE(Object sender, GH_SolutionEventArgs e)
        {
            t.Stop();
        }
        static private void abort(Object source, System.Timers.ElapsedEventArgs e)
        {
            Grasshopper.Instances.ActiveCanvas.Document.RequestAbortSolution();
            new Thread(() => {
                Thread.CurrentThread.IsBackground = true;
                System.Windows.Forms.MessageBox.Show("Fuse blown, High load detected");
            }).Start();
            //int n1, n2;
            //Grasshopper.Instances.ActiveCanvas.Document.SolutionProgress(out n1, out n2);
            //((GH_ActiveObject)Grasshopper.Instances.ActiveCanvas.Document.Objects[n1]).Locked = true;
            //((GH_ActiveObject)Grasshopper.Instances.ActiveCanvas.Document.Objects[n1]).Phase = GH_SolutionPhase.Failed;
            //Grasshopper.Instances.ActiveCanvas.Document.RemoveObject(Grasshopper.Instances.ActiveCanvas.Document.Objects[n1],false);
        }
    }
}
