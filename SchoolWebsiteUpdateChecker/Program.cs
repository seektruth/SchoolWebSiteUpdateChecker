using System;
using System.IO;
using System.Windows.Forms;


namespace SchoolWebsiteUpdateChecker
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Directory.SetCurrentDirectory(Application.StartupPath);
            new MenuController();
            Application.Run();
        }
    }
}
