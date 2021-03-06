﻿/******************************************************
 *     ROMVault2 is written by Gordon J.              *
 *     Contact gordon@romvault.com                    *
 *     Copyright 2014                                 *
 ******************************************************/
using System;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ROMVault2
{
    static class Program
    {
        public static readonly Encoding Enc = Encoding.GetEncoding(28591);
        public const string Version = "2.2";
        public const string SubVersion = "4b";
        
        public static SynchronizationContext SyncCont;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            Application.ThreadException += ReportError.UnhandledExceptionHandler;
#endif

            FrmSplashScreen progress = new FrmSplashScreen();
            progress.ShowDialog();

            progress.Dispose();
                       
            Application.Run(new FrmMain());
        }
    }
}
