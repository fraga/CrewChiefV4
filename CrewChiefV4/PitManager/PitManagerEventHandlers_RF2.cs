using System;
using System.Collections.Generic;
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
//"Relative Fuel Strategy":false,
//"Relative Fuel Strategy#":"Show how much fuel to ADD, rather than how much TOTAL fuel to fill the tank up to (note: new default is true)",
//"Smart Pitcrew":true,
//"Smart Pitcrew#":"Pitcrew does things even if you mistakenly forgot to ask (one example is changing a damaged tire)",

namespace CrewChiefV4.PitManager
{
    internal class PitManagerEventHandlers_RF2
    {
        private static readonly PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();

        #region Private Fields

        // Complicated because rF2 has many names for tyres so use a dict of
        // possible alternative names for each type
        // Each entry has a list of possible matches in declining order
        // Sample:
        private static readonly Dictionary<string, List<string>> SampleTyreTranslationDict =
          new Dictionary<string, List<string>>() {
            { "Hypersoft",    new List <string> {"hypersoft", "ultrasoft", "supersoft", "soft",
                        "s310", "slick", "dry", "all-weather", "medium" } },
            { "Ultrasoft",    new List <string> {"ultrasoft","hypersoft", "supersoft", "soft",
                        "s310", "slick", "dry", "all-weather", "medium" } },
            { "Supersoft",    new List <string> {"supersoft", "hypersoft", "ultrasoft", "soft",
                        "s310", "slick", "dry", "all-weather", "medium" } },
            { "Soft",         new List <string> {"soft",
                        "s310", "slick", "dry", "all-weather", "medium" } },
            { "Medium",       new List <string> { "medium", "default",
                        "s310", "slick", "dry", "all-weather" } },
            { "Hard",         new List <string> {"hard", "p310", "endur",
                        "medium", "default",
                                "slick", "dry", "all-weather" } },
            { "Intermediate", new List <string> { "intermediate",
                        "wet", "rain", "monsoon", "all-weather" } },
            { "Wet",          new List <string> {
                        "wet", "rain", "monsoon", "all-weather", "intermediate" } },
            { "Monsoon",      new List <string> {"monsoon",
                        "wet", "rain",  "all-weather", "intermediate" } },
            { "No Change",    new List <string> {"no change"} }
            };

        static private Dictionary<string, List<string>> tyreTranslationDict =
                    SampleTyreTranslationDict;

        static private CurrentRf2TyreType currentRf2TyreType = new CurrentRf2TyreType();

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Take a list of tyre types available in the menu and map them on to
        /// the set of generic tyre types
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
        /// </summary>
        /// <param name="tyreDict">
        /// The dict used for translation
        /// </param>
        /// <param name="inMenu">
        /// The list returned by GetTyreTypes()
        /// </param>
        /// <returns>
        /// Dictionary mapping generic tyre types to names of those available
        /// </returns>
        public static Dictionary<string, string> TranslateTyreTypes(
          Dictionary<string, List<string>> tyreDict,
          List<string> inMenu)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            inMenu.Remove("No Change");
            int columnCount = 1; // will increase

            for (var col = 0; col < columnCount; col++)
            {
                foreach (var genericTyretype in tyreDict)
                { // "Hypersoft", "Ultrasoft", "Supersoft", "Soft"...
                    foreach (var availableTyretype in inMenu)
                    {  // Tyre type in the menu
                        if (genericTyretype.Value.Count > columnCount)
                        {
                            columnCount = genericTyretype.Value.Count;
                        }
                        if (col < genericTyretype.Value.Count)
                        {
                            var tyreName = genericTyretype.Value[col];
                            // Type that generic type can match to
                            if (availableTyretype.IndexOf(tyreName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                if (!result.ContainsKey(genericTyretype.Key))
                                {
                                    result[genericTyretype.Key] = availableTyretype;
                                    break;
                                }
                            }
                        }
                    }
                }
                foreach (var genericTyretype in tyreDict)
                {
                    if (!result.ContainsKey(genericTyretype.Key))
                    {   // Didn't match, give it something
                        result[genericTyretype.Key] = inMenu[0];
                    }
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
            PitMenu.startUsingPitMenu();
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

        static public bool PMrF2eh_TyreCompoundWet(string __)
        {
            return setTyreCompound("Wet");
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
            bool response = false;
            List<string> tyreTypes = Pmal.GetTyreTypeNames();
            tyreTypes.Remove("No Change");
            string currentTyreTypeStr = Pmal.GetCurrentTyreType();

            int currentTyreTypeIndex = tyreTypes.IndexOf(currentTyreTypeStr) + 1;
            if (currentTyreTypeIndex >= tyreTypes.Count)
                currentTyreTypeIndex = 0;
            response = true;
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
            if (UserSettings.GetUserSettings().getBoolean("rf2_enable_auto_fuel_to_end_of_race"))
            {   // Ignore the voice command if we're going to do it automatically
                return false;
            }
            var litresNeeded = PitFuelling.fuelToEnd(
                PitManagerVoiceCmds.getFuelCapacity(),
                PitManagerVoiceCmds.getCurrentFuel());
            if (litresNeeded == 0)
            {
                return false;
            }
            return rF2SetFuel(litresNeeded);
        }

        static public bool PMrF2eh_FuelNone(string __)
        {
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
                    PMrF2eh_RepairNone(null);
                }
                if (categories.Contains("DAMAGE:"))
                {
                    PMrF2eh_PenaltyServeNone(null);
                }
            }
            return false;
        }


        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Set the current tyre compound and fit them
        /// </summary>
        /// <param name="genericTyreType">Soft / Medium / Wet etc.</param>
        /// <returns></returns>
        static private bool setTyreCompound(string genericTyreType)
        {
            var inMenu = Pmal.GetTyreTypeNames();
            var result = TranslateTyreTypes(tyreTranslationDict, inMenu);
            if (!result.ContainsKey(genericTyreType))
            {   // Didn't find a match
                PitManagerResponseHandlers.PMrh_TyreCompoundNotAvailable();
                return false;
            }

            currentRf2TyreType.Set(result[genericTyreType]);
            Log.Commentary($"Fitting {result[genericTyreType]}");
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
                    // dict is the other direction currentGenericTyreCompound = ttDict[tyreType];
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
                PitManagerResponseHandlers.PMrh_CantDoThat();
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

            private static string currentTyreType = "No Change";

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
    }
}