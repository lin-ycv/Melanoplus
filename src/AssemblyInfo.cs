namespace Melanoplus
{
    public class MelanoplusInfo : GH_AssemblyInfo
    {
        public override string Name => "Melanoplus";
        public override Bitmap Icon => Properties.Resources.MelanoplusSimple;
        public override string Description => "Collection of Utilities";
        public override Guid Id => new("F6312EC6-3C80-4959-A5A9-E17DB6063543");
        public override string AuthorName => "Victor (Yu Chieh) Lin";
        public override string AuthorContact => "https://github.com/lin-ycv";
#if NET7_0_OR_GREATER
        public override string Version => _v.ToString();
#else
        public override string Version => $"{_v.Major}.0.{_v.Build}";
#endif
        private readonly Version _v = Assembly.GetExecutingAssembly().GetName().Version;
    }
}