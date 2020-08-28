using System;
using System.Collections.Generic;
using System.Windows.Forms;



namespace DALSA.SaperaLT.Demos.NET.CSharp.MultiBoardSyncGrabDemo
{
    static class MultiBoardSyncGrabDemo
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

           // Application.Run(new ParamSettingForm());
            Application.Run(new Form1());
            // MultiBoardSyncGrabDemoDlg form = new MultiBoardSyncGrabDemoDlg();
          // if (!form.IsDisposed)
           //  Application.Run(form);
        }
    }
}