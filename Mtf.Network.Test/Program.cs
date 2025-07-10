using System;
using System.Text;
using System.Windows.Forms;

namespace Mtf.Network.Test
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
#if NETCOREAPP
            ApplicationConfiguration.Initialize();
#endif
            Application.Run(new MainForm());
        }
    }
}