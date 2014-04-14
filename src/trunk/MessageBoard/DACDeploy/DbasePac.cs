using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.SqlServer.Dac;

namespace DACDeploy
{

    class DacConfig
    {
        public string ExportSourceDatabaseName { get; set; }
        public string RemoteConnectionString { get; set; }
        public string LocalConnectionString { get; set; }
        public string ImportDatabaseName { get; set; }
        public string BACPACDirPath{ get; set; } 
    }

    class DbasePac
    {
        public static DacServices dacService;

        /// <summary>
        /// Load DAC config parameters for program execution
        /// </summary>
        /// <param name="dacConfigPath">path to dac.config file</param>
        /// <param name="dacConfig">DacConfig instance</param>
        /// <returns>success status of opertaion</returns>
        public static bool LoadDacConfig(string dacConfigPath, out DacConfig dacConfig)
        {
            dacConfig = new DacConfig();
            Console.WriteLine(" Verifying and loading dac.config file...");

            try
            {
                var configFileInfo = new FileInfo(dacConfigPath);

                if (!configFileInfo.Exists)
                {
                    Console.WriteLine(" dac.config file could not be found at: " + dacConfigPath);
                    return false;
                }

                Console.WriteLine(" Found dac.config file at: " + dacConfigPath);


                var dacXml = new XmlDocument();
                dacXml.Load(dacConfigPath);

                var connStringNode = dacXml.SelectSingleNode("/configuration/remoteDbaseConnString");
                if (connStringNode != null)
                {
                    var networkPathNode = connStringNode["networkPath"];
                    if (networkPathNode == null)
                    {
                        Console.WriteLine(" dac.config Error: networkPath node missing in remoteDbaseConnString node.");
                        return false;
                    }

                    var nameNode = connStringNode["name"];
                    if (nameNode == null)
                    {
                        Console.WriteLine(" dac.config Error: name node missing in remoteDbaseConnString node.");
                        return false;
                    }

                    var userIdNode = connStringNode["userId"];
                    if (userIdNode == null)
                    {
                        Console.WriteLine(" dac.config Error: userId node missing in targetDbaseConnString node.");
                        return false;
                    }

                    var passwordNode = connStringNode["password"];
                    if (passwordNode == null)
                    {
                        Console.WriteLine(" dac.config Error: password node missing in targetDbaseConnString node.");
                        return false;
                    }

                    var encryptNode = connStringNode["encrypt"];
                    if (encryptNode == null)
                    {
                        Console.WriteLine(" dac.config Error: encrypt node missing in remoteDbaseConnString node.");
                        return false;
                    }

                    var timeoutNode = connStringNode["timeout"];
                    if (timeoutNode == null)
                    {
                        Console.WriteLine(" dac.config Error: timeout node missing in remoteDbaseConnString node.");
                        return false;
                    }

                    dacConfig.RemoteConnectionString =
                        "Server=" + networkPathNode.InnerText + ";Database=" +
                        nameNode.InnerText + ";User ID=" + userIdNode.InnerText + ";Password=" +
                        passwordNode.InnerText + ";Trusted_Connection=False;Encrypt=" + encryptNode.InnerText +
                        ";Connection Timeout=" + timeoutNode.InnerText + ";";
                }


                connStringNode = dacXml.SelectSingleNode("/configuration/localDbaseConnString");
                if (connStringNode != null)
                {
                    var integratedSecurityNode = connStringNode["integratedSecurity"];
                    if (integratedSecurityNode == null)
                    {
                        Console.WriteLine(" dac.config Error: integratedSecurity node missing in localDbaseConnString node.");
                        return false;
                    }

                    dacConfig.LocalConnectionString =
                        "Server=localhost;Integrated Security=" + integratedSecurityNode.InnerText + ";";

                    var userIdNode = connStringNode["userId"];
                    if (userIdNode != null && string.IsNullOrEmpty(userIdNode.InnerText))
                    {
                        dacConfig.LocalConnectionString += "User Id=" + userIdNode.InnerText + ";";
                    }

                    var passwordNode = connStringNode["password"];
                    if (passwordNode != null && string.IsNullOrEmpty(passwordNode.InnerText))
                    {
                        dacConfig.LocalConnectionString += "Password=" + passwordNode.InnerText + ";";
                    }

                }


                var exportDbaseNameNode = dacXml.SelectSingleNode("/configuration/exportSourceDatabaseName");
                if (exportDbaseNameNode == null)
                {
                    Console.WriteLine(" dac.config Error: exportSourceDatabaseName node missing.");
                    return false;
                }

                dacConfig.ExportSourceDatabaseName = exportDbaseNameNode.InnerText;


                var importDbaseNameNode = dacXml.SelectSingleNode("/configuration/importTargetDatabaseName");
                if (importDbaseNameNode == null)
                {
                    Console.WriteLine(" dac.config Error: importTargetDatabaseName node missing.");
                    return false;
                }

                dacConfig.ImportDatabaseName = importDbaseNameNode.InnerText;


                var bacpacDirPathNode = dacXml.SelectSingleNode("/configuration/bacpacTempDirPath");
                if (bacpacDirPathNode != null)
                {
                    dacConfig.BACPACDirPath = bacpacDirPathNode.InnerText;
                    return true;
                }
                Console.WriteLine(" bacpacPath node not found in dac.config.");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(" dac.config file could not be found at: " + dacConfigPath);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                return false;
            }
        }

        
        /// <summary>
        /// Export bacpac file from a local dbase
        /// </summary>
        /// <param name="targetConnString">connection string of target database</param>
        /// <param name="sourceDatabaseName"></param>
        // <param name="bacpacPath">bacpac path file</param>
        /// <returns>success status of opertaion</returns>
        public static bool ExportLocalBacpac( string targetConnString, string sourceDatabaseName , string bacpacPath)
        {
            try
            {
                dacService = new DacServices(targetConnString);

                dacService.Message += new EventHandler<DacMessageEventArgs>(DacMessageEvent);
                dacService.ProgressChanged += new EventHandler<DacProgressEventArgs>(DacProgressEvent);


                
                Console.WriteLine(" Exporting {0} to {1}", sourceDatabaseName, bacpacPath);
                dacService.ExportBacpac(bacpacPath, sourceDatabaseName);
                Console.WriteLine(" Done Exporting {0} to {1}", sourceDatabaseName, bacpacPath);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " +  ex.Message);
                return false;
            }

        }

