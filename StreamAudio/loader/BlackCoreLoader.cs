using System;
using System.Collections.Generic;
using BlackCore;
using System.Text;

namespace StreamAudio
{
    partial class Program
    {
        public static BlackCore.basic.cAppThreadPool thApp;

        #region Application params
        private static string _helphandler(string basecmd = "", string arg1 = "", string arg2 = "")
        {
            switch (basecmd)
            {
                case "user":
                    return "username for ssh connection";
                case "pass":
                    return "password for ssh connection";
                case "host":
                    return "remote host, to which we should connect";
                case "port":
                    return "port, which will be used internally for data transfer";
            }
            return "";
        }

        static void registerCommadLine()
        {
            bcore.app.args.arghelp.Add("user", _helphandler);
            bcore.app.args.arghelp.Add("pass", _helphandler);
            bcore.app.args.arghelp.Add("host", _helphandler);
            bcore.app.args.arghelp.Add("port", _helphandler);
            if (!BlackCore.bcore.app.isSilent) BlackCore.bcore.app.openConsole();
        }
        #endregion


        #region BlackCore init routine
        static void doParams()
        {
            thApp.applicationStart("_main", _main);
        }
        #endregion

        static void bcore_main()
        {
            thApp = new BlackCore.basic.cAppThreadPool(doParams, registerCommadLine, false);
            thApp.app.Start();
            bcore.app.ReleaseConsoleHandles();
        }

    }
}
