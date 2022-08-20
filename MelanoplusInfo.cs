using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Melanoplus
{
    public class MelanoplusInfo : GH_AssemblyInfo
    {
        public override string Name => "Melanoplus";
        public override Bitmap Icon => Properties.Resources.MelanoplusSimple;
        public override string Description => "Collection of Utilities";
        public override Guid Id => new Guid("F6312EC6-3C80-4959-A5A9-E17DB6063543");
        public override string AuthorName => "Victor (Yu Chieh) Lin";
        public override string AuthorContact => "https://github.com/lin-ycv";
        public override string Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}
