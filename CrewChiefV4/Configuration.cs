using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CrewChiefV4
{
    public class Configuration
    {
        private static String UI_TEXT_FILENAME = "ui_text\\en.txt";
        private static String UI_TEXT_FILENAME_LEGACY = "ui_text.txt";  // this is used to load old UI translations from AppData/local/CrewChiefV4/ui_text.txt
        private static String UI_TEXT_LOCALISED_FILENAME = "ui_text\\{locale}.txt";
        private static String SPEECH_RECOGNITION_CONFIG_FILENAME = "speech_recognition_config.txt";
        private static String SOUNDS_CONFIG_FILENAME = "sounds_config.txt";

        private static Dictionary<String, String> UIStrings = LoadUIStrings();
        private static Dictionary<String, String> SpeechRecognitionConfig = LoadSpeechRecognitionConfig();
        private static Dictionary<String, String> SoundsConfig = LoadSoundsConfig();

        public static String getUIString(String key) {
            string uiString = null;
            if (UIStrings.TryGetValue(key, out uiString)) {
                return uiString;
            }
            return key;
        }

        public static String getUIStringStrict(String key)
        {
            string uiString = null;
            if (UIStrings.TryGetValue(key, out uiString))
            {
                return uiString;
            }
            return null;
        }

        public static String getSoundConfigOption(String key)
        {
            string soundConfig = null;
            if (SoundsConfig.TryGetValue(key, out soundConfig))
            {
                return soundConfig;
            }
            return key;
        }

        public static String getSpeechRecognitionConfigOption(String key)
        {
            string sreConfig = null;
            if (SpeechRecognitionConfig.TryGetValue(key, out sreConfig))
            {
                return sreConfig;
            }
            return key;
        }

        /// <summary>
        /// Add phrase to phrasesList
        /// If the phrase starts with "pitstop" add the phrase starting
        /// with "box" as well. Throw away any starting with "box"
        /// </summary>
        /// <param name="phrasesList"></param>
        /// <param name="phrase"></param>
        private static void pitstopBox(List<String> phrasesList, String phrase)
        { 
            if (phrase.StartsWith("pitstop "))
            {
                phrasesList.Add(phrase);
                int LEN_PITSTOP = "pitstop ".Length;
                phrasesList.Add($"box {phrase.Substring(LEN_PITSTOP)}");
            }
            else if (!phrase.StartsWith("box "))
            {
                phrasesList.Add(phrase);
            }
        }

        public static String[] getSpeechRecognitionPhrases(String key, bool loadHomophones = false)
        {
            List<string> phrasesList = new List<string>();
            string options = null;
            if (SpeechRecognitionConfig.TryGetValue(key, out options))
            {
                if (options.Contains(":"))
                {
                    var phrases = options.Split(':');
                    for (int i = 0; i < phrases.Length; ++i)
                    {
                        String phrase = phrases[i].Trim();
                        if (phrase.Length > 0)
                        {
                            pitstopBox(phrasesList, phrase);
                        }
                    }
                }
                else if (options.Length > 0)
                {
                    pitstopBox(phrasesList, options);
                }
                if (loadHomophones)
                {
                    string homophonesOptions = null;
                    if (SpeechRecognitionConfig.TryGetValue(key + "_HOMOPHONES", out homophonesOptions))
                    {
                        if (homophonesOptions.Contains(":"))
                        {
                            var phrases = homophonesOptions.Split(':');
                            for (int i = 0; i < phrases.Length; ++i)
                            {
                                String phrase = phrases[i].Trim();
                                if (phrase.Length > 0)
                                {
                                    phrasesList.Add(phrase);
                                }
                            }
                        }
                        else if (homophonesOptions.Length > 0)
                        {
                            phrasesList.Add(homophonesOptions);
                        }
                    }
                }
            }
            return phrasesList.ToArray();
        }

        public static String getDefaultFileLocation(String filename)
        {
            return getDefaultLocation(filename, true);
        }

        public static String getDefaultFolderLocation(String folderName)
        {
            return getDefaultLocation(folderName, false);
        }

        private static String getDefaultLocation(String name, Boolean isFile)
        {
            String regularPath = Application.StartupPath + @"\" + name;
            String debugPath = Application.StartupPath + @"\..\..\" + name;
            if (CrewChief.UseDebugFilePaths)
            {
                if (isFile)
                {
                    return File.Exists(debugPath) ? debugPath : regularPath;
                }
                else
                {
                    return Directory.Exists(debugPath) ? debugPath : regularPath;
                }
            }
            else
            {
                if (isFile)
                {
                    return File.Exists(regularPath) ? regularPath : debugPath;
                }
                else
                {
                    return Directory.Exists(regularPath) ? regularPath : debugPath;
                }
            }
        }

        public static String getUserOverridesFileLocation(String filename)
        {
            return Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\CrewChiefV4\" + filename);
        }

        private static void merge(StreamReader file, Dictionary<String, String> dict)
        {
            String line;
            while ((line = file.ReadLine()) != null)
            {
                if (!line.StartsWith("#") && line.Contains("="))
                {
                    try
                    {
                        String[] split = line.Split('=');
                        String key = split[0].Trim();
                        dict.Remove(key);
                        dict.Add(split[0].Trim(), split[1].Trim());
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static Dictionary<string, string> LoadConfigHelper(string configFileName, string localisedConfigFileName)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            LoadAndMerge(dict, getDefaultFileLocation(configFileName));
            if (localisedConfigFileName != null)
            {
                try
                {
                    var locale = UserSettings.GetUserSettings().getString("ui_language");
                    if (string.IsNullOrWhiteSpace(locale))
                    {
                        locale = CultureInfo.InstalledUICulture.Name;
                    }
                    var localised = getDefaultFileLocation(localisedConfigFileName.Replace("{locale}", locale));
                    var language = getDefaultFileLocation(localisedConfigFileName.Replace("{locale}", locale.Substring(0, 2)));
                    if (File.Exists(language))
                    {
                        Debug.WriteLine("Found language override for: " + configFileName + ". Locale " + locale + ". Merging.");
                        LoadAndMerge(dict, language);
                    }
                    if (File.Exists(localised))
                    {
                        Debug.WriteLine("Found localised override for: " + configFileName + ". Locale " + locale + ". Merging.");
                        LoadAndMerge(dict, localised);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error loading localised ui: " + e.Message);
                }
            }

            var overrideFileName = getUserOverridesFileLocation(configFileName);
            // fall back to old UI text override location if no new version is provided to maintain backwards compatibility
            var legacyOverrideFileName = getUserOverridesFileLocation(UI_TEXT_FILENAME_LEGACY);
            if (File.Exists(overrideFileName))
            {
                Debug.WriteLine("Found user override for: " + configFileName + ". Merging.");
                LoadAndMerge(dict, overrideFileName);
            }
            else if (File.Exists(legacyOverrideFileName))
            {
                Debug.WriteLine("Found user override for: " + legacyOverrideFileName + ". Merging.");
                LoadAndMerge(dict, legacyOverrideFileName);
            }
            return dict;
        }

        private static void LoadAndMerge(Dictionary<string, string> dict, string language)
        {
            using (var file = new StreamReader(language))
            {
                try
                {

                    merge(file, dict);
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (file != null)
                    {
                        file.Close();
                    }
                }
            }
        }

        private static Dictionary<String, String> LoadSpeechRecognitionConfig()
        {
            return LoadConfigHelper(SPEECH_RECOGNITION_CONFIG_FILENAME, null);
        }

        private static Dictionary<String, String> LoadSoundsConfig()
        {
            return LoadConfigHelper(SOUNDS_CONFIG_FILENAME, null);
        }

        private static Dictionary<String, String> LoadUIStrings()
        {
            return LoadConfigHelper(UI_TEXT_FILENAME, UI_TEXT_LOCALISED_FILENAME);
        }
    }
}
