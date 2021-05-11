using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace WindowsService1
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        static void Main(string[] args)
        {
            /*if (Environment.UserInteractive)
            {
                if (args.Length > 0)
                {
                    switch (args[0])
                    {
                        case "-i":
                            try
                            {
                                var appPath = Assembly.GetExecutingAssembly().Location;
                                System.Configuration.Install.ManagedInstallerClass.InstallHelper(new string[] { appPath });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;
                        case "-u":
                            try
                            {
                                var appPath = Assembly.GetExecutingAssembly().Location;
                                System.Configurat   ion.Install.ManagedInstallerClass.InstallHelper(new string[] { "/u", appPath });
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            break;
                    }
                }
            }
            else
            {*/
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                new KSIS_4()
                };
                ServiceBase.Run(ServicesToRun);
          //  }
        }
    }
}
