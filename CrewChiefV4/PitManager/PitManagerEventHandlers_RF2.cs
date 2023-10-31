using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PitMenuAPI;

// Pit stop texts
// TIRES:
// STOP/GO:
// R TIRES:
// F TIRES:
// DRIVER:
// R SPOILER:
// GRILLE:
// WEDGE:
// TRACK BAR:
// RADIATOR:
// FR PRESS:
// FL PRESS:
// RR PRESS:
// RL PRESS:
// FR RUBBER:
// FL RUBBER:
// RR RUBBER:
// RL RUBBER:
// PITSTOPS:
// DAMAGE:
// RT TIRES:
// LF TIRES:
// FR TIRE:
// FL TIRE:
// RR TIRE:
// RL TIRE:
// L FENDER:
// L FLIP UP:
// R FENDER:
// R FLIP UP:
// F WING:
// FRONT DF:
// F AIR DAM:
// F SPLITTER:
// R WING:
// REAR DF:
//
// Player.JSON entries relevant to the Pit Menu:
//"Relative Fuel Strategy":false,
//"Relative Fuel Strategy#":"Show how much fuel to ADD, rather than how much TOTAL fuel to fill the tank up to (note: new default is true)",
//      Pit Manager handles true or false
//"Smart Pitcrew":true,
//"Smart Pitcrew#":"Pitcrew does things even if you mistakenly forgot to ask (one example is changing a damaged tire)",
//      Doesn't affect Pit Manager

namespace CrewChiefV4.PitManager
{
    public class PitManagerEventHandlers_RF2 // public for unit testing
    {
        private static readonly PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();

        #region Public struct
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
        public class TyreDictionary : Dictionary<string, List<string>>
#pragma warning restore S3925 // "ISerializable" should be implemented correctly
        {
            public TyreDictionary()
            {
                TyreTranslationDict = new Dictionary<string, List<string>>();
            }
            public Dictionary<string, List<string>> TyreTranslationDict
            {
                get; set;
            }
        }
        #endregion Public struct

        public static class FuelVoiceCommand
        {
            private static bool _fuelVoiceCommandGiven = false;

            public static bool Given
            {
                get { return _fuelVoiceCommandGiven; }
                set { _fuelVoiceCommandGiven = value; }
            }
        }
        #region Private field made Public for unit testing
        // Complicated because rF2 has many names for tyres so use a dict of
        // possible alternative names for each type
        // Each entry has a list of possible matches in declining order
        // This is the default dict which is loaded into MyDocuments\CrewChiefV4\rF2\TyreDictionary.json
        // The user can edit that file to add new names if required
        public static readonly TyreDictionary SampleTyreTranslationDict =
          new TyreDictionary() {
            { "Hypersoft",    new List <string> {"hypersoft", "c1", "ultrasoft", "supersoft", "soft", "alternates",
                        "s310", "slick", "dry", "race", "allweather", "medium" } },
            { "Ultrasoft",    new List <string> {"ultrasoft","c1", "hypersoft", "supersoft", "soft", "alternates",
                        "s310", "slick", "dry", "race", "allweather", "medium" } },
            { "Supersoft",    new List <string> {"supersoft", "c2", "hypersoft", "ultrasoft", "soft", "alternates",
                        "s310", "slick", "dry", "race", "allweather", "medium" } },
            { "Soft",         new List <string> {"soft", "c3", "alternates",
                        "s310", "slick", "dry", "race", "allweather", "medium" } },
            { "Medium",       new List <string> { "medium", "c4", "default",
                        "s310", "slick", "dry", "race", "allweather" } },
            { "Hard",         new List <string> {"hard", "c5", "p310", "endur", "primary",
                        "medium", "default",
                                "slick", "dry", "race", "allweather" } },
            { "Intermediate", new List <string> { "intermediate", "inter", "inters",
                        "wet", "rain", "monsoon", "allweather" } },
            { "Wet",          new List <string> {
                        "wet", "rain", "monsoon", "allweather", "intermediate", "inter", "inters" } },
            { "Monsoon",      new List <string> {"monsoon",
                        "wet", "rain",  "allweather", "intermediate", "inter", "inters" } },
            { "No Change",    new List <string> {"no change"} }
            };
        #endregion Private field made Public for unit testing

