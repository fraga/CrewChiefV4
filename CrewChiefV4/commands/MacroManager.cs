using CrewChiefV4.Audio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CrewChiefV4.commands
{
    class MacroManager
    {
        // these are the macro names used to identify certain macros which have special hard-coded behaviours. Not ideal...
        public static readonly String REQUEST_PIT_IDENTIFIER = "request pit";
        public static readonly String CANCEL_REQUEST_PIT_IDENTIFIER = "cancel pit request";

        public static readonly String MULTIPLE_PRESS_IDENTIFIER = "MULTIPLE";
        public static readonly String FREE_TEXT_IDENTIFIER = "FREE_TEXT";
        public static readonly String MULTIPLE_PRESS_FROM_VOICE_TRIGGER_IDENTIFIER = "VOICE_TRIGGER";
        public static readonly String WAIT_IDENTIFIER = "WAIT";

        public static Boolean enablePitExitPositionEstimates = UserSettings.GetUserSettings().getBoolean("enable_pit_exit_position_estimates");

        public static Boolean bringGameWindowToFrontForMacros = UserSettings.GetUserSettings().getBoolean("bring_game_window_to_front_for_macros");

        public static Boolean stopped = false;

        // make all the macros available so the events can press buttons as they see fit:
        public static Dictionary<string, ExecutableCommandMacro> macros = new Dictionary<string, ExecutableCommandMacro>();

        public static int MAX_FUEL_RESET_COUNT = 150;

        public static void stop()
        {
            stopped = true;
            KeyPresser.releasePressedKey();
        }

        // converter 
        public static MacroContainer convertAssignmentToKey(MacroContainer macroContainer)
        {
            foreach(var assignment in macroContainer.assignments)
            {
                foreach(var keyBinding in assignment.keyBindings)
                {
                    foreach(var macro in macroContainer.macros)
                    {
                        foreach(CommandSet cs in macro.commandSets)
                        {
                            List<string> convertedActions = new List<string>();
                            foreach (var action in cs.actionSequence)
                            {
                                if(action.Contains(keyBinding.action))                                
                                {
                                    string convertesAction = action.Replace(keyBinding.action, keyBinding.key);
                                    convertedActions.Add(convertesAction);
                                }
                                else
                                {
                                    convertedActions.Add(action);
                                }
                            }
                            cs.actionSequence = convertedActions.ToArray();
                        }
                    }
                }
            }
            macroContainer.assignments = null;
            return macroContainer;
        }
        // This is called immediately after initialising the speech recogniser in MainWindow
        public static void initialise(AudioPlayer audioPlayer, SpeechRecogniser speechRecogniser)
        {
            stopped = false;
            macros.Clear();
            if (UserSettings.GetUserSettings().getBoolean("enable_command_macros"))
            {
                // load the json:
                MacroContainer macroContainer = loadCommands(getMacrosFileLocation());

                // if it's valid, load the command sets:
                Dictionary<string, ExecutableCommandMacro> voiceTriggeredMacros = new Dictionary<string, ExecutableCommandMacro>();
                foreach (Macro macro in macroContainer.macros)
                {
                    Boolean hasCommandForCurrentGame = false;
                    // eagerly load the key bindings for each macro:
                    foreach (CommandSet commandSet in macro.commandSets)
                    {
                        if (commandSet.gameDefinition.Equals(CrewChief.gameDefinition.gameEnum.ToString(), StringComparison.InvariantCultureIgnoreCase))
                        {
                            // this does the conversion from key characters to key enums and stores the result to save us doing it every time
                            if (!commandSet.loadActionItems())
                            {
                                Console.WriteLine("Macro \"" + macro.name + "\" failed to load - some actionItems didn't parse succesfully");
                            }
                            else
                            {
                                hasCommandForCurrentGame = true;
                            }
                            break;
                        }
                    }
                    if (hasCommandForCurrentGame)
                    {
                        // make this macro globally visible:
                        ExecutableCommandMacro commandMacro = new ExecutableCommandMacro(audioPlayer, macro);
                        macros.Add(macro.name, commandMacro);
                        // if there's a voice command, load it into the recogniser:
                        if (macro.voiceTriggers != null && macro.voiceTriggers.Length > 0)
                        {
                            foreach (String voiceTrigger in macro.voiceTriggers)
                            {
                                if (voiceTriggeredMacros.ContainsKey(voiceTrigger))
                                {
                                    Console.WriteLine("Voice trigger " + voiceTrigger + " has already been allocated to a different command");
                                }
                                else
                                {
                                    voiceTriggeredMacros.Add(voiceTrigger, commandMacro);
                                }
                            }
                        }
                        else if (macro.integerVariableVoiceTrigger != null && macro.integerVariableVoiceTrigger.Length > 0)
                        {
                            if (voiceTriggeredMacros.ContainsKey(macro.integerVariableVoiceTrigger))
                            {
                                Console.WriteLine("Voice trigger " + macro.integerVariableVoiceTrigger + " has already been allocated to a different command");
                            }
                            else
                            {
                                voiceTriggeredMacros.Add(macro.integerVariableVoiceTrigger, commandMacro);
                            }
                        }
                    }
                }
                try
                {
                    speechRecogniser.loadMacroVoiceTriggers(voiceTriggeredMacros);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load command macros into speech recogniser: " + e.Message);
                }
            }
            else
            {
                Console.WriteLine("Command macros are disabled");
            }
        }

        // file loading boilerplate - needs refactoring
        public static MacroContainer loadCommands(String filename)
        {
            if (filename != null)
            {
                try
                {                    
                    MacroContainer macroContainer = JsonConvert.DeserializeObject<MacroContainer>(getFileContents(filename));
                    // Conver any existing user created command macros to new "assignment" format
                    if (macroContainer != null)
                    {
                        if (macroContainer.assignments != null)
                        {
                            macroContainer = convertAssignmentToKey(macroContainer);
                            saveCommands(macroContainer);
                        }
                        return macroContainer;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error pasing " + filename + ": " + e.Message);
                }
            }
            return new MacroContainer();
        }

        public static void saveCommands(MacroContainer macroContainer)
        {
            String fileName = "saved_command_macros.json";
            String path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4");
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating " + path + ": " + e.Message);
                }
            }
            if (fileName != null)
            {
                try
                {
                    using (StreamWriter file = File.CreateText(System.IO.Path.Combine(path, fileName)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(file, macroContainer);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error parsing " + fileName + ": " + e.Message);
                }
            }         
        }

        private static String getFileContents(String fullFilePath)
        {
            StringBuilder jsonString = new StringBuilder();
            StreamReader file = null;
            try
            {
                file = new StreamReader(fullFilePath);
                String line;
                while ((line = file.ReadLine()) != null)
                {
                    if (!line.Trim().StartsWith("#"))
                    {
                        jsonString.AppendLine(line);
                    }
                }
                return jsonString.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error reading file " + fullFilePath + ": " + e.Message);
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            return null;
        }

        public static String getMacrosFileLocation(bool forceDefault = false)
        {
            String path = System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "saved_command_macros.json");
           
            if (File.Exists(path) && !forceDefault) // forceDefault can/should only be true when called from the macro editor
            {
                Console.WriteLine("Loading user-configured command macros from Documents/CrewChiefV4/ folder");
                return path;
            }
            // make sure we save a copy to the user config directory
            // no need to worry about forceDefault as content of the file will be same.
            else if (!File.Exists(path))
            {
                try 
                {                    
                    File.Copy(Configuration.getDefaultFileLocation("saved_command_macros.json"), path);                    
                    return path;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error copying default macro configuration file to user dir : " + e.Message);
                    Console.WriteLine("Loading default command macros from installation folder");
                    return Configuration.getDefaultFileLocation("saved_command_macros.json");
                }                
            }
            else
            {
                Console.WriteLine("Loading default command macros from installation folder");
                return Configuration.getDefaultFileLocation("saved_command_macros.json");
            }
        }
    }
}
