// path: WindowsSidekick/Program.cs
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WindowsSidekick
{

    internal static class Program
    {

        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main()
        {
            SetProcessDPIAware();
            Application.EnableVisualStyles();
            ApplicationConfiguration.Initialize();
            Application.Run(new TrayApplicationContext());
        }
    }
}
