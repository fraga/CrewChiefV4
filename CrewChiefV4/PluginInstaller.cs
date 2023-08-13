using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.Xml;

namespace CrewChiefV4
{
    class PluginInstaller
    {
        Boolean messageBoxPresented;
        Boolean errorMessageBoxPresented;
        Boolean messageBoxResult;
        private readonly String rf2PluginFileName = "rFactor2SharedMemoryMapPlugin64.dll";

        private const string accBroadcastFileContents = "{\n    \"updListenerPort\": 9000,\n    \"connectionPassword\": \"asd\",\n    \"commandPassword\": \"\"\n}";

        public PluginInstaller()
        {
            messageBoxPresented = false;
            messageBoxResult = false;
        }

        private string checkMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return Encoding.Default.GetString(md5.ComputeHash(stream));
                }
            }
        }

        //referance https://github.com/Jamedjo/RSTabExplorer/blob/master/RockSmithTabExplorer/Services/RocksmithLocator.cs
        private string getSteamFolder()
        {
            string steamInstallPath = "";
            try
            {
                RegistryKey steamKey = Registry.LocalMachine.OpenSubKey(@"Software\Valve\Steam") ?? Registry.LocalMachine.OpenSubKey(@"Software\Wow6432Node\Valve\Steam");
                if (steamKey != null)
                {
                    steamInstallPath = steamKey.GetValue("InstallPath").ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception getting steam folder: " + e.Message);
            }
            return steamInstallPath;
        }

        private List<string> getSteamLibraryFolders()
        {
            List<string> folders = new List<string>();

            string steamFolder = getSteamFolder();
            if (Directory.Exists(steamFolder))
            {
                folders.Add(steamFolder);
                string configFile = Path.Combine(steamFolder, @"config\config.vdf");

                if (File.Exists(configFile))
                {
                    Regex regex = new Regex("BaseInstallFolder[^\"]*\"\\s*\"([^\"]*)\"");
                    using (StreamReader reader = new StreamReader(configFile))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Match match = regex.Match(line);
                            if (match.Success)
                            {
                                folders.Add(Regex.Unescape(match.Groups[1].Value));
                            }
                        }
                    }
                }

            }
            return folders;
        }

        private Boolean presentInstallMessagebox(string gameName)
        {
            if (messageBoxPresented == false)
            {
                messageBoxPresented = true;
                if (DialogResult.OK == MessageBox.Show(Configuration.getUIString("install_plugin_popup_text") + " " + gameName, Configuration.getUIString("install_plugin_popup_title"),
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
                {
                    messageBoxResult = true;
                }
            }
            return messageBoxResult;
        }

        private Boolean presentEnableMessagebox()
        {
            if (DialogResult.OK == MessageBox.Show(Configuration.getUIString("install_plugin_popup_enable_text"), Configuration.getUIString("install_plugin_popup_enable_title"),
                MessageBoxButtons.OKCancel, MessageBoxIcon.Information))
            {
                return true;
            }
            return false;
        }

        private void presentInstallUpdateErrorMessagebox(string errorText)
        {
            MessageBox.Show(Configuration.getUIString("install_plugin_popup_error_text") + Environment.NewLine + errorText, Configuration.getUIString("install_plugin_popup_error_title"),
                MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Make sure we wil retry again if user does not restart CC.
            messageBoxPresented = false;
        }

        //I stole this from the internetz(http://stackoverflow.com/questions/3201598/how-do-i-create-a-file-and-any-folders-if-the-folders-dont-exist)
        private bool installOrUpdatePlugin(string source, string destination, string gameName)
        {
            try
            {
                string[] files = null;

                if (destination[destination.Length - 1] != Path.DirectorySeparatorChar)
                {
                    destination += Path.DirectorySeparatorChar;
                }

                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }

                files = Directory.GetFileSystemEntries(source);
                foreach (string element in files)
                {
                    // Sub directories                    
                    if (Directory.Exists(element))
                    {
                        installOrUpdatePlugin(element, destination + Path.GetFileName(element), gameName);
                    }
                    else
                    {
                        // Files in directory
                        string destinationFile = destination + Path.GetFileName(element);
                        //if the file exists we will check if it needs updating
                        if (File.Exists(destinationFile))
                        {
                            if (!checkMD5(element).Equals(checkMD5(destinationFile)))
                            {
                                //ask the user if they want to update the plugin
                                if (presentInstallMessagebox(gameName))
                                {
                                    File.Copy(element, destinationFile, true);
                                    Console.WriteLine("Updated plugin file: " + destinationFile);    
                                }
                            }
                        }
                        else
                        {
                            if (presentInstallMessagebox(gameName))
                            {
                                File.Copy(element, destinationFile, true);
                                Console.WriteLine("Installed plugin file: " + destinationFile);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Copy plugin files: " + e.Message);
                presentInstallUpdateErrorMessagebox(e.Message);
                return false;
            }
            return true;
        }

        public void InstallOrUpdatePlugins(GameDefinition gameDefinition)
        {
            //gameInstallPath is also used to check if the user already was asked to update
            string gameInstallPath = "";
            switch (gameDefinition.gameEnum)
            {
                case GameEnum.ACC:
                    {
                        string content = "[file not found]";
                        // treading as lightly as possible, use the same encoding that the game is using (unicode LE, no BOM)
                        Encoding LEunicodeWithoutBOM = new UnicodeEncoding(false, false);
                        bool writeBroadcastFile = true;
                        var broadcastPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                "Assetto Corsa Competizione",
                                "Config",
                                "broadcasting.json");
                        if (File.Exists(broadcastPath))
                        {
                            try
                            {
                                // again, treading as lightly as possible read the file content without locking allowing for the file being locked by the game
                                using (FileStream fileStream = new FileStream(
                                    broadcastPath,
                                    FileMode.Open,
                                    FileAccess.Read,
                                    FileShare.ReadWrite))
                                {
                                    using (StreamReader streamReader = new StreamReader(fileStream, LEunicodeWithoutBOM))
                                    {
                                        content = streamReader.ReadToEnd();
                                        if (accBroadcastFileContents.Equals(content))
                                        {
                                            Console.WriteLine("ACC broadcast file has expected contents");
                                            writeBroadcastFile = false;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception getting broadcast.json: " + ex.Message);
                            }
                        }
                        if (writeBroadcastFile)
                        {
                            try
                            {
                                // if the game is running it'll need to be bounced to pick up this change
                                if (Utilities.IsGameRunning(gameDefinition.processName, gameDefinition.alternativeProcessNames, out var parentDir))
                                {
                                    MessageBox.Show("broadcasting.json needs to be updated and the game restarted. Please exit the game then click 'OK'");
                                }
                                Console.WriteLine("Updating ACC broadcast file");
                                Console.WriteLine("Expected content:");
                                Console.WriteLine(accBroadcastFileContents);
                                Console.WriteLine("Actual content:");
                                Console.WriteLine(content);
                                // again, write with the same encoding the game uses
                                File.WriteAllText(broadcastPath, accBroadcastFileContents, LEunicodeWithoutBOM);
                            }
                            catch (Exception e) { Log.Exception(e); }
                        }
                        return;
                    }

                case GameEnum.RF2_64BIT:
                    gameInstallPath = UserSettings.GetUserSettings().getString("rf2_install_path");
                    break;
                case GameEnum.ASSETTO_32BIT:
                case GameEnum.ASSETTO_64BIT:
                case GameEnum.ASSETTO_64BIT_RALLY:
                    gameInstallPath = UserSettings.GetUserSettings().getString("acs_install_path");
                    break;
                case GameEnum.RF1:
                    //special case here, will figure something clever out so we dont need to have Dan's dll included in every plugin folder.
                    switch (gameDefinition.lookupName)
                    {
                        case "automobilista":
                            {
                                gameInstallPath = UserSettings.GetUserSettings().getString("ams_install_path");
                                break;
                            }
                        case "rFactor1":
                            {
                                gameInstallPath = UserSettings.GetUserSettings().getString("rf1_install_path");
                                break;
                            }
                        case "asr":
                            {
                                gameInstallPath = UserSettings.GetUserSettings().getString("asr_install_path");
                                break;
                            }
                        default:
                            {
                                // this is an rFactor based game that's not rFactor or AMS (so it's fTruck, Marcas or GSC) - no automatic installation of
                                // plugin for these old games
                                Console.WriteLine("Auto-install of plugin not supported for " + gameDefinition.friendlyName);
                                Console.WriteLine("Assuming that the plugin in install folder" +
                                    " (default location C:\\Program Files(x86)\\Britton IT Ltd\\CrewChiefV4\\plugins\\rFactor\\Plugins) has been copied to your game's install folder");
                                return;
                            }
                    }
                    break;
                case GameEnum.RBR:
                    gameInstallPath = UserSettings.GetUserSettings().getString("rbr_install_path");
                    break;
                case GameEnum.DIRT:
                    UpdateDirtRallyXML(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally\hardwaresettings\hardware_settings_config.xml",
                            UserSettings.GetUserSettings().getInt("dirt_rally_udp_data_port"));
                    return;
                case GameEnum.DIRT_2:
                    UpdateDirtRallyXML(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally 2.0\hardwaresettings\hardware_settings_config.xml",
                                    UserSettings.GetUserSettings().getInt("dirt_rally_2_udp_data_port"));
                    UpdateDirtRallyXML(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\DiRT Rally 2.0\hardwaresettings\hardware_settings_config_vr.xml",
                        UserSettings.GetUserSettings().getInt("dirt_rally_2_udp_data_port"));
                    return;
                case GameEnum.GTR2:
                    gameInstallPath = UserSettings.GetUserSettings().getString("gtr2_install_path");
                    break;
            }
            
            if (!Directory.Exists(gameInstallPath))
            {
                //Present a messagebox to the user asking if they want to install plugins
                if (presentInstallMessagebox(gameDefinition.friendlyName))
                {   // First try to get the install folder from steam common install folders.
                    List<string> steamLibs = getSteamLibraryFolders();
                    foreach (string lib in steamLibs)
                    {
                        string commonPath = Path.Combine(lib, @"steamapps\common\" + gameDefinition.gameInstallDirectory);
                        if (Directory.Exists(commonPath))
                        {
                            gameInstallPath = commonPath;
                            break;
                        }
                    }
                    if (!Directory.Exists(gameInstallPath))
                    {   //Not found in steam folders ask the user to locate the directory
                        FolderBrowserDialog dialog = new FolderBrowserDialog();
                        dialog.ShowNewFolderButton = false;
                        dialog.Description = Configuration.getUIString("install_plugin_select_directory_start") + " " +
                            gameDefinition.gameInstallDirectory + " " + Configuration.getUIString("install_plugin_select_directory_end");
                        dialog.RootFolder = Environment.SpecialFolder.MyComputer;

                        DialogResult result = dialog.ShowDialog();

                        if (result == DialogResult.OK && dialog.SelectedPath.Length > 0)
                        {
                            //This should now take care of checking against the main .exe instead of the folder name, special case for rFactor 2 as its has the file installed in ..\Bin64
                            if (gameDefinition.gameEnum == GameEnum.RF2_64BIT)
                            {
                                if (File.Exists(Path.Combine(dialog.SelectedPath, @"Bin64", gameDefinition.processName + ".exe")))
                                {
                                    gameInstallPath = dialog.SelectedPath;
                                }
                            }
                            else if (File.Exists(Path.Combine(dialog.SelectedPath, gameDefinition.processName + ".exe")))
                            {
                                gameInstallPath = dialog.SelectedPath;
                            }
                            else
                            {
                                //present again if user didn't select the correct folder 
                                InstallOrUpdatePlugins(gameDefinition);
                            }
                        }
                        else if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                }
            }
            
            if (Directory.Exists(gameInstallPath))
            {   //we have a gameInstallPath so we can go on with installation/updating assuming that the user wants to enable the plugin.
                installOrUpdatePlugin(
                    Path.Combine(Configuration.getDefaultFolderLocation("plugins"),
                                 gameDefinition.gameInstallDirectory),
                    gameInstallPath,
                    gameDefinition.friendlyName);
                switch (gameDefinition.gameEnum)
                {
                    case GameEnum.RF2_64BIT:
                        {
                            UserSettings.GetUserSettings().setProperty("rf2_install_path", gameInstallPath);
                            try
                            {
                                string configPath = Path.Combine(gameInstallPath, @"UserData\player\CustomPluginVariables.JSON");
                                if (File.Exists(configPath))
                                {
                                    string json = File.ReadAllText(configPath);
                                    Dictionary<string, Dictionary<string, int>> plugins = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);
                                    Dictionary<string, int> plugin = null;
                                    if (plugins.TryGetValue(rf2PluginFileName, out plugin))
                                    {
                                        //the whitespace is intended, this is how the game writes it.
                                        if (plugin[" Enabled"] == 0)
                                        {
                                            if (presentEnableMessagebox())
                                            {
                                                plugin[" Enabled"] = 1;
                                                json = JsonConvert.SerializeObject(plugins, Newtonsoft.Json.Formatting.Indented);
                                                File.WriteAllText(configPath, json);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (presentEnableMessagebox())
                                        {
                                            plugins.Add(rf2PluginFileName, new Dictionary<string, int>() { { " Enabled", 1 } });
                                            json = JsonConvert.SerializeObject(plugins, Newtonsoft.Json.Formatting.Indented);
                                            File.WriteAllText(configPath, json);
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to enable plugin" + e.Message);
                            }

                            break;
                        }

                    case GameEnum.ASSETTO_32BIT:
                    case GameEnum.ASSETTO_64BIT:
                    case GameEnum.ASSETTO_64BIT_RALLY:
                        {
                            UserSettings.GetUserSettings().setProperty("acs_install_path", gameInstallPath);
                            string pythonConfigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Assetto Corsa\cfg", @"python.ini");
                            if (File.Exists(pythonConfigPath))
                            {
                                string valueActive = Utilities.ReadIniValue("CREWCHIEFEX", "ACTIVE", pythonConfigPath, "0");
                                if (!valueActive.Equals("1"))
                                {
                                    if (presentEnableMessagebox())
                                    {
                                        Utilities.WriteIniValue("CREWCHIEFEX", "ACTIVE", "1", pythonConfigPath);
                                    }
                                }
                            }

                            break;
                        }

                    case GameEnum.RF1:
                        switch (gameDefinition.lookupName)
                        {
                            case "automobilista":
                                {
                                    UserSettings.GetUserSettings().setProperty("ams_install_path", gameInstallPath);
                                    break;
                                }
                            case "rFactor1":
                                {
                                    UserSettings.GetUserSettings().setProperty("rf1_install_path", gameInstallPath);
                                    break;
                                }
                            case "asr":
                                {
                                    UserSettings.GetUserSettings().setProperty("asr_install_path", gameInstallPath);
                                    break;
                                }
                        }
                        break;
                    case GameEnum.RBR:
                        UserSettings.GetUserSettings().setProperty("rbr_install_path", gameInstallPath);
                        break;
                    case GameEnum.GTR2:
                        UserSettings.GetUserSettings().setProperty("gtr2_install_path", gameInstallPath);
                        break;
                }
                UserSettings.GetUserSettings().saveUserSettings();
            }
        }

        private void UpdateDirtRallyXML(string fileName, int udpPort)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    bool save = false;
                    XmlDocument doc = new XmlDocument();
                    doc.Load(fileName);
                    XmlNode root = doc.DocumentElement;
                    XmlNode udpNode = root.SelectSingleNode("descendant::udp");
                    if (udpNode == null)
                    {
                        // no UDP node, create it and it's motion_platform parent, with the attributes we need
                        save = true;
                        CreateDirtRallyUDPElement(doc, root, udpPort);
                    }
                    else
                    {
                        // check the attributes and update them if necessary
                        save = UpdateDirtRallyUDPAttributes(udpNode, udpPort);
                    }
                    if (save)
                    {
                        Console.WriteLine("Updating UDP element in " + fileName);
                        if (!File.Exists(fileName + "_backup"))
                        {
                            File.Copy(fileName, fileName + "_backup");
                        }
                        doc.Save(fileName);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to update settings XML file " + fileName + ", " + e.Message);
                }
            }
        }

        private bool UpdateDirtRallyUDPAttributes(XmlNode udpNode, int udpPort)
        {
            bool save = false;
            if (udpNode.Attributes["enabled"] == null || !udpNode.Attributes["enabled"].Value.Equals("true"))
            {
                save = true;
                udpNode.Attributes["enabled"].Value = "true";
            }
            if (udpNode.Attributes["extradata"] == null || !int.TryParse(udpNode.Attributes["extradata"].Value, out var edv) || edv < 3)
            {
                // extradata doesn't exist, or it exists but it's not set to "3" or above
                save = true;
                udpNode.Attributes["extradata"].Value = "3";
            }
            if (udpNode.Attributes["port"] == null || !udpNode.Attributes["port"].Value.Equals(udpPort.ToString()))
            {
                save = true;
                udpNode.Attributes["port"].Value = udpPort.ToString();
            }
            return save;
        }

        private void CreateDirtRallyUDPElement(XmlDocument doc, XmlNode root, int udpPort)
        {
            // try to create it
            XmlNode motionPlatform = root.SelectSingleNode("descendant::motion_platform");
            if (motionPlatform == null)
            {
                motionPlatform = doc.CreateElement("motion_platform");
                root.AppendChild(motionPlatform);
            }
            XmlNode udpNode = doc.CreateElement("udp");
            XmlAttribute enabledAttrib = doc.CreateAttribute("enabled");
            enabledAttrib.Value = "true";
            udpNode.Attributes.Append(enabledAttrib);
            XmlAttribute extradataAttrib = doc.CreateAttribute("extradata");
            extradataAttrib.Value = "3";
            udpNode.Attributes.Append(extradataAttrib);
            XmlAttribute ipAttrib = doc.CreateAttribute("ip");
            ipAttrib.Value = "127.0.0.1";
            udpNode.Attributes.Append(ipAttrib);
            XmlAttribute portAttrib = doc.CreateAttribute("port");
            portAttrib.Value = udpPort.ToString();
            udpNode.Attributes.Append(portAttrib);
            XmlAttribute delayAttrib = doc.CreateAttribute("delay");
            delayAttrib.Value = "1";
            udpNode.Attributes.Append(delayAttrib);
            motionPlatform.AppendChild(udpNode);
        }
    }
}
