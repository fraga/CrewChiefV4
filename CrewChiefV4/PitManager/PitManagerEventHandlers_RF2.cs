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
    using Pmal = PitMenuAbstractionLayer;

    class PitManagerEventHandlers_RF2
    {
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
        static private Dictionary<string, string> ttDict =
            TranslateTyreTypes(tyreTranslationDict, Pmal.GetTyreTypeNames());
        static private List<string> tyreCategories;
        static List<string> tyreTypes = Pmal.GetTyreTypeNames();

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
        static private string currentGenericTyreCompound = "";
        static private currentTyreType xx = new currentTyreType();

        private class currentTyreType
        {
            static string _currentTyreType = "";
            public void Set(string tyreType)
            {
                _currentTyreType = tyreType;
            }
            public string Get()
            {
                if (_currentTyreType == "")
                {
                    _currentTyreType = Pmal.GetCurrentTyreType();
                    if (_currentTyreType == "No Change")
                    {
                        _currentTyreType = TranslateTyreTypes(tyreTranslationDict, tyreTypes)["Medium"];
                    }
                }
                return _currentTyreType;
            }
        }
        static private int amountCache;

        /// <summary>
        /// Invoked by "Pitstop Add..." command, an event (e.g. fuel) will
        /// come along and use this value later
        /// </summary>
        /// <param name="amount"></param>
        static public void amountHandler(int amount)
        {
            amountCache = amount;
        }
        /// <summary> PMrF2eh_example
        /// Dummy action handler for rF2
        /// </summary>
        static public bool PMrF2eh_example()
        {
            return true;

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
        static private bool setTyreCompound(string genericTyreType)
        {
            xx.Set(TranslateTyreTypes(tyreTranslationDict, Pmal.GetTyreTypeNames())[genericTyreType]);
            return true;
        }

        static private bool changeTyre(string tyreCategory, bool noChange=false)
        {
            bool response = false;
            string tyreType = noChange ? "No Change" : xx.Get();

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

        static public bool PMrF2eh_TyreCompoundHard()
        {
            return setTyreCompound("Hard");
        }
        static public bool PMrF2eh_TyreCompoundMedium()
        {
            return setTyreCompound("Medium");
        }
        static public bool PMrF2eh_TyreCompoundSoft()
        {
            return setTyreCompound("Soft");
        }
        static public bool PMrF2eh_TyreCompoundWet()
        {
            return setTyreCompound("Wet");
        }
        static public bool PMrF2eh_TyreCompoundOption()
        {
            return setTyreCompound("Soft"); // tbd:
        }
        static public bool PMrF2eh_TyreCompoundPrime()
        {
            return setTyreCompound("Soft"); // tbd:
        }
        static public bool PMrF2eh_TyreCompoundAlternate()
        {
            return setTyreCompound("Soft"); // tbd:
        }

        static public bool PMrF2eh_TyreCompoundNext()
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

        static public bool PMrF2eh_changeAllTyres()
        {
            return changeTyres(Pmal.GetAllTyreCategories());
        }
        static public bool PMrF2eh_changeNoTyres()
        {
            return changeTyres(Pmal.GetAllTyreCategories(), true);
        }


        static public bool PMrF2eh_changeFrontTyres()
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetFrontTyreCategories());
        }
        static public bool PMrF2eh_changeRearTyres()
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRearTyreCategories());
        }
        static public bool PMrF2eh_changeLeftTyres()
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetLeftTyreCategories());
        }
        static public bool PMrF2eh_changeRightTyres()
        {
            changeTyres(Pmal.GetAllTyreCategories(), true);
            return changeTyres(Pmal.GetRightTyreCategories());
        }
        static public bool PMrF2eh_changeFLTyre()
        {
            return changeTyre("FL TIRE:");
        }
        static public bool PMrF2eh_changeFRTyre()
        {
            return changeTyre("FR TIRE:");
        }
        static public bool PMrF2eh_changeRLTyre()
        {
            return changeTyre("RL TIRE:");
        }
        static public bool PMrF2eh_changeRRTyre()
        {
            return changeTyre("RR TIRE:");
        }


        static public bool PMrF2eh_FuelAddXlitres()
        {
            var amount = Pmal.Pmc.GetFuelLevel();
            return Pmal.Pmc.SetFuelLevel(amount + amountCache);
        }
        static public bool PMrF2eh_FuelToXlitres()
        {
            return Pmal.Pmc.SetFuelLevel(amountCache);
        }
        static public bool PMrF2eh_FuelNone()
        {
            return Pmal.Pmc.SetFuelLevel(1);
        }

        static public bool PMrF2eh_RepairAll()
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair All");
            }
            return false;
        }
        static public bool PMrF2eh_RepairNone()
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair None");
            }
            return false;
        }
        static public bool PMrF2eh_RepairBody()
        {
            if (Pmal.Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmal.Pmc.SetChoice("Repair Body");
            }
            return false;
        }

        static public bool PMrF2eh_PenaltyServe()
        {
            if (Pmal.Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.Pmc.SetChoice("YES");
            }
            return false;
        }
        static public bool PMrF2eh_PenaltyServeNone()
        {
            if (Pmal.Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmal.Pmc.SetChoice("NO");
            }
            return false;
        }
    }
}
