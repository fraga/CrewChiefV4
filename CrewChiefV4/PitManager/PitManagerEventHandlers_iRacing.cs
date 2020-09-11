using System;
using System.Collections.Generic;
using PitMenuAPI;

namespace CrewChiefV4.PitManager
{
    internal class PitManagerEventHandlers_iRacing
    {
        private static readonly PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();

        #region Private Fields

        // Complicated because rF2 has many names for tyres so use a dict of
        // possible alternative names for each type
        // Each entry has a list of possible matches in declining order
        // Sample:
        private static readonly Dictionary<string, List<string>> SampleTyreTranslationDict =
          new Dictionary<string, List<string>>() {
            { "Supersoft",    new List <string> {"supersoft", "soft",
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
        /// Supersoft
        /// Soft
        /// Medium
        /// Hard
        /// Intermediate
        /// Wet
        /// Monsoon
        /// (No Change) for completeness
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
            foreach (var genericTyretype in tyreDict)
            { // "Supersoft", "Soft"...
                foreach (var availableTyretype in inMenu)
                {  // Tyre type in the menu
                    foreach (var tyreName in genericTyretype.Value)
                    { // Type that generic type can match to
                        if (availableTyretype.IndexOf(tyreName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            result[genericTyretype.Key] = availableTyretype;
                            break;
                        }
                    }
                }
            }
            return result;
        }

        ///////////////////////////////////////////////////////////////////////
        // Event handlers

        static public bool PMiReh_initialise(string __)
        {
            Pmal.PmalConnect();
            currentRf2TyreType.Set(Pmal.GetTyreTypeNames()[0]);
            Console.WriteLine("Pit Manager initialise");
            return true;
        }
        static public bool PMiReh_teardown(string __)
        {
            Pmal.Disconnect();
            return true;
        }

        /// <summary>
        /// tbd Switch iRacing to the menu page and wake up the inputs driver
        /// </summary>
        static public bool PMiReh_prepareToUseMenu(string __)
        {
            PitMenu.startUsingPitMenu();
            return true;
        }

        /// <summary> PMiReh_example
        /// Dummy action handler for iRacing
        /// </summary>
        static public bool PMiReh_example(string __)
        {
            return true;
        }

        #region Tyre compounds
        static public bool PMiReh_TyreCompoundHard(string __)
        {
            return setTyreCompound("Hard");
        }

        static public bool PMiReh_TyreCompoundMedium(string __)
        {
            return setTyreCompound("Medium");
        }

        static public bool PMiReh_TyreCompoundSoft(string __)
        {
            return setTyreCompound("Soft");
        }

        static public bool PMiReh_TyreCompoundWet(string __)
        {
            return setTyreCompound("Wet");
        }

        static public bool PMiReh_TyreCompoundOption(string __)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMiReh_TyreCompoundPrime(string __)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMiReh_TyreCompoundAlternate(string __)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMiReh_TyreCompoundNext(string __)
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
            return response;
        }
        #endregion Tyre compounds

        #region Which tyres to change
        static public bool PMiReh_changeAllTyres(string __)
        {
            return changeTyres(Pmal.GetAllTyreCategories());
        }

        static public bool PMiReh_changeNoTyres(string __)
        {
            return changeTyres(Pmal.GetAllTyreCategories(), true);
        }

        static public bool PMiReh_changeFrontTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetFrontTyreCategories());
        }

        static public bool PMiReh_changeRearTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRearTyreCategories());
        }

        static public bool PMiReh_changeLeftTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetLeftTyreCategories());
        }

        static public bool PMiReh_changeRightTyres(string __)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRightTyreCategories());
        }

        static public bool PMiReh_changeFLTyre(string __)
        {
            return changeTyre("FL TIRE:");
        }

        static public bool PMiReh_changeFRTyre(string __)
        {
            return changeTyre("FR TIRE:");
        }

        static public bool PMiReh_changeRLTyre(string __)
        {
            return changeTyre("RL TIRE:");
        }

        static public bool PMiReh_changeRRTyre(string __)
        {
            return changeTyre("RR TIRE:");
        }
        #endregion Which tyres to change

        #region Tyre pressures
        static public bool PMiReh_changeFLpressure(string voiceMessage)
        {
            return changeTyrePressure("FL PRESS:", voiceMessage);
        }
        #endregion Tyre pressures

        #region Fuel
        // tbd: iRacing can only add fuel
        static public bool PMiReh_FuelAddXlitres(string voiceMessage)
        {
            var amount = Pmal.GetFuelLevel();
            if (amount > 0)
            {
                var amountAdd = PitNumberHandling.processNumber(voiceMessage);
                if (amountAdd == 0)
                {
                    return false;
                }
                amount += PitNumberHandling.processLitresGallons(amountAdd, voiceMessage);
                if (amount > PitManagerVoiceCmds.getFuelCapacity())
                {
                    amount = (int)PitManagerVoiceCmds.getFuelCapacity();
                }
                return rF2SetFuel(amount);
            }
            return false;
        }

        static public bool PMiReh_FuelToXlitres(string voiceMessage)
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
            return rF2SetFuel(amount);
        }

        static public bool PMiReh_FuelToEnd(string __)
        {
            if (UserSettings.GetUserSettings().getBoolean("iracing_enable_auto_fuel_to_end_of_race"))
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

        static public bool PMiReh_FuelNone(string __)
        {
            return rF2SetFuel(1);
        }

        static public bool rF2SetFuel(int amount)
        {
            return Pmal.SetFuelLevel(amount);
        }
        #endregion Fuel

        #region Repairs
        static public bool PMiReh_RepairAll(string __)
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

        static public bool PMiReh_RepairNone(string __)
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

        static public bool PMiReh_RepairBody(string __)
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
        static public bool PMiReh_PenaltyServe(string __)
        {
            if (!Pmal.SoftMatchCategory("STOP/GO"))
            {
                Pmal.RereadPitMenu();   // STOP/GO is not in initial menu, check if it is now
            }
            if (Pmal.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.SetChoice("YES");
            }
            return false;
        }

        static public bool PMiReh_PenaltyServeNone(string __)
        {
            if (!Pmal.SoftMatchCategory("STOP/GO"))
            {
                Pmal.RereadPitMenu();   // STOP/GO is not in initial menu, check if it is now
            }
            if (Pmal.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.SetChoice("NO");
            }
            return false;
        }
        #endregion Penalties
        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Set the current tyre compound
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
            return true;
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
                        Console.WriteLine("Pit Manager tyre compound set to (" +
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
                        Console.WriteLine("Pit Manager tyre pressure set to (" +
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