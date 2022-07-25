using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace CrewChiefV4
{
    class UserSettings
    {
        public static String userConfigFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Britton_IT_Ltd";

        // blat the user config folder for cases where it gets fucked up.
        public static void ForceablyDeleteConfigDirectory()
        {
            ForceablyDeleteDirectory(userConfigFolder);
        }

        /// Depth-first recursive delete, with handling for descendant
        /// directories open in Windows Explorer and other Windows "not doing what its been told" arseholery.
        private static void ForceablyDeleteDirectory(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                ForceablyDeleteDirectory(directory);
            }

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException)
            {
                Directory.Delete(path, true);
            }
            catch (UnauthorizedAccessException)
            {
                Directory.Delete(path, true);
            }
        }

        public Boolean initFailed = false;
        public string initFailedStack = "";
        public string initFailedMessage = "";

        private static  String[] reservedNameStarts = new String[] { "CHANNEL_", "TOGGLE_", "VOICE_OPTION", "background_volume",
            "messages_volume", "last_game_definition", "UpdateSettings",ControllerConfiguration.ControllerData.PROPERTY_CONTAINER,
            "PERSONALISATION_NAME", "app_version", "spotter_name", "codriver_name", "codriver_style", "racing_type", "update_notify_attempted", "last_trace_file_name",
            "NAUDIO_DEVICE_GUID", "NAUDIO_RECORDING_DEVICE_GUID", "chief_name", "current_settings_profile", "main_window_position"};

        private static String defaultUserSettingsfileName = "defaultSettings.json";
        public static String userProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "Profiles");
        public static String currentUserProfileFileName = "";

        public class UserProfileSettings
        {
            public Dictionary<string, object> userSettings { get; set; }
            public UserProfileSettings()
            {
                userSettings = new Dictionary<string, object>();
            }
        }

        public static UserProfileSettings currentActiveProfile = new UserProfileSettings();

        public Dictionary<string, object> currentApplicationSettings = new Dictionary<string, object>();

        public void loadActiveUserSettingsProfile(String fileName, Boolean loadingDefault)
        {
            Boolean settingsProfileBroken = false;
            // Create a user profile with the users current settings if it does not yet exist(new user, first time upgrade to new format, default file deleted)
            if (!File.Exists(Path.Combine(userProfilesPath, defaultUserSettingsfileName)))
            {
                UserProfileSettings userProfileSettings = new UserProfileSettings();
                foreach (SettingsProperty prop in getProperties())
                {
                    userProfileSettings.userSettings.Add(prop.Name, Properties.Settings.Default[prop.Name]);
                }
                saveUserSettingsFile(userProfileSettings, Path.Combine(userProfilesPath, defaultUserSettingsfileName));
            }

            try
            {
                // If the requested file does not exist load default settings profile
                if (!File.Exists(fileName))
                {
                    fileName = Path.Combine(userProfilesPath, defaultUserSettingsfileName);
                    setProperty("current_settings_profile", defaultUserSettingsfileName);
                    saveUserSettings();
                    currentUserProfileFileName = getString("current_settings_profile");
                }
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    currentActiveProfile = JsonConvert.DeserializeObject<UserProfileSettings>(json);
                    if (currentActiveProfile == null)
                    {
                        currentActiveProfile = new UserProfileSettings();
                        settingsProfileBroken = true;
                    }
                    else if (currentActiveProfile.userSettings == null)
                    {
                        currentActiveProfile.userSettings = new Dictionary<string, object>();
                        settingsProfileBroken = true;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                settingsProfileBroken = true;
            }

            if (settingsProfileBroken
                && loadingDefault)
            {
                Console.WriteLine($"Failed to Load default settings file at: '{fileName}', giving up.");
                return;
            }

            try
            {
                if (settingsProfileBroken)
                {
                    Utilities.TryBackupBrokenFile(fileName, "broken", "Broken user settings profile " + fileName);
                    // if the default settings file is broken we have to recreate it.
                    if (Path.Combine(userProfilesPath, defaultUserSettingsfileName).Equals(fileName))
                    {
                        UserProfileSettings userProfileSettings = new UserProfileSettings();
                        foreach (SettingsProperty prop in getProperties())
                        {
                            userProfileSettings.userSettings.Add(prop.Name, Properties.Settings.Default[prop.Name]);
                        }
                        saveUserSettingsFile(userProfileSettings, Path.Combine(userProfilesPath, defaultUserSettingsfileName));
                    }
                    setProperty("current_settings_profile", defaultUserSettingsfileName);
                    saveUserSettings();
                    currentUserProfileFileName = getString("current_settings_profile");
                    // Load default file
                    loadActiveUserSettingsProfile(fileName:Path.Combine(userProfilesPath, defaultUserSettingsfileName), loadingDefault:true);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to Load settings file ");
            }
        }
        public UserProfileSettings loadUserSettings(String fileName)
        {
            try
            {
                // If the requested file does not exist load default settings profile
                if (!File.Exists(fileName))
                {
                    fileName = Path.Combine(userProfilesPath, defaultUserSettingsfileName);
                }
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    UserProfileSettings userProfileSettings = JsonConvert.DeserializeObject<UserProfileSettings>(json);
                    if (userProfileSettings != null)
                    {
                        return userProfileSettings;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing " + fileName + ": " + e.Message);
            }
            return null;
        }
        public static void saveUserSettingsFile(UserProfileSettings profileSettings, String fileName)
        {
            if (!Directory.Exists(userProfilesPath))
            {
                try
                {
                    Directory.CreateDirectory(userProfilesPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating " + userProfilesPath + ": " + e.Message);
                }
            }
            if (fileName != null)
            {
                try
                {
                    using (StreamWriter file = File.CreateText(Path.Combine(userProfilesPath, fileName)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(file, profileSettings);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + fileName + ": " + e.Message);
                }
            }
        }


        // upgrade settings in all user profiles found in user profiles folder
        private void upgradeUserProfileSettings()
        {
            try
            {
                string[] files = Directory.GetFiles(userProfilesPath, "*.json", SearchOption.TopDirectoryOnly);
                UserProfileSettings defaultAppSettings = new UserProfileSettings();
                //build a list of current user scope settings.
                foreach (SettingsProperty prop in getProperties())
                {
                    defaultAppSettings.userSettings.Add(prop.Name, Properties.Settings.Default[prop.Name] );
                }

                foreach (var file in files)
                {
                    UserProfileSettings userProfileSetting = loadUserSettings(file);
                    if(userProfileSetting != null)
                    {
                        Boolean save = false;
                        // add any missing items
                        var addedDefaultItems = defaultAppSettings.userSettings.Where(ups2 => !userProfileSetting.userSettings.Any(ups1 => ups1.Key == ups2.Key)).ToList();
                        if (addedDefaultItems.Count > 0)
                        {
                            foreach (var item in addedDefaultItems)
                            {
                                userProfileSetting.userSettings.Add(item.Key, item.Value);
                            }
                            save = true;
                        }
                        // remove items no longer used
                        var removedDefaultItems = userProfileSetting.userSettings.Where(ups2 => !defaultAppSettings.userSettings.Any(ups1 => ups1.Key == ups2.Key)).ToList();
                        if (removedDefaultItems.Count > 0)
                        {
                            foreach (var item in removedDefaultItems)
                            {
                                userProfileSetting.userSettings.Remove(item.Key);
                            }
                            save = true;
                        }
                        if (save)
                        {
                            saveUserSettingsFile(userProfileSetting, file);
                        }
                    }

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Load the user settings from either "current_settings_profile"
        /// or the profile specified in the command line arg -profile <profile name>
        /// If the command line profile file does not exist use the default
        /// (not the current) profile instead.
        /// </summary>
        internal UserSettings()
        {
            // Set profile from command line '-profile "file name without extension" ...'.  This needs to be
            // done here, because this executes before Main.
            var profileRequestedFromCommandLine = CrewChief.CommandLine.Get("profile");

            if (!string.IsNullOrWhiteSpace(profileRequestedFromCommandLine))
            {
                // Initialise to defaultUserSettingsfileName for the case where
                // the specified profile does not exist
                Properties.Settings.Default["current_settings_profile"] = defaultUserSettingsfileName;
                var files = Directory.GetFiles(userProfilesPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
                foreach (var file in files)
                {
                    var fileNameNoExt = Path.GetFileNameWithoutExtension(file);
                    if (profileRequestedFromCommandLine.Equals(fileNameNoExt, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var fileName = Path.GetFileName(file);
                        Properties.Settings.Default["current_settings_profile"] = fileName;
                        break;
                    }
                }
            }
            try
            {
                // start by checked we can actually read a property value - this will throw an exception if the
                // user settings in AppData are broken
                int x = Properties.Settings.Default.main_window_position.X;

                // Add build in action mappings to reserved name list.
                List<string> nameList = reservedNameStarts.ToList();
                nameList.AddRange(ControllerConfiguration.builtInActionMappings.Keys);
                reservedNameStarts = nameList.ToArray();

                foreach (SettingsProperty prop in getProperties(true))
                {
                    currentApplicationSettings.Add(prop.Name, Properties.Settings.Default[prop.Name]);
                }

                // Copy user settings from previous application version if necessary
                String savedAppVersion = getString("app_version");
                if (savedAppVersion == null || !savedAppVersion.Equals(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                {
                    Properties.Settings.Default.Upgrade();
                    setProperty("app_version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Properties.Settings.Default.Save();
                    upgradeUserProfileSettings();

                    // We need to reload Application settings if we've upgraded (otherwise we miss a lot of stuff set in the prev app version, including active profile).
                    currentApplicationSettings.Clear();
                    foreach (SettingsProperty prop in getProperties(true))
                    {
                        currentApplicationSettings.Add(prop.Name, Properties.Settings.Default[prop.Name]);
                    }
                }

                // get the filename of the current active profile
                currentUserProfileFileName = getString("current_settings_profile");

                if (!string.IsNullOrWhiteSpace(currentUserProfileFileName))
                {
                    loadActiveUserSettingsProfile(fileName: Path.Combine(userProfilesPath, currentUserProfileFileName), loadingDefault: false);
                }
                else
                {
                    loadActiveUserSettingsProfile(fileName: Path.Combine(userProfilesPath, defaultUserSettingsfileName), loadingDefault: true);
                }
            }
            catch (Exception exception)
            {
                // if any of this initialisation fails, the app is in an unusable state.
                Console.WriteLine(exception.Message);
                initFailed = true;
                initFailedMessage = exception.Message;
                initFailedStack = exception.StackTrace;
            }
        }

        private static List<SettingsProperty> getProperties(bool applicationScopeOnly = false)
        {
            List<SettingsProperty> props = new List<SettingsProperty>();

            foreach (SettingsProperty prop in Properties.Settings.Default.Properties)
            {
                Boolean isReserved = false;
                foreach (String reservedNameStart in reservedNameStarts)
                {
                    if (prop.Name.StartsWith(reservedNameStart))
                    {
                        if(applicationScopeOnly)
                        {
                            props.Add(prop);
                        }
                        isReserved = true;
                        break;
                    }
                }
                if (!isReserved && !applicationScopeOnly)
                {
                    props.Add(prop);
                }
            }
            return props.OrderBy(x => x.Name).ToList();
        }

        public List<SettingsProperty> getProperties(Type requiredType, String nameMustStartWith, String nameMustNotStartWith)
        {
            List<SettingsProperty> props = new List<SettingsProperty>();
            if (!initFailed)
            {
                foreach (SettingsProperty prop in Properties.Settings.Default.Properties)
                {
                    Boolean isReserved = false;
                    foreach (String reservedNameStart in reservedNameStarts)
                    {
                        if (prop.Name.StartsWith(reservedNameStart))
                        {
                            isReserved = true;
                            break;
                        }
                    }
                    if (!isReserved &&
                        (nameMustStartWith == null || nameMustStartWith.Length == 0 || prop.Name.StartsWith(nameMustStartWith)) &&
                        (nameMustNotStartWith == null || nameMustNotStartWith.Length == 0 || !prop.Name.StartsWith(nameMustNotStartWith)) &&
                        !prop.IsReadOnly && prop.PropertyType == requiredType)
                    {
                        props.Add(prop);
                    }
                }
            }
            return props.OrderBy(x => x.Name).ToList();
        }

        private static UserSettings _userSettings = new UserSettings();

        private Boolean propertiesUpdated = false;
        private Boolean userProfilePropertiesUpdated = false;

        public static UserSettings GetUserSettings()
        {
            if (_userSettings == null)
            {
                _userSettings = new UserSettings();
            }
            return _userSettings;
        }

        public String getString(String name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    if (currentActiveProfile.userSettings.TryGetValue(name, out object value))
                    {
                        return (String)value;
                    }
                    else if (currentApplicationSettings.TryGetValue(name, out value))
                    {
                        return (String)value;
                    }
                    else
                    {
                        return (String)Properties.Settings.Default[name];
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("PROPERTY " + name + " NOT FOUND");
                }
            }

            return "";
        }

        public float getFloat(String name)
        {

            try
            {
                if (currentActiveProfile.userSettings.TryGetValue(name, out object value))
                {
                    return Convert.ToSingle(value);
                }
                else if (currentApplicationSettings.TryGetValue(name, out value))
                {
                    return Convert.ToSingle(value);
                }
                else
                {
                    return (float)Properties.Settings.Default[name];
                }
            }
            catch (Exception)
            {
                Console.WriteLine("PROPERTY " + name + " NOT FOUND");
            }

            return 0f;
        }

        public Boolean getBoolean(String name)
        {
            try
            {
                if (currentActiveProfile.userSettings.TryGetValue(name, out object value))
                {
                    return Convert.ToBoolean(value);
                }
                else if (currentApplicationSettings.TryGetValue(name, out value))
                {
                    return Convert.ToBoolean(value);
                }
                else
                {
                    return (Boolean)Properties.Settings.Default[name];
                }
            }
            catch (Exception)
            {
                Console.WriteLine("PROPERTY " + name + " NOT FOUND");
            }

            return false;
        }

        public int getInt(String name)
        {
            try
            {
                if (currentActiveProfile.userSettings.TryGetValue(name, out object value))
                {
                    return Convert.ToInt32(value);
                }
                else if (currentApplicationSettings.TryGetValue(name, out value))
                {
                    return Convert.ToInt32(value);
                }
                else
                {
                    return (int)Properties.Settings.Default[name];
                }
            }
            catch (Exception)
            {
                Console.WriteLine("PROPERTY " + name + " NOT FOUND");
            }
            return 0;
        }

        public void setProperty(String name, Object value)
        {
            if (!initFailed)
            {
                if (currentActiveProfile.userSettings.ContainsKey(name))
                {
                    if (!value.Equals(currentActiveProfile.userSettings[name]))
                    {
                        userProfilePropertiesUpdated = true;
                        currentActiveProfile.userSettings[name] = value;
                    }
                }
                else if (!value.Equals(Properties.Settings.Default[name]))
                {
                    Properties.Settings.Default[name] = value;
                    currentApplicationSettings[name] = value;
                    propertiesUpdated = true;
                }
            }
        }

        public void saveUserSettings()
        {
            // By MSDN it is not ok to write from multiple threads simultaneously, so lock here.
            lock (this)
            {
                if (!initFailed)
                {
                    if(propertiesUpdated)
                    {
                        Properties.Settings.Default.Save();
                    }
                    if(userProfilePropertiesUpdated)
                    {
                        saveUserSettingsFile(currentActiveProfile, currentUserProfileFileName);
                    }
                }
            }
        }

        /// <summary>
        /// Return the settings that have been changed
        /// </summary>
        /// <returns>String of lines, one per changed setting</returns>
        public static string getNonDefaultUserSettings()
        {
            string changes = null;
            string value = null;
            foreach (SettingsProperty prop in getProperties())
            {
                if (currentActiveProfile.userSettings.ContainsKey(prop.Name))
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        if (prop.DefaultValue.Equals(currentActiveProfile.userSettings[prop.Name]))
                        {
                            continue;
                        }
                        // Quote strings to show up any white space
                        value = $"'{currentActiveProfile.userSettings[prop.Name]}'";
                    }
                    else
                    {
                        if (prop.DefaultValue.Equals(currentActiveProfile.userSettings[prop.Name].ToString()))
                        {
                            continue;
                        }
                        value = $"{currentActiveProfile.userSettings[prop.Name]}";
                    }
                    changes += $"{prop.Name}: {value}\n";
                }
                else
                {
                    changes += $"Error: '{prop.Name}' not in userSettings\n";
                }
            }
            return changes;
        }
    }
}