        #region Private Fields
        static private TyreDictionary tyreTranslationDict =
                    TyreDictFile.getTyreDictionaryFromFile();

        static private CurrentRf2TyreType currentRf2TyreType = new CurrentRf2TyreType();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Take a list of tyre types available in the menu and map them on to
        /// the set of cc tyre types
        /// Hypersoft
        /// Ultrasoft
        /// Supersoft
        /// Soft
        /// Medium
        /// Hard
        /// Intermediate
        /// Wet
        /// Monsoon
        /// (No Change) for completeness
        ///
        /// Algorithm:
        /// Check the first list item for each key in tyreDict
        /// if the word is in inMenu then that key is DONE
        /// if not, check the 2nd list item
        /// If there are no exact matches in the whole dictionary then see if
        /// one of the tyre dict values is a sub-string of one of the game's entries
        /// e.g. "soft" in "Soft COMPOUND"
        /// </summary>
        /// <param name="tyreDict">
        /// The dict used for translation
        /// </param>
        /// <param name="inMenu">
        /// The list returned by GetTyreTypes()
        /// </param>
        /// <returns>
        /// Dictionary mapping CC tyre types to names of those available
        /// </returns>
        public static Dictionary<string, string> TranslateTyreTypes(
          TyreDictionary tyreDict,
          List<string> inMenu)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            inMenu.Remove("No Change");
            int columnCount = 1; // will increase

