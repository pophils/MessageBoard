using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Build.BuildEngine;

namespace AzurePublishing
{
    class PackageProject
    {
        /// <summary>
        /// Function update connection string in Web.config file
        /// </summary>
        /// <param name="path">Full path to Web.config file</param>
        /// <param name="connString">connection string</param>
        public static bool UpdateConnString(string path, string connString)
        {
            Console.WriteLine(" Updating Connection string in web.config file");
            var webConfig = new XmlDocument();
            try
            {
                Console.WriteLine(" Loading Web.config...");
                webConfig.Load(path);

                var connStringNode = webConfig.SelectSingleNode("/configuration/connectionStrings");
                if (connStringNode != null)
                {
                    Console.WriteLine(" Updating Connection string...");
                    var addChildNode = connStringNode.SelectSingleNode("add");
                    if (addChildNode != null)
                    {
                        if (addChildNode.Attributes != null)
                        {
                            addChildNode.Attributes["connectionString"].Value = connString;
                        }

                        Console.WriteLine(" Saving update to config file.");
                        webConfig.Save(path);
                        Console.WriteLine(" Done Updating Connection string in web.config file");
                        return true;
                    }
                    Console.WriteLine(" Web.config Error: Add child node not found in the connectionStrings node.");
                    return false;
                }
                Console.WriteLine(" Web.config Error: Connection string node could not be found in the app config file.");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(" Web.config file could not be found at " + path);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" "+ex.GetType() + ": " + ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Load and process publish.config file which the azure cspkg packaging program is dependent.
        /// </summary>
        /// <param name="pubConfigPath">Full path to publish.config file</param>
        /// <param name="publishConfig"> reference to PublishConfig object </param>
        /// <returns>Processing status</returns>
        public static bool LoadDeployConfig(string pubConfigPath, out PublishConfig publishConfig)
        {
            publishConfig = new PublishConfig();
            Console.WriteLine(" Verifying and loading publish.config file...");

            try
            {
             
            var configFileInfo = new FileInfo(pubConfigPath);

            if (!configFileInfo.Exists)
            {
                Console.WriteLine(" publish.config file could not be found at: " + pubConfigPath);
                return false;
            }

            Console.WriteLine(" Found publish.config file at: " + pubConfigPath);


            var pubConfig = new XmlDocument();
              pubConfig.Load(pubConfigPath);

                var connStringNode = pubConfig.SelectSingleNode("/configuration/databaseConnString");
                if (connStringNode != null)
                {
                    var networkPathNode = connStringNode["networkPath"];
                    if (networkPathNode == null)
                    {
                        Console.WriteLine(" publish.config Error: networkPath node missing in databaseConnString node.");
                        return false;
                    }

                    var nameNode = connStringNode["name"];
                    if (nameNode == null)
                    {
                        Console.WriteLine(" publish.config Error: name node missing in databaseConnString node.");
                        return false;
                    }

                    var userIdNode = connStringNode["userId"];
                    if (userIdNode == null)
                    {
                        Console.WriteLine(" publish.config Error: userId node missing in databaseConnString node.");
                        return false;
                    }

                    var passwordNode = connStringNode["password"];
                    if (passwordNode == null)
                    {
                        Console.WriteLine(" publish.config Error: password node missing in databaseConnString node.");
                        return false;
                    }

                    var encryptNode = connStringNode["encrypt"];
                    if (encryptNode == null)
                    {
                        Console.WriteLine(" publish.config Error: encrypt node missing in databaseConnString node.");
                        return false;
                    }

                    var timeoutNode = connStringNode["timeout"];
                    if (timeoutNode == null)
                    {
                        Console.WriteLine(" publish.config Error: timeout node missing in databaseConnString node.");
                        return false;
                    }
                    publishConfig = new PublishConfig()
                    {
                        ConnectionString =
                            "Server=" + networkPathNode.InnerText + ";Database=" +
                            nameNode.InnerText + ";User ID=" + userIdNode.InnerText + ";Password=" +
                            passwordNode.InnerText + ";Trusted_Connection=False;Encrypt=" + encryptNode.InnerText +
                            ";Connection Timeout=" + timeoutNode.InnerText + ";"
                    };
                }

                var serviceDefinitonNode = pubConfig.SelectSingleNode("/configuration/serviceDefiniton");

                if (serviceDefinitonNode != null)
                {
                    var pathNode = serviceDefinitonNode["path"];
                    if (pathNode == null)
                    {
                        Console.WriteLine(" publish.config Error: path node missing in serviceDefinition node.");
                        return false;
                    }
                    var webRoleNameNode = serviceDefinitonNode["webRoleName"];
                    if (webRoleNameNode == null)
                    {
                        Console.WriteLine(" publish.config Error: webRoleName node missing in serviceDefinition node.");
                        return false;
                    }
                    var siteNameNode = serviceDefinitonNode["siteName"];
                    if (siteNameNode == null)
                    {
                        Console.WriteLine(" publish.config Error: siteName node missing in serviceDefinition node.");
                        return false;
                    }
                    publishConfig.ServiceDefinitonPath = pathNode.InnerText;
                    publishConfig.WebRoleName = webRoleNameNode.InnerText;
                    publishConfig.SiteName = siteNameNode.InnerText;
                }
                else
                {
                    Console.WriteLine(" serviceDefiniton node not found in publish.config.");
                    return false;
                }


                var csProjNode = pubConfig.SelectSingleNode("/configuration/webRoleCsProjPath");
                if (csProjNode != null)
                {
                    publishConfig.WebRoleCsProjPath = csProjNode.InnerText;
                }
                else
                {
                    Console.WriteLine(" webRoleCsProjPath node not found in publish.config.");
                    return false;
                }

                var csPackNode = pubConfig.SelectSingleNode("/configuration/csPackPath");
                if (csPackNode != null)
                {
                    publishConfig.CSPackPath = csPackNode.InnerText;
                    return true;
                }
                Console.WriteLine(" CSPack node not found in publish.config.");
                return false;
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine(" publish.config file could not be found at: " + pubConfigPath);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " +ex.GetType() + ": " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// MSDeploy web role project file.
        /// </summary>
        /// <param name="csprojPath">Full path to web role .csproj file</param>
        /// <param name="msDeployDir">Temporary packageTmp folder</param>
        /// <returns>MsDeploy success status</returns>
        public static bool MsDeploy(string csprojPath, string msDeployDir)
        {
            Console.WriteLine(" Verifying csproj path...");
            try
            {
                var csprojFileInfo = new FileInfo(csprojPath);
                if (!csprojFileInfo.Exists || !csprojFileInfo.Name.EndsWith(".csproj"))
                {
                    Console.WriteLine(" csproj file could not be found at: " + csprojPath);
                    return false;
                }

                Console.WriteLine(" csproj found.");
                Console.WriteLine(" Building project...");

                var engine = new Engine();
                var csprojParentDir = csprojFileInfo.Directory;
                var logger = new FileLogger { Parameters = "logfile=" + csprojParentDir + @"\publish.log" };
                engine.RegisterLogger(logger);

                var buildPropGroup = new BuildPropertyGroup();
                buildPropGroup.SetProperty("OutDir", msDeployDir);
                var deployOnBuildSuccess = engine.BuildProjectFile(csprojPath, new[] { "Rebuild" }, buildPropGroup);

                if (deployOnBuildSuccess)
                    Console.WriteLine(" Successfully MsDeploy project.");
                else
                    Console.WriteLine(" Build failed. Check " + csprojParentDir + @"\publish.log for details.");

                engine.UnloadAllProjects();
                engine.UnregisterAllLoggers();

                return deployOnBuildSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// CSPack the MSdeploy package files
        /// </summary>
        /// <param name="msDeployDir">Temporary packageTmp folder</param>
        /// <param name="serviceDefPath">Full path to ServiceDefinition.csdef file</param>
        /// <param name="roleName">web role name as defined in ServiceDefinition.csdef</param>
        /// <param name="siteName">site name as defined in ServiceDefinition.csdef</param>
        /// <param name="cspackPath">Full path to cspack.exe</param>
        /// <returns>Cspack success status</returns>
        public static bool CsPack(string msDeployDir, string serviceDefPath, string roleName, string siteName, string cspackPath)
        {
            try
            {
                var csPackInfo = new FileInfo(cspackPath);
                var serviceDefFileInfo = new FileInfo(serviceDefPath);

                Console.WriteLine(" Verifying and loading cspack path...");

                if (!csPackInfo.Exists || !csPackInfo.Name.Equals("cspack.exe"))
                {
                    Console.WriteLine(" cspack.exe file could not be found at: " + cspackPath);
                    return false;
                }
                Console.WriteLine(" CSPack found.");

                Console.WriteLine(" Verifying the serviceDefinition.csdef path...");
                if (!serviceDefFileInfo.Exists || !serviceDefFileInfo.Name.ToLower().Equals("servicedefinition.csdef"))
                {
                    Console.WriteLine(" Service definition file could not be found at: " + serviceDefPath);
                    return false;
                }

                Console.WriteLine(" ServiceDefinition found.");

                Console.WriteLine(" Loading MSDeploy web package from: " + msDeployDir + @"_PublishedWebsites\...");

                var directoryInfo = new DirectoryInfo(msDeployDir + @"_PublishedWebsites");
                if (directoryInfo.Exists)
                {
                    var subDir = directoryInfo.GetDirectories();
                    if (subDir.Any())
                    {
                        var packageDir = msDeployDir + @"_PublishedWebsites\" + subDir.First().Name;

                        var csPackCommand = serviceDefPath + " /role:" + roleName + ";" + packageDir + " /sitePhysicalDirectories:" +
                                   roleName + ";" + siteName + ";" + packageDir + " /out:" + serviceDefFileInfo.Directory + @"\" + roleName + "-Release.cspkg";

                        var csPackProc = new Process();

                        var csPackProcStartInfo = new ProcessStartInfo()
                        {
                            FileName = cspackPath,
                            UseShellExecute = true,
                            WindowStyle = ProcessWindowStyle.Hidden,
                            Arguments = " " + csPackCommand
                        };

                        csPackProc.StartInfo = csPackProcStartInfo;
                        csPackProc.EnableRaisingEvents = true;

                        csPackProc.Exited += (sender, eventArgs) =>
                        {
                            DeleteMsDeployDir(msDeployDir);
                            if (CheckAzurePackage(serviceDefFileInfo.Directory + @"\" + roleName + "-Release.cspkg"))
                            {
                                Console.WriteLine(" Done creating a new Azure package at: " +
                                                   serviceDefFileInfo.Directory + @"\" + roleName + "-Release.cspkg \n");
                                ShowDirectoryViaExplorer(serviceDefFileInfo.Directory.ToString());
                            }
                        };

                        csPackProc.Start();
                        csPackProc.WaitForExit();

                        return true;
                    }

                    Console.WriteLine(" MSDeploy web package could not be found at: " + msDeployDir + @"_PublishedWebsites\ ");
                    return false;
                }

                Console.WriteLine(" MSDeploy web package could not be found at: " + msDeployDir + @"_PublishedWebsites\ ");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                return false;
            }
            
        }

      
        /// <summary>
        /// Delete packageTmp folder
        /// </summary>
        /// <param name="msDeployDir">Temporary packageTmp folder</param>
        public static void DeleteMsDeployDir(string msDeployDir)
        {
            try
            {
                Console.WriteLine(" Deleting MSDeploy package directory...");

                var directoryInfo = new DirectoryInfo(msDeployDir);
                if (directoryInfo.Exists)
                {
                    directoryInfo.Delete(true);
                }
                Console.WriteLine(" Done deleting MSDeploy package directory."); 
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                Console.WriteLine(" Error deleting MSDeploy package directory.");
            }
           
        }

        /// <summary>
        /// Check existence of .cspkg
        /// </summary>
        /// <param name="cspkgPath">Full path to .cspkg file</param>
        /// <returns></returns>
        public static bool CheckAzurePackage(string cspkgPath)
        {
            var cspkgInfo = new FileInfo(cspkgPath);
            return cspkgInfo.Exists;
        }

        /// <summary>
        /// Show directory window
        /// </summary>
        /// <param name="directory">Directory path</param>
        public static void ShowDirectoryViaExplorer(string directory)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo
                                {
                                    WindowStyle = ProcessWindowStyle.Hidden,
                                    FileName = "cmd.exe",
                                    Arguments = "/C explorer " + directory
                                };

            try
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine(" " + ex.GetType() + ": " + ex.Message);
                Console.WriteLine(" Error showing azure package parent directory.");
            }
           
        }
      
    }
}
