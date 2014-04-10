using System;
using System.IO;

namespace AzurePublishing
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Console.WriteLine(" ........................................................");
            Console.WriteLine(" Program is dependent on publish.config file.");
            Console.WriteLine(" Check README.txt for more information before continuing.");
            Console.WriteLine(" ........................................................ \n");

            ExecProg();
        }

        static void ExecProg()
        {
            Console.Write(" Enter the path to publish.config file: ");
            var pubConfigPath = Console.ReadLine();

            if (string.IsNullOrEmpty(pubConfigPath))
            {
                Console.WriteLine(" Path to publish.config file cannot be empty.\n");
                ExecProg();
            }


            PublishConfig publishConfig;
            var success = PackageProject.LoadDeployConfig(pubConfigPath, out publishConfig);
            
            if (success)
            {
                //get path to web.config and update it
                var csprojFileInfo = new FileInfo(publishConfig.WebRoleCsProjPath);
                if (csprojFileInfo.Exists)
                {
                    if (PackageProject.UpdateConnString(csprojFileInfo.Directory + @"\web.config", publishConfig.ConnectionString))
                    {
                        const string msDeployDir = @"C:\packageTmp\";
                        if (PackageProject.MsDeploy(publishConfig.WebRoleCsProjPath, msDeployDir))
                        {
                            PackageProject.CsPack(msDeployDir, publishConfig.ServiceDefinitonPath,
                                                  publishConfig.WebRoleName, publishConfig.SiteName,
                                                  publishConfig.CSPackPath);

                        }
                    }
                }
                else
                {
                    Console.WriteLine(" csproj not found.");
                }
            }
            else
            {
                Console.WriteLine(" Publish failed. \n");
            }

            Console.Write(" Reload program Y or N:");
            var readLine = Console.ReadLine();

            if (!string.IsNullOrEmpty(readLine) && readLine.ToLower().Equals("y"))
            {
                ExecProg();
            }
            else
                Environment.Exit(0);
        }
    }
}
