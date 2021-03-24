using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QQPlugiPoke
{
    static class Program
    {
        private static string ErrorPath = "";
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        /// 
        [STAThread]
       
        static void Main()
        {
            ErrorPath = Application.StartupPath + @"\error.txt";
              Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            //处理UI线程异常
               Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            //处理非UI线程异常
              AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Set());
        }
        private static void LogHelper(string msg, int code)
        {

            if (false == System.IO.Directory.Exists(ErrorPath))
            {
                System.IO.Directory.CreateDirectory(ErrorPath);
            }
            using (StreamWriter fs = new StreamWriter(ErrorPath + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", true))
            {
                fs.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "][" + code + "]" + msg + "\r\n");
            }
        }
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogHelper("Application_ThreadException:" +
                e.Exception.Message, 2);
            LogHelper(e.Exception.ToString(), 2);
            LogHelper(e.ToString(), 2);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogHelper("CurrentDomain_UnhandledException", 1);
            LogHelper("IsTerminating : " + e.IsTerminating.ToString(), 1);
            LogHelper(e.ExceptionObject.ToString(), 1);
            LogHelper(e.ToString(), 1);
            System.Environment.Exit(0);
        }
    }
}