            for (var run = 0; run < 2; run++)
            {   // run = 0, exact match; run = 1, any matching word
                for (var col = 0; col < columnCount; col++)
                {
                    foreach (var ccTyreType in tyreDict)
                    { // "Hypersoft", "Ultrasoft", "Supersoft", "Soft"...
                        if (!result.ContainsKey(ccTyreType.Key))
                        { // Didn't match in run 0
                            foreach (var rF2TyreType in inMenu)
                            {  // Tyre type in the menu
                                if (ccTyreType.Value.Count > columnCount)
                                {
                                    columnCount = ccTyreType.Value.Count;
                                }
                                if (col < ccTyreType.Value.Count)
                                {
                                    var dictTyreName = ccTyreType.Value[col];
                                    // Normalise the rF2 tyre type name by removing spaces and -
                                    var normalisedRf2TyreType = Regex.Replace(rF2TyreType, " |-|_", "");
                                    if (run == 0)
                                    {
                                        if (normalisedRf2TyreType.Length == dictTyreName.Length &&
                                            normalisedRf2TyreType.IndexOf(dictTyreName, StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
#pragma warning disable S1066
                                            if (!result.ContainsKey(ccTyreType.Key))
                                            {
                                                result[ccTyreType.Key] = rF2TyreType;
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // Didn't find an exact match, see if dictTyreName
                                        // is in one of the menu items, e.g. "soft" in "Soft COMPOUND"
                                        if (normalisedRf2TyreType.IndexOf(dictTyreName, StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            // (Already checked that it's not in result)
                                            result[ccTyreType.Key] = rF2TyreType;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            foreach (var ccTyretype in tyreDict)
            {
                if (!result.ContainsKey(ccTyretype.Key))
                {
                // Still didn't match, give it something
                result[ccTyretype.Key] = inMenu[0];
                }
            }
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        // Event handlers

        static public bool PMrF2eh_initialise(string __)
        {
            Pmal.PmalConnect();
            List<string> tyreTypeNames = Pmal.GetTyreTypeNames();
            currentRf2TyreType.Set(tyreTypeNames[0]);
            Log.Commentary("Pit Manager initialise");
            foreach (var tyre in tyreTypeNames)
            {
                Log.Commentary($"Tyre type '{tyre}'");
            }
            return true;
        }
        static public bool PMrF2eh_teardown(string __)
        {
            Pmal.Disconnect();
            return true;
        }

        /// <summary>
        /// Switch rFactor to the menu page and wake up the inputs driver
        /// </summary>
        static public bool PMrF2eh_prepareToUseMenu(string __)
        {
            PitMenu pm = new PitMenu();
            pm.startUsingPitMenu();
            return true;
        }

        /// <summary> PMrF2eh_example
        /// Dummy action handler for rF2
        /// </summary>
        static public bool PMrF2eh_example(string __)
        {
            return true;
        }

        #region Tyre compounds
        static public bool PMrF2eh_TyreCompoundHard(string __)
        {
            return setTyreCompound("Hard");
        }

        static public bool PMrF2eh_TyreCompoundMedium(string __)
        {
            return setTyreCompound("Medium");
        }

        static public bool PMrF2eh_TyreCompoundSoft(string __)
        {
            return setTyreCompound("Soft");
        }

        static public bool PMrF2eh_TyreCompoundSupersoft(string __)
        {
            return setTyreCompound("Supersoft");
        }

        static public bool PMrF2eh_TyreCompoundUltrasoft(string __)
        {
            return setTyreCompound("Ultrasoft");
        }

        static public bool PMrF2eh_TyreCompoundHypersoft(string __)
        {
            return setTyreCompound("Hypersoft");
        }

        static public bool PMrF2eh_TyreCompoundIntermediate(string __)
        {
            return setTyreCompound("Intermediate");
        }

        static public bool PMrF2eh_TyreCompoundWet(string __)
        {
            return setTyreCompound("Wet");
        }

        static public bool PMrF2eh_TyreCompoundMonsoon(string __)
        {
            return setTyreCompound("Monsoon");
        }

        static public bool PMrF2eh_TyreCompoundOption(string __)
        {
            return setTyreCompound("Soft");
        }

        static public bool PMrF2eh_TyreCompoundPrime(string __)
        {
            return setTyreCompound("Hard");
        }

        static public bool PMrF2eh_TyreCompoundAlternate(string __)
        {
            return setTyreCompound("Soft");
        }

        static public bool PMrF2eh_TyreCompoundNext(string __)
        {
            // Select the next compound available for this car
            // Get the current tyre type
            // Get the list of tyre type, remove "No Change"
            List<string> tyreTypes = Pmal.GetTyreTypeNames();
            tyreTypes.Remove("No Change");
            string currentTyreTypeStr = Pmal.GetCurrentTyreType();

            int currentTyreTypeIndex = tyreTypes.IndexOf(currentTyreTypeStr) + 1;
            if (currentTyreTypeIndex >= tyreTypes.Count)
                currentTyreTypeIndex = 0;
            currentRf2TyreType.Set(tyreTypes[currentTyreTypeIndex]);
            return PMrF2eh_changeAllTyres(null);
        }
        #endregion Tyre compounds

        #region Which tyres to change
        static public bool PMrF2eh_changeAllTyres(string __)
        {
            return changeTyres(Pmal.GetAllTyreCategories());
        }

        static public bool PMrF2eh_changeNoTyres(string __)
        {
            return changeTyres(Pmal.GetAllTyreCategories(), true);
        }

        static public bool PMrF2eh_changeFrontTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetFrontTyreCategories());
        }

        static public bool PMrF2eh_changeRearTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRearTyreCategories());
        }

        static public bool PMrF2eh_changeLeftTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetLeftTyreCategories());
        }

        static public bool PMrF2eh_changeRightTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRightTyreCategories());
        }

        static public bool PMrF2eh_changeFLTyre(string __)
        {
            return changeTyre("FL TIRE:");
        }

        static public bool PMrF2eh_changeFRTyre(string __)
        {
            return changeTyre("FR TIRE:");
        }

        static public bool PMrF2eh_changeRLTyre(string __)
        {
            return changeTyre("RL TIRE:");
        }

        static public bool PMrF2eh_changeRRTyre(string __)
        {
            return changeTyre("RR TIRE:");
        }
        #endregion Which tyres to change

        #region Tyre pressures
        static public bool PMrF2eh_changeFLpressure(string voiceMessage)
        {
            return changeTyrePressure("FL PRESS:", voiceMessage);
        }
        #endregion Tyre pressures

        #region Fuel
        // Fuel Add:
        // If "Relative Fuel Strategy" set menu to X litres else set to X+current
        // X' = X+current
        // Fuel To:
        // If "Relative Fuel Strategy" set menu to X litres-current else set to X
        // X' = X
        //
        // in rF2SetFuel(X')
        // If "Relative Fuel Strategy" set to X'-current else set to X'
        static private bool FuelAddXlitres(string voiceMessage, int current)
        {
            var amount = PitNumberHandling.processNumber(voiceMessage);
            amount = PitNumberHandling.processLitresGallons(amount, voiceMessage);
            if (amount == 0)
            {
                return false;
            }
            if (amount > PitManagerVoiceCmds.getFuelCapacity())
            {
                amount = (int)PitManagerVoiceCmds.getFuelCapacity();
            }
            FuelVoiceCommand.Given = true;
            return rF2SetFuel(amount + current);
        }
        static public bool PMrF2eh_FuelAddXlitres(string voiceMessage)
        {
            return FuelAddXlitres(voiceMessage, (int)PitManagerVoiceCmds.getCurrentFuel());
        }
        static public bool PMrF2eh_FuelToXlitres(string voiceMessage)
        {
            return FuelAddXlitres(voiceMessage, 0);
        }

        static public bool PMrF2eh_FuelToEnd(string __)
        {
            var litresNeeded = PitFuelling.fuelToEnd(
                PitManagerVoiceCmds.getFuelCapacity(),
                PitManagerVoiceCmds.getCurrentFuel());
            if (litresNeeded < 0)
            {
                return true;    // Couldn't calculate
            }
            FuelVoiceCommand.Given = true;
            return rF2SetFuel(litresNeeded);
        }

        static public bool PMrF2eh_FuelNone(string __)
        {
            FuelVoiceCommand.Given = true;
            return rF2SetFuel(1);
        }

        static public bool rF2SetFuel(int amount)
        {
            if (Pmal.RelativeFuelStrategy())
            {
                amount = Math.Max(amount - (int)PitManagerVoiceCmds.getCurrentFuel(), 0);
            }
            return Pmal.SetFuelLevel(amount);
        }
        #endregion Fuel

        #region Repairs
        static public bool PMrF2eh_RepairAll(string __)
    {
        if (!Pmal.SoftMatchCategory("DAMAGE:"))
        {
            Pmal.RereadPitMenu();   // DAMAGE is not in initial menu, check if it is now
        }
        if (Pmal.SoftMatchCategory("DAMAGE:"))
        {
            return Pmal.SetChoice("Repair All");
        }
        return false;
    }

    static public bool PMrF2eh_RepairNone(string __)
    {
        if (!Pmal.SoftMatchCategory("DAMAGE:"))
        {
            Pmal.RereadPitMenu();   // DAMAGE is not in initial menu, check if it is now
        }
        if (Pmal.SoftMatchCategory("DAMAGE:"))
        {
            return Pmal.SetChoice("Do Not Repair");
        }
        return false;
    }

    static public bool PMrF2eh_RepairBody(string __)
    {
        if (!Pmal.SoftMatchCategory("DAMAGE:"))
        {
            Pmal.RereadPitMenu();   // DAMAGE is not in initial menu, check if it is now
        }
        if (Pmal.SoftMatchCategory("DAMAGE:"))
        {
            return Pmal.SetChoice("Repair Body");
        }
        return false;
    }
    #endregion Repairs
        #region Penalties
        static public bool PMrF2eh_PenaltyServe(string __)
        {
            if (!Pmal.GetCategories().Contains("STOP/GO"))
            {
                Pmal.RereadPitMenu();   // STOP/GO is not in initial menu, check if it is now
            }
            if (Pmal.GetCategories().Contains("STOP/GO"))
            {
                return Pmal.SetChoice("YES");
            }
            return false;
        }

        static public bool PMrF2eh_PenaltyServeNone(string __)
        {
            if (!Pmal.GetCategories().Contains("STOP/GO"))
            {
                Pmal.RereadPitMenu();   // STOP/GO is not in initial menu, check if it is now
            }
            if (Pmal.GetCategories().Contains("STOP/GO"))
            {
                return Pmal.SetChoice("NO");
            }
            return false;
        }
        #endregion Penalties

        static public bool PMrF2eh_ClearAll(string __)
        {
            if (PMrF2eh_FuelNone(null) &&
                PMrF2eh_changeNoTyres(null))
            {
                Pmal.RereadPitMenu();   // STOP/GO or DAMAGE: is not in initial menu, check if it is now
                var categories = Pmal.GetCategories();
                if (categories.Contains("STOP/GO"))
                {
                    PMrF2eh_PenaltyServeNone(null);
                }
                if (categories.Contains("DAMAGE:"))
                {
                    PMrF2eh_RepairNone(null);
                }
            }
            return true;
        }


        #endregion Public Methods

        #region MFD
        static string currentMFD = "MFDF";
        static private bool changeMFD(string mfd)
        {
            if (Pmal.MfdPage(mfd))
            {
                currentMFD = mfd;
                Log.Commentary($"Displaying {currentMFD}");
                return true;
            }
            return false;
        }
        static public bool PMrF2eh_DisplaySectors(string __)
        {
            return changeMFD("MFDA");
        }
        static public bool PMrF2eh_DisplayPitMenu(string __)
        {
            return changeMFD("MFDB");
        }
        static public bool PMrF2eh_DisplayTyres(string __)
        {
            return changeMFD("MFDC");
        }
        // MFDD is Driving Aids, not worth a command
        static public bool PMrF2eh_DisplayTemps(string __)
        {
            return changeMFD("MFDE");
        }
        static public bool PMrF2eh_DisplayRaceInfo(string __)
        {
            return changeMFD("MFDF");
        }
        static public bool PMrF2eh_DisplayStandings(string __)
        {
            return changeMFD("MFDG");
        }
        static public bool PMrF2eh_DisplayPenalties(string __)
        {
            return changeMFD("MFDH");
        }
        /// <summary>
        /// Display the next MFD screen
        /// </summary>
        static public bool PMrF2eh_DisplayNext(string __)
        {
            var lastLetter = currentMFD[currentMFD.Length - 1] + 1;
            if (lastLetter == 'I')
            {
                lastLetter = 'A';
            }
            return changeMFD("MFD" + (char)lastLetter);
        }
        #endregion MFD

        #region Private Methods

        /// <summary>
        /// Set the current tyre compound and fit them
        /// </summary>
        /// <param name="ccTyreType">Soft / Medium / Wet etc.</param>
        /// <returns></returns>
        static private bool setTyreCompound(string ccTyreType)
        {
            var inMenu = Pmal.GetTyreTypeNames();
            var result = TranslateTyreTypes(tyreTranslationDict, inMenu);
            if (!result.ContainsKey(ccTyreType))
            {   // Didn't find a match
                PitManagerResponseHandlers.PMrh_TyreCompoundNotAvailable();
                return false;
            }

            currentRf2TyreType.Set(result[ccTyreType]);
            Log.Commentary($"Fitting {result[ccTyreType]}");
            return PMrF2eh_changeAllTyres(null);
        }

        /// <summary>
        /// Change a single tyre to the current compound
        /// </summary>
        /// <param name="tyreCategory"></param>
        /// <param name="noChange">Set it to "No change"</param>
        /// <returns>true => success</returns>
        static private bool changeTyre(string tyreCategory, bool noChange = false)
        {
            bool response = false;
            string tyreType = noChange ? "No Change" : currentRf2TyreType.Get();

            if (Pmal.GetAllTyreCategories().Contains(tyreCategory))
            {
                response = Pmal.SetCategoryAndChoice(tyreCategory, tyreType);
                if (response)
                {
                    // dict is the other direction currentccTyreCompound = ttDict[tyreType];
                    if (CrewChief.Debugging)
                    {
                        Log.Info("Pit Manager tyre compound set to (" +
                            tyreCategory + ") " + tyreType);
                    }
                }
                else
                {   // Compound is not available
                    PitManagerResponseHandlers.PMrh_TyreCompoundNotAvailable();
                }
            }
            else
            {   // Category is not available
                PitManagerResponseHandlers.PMrh_CantChangeThose();
            }
            return response;
        }

        /// <summary>
        /// Change a set of tyres to the current compound
        /// </summary>
        /// <param name="tyreCategories"></param>
        /// <param name="noChange">Set them to "No Change"</param>
        /// <returns>true => success</returns>
        static private bool changeTyres(List<string> tyreCategories, bool noChange = false)
        {
            bool result = true;
            foreach (string tyreCategory in tyreCategories)
            {
                if (result && Pmal.SmartSetCategory(tyreCategory))
                {
                    result = changeTyre(tyreCategory, noChange);
                }
            }
            return result;
        }

        static private bool changeTyrePressure(string tyreCategory, string voiceMessage)
        {
            bool response = false;

            var pressure = PitNumberHandling.processNumber(voiceMessage);
            if (pressure > 0)
            {
                response = Pmal.SetCategoryAndChoice(tyreCategory, pressure.ToString());
                if (response)
                {
                    if (CrewChief.Debugging)
                    {
                        Log.Commentary("Pit Manager tyre pressure set to (" +
                            tyreCategory + ") " + pressure);
                    }
                }
                else
                {   // tbd: what failed? tyreCategory not available?
                    PitManagerResponseHandlers.PMrh_CantDoThat(); //tbd
                }
            }
            return response;
        }

        #endregion Private Methods

        #region Private Classes

        private class CurrentRf2TyreType
        {
            #region Private Fields

            private string currentTyreType = "No Change";

            #endregion Private Fields

            #region Public Methods

            public void Set(string tyreType)
            {
                currentTyreType = tyreType;
            }

            public string Get()
            {
                if (currentTyreType == "No Change")
                {
                    currentTyreType = Pmal.GetCurrentTyreType();
                    if (currentTyreType == "No Change")
                    {
                        // tbd: _currentTyreType = TranslateTyreTypes(tyreTranslationDict, tyreTypes)["Medium"];
                    }
                }
                return currentTyreType;
            }

            #endregion Public Methods
        }

        #endregion Private Classes

        public static class TyreDictFile
        {
            private static Tuple<string,  string> getUserTyreDictionaryFileLocation()
            {
                var path = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "RF2");

                return new Tuple<string, string>(path, "TyreDictionary.json");
            }
            public static TyreDictionary getTyreDictionaryFromFile()
            {
                var filepath = Path.Combine(getUserTyreDictionaryFileLocation().Item1,
                    getUserTyreDictionaryFileLocation().Item2);
                if (File.Exists(filepath))
                {
                    try
                    {
                        using (StreamReader r = new StreamReader(filepath))
                        {
                            string json = r.ReadToEnd();
                            // Check if file contents are the old version
                            int hash = json.GetHashCode();
                            if (hash != 0X37AF3AAF) // It's not
                            {
                                TyreDictionary data = JsonConvert.DeserializeObject<TyreDictionary>(json);
                                if (data != null)
                                {
                                    return data;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error parsing {filepath}: {e.Message}");
                    }
                }
                // else
                // No file or file is an old version so create a default one
                saveTyreDictionaryFile(SampleTyreTranslationDict);
                return SampleTyreTranslationDict;
            }

            public static void saveTyreDictionaryFile(TyreDictionary tyreDict)
            {
                var path = getUserTyreDictionaryFileLocation().Item1;
                var fileName = getUserTyreDictionaryFileLocation().Item2;
                var filePath = Path.Combine(path, fileName);

                if (!Directory.Exists(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception e)
                    {
                        Log.Fatal($"Error creating {path}: {e.Message}");
                    }
                }


                if (fileName != null)
                {
                    try
                    {
                        using (StreamWriter file = File.CreateText(filePath))
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                            serializer.Serialize(file, tyreDict);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"Error serialising {filePath}: {e.Message}");
                    }
                }
            }
        }
    }
}