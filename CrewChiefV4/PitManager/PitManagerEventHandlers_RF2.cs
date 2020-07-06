using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using rF2SharedMemoryAPI;

namespace CrewChiefV4.PitManager
{
    class PitManagerEventHandlers_RF2
    {
        static private PitMenuController Pmc = new PitMenuController();
        static private bool xxx = Pmc.Connect();    // tbd
        static private Dictionary<string, string> ttDict;
        static private List<string> tyreCategories;

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
        static private string[] compounds = new string[] {
            "Hard",     // tbd: yet to find where this come from in rF2
            "Medium",
            "Soft",
            "Wet",
            "Option",
            "Prime",
            "Alternate" };
        static private string TyreCompound = "Medium";  // Initialise to *something*


        /// <summary> responseHandler_TyreCompoundNotAvailable
        /// The event handlers for rF2
        /// </summary>
        static public bool actionHandler_example()
        {
            return true;

        }
        static public bool actionHandler_example2()
        {
            return true;
        }

        static private bool setTyreCompound(string compound)
        {
            bool response = false;
            Pmc.startUsingPitMenu();
            List<string> tyreTypes = Pmc.GetTyreTypeNames();
            // Don't need to do these every time, just when track loaded but for now...
            ttDict = Pmc.translateTyreTypes(tyreTypes);
            tyreCategories = Pmc.GetTyreChangeCategories();

            foreach (string tyre in tyres)
            {
                if (Pmc.SetCategory(tyre))  // tbd: should be tyreCategories
                {
                    response = Pmc.SetTyreType(ttDict[compound]);
                    if (response)
                    {
                        TyreCompound = ttDict[compound];
                        if (CrewChief.Debugging)
                        {
                            Console.WriteLine("Pit Manager tyre compound set to (" + compound + ") " + TyreCompound);
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
            bool response = false;
            foreach (string compound in compounds)
            {
                if (setTyreCompound(compound))
                { // Found one
                    response = true;
                    break;
                }
            }
            return response;    // Failed to find any???
        }
        static public bool actionHandler_changeAllTyres()
        {
            bool response = false;
            foreach (string tyre in tyres)
            {
                if (Pmc.SetCategory(tyre))
                {
                    response = Pmc.SetTyreType(TyreCompound);
                }
            }

            return response;
        }
    }
}
