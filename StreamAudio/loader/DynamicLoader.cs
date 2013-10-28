using System;
using System.Reflection;
using StreamAudio.resources;
using System.Text;


namespace StreamAudio
{
    partial class Program
    {
        #region "1. Dynamic Load Routine"


        static Assembly MyResolver(object sender, ResolveEventArgs args)
        {
            AppDomain domain = (AppDomain)sender;
            byte[] rawAssembly = null;
            string[] arr = args.Name.Split(',');

            switch (arr[0])
            {
                case "BlackCore":
                    rawAssembly = DynLibraries.BlackCore;
                    break;
                case "SQLite":
                    rawAssembly = DynLibraries.SQLite;
                    break;
                case "NAudio":
                    rawAssembly = DynLibraries.NAudio;
                    break;
                case "Renci.SshNet":
                    rawAssembly = DynLibraries.Renci_SshNet;
                    break;
                default:
                    throw new Exception("No assembly with name\"" + arr[0] + "\" found.");
            }

            Assembly assembly = domain.Load(rawAssembly);

            return assembly;
        }
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolver);
            bcore_main();
        }

        #endregion

    }
}
