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
    class PitManagerEventHandlers_RF2
    {
        static private PitMenuController Pmc = new PitMenuController();
        static private PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();
        static private Dictionary<string, List<string>> tyreDict =
            PitMenuAbstractionLayer.SampleTyreDict;
        static private Dictionary<string, string> ttDict;
        static private List<string> tyreCategories;
        static List<string> tyreTypes;

        static private string[] tyres = new string[] {
            "RR TIRE:",
            "RL TIRE:",
            "FR TIRE:",
            "FL TIRE:",
            "R TIRES:",
            "F TIRES:",
            "RT TIRES:",
            "LF TIRES:"
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
        static private string currentGenericTyreCompound = "";

        public PitManagerEventHandlers_RF2()
        {
            _ = Pmc.Connect();
            _ = Pmc.GetMenuDict();
            tyreTypes = Pmc.GetTyreTypeNames();
        }
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
                        _currentTyreType = Pmal.TranslateTyreTypes(tyreDict, tyreTypes)["Medium"];
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
        /// <summary> responseHandler_TyreCompoundNotAvailable
        /// The event handlers for rF2
        /// </summary>
        static public bool actionHandler_example()
        {
            return true;

        }
        static private bool setTyreCompound(string compound)
        {
            bool response = false;
            Pmc.startUsingPitMenu();
            // Don't need to do these every time, just when track loaded but for now...
            ttDict = Pmal.TranslateTyreTypes(tyreDict, tyreTypes);
            tyreCategories = Pmc.GetTyreChangeCategories();

            foreach (string tyre in tyreCategories)
            {
                if (Pmc.SetCategory(tyre))
                {
                    response = Pmc.SetTyreType(ttDict[compound]);
                    if (response)
                    {
                        currentGenericTyreCompound = ttDict[compound];
                        if (CrewChief.Debugging)
                        {
                            Console.WriteLine("Pit Manager tyre compound set to (" + compound + ") " + currentGenericTyreCompound);
                        }
                    }
                    else
                    {   // Compound is not available
                        PitManagerResponseHandlers.responseHandler_TyreCompoundNotAvailable();
                        break;
                    }
                }
            }
            return response;
        }

        static public bool actionHandler_TyreCompoundHard()
        {
            return setTyreCompound("Hard");
        }
        static public bool actionHandler_TyreCompoundMedium()
        {
            return setTyreCompound("Medium");
        }
        static public bool actionHandler_TyreCompoundSoft()
        {
            return setTyreCompound("Soft");
        }
        static public bool actionHandler_TyreCompoundWet()
        {
            return setTyreCompound("Wet");
        }
        static public bool actionHandler_TyreCompoundOption()
        {
            return setTyreCompound("Option");
        }
        static public bool actionHandler_TyreCompoundPrime()
        {
            return setTyreCompound("Prime");
        }
        static public bool actionHandler_TyreCompoundAlternate()
        {
            return setTyreCompound("Alternate");
        }
        static public bool actionHandler_TyreCompoundNext()
        {
            // Select the next compound available for this car
            // Get the current tyre type
            // Get the list of tyre type, remove "No Change"
            bool response = false;
            List<string> tyreTypes = Pmc.GetTyreTypeNames();
            string currentTyreTypeStr = "what?";
            int currentTyreType = genericCompounds.IndexOf(Pmal.GetGenericTyreType());
            for (var i = currentTyreType+1; i != currentTyreType; i++)
            {
                if (i >= genericCompounds.Count)
                    i = 0;
                if (Pmc.SetTyreType(genericCompounds[i]))
                { // Found one
                    response = true;
                    break;
                }
            }
            return response;    // Failed to find any???
        }
        static public bool actionHandler_changeAllTyres()
        {
            return setTyreCompound(currentGenericTyreCompound);
        }

        static public bool actionHandler_changeNoTyres()
        {
            return setTyreCompound("No Change");
        }

        static private bool changeCategoryOfTyre(string tyreCategory)
        {
            bool response = false;
            Pmc.startUsingPitMenu();
            List<string> tyreTypes = Pmc.GetTyreTypeNames();
            // Don't need to do these every time, just when track loaded but for now...
            ttDict = Pmal.TranslateTyreTypes(tyreDict, tyreTypes);
            tyreCategories = Pmc.GetTyreChangeCategories();

            if (tyreCategories.Contains(tyreCategory))
            {
                if (Pmc.SetCategory(tyreCategory))
                {
                    string tyreCompound = ttDict[currentGenericTyreCompound];
                    response = Pmc.SetTyreType(tyreCompound);
                    if (response)
                    {
                        if (CrewChief.Debugging)
                        {
                            Console.WriteLine("Pit Manager tyre compound set to (" + currentGenericTyreCompound + ") " + tyreCompound);
                        }
                    }
                    else
                    {   // Compound is not available
                        PitManagerResponseHandlers.responseHandler_TyreCompoundNotAvailable();
                    }
                }
            }
            return response;
        }

        static public bool actionHandler_changeFrontTyres()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("F TIRES:");
        }
        static public bool actionHandler_changeRearTyres()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("R TIRES:");
        }
        static public bool actionHandler_changeLeftTyres()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("LF TIRES:");
        }
        static public bool actionHandler_changeRightTyres()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("RT TIRES:");
        }
        static public bool actionHandler_changeFLTyre()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("FL TIRE:");
        }
        static public bool actionHandler_changeFRTyre()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("FR TIRE:");
        }
        static public bool actionHandler_changeRLTyre()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("RL TIRE:");
        }
        static public bool actionHandler_changeRRTyre()
        {
            setTyreCompound("No Change");
            return changeCategoryOfTyre("RR TIRE:");
        }


        static public bool actionHandler_FuelAddXlitres()
        {
            return Pmc.SetFuelLevel(amountCache);
        }

        static public bool actionHandler_RepairAll()
        {
            if (Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmc.SetChoice("Repair All");
            }
            return false;
        }
        static public bool actionHandler_RepairNone()
        {
            if (Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmc.SetChoice("Repair None");
            }
            return false;
        }
        static public bool actionHandler_RepairBody()
        {
            if (Pmc.SoftMatchCategory("DAMAGE:"))
            {
                return Pmc.SetChoice("Repair Body");
            }
            return false;
        }

        static public bool actionHandler_PenaltyServe()
        {
            if (Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmc.SetChoice("YES");
            }
            return false;
        }
        static public bool actionHandler_PenaltyServeNone()
        {
            if (Pmc.SoftMatchCategory("STOP/GO"))
            {
                return Pmc.SetChoice("NO");
            }
            return false;
        }
    }
}
