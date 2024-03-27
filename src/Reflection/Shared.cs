namespace Melanoplus.Reflection
{
    internal class R8
    {
        internal static void UseRhinoStyle(dynamic eto)
        {
            if (RhinoApp.ExeVersion < 8) return;

            Assembly assembly = Assembly.Load("Rhino.UI");
            Type type = assembly.GetType("Rhino.UI.EtoExtensions");
            switch(eto)
            {
                case EF.Dialog:
                    MethodInfo method = type.GetMethod("UseRhinoStyle", [typeof(EF.Dialog)]);
                    goto final;
                case EF.FloatingForm:
                    method = type.GetMethod("UseRhinoStyle", [typeof(EF.FloatingForm)]);
                    goto final;
                final:
                    method?.Invoke(null, [eto]);
                    break;
            }
        }
    }
}
