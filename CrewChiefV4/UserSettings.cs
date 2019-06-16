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

        private static  String[] reservedNameStarts = new String[] { "CHANNEL_", "TOGGLE_", "VOICE_OPTION", "background_volume", 
            "messages_volume", "last_game_definition", "REPEAT_LAST_MESSAGE_BUTTON", "UpdateSettings", "VOLUME_UP", "VOLUME_DOWN", "GET_FUEL_STATUS",
            ControllerConfiguration.ControllerData.PROPERTY_CONTAINER, "PERSONALISATION_NAME", "app_version", "PRINT_TRACK_DATA", "spotter_name",
            "update_notify_attempted", "last_trace_file_name", "READ_CORNER_NAMES_FOR_LAP", "GET_CAR_STATUS", "GET_SESSION_STATUS", "GET_DAMAGE_REPORT", "GET_STATUS",
            "TOGGLE_PACE_NOTES_", "NAUDIO_DEVICE_GUID", "NAUDIO_RECORDING_DEVICE_GUID", "TOGGLE_TRACK_LANDMARKS_RECORDING", "ADD_TRACK_LANDMARK",
            "PIT_PREDICTION", "TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS", "chief_name", "current_settings_profile"};

        private static String defaultUserSettingsfileName = "defaultSettings.json";
        private static String userProfilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "Profiles");
        private static String currentUserProfileFileName = "";

        public class UserSetting
        {
            public string name { get; set; }
            public object value { get; set; }
            public Type type { get; set; }
        }
        public class UserProfileSettings
        {
            public string profileName { get; set; }
            public List<UserSetting> userSettings { get; set; }
            public UserProfileSettings()
            {
                userSettings = new List<UserSetting>();
            }
        }
        UserProfileSettings currentProfile = new UserProfileSettings();

        public static UserProfileSettings loadUserSettings(String fileName)
        {
            try
            {
                using (StreamReader r = new StreamReader(fileName))
                {
                    string json = r.ReadToEnd();
                    UserProfileSettings data = JsonConvert.DeserializeObject<UserProfileSettings>(json);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch (Exception e)
            {

            }
            return new UserProfileSettings();

        }

        public static void saveUserSettinsFile(UserProfileSettings profileSettings, String fileName)
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
            string[] files = Directory.GetFiles(userProfilesPath, "*.json", SearchOption.TopDirectoryOnly);
            UserProfileSettings defaultAppSettings = new UserProfileSettings();
            //build a list of current user scope settings.
            foreach (SettingsProperty prop in getProperties())
            {
                defaultAppSettings.userSettings.Add(new UserSetting() { name = prop.Name, value = Properties.Settings.Default[prop.Name], type = prop.PropertyType });
            }

            foreach (var file in files)
            {                
                UserProfileSettings userProfileSetting = loadUserSettings(file);
                Boolean save = false;
                // add any missing items
                var addedDefaultItems = defaultAppSettings.userSettings.Where(ups2 => !userProfileSetting.userSettings.Any(ups1 => ups1.name == ups2.name));
                if (addedDefaultItems.ToList().Count > 0)
                {
                    save = true;
                    userProfileSetting.userSettings.AddRange(addedDefaultItems);
                }
                // remove items no longer used
                var removedDefaultItems = userProfileSetting.userSettings.Where(ups2 => !defaultAppSettings.userSettings.Any(ups1 => ups1.name == ups2.name));
                if(removedDefaultItems.ToList().Count > 0)
                {
                    save = true;
                    foreach (var item in removedDefaultItems)
                    {
                        userProfileSetting.userSettings.Remove(item);
                    }                    
                }
                if(save)
                {
                    saveUserSettinsFile(userProfileSetting, file);
                }
            }            
        }
        private UserSettings()
        {
            try
            {
                // Copy user settings from previous application version if necessary
                String savedAppVersion = getString("app_version");                
                if (savedAppVersion == null || !savedAppVersion.Equals(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()))
                {
                    Properties.Settings.Default.Upgrade();
                    setProperty("app_version", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                    Properties.Settings.Default.Save();
                    upgradeUserProfileSettings();
                }
                
                // get the filename of the current active profile
                currentUserProfileFileName = getString("current_settings_profile");
                // Create a user profile with the users current settings if it does not yet exist(new user and first time upgrade to new format)
                if (!File.Exists(Path.Combine(userProfilesPath, defaultUserSettingsfileName)))
                {
                    foreach (SettingsProperty prop in getProperties())
                    {
                        currentProfile.userSettings.Add(new UserSetting() { name = prop.Name, value = Properties.Settings.Default[prop.Name], type = prop.PropertyType });
                    }
                    saveUserSettinsFile(currentProfile, Path.Combine(userProfilesPath, defaultUserSettingsfileName));
                }
                else
                {
                    
                    if (File.Exists(Path.Combine(userProfilesPath, currentUserProfileFileName)))
                    {
                        currentProfile = loadUserSettings(Path.Combine(userProfilesPath, currentUserProfileFileName));
                    }
                    else
                    {
                        currentProfile = loadUserSettings(Path.Combine(userProfilesPath, defaultUserSettingsfileName));
                        setProperty("current_settings_profile", defaultUserSettingsfileName);
                        Properties.Settings.Default.Save();
                        currentUserProfileFileName = getString("current_settings_profile");
                    }
                }
            }
            catch (Exception exception)
            {
                // if any of this initialisation fails, the app is in an unusable state.
                Console.WriteLine(exception.Message);
                initFailed = true;
            }
        }

        private List<SettingsProperty> getProperties()
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
                    if (!isReserved)
                    {
                        props.Add(prop);
                    }
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

        private static readonly UserSettings _userSettings = new UserSettings();

        private Boolean propertiesUpdated = false;

        public static UserSettings GetUserSettings()
        {
            return _userSettings;
        }

        public String getString(String name)
        {
            UserSetting setting = currentProfile.userSettings.FirstOrDefault(s => s.name == name);
            if (setting != null)
            {
                return (String)setting.value;
            }
            else
            {
                try
                {
                    return (String)Properties.Settings.Default[name];
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
            UserSetting setting = currentProfile.userSettings.FirstOrDefault(s => s.name == name);
            if (setting != null)
            {
                return Convert.ToSingle(setting.value);
            }
            else
            {
                try
                {
                    return (float)Properties.Settings.Default[name];
                }
                catch (Exception)
                {
                    Console.WriteLine("PROPERTY " + name + " NOT FOUND");
                }
            }

            return 0f;
        }

        public Boolean getBoolean(String name)
        {
            UserSetting setting = currentProfile.userSettings.FirstOrDefault(s => s.name == name);
            if (setting != null)
            {
                return Convert.ToBoolean(setting.value);
            }
            else
            {
                try
                {
                    return (Boolean)Properties.Settings.Default[name];
                }
                catch (Exception)
                {
                    Console.WriteLine("PROPERTY " + name + " NOT FOUND");
                }
            }

            return false;
        }

        public int getInt(String name)
        {
            UserSetting setting = currentProfile.userSettings.FirstOrDefault(s => s.name == name);
            if (setting != null)
            {
                return Convert.ToInt32(setting.value);
            }
            else
            {
                try
                {
                    return (int)Properties.Settings.Default[name];
                }
                catch (Exception)
                {
                    Console.WriteLine("PROPERTY " + name + " NOT FOUND");
                }
            }
            return 0;
        }

        public void setProperty(String name, Object value)
        {
            if (!initFailed)
            {
                UserSetting setting = currentProfile.userSettings.FirstOrDefault(s => s.name == name);
                if (setting != null)
                {
                    if (value != setting.value)
                    {
                        propertiesUpdated = true;
                        setting.value = value;                 
                    }
                }
                else if (value != Properties.Settings.Default[name])
                {
                    Properties.Settings.Default[name] = value;
                    propertiesUpdated = true;
                }
            }
        }

        public void saveUserSettings()
        {
            // By MSDN it is not ok to write from multiple threads simultaneously, so lock here.
            lock (this)
            {
                if (!initFailed && propertiesUpdated)
                {
                    Properties.Settings.Default.Save();
                    saveUserSettinsFile(currentProfile, currentUserProfileFileName);
                }
            }
        }
    }
}
