using System;

namespace DACDeploy
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Console.WriteLine(" ........................................................");
            Console.WriteLine(" Program is dependent on dac.config file.");
            Console.WriteLine(" Check README.txt for more information before continuing.");
            Console.WriteLine(" ........................................................ \n");

            ExecProg();
        }

        static void ExecProg()
        {
            Console.Write(" Enter the file path to dac.config: ");
            var dacConfigPath = Console.ReadLine();

            if (string.IsNullOrEmpty(dacConfigPath))
            {
                Console.WriteLine(" File path to dac.config cannot be empty.\n");
                ExecProg();
            }


            DacConfig dacConfig;
            var successLoad = DbasePac.LoadDacConfig(dacConfigPath, out dacConfig);

            if (successLoad)
            {
                if (DbasePac.CreateTempDir(dacConfig.BACPACDirPath))
                {
                    var bacpacPath = dacConfig.BACPACDirPath + @"\" + dacConfig.ExportSourceDatabaseName + ".bacpac";
                    if (DbasePac.ExportLocalBacpac(dacConfig.LocalConnectionString, dacConfig.ExportSourceDatabaseName, bacpacPath))
                    {
                        if (DbasePac.ImportBacpac(dacConfig.RemoteConnectionString, dacConfig.ImportDatabaseName, bacpacPath))
                            DbasePac.DeleteBacpac(bacpacPath);
                    }
                }
            }
            else
            {
                Console.WriteLine(" Dac operation failed. \n");
            }

            InitProgramReload();
          
            }

        static void InitProgramReload()
        {
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
