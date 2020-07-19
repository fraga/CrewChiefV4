using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
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
    //using Pmal = PitMenuAbstractionLayer;

    internal class PitManagerEventHandlers_RF2
    {
        private static PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();
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
        /// <param name="inMenu">
        /// The list returned by GetTyreTypes()
        /// </param>
        /// <returns>
        /// Dictionary mapping generic tyre types to names of those available
        /// </returns>

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

        static private readonly List<string> genericCompounds = new List<string> {
            "Hard",
            "Medium",
            "Soft",
            "Intermediate",
            "Wet",
            "Monsoon",
            "Option",
            "Prime",
            "Alternate" };

        static private Dictionary<string, List<string>> tyreTranslationDict =
                    SampleTyreTranslationDict;

        static private List<string> tyreCategories;
        //private static List<string> tyreTypes = Pmal.GetTyreTypeNames();
        static private string currentGenericTyreCompound = "";
        static private currentTyreType xx = new currentTyreType();

        static private int amountCache;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Invoked by "Pitstop Add..." command, an event (e.g. fuel) will
        /// come along and use this value later
        /// </summary>
        /// <param name="amount"></param>
        static public void amountHandler(int amount)
        {
            amountCache = amount;
        }

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

        static public bool PMrF2eh_initialise(string defaultTyreType)
        {
            Pmal.Connect();
            xx.Set("");
            return true;
        }
        static public bool PMrF2eh_teardown(string voiceMessage)
        {
            Pmal.Disconnect();
            return true;
        }

        /// <summary> PMrF2eh_example
        /// Dummy action handler for rF2
        /// </summary>
        static public bool PMrF2eh_example(string voiceMessage)
        {
            return true;
        }

        static public bool PMrF2eh_TyreCompoundHard(string voiceMessage)
        {
            return setTyreCompound("Hard");
        }

        static public bool PMrF2eh_TyreCompoundMedium(string voiceMessage)
        {
            return setTyreCompound("Medium");
        }

        static public bool PMrF2eh_TyreCompoundSoft(string voiceMessage)
        {
            return setTyreCompound("Soft");
        }

        static public bool PMrF2eh_TyreCompoundWet(string voiceMessage)
        {
            return setTyreCompound("Wet");
        }

        static public bool PMrF2eh_TyreCompoundOption(string voiceMessage)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMrF2eh_TyreCompoundPrime(string voiceMessage)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMrF2eh_TyreCompoundAlternate(string voiceMessage)
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMrF2eh_TyreCompoundNext(string voiceMessage)
        {
            // Select the next compound available for this car
            // Get the current tyre type
            // Get the list of tyre type, remove "No Change"
            bool response = false;
            List<string> tyreTypes = Pmal.Pmc.GetTyreTypeNames();
            tyreTypes.Remove("No Change");
            string currentTyreTypeStr = Pmal.GetCurrentTyreType();

            int currentTyreType = tyreTypes.IndexOf(currentTyreTypeStr) + 1;
            if (currentTyreType >= tyreTypes.Count)
                currentTyreType = 0;
            response = true;
            xx.Set(tyreTypes[currentTyreType]);
            return response;
        }

        static public bool PMrF2eh_changeAllTyres(string voiceMessage)
        {
            return changeTyres(Pmal.GetAllTyreCategories());
        }

        static public bool PMrF2eh_changeNoTyres(string voiceMessage)
        {
            return changeTyres(Pmal.GetAllTyreCategories(), true);
        }

        static public bool PMrF2eh_changeFrontTyres(string voiceMessage)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetFrontTyreCategories());
        }

        static public bool PMrF2eh_changeRearTyres(string voiceMessage)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRearTyreCategories());
        }

        static public bool PMrF2eh_changeLeftTyres(string voiceMessage)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetLeftTyreCategories());
        }

        static public bool PMrF2eh_changeRightTyres(string voiceMessage)
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRightTyreCategories());
        }

        static public bool PMrF2eh_changeFLTyre(string voiceMessage)
        {
            return changeTyre("FL TIRE:");
        }

        static public bool PMrF2eh_changeFRTyre(string voiceMessage)
        {
            return changeTyre("FR TIRE:");
        }

        static public bool PMrF2eh_changeRLTyre(string voiceMessage)
        {
            return changeTyre("RL TIRE:");
        }

        static public bool PMrF2eh_changeRRTyre(string voiceMessage)
        {
            return changeTyre("RR TIRE:");
        }

        static public bool PMrF2eh_changeFLpressure(string voiceMessage)
        {
            var pressure = FuelHandling.processNumber(voiceMessage);
            return changeTyrePressure("FL PRESS:", pressure);
        }

        static public bool PMrF2eh_FuelAddXlitres(string voiceMessage)
        {
            var amount = Pmal.Pmc.GetFuelLevel();
            var amountAdd = FuelHandling.processNumber(voiceMessage);
            amountAdd = FuelHandling.processLitresGallons(amountAdd, voiceMessage);
            return Pmal.Pmc.SetFuelLevel(amount + amountAdd);
        }

        static public bool PMrF2eh_FuelToXlitres(string voiceMessage)
        {
            var amount = FuelHandling.processNumber(voiceMessage);
            amount = FuelHandling.processLitresGallons(amount, voiceMessage);
            return Pmal.Pmc.SetFuelLevel(amount);
        }

        static public bool PMrF2eh_FuelToEnd(string voiceMessage)
        {
            return FuelHandling.fuelToEnd(100, 0); // tbd
        }

        static public bool PMrF2eh_FuelNone(string voiceMessage)
        {
            return Pmal.Pmc.SetFuelLevel(1);
        }

        static public bool PMrF2eh_RepairAll(string voiceMessage)
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair All");
            }
            return false;
        }

        static public bool PMrF2eh_RepairNone(string voiceMessage)
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair None");
            }
            return false;
        }

        static public bool PMrF2eh_RepairBody(string voiceMessage)
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair Body");
            }
            return false;
        }

        static public bool PMrF2eh_PenaltyServe(string voiceMessage)
        {
            if (Pmal.Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.Pmc.SetChoice("YES");
            }
            return false;
        }

        static public bool PMrF2eh_PenaltyServeNone(string voiceMessage)
        {
            if (Pmal.Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.Pmc.SetChoice("NO");
            }
            return false;
        }

        #endregion Public Methods

        #region Private Methods

        static private bool setTyreCompound(string genericTyreType)
        {
            xx.Set(TranslateTyreTypes(tyreTranslationDict, Pmal.GetTyreTypeNames())[genericTyreType]);
            return true;
        }

        static private bool changeTyre(string tyreCategory, bool noChange = false)
        {
            bool response = false;
            string tyreType = noChange ? "No Change" : xx.Get();

            if (Pmal.GetAllTyreCategories().Contains(tyreCategory))
            {
                response = Pmal.setCategoryAndChoice(tyreCategory, tyreType);
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

        static private bool changeTyres(List<string> tyreCategories, bool noChange = false)
        {
            bool result = true;
            foreach (string tyreCategory in tyreCategories)
            {
                if (result && Pmal.Pmc.SetCategory(tyreCategory))
                {
                    result = changeTyre(tyreCategory, noChange);
                }
            }
            return result;
        }

        static private bool changeTyrePressure(string tyreCategory, int pressure)
        {
            bool response = false;

            if (pressure > 0)
            {
                // tbd: response = Pmal.setCategoryAndChoice(tyreCategory, tyreType);
                if (response)
                {
                    // dict is the other direction currentGenericTyreCompound = ttDict[tyreType];
                    if (CrewChief.Debugging)
                    {
                        Console.WriteLine("Pit Manager tyre pressure set to (" +
                            tyreCategory + ") " + pressure);
                    }
                }
                else
                {   // Compound is not available
                    // tbd: PitManagerResponseHandlers.PMrh_TyreCompoundNotAvailable();
                }
            }
            return response;
        }

        #endregion Private Methods

        #region Private Classes

        private class currentTyreType
        {
            #region Private Fields

            private static string _currentTyreType = "No Change";

            #endregion Private Fields

            #region Public Methods

            public void Set(string tyreType)
            {
                _currentTyreType = tyreType;
            }

            public string Get()
            {
                if (_currentTyreType == "No Change")
                {
                    _currentTyreType = Pmal.GetCurrentTyreType();
                    if (_currentTyreType == "No Change")
                    {
                        // tbd: _currentTyreType = TranslateTyreTypes(tyreTranslationDict, tyreTypes)["Medium"];
                    }
                }
                return _currentTyreType;
            }

            #endregion Public Methods
        }

        #endregion Private Classes
    }
}