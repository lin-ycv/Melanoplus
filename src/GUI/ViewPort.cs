//using Rhino.UI.Controls;
//namespace Melanoplus.GUI
//{
//    internal class ViewPort
//    {
//        internal static void Init()
//        {
//            //RhinoViewport rv = new();
//            ViewportControl vpc = new()
//            {
//                Size = new(400, 400),                
//            };

//            //EF.Form form = new()
//            //{
//            //    ShowInTaskbar = true,
//            //    Owner = Instances.EtoDocumentEditor,
//            //    MinimumSize = new(400, 400),
//            //};
//            //EF.DynamicLayout layout = new();
//            //EF.Button button = new()
//            //{
//            //    Text = "Sync",
//            //    Height = 30,
//            //};
//            //button.Click += (sender, e) =>
//            //{
//            //    vpc.Viewport.SetCameraDirection(RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraDirection, true);
//            //    vpc.Viewport.SetCameraTarget(RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraTarget, true);
//            //    vpc.Viewport.SetCameraLocation(RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.CameraLocation, true);
//            //    vpc.Viewport.Camera35mmLensLength = RhinoDoc.ActiveDoc.Views.ActiveView.ActiveViewport.Camera35mmLensLength;
//            //    vpc.Refresh();
//            //};
//            //layout.AddRow(button);
//            //layout.AddRow(vpc);
//            //form.Content = layout;
//            //form.Show();

//            //Control panel = FindPanel();
//            //panel.Controls.Add(new Splitter());
//            //ViewportControl vpc = new();
//            //foreach(Control c in ((dynamic)vpc).Child.Controls)
//            //    panel.Controls.Add(c);
//        }
//        //private static Control FindPanel()
//        //{
//        //    foreach (Control c in Instances.DocumentEditor.Controls)
//        //        foreach (Control e in c.Controls)
//        //            if (e.Name == "Canvas")
//        //                return c;

//        //    return null;
//        //}
//    }
//}