        public static void DacMessageEvent(object sender, DacMessageEventArgs e)
        {
            Console.WriteLine("Message: " + e.Message.Message);
        }

        public static void DacProgressEvent(object sender, DacProgressEventArgs e)
        {
            Console.WriteLine("Progress Event:{0} Progress Status:{1}", e.Message, e.Status);
        }


        public static bool CreateTempDir(string dir)
        {
            try
            {
                var dirInfo = new DirectoryInfo(dir);
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                return false;
            }
           
        }


        /// <summary>
        ///  Import a bacpac file into a database
        /// </summary>
        /// <param name="targetConnString">connection string of target database</param>
        /// <param name="targetDatabaseName">target database name</param>
        /// <param name="bacpacPath">bacpac path file</param>
        /// <returns>success status of opertaion</returns>
        public static bool ImportBacpac(string targetConnString, string targetDatabaseName, string bacpacPath)
        {
            try
            {
                dacService = new DacServices(targetConnString);

                dacService.Message += new EventHandler<DacMessageEventArgs>(DacMessageEvent);
                dacService.ProgressChanged += new EventHandler<DacProgressEventArgs>(DacProgressEvent);

                Console.WriteLine(" Importing {0} to {1} ", bacpacPath, targetDatabaseName);

                using (var bacpac = BacPackage.Load(bacpacPath))
                {
                    dacService.ImportBacpac(bacpac, targetDatabaseName);
                }

                Console.WriteLine(" Done Importing {0} ", targetDatabaseName);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                return false;
            }

        }


        /// <summary>
        /// Delete bacpac file
        /// </summary>
        /// <param name="bacpacPath">bacpac path</param>
        public static void DeleteBacpac(string bacpacPath)
        {
            try
            {
                Console.WriteLine(" Deleting bacpac file...");

                var fileInfo = new FileInfo(bacpacPath);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }
                Console.WriteLine(" Done deleting bacpac file.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                Console.WriteLine(" Error deleting bacpac file.");
            }

        }
    }
}
