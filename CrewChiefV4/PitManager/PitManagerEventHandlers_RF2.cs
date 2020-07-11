using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using PitMenuAPI;

namespace CrewChiefV4.PitManager
{
    class PitManagerEventHandlers_RF2
    {
        static private PitMenuController Pmc = new PitMenuController();
        static private PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();
        static private Dictionary<string, List<string>> tyreDict =
            PitMenuAbstractionLayer.SampleTyreDict;
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
        static private List<string> compounds = new List<string> {
            "Hard",     // tbd: yet to find where this come from in rF2
            "Medium",
            "Soft",
            "Wet",
            "Option",
            "Prime",
            "Alternate" };
        static private string GenericTyreCompound = "Medium";  // Initialise to *something*
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
            ttDict = Pmal.TranslateTyreTypes(tyreDict, tyreTypes);
            tyreCategories = Pmc.GetTyreChangeCategories();

            foreach (string tyre in tyreCategories)
            {
                if (Pmc.SetCategory(tyre))
                {
                    response = Pmc.SetTyreType(ttDict[compound]);
                    if (response)
                    {
                        GenericTyreCompound = ttDict[compound];
                        if (CrewChief.Debugging)
                        {
                            Console.WriteLine("Pit Manager tyre compound set to (" + compound + ") " + GenericTyreCompound);
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
            List<string> tyreTypes = Pmc.GetTyreTypeNames();
            int currentTyreType = compounds.IndexOf(Pmal.GetGenericTyreType());
            for (var i = currentTyreType+1; i != currentTyreType; i++)
            {
                if (i >= compounds.Count)
                    i = 0;
                if (Pmc.SetTyreType(compounds[i]))
                { // Found one
                    response = true;
                    break;
                }
            }
            return response;    // Failed to find any???
        }
        static public bool actionHandler_changeAllTyres()
        {
            return setTyreCompound(GenericTyreCompound);
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
                    string tyreCompound = ttDict[GenericTyreCompound];
                    response = Pmc.SetTyreType(tyreCompound);
                    if (response)
                    {
                        if (CrewChief.Debugging)
                        {
                            Console.WriteLine("Pit Manager tyre compound set to (" + GenericTyreCompound + ") " + tyreCompound);
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
    }
}
