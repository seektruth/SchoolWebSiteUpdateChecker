using System;
using System.IO;


/*
 等待解决的问题：
 1.如何实现开机启动
 2.网络错误处理
 3.菜单设计
 4.sleep
     */
namespace SchoolWebsiteUpdateChecker
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// [STAThread]
        static void Main()
        {
            var testChecker = new BksyChecker();
            testChecker.check();
        }
    }
}
