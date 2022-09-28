/*
Set the rFactor 2 Pit Menu using TheIronWolf's rF2 Shared Memory Map plugin
https://github.com/TheIronWolfModding/rF2SharedMemoryMapPlugin

Crew Chief wants to refer to tyres as Soft, Hard, Wet etc. but rFactor uses
names that are defined in the vehicle data files (the *.tbc file).
This handles the translation both ways

Author: Tony Whitley (sven.smiles@gmail.com)
*/
using CrewChiefV4;
using System.Collections.Generic;
using System.Linq;

namespace PitMenuAPI
{
    /// <summary>
    /// PitMenuAPI consists PitMenuAbstractionLayer : PitMenuController : PitMenu
    /// </summary>
    public class PitMenuAbstractionLayer : PitMenuController
    {
        #region Private Fields

        /// <summary>
        /// All the Pit Menu categories of tyres that rF2 selects from
        /// </summary>
        private readonly string[] tyreCategories = {
            "RR TIRE:",
            "RL TIRE:",
            "FR TIRE:",
            "FL TIRE:",
            "R TIRES:",
            "F TIRES:",
            "RT TIRES:",
            "LF TIRES:",
            "TIRES:"
        };

        /// <summary>
        /// The Pit Menu categories of tyres that rF2 uses to select compounds,
        /// the remainder sometimes only choose this compound or NO CHANGE
        /// </summary>
        private readonly string[] frontTyreCategories = {
            "FR TIRE:",
            "FL TIRE:",
            "F TIRES:",
            "RT TIRES:",
            "LF TIRES:",
            "TIRES:"
        };

        private readonly string[] leftTyreCategories = {
            "FL TIRE:",
            "RL TIRE:",
            "LF TIRES:",
        };

        private readonly string[] rightTyreCategories = {
            "FR TIRE:",
            "RR TIRE:",
            "RT TIRES:",
        };

        private MenuLayout menuLayout;
        private readonly PitMenuController pmc = new PitMenuController();
        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Connect to the Shared Memory running in rFactor
        /// </summary>
        public bool PmalConnect()  // tbd Naming
        {
            menuLayout = new MenuLayout();
            menuLayout.NewCar();
            return Connect();
        }

        public void RereadPitMenu()
        {
            menuLayout.NewCar();
            menuLayout.GetKeys();
        }

        public void MfdPage(string Mfd)
        {
            switchMFD(Mfd);
        }

        public List<string> GetCategories()
        {
            return menuLayout.GetKeys();
        }
        new public bool SmartSetCategory(string category)
        {
            return pmc.SmartSetCategory(category);
        }

        /// <summary>
        /// Get a list of the front tyre changes provided for this vehicle.  Fronts
        /// sometimes have to be changed before the rears will be given the same
        /// set of compounds available
        /// </summary>
        /// <returns>
        /// A sorted list of the front tyre changes provided for this vehicle
        /// </returns>
        public List<string> GetFrontTyreCategories()
        {
            List<string> result =
                frontTyreCategories.Intersect(menuLayout.GetKeys()).ToList();
            result.Sort();
            Log.Debug("Front tyre categories in menu: " + string.Join(", ", result.Select(s => $"'{s}'")));
            return result;
        }

        /// <summary>
        /// Get a list of all the tyre changes provided for this vehicle.
        /// </summary>
        /// <returns>
        /// A sorted list of all the tyre changes provided for this vehicle
        /// </returns>
        public List<string> GetAllTyreCategories()
        {
            List<string> result =
                tyreCategories.Intersect(menuLayout.GetKeys()).ToList();
            result.Sort();
            return result;
        }

        public string GetCurrentTyreType()
        {
            string result;
            foreach (string category in GetFrontTyreCategories())
            {
                SmartSetCategory(category);
                result = GetChoice();
                if (result != "No Change")
                {
                    return result;
                }
            }
            foreach (string category in GetRearTyreCategories())
            {
                SmartSetCategory(category);
                result = GetChoice();
                if (result != "No Change")
                {
                    return result;
                }
            }
            return "No Change";
        }

        /// <summary>
        /// Get a list of the rear tyre changes provided for this vehicle.
        /// </summary>
        /// <returns>
        /// A list of the rear tyre changes provided for this vehicle
        /// </returns>
        public List<string> GetRearTyreCategories()
        {
            // There are simpler ways to do this but...
            return tyreCategories.Except(frontTyreCategories)
              .Intersect(menuLayout.GetKeys()).ToList();
        }

        public List<string> GetLeftTyreCategories()
        {
            return leftTyreCategories.Intersect(menuLayout.GetKeys()).ToList();
        }

        public List<string> GetRightTyreCategories()
        {
            return rightTyreCategories.Intersect(menuLayout.GetKeys()).ToList();
        }

        new public List<string> GetTyreTypeNames()
        {
            string tyre = GetFrontTyreCategories()[0];
            var result = menuLayout.Get(tyre);
            Log.Debug("Tyre type names " + string.Join(", ", result.Select(s => $"'{s}'")));
            return result;
        }

        /// <summary>
        /// Set the tyre compound selection in the Pit Menu.
        /// Set the front tyres first as the rears may may depend on what is
        /// selected for the fronts
        /// Having changed them all, the client can then set specific tyres to
        /// NO CHANGE
        /// </summary>
        /// <param name="tyreType">Name of actual tyre type or NO CHANGE</param>
        /// <returns>true all tyres changed</returns>
        public bool SetAllTyreTypes(string tyreType)
        {
            bool response = true;

            foreach (string whichTyre in GetFrontTyreCategories())
            {
                if (response)
                {
                    response = SmartSetCategory(whichTyre);
                }
                if (response)
                {
                    response = SetTyreType(tyreType);
                }
            }
            foreach (string whichTyre in GetRearTyreCategories())
            {
                if (response)
                {
                    response = SmartSetCategory(whichTyre);
                }
                if (response)
                {
                    response = SetTyreType(tyreType);
                }
            }
            return response;
        }

        public bool SetCategoryAndChoice(string category, string choice)
        {
            int tryNo = 5;
            bool response;
            while (tryNo-- > 0)
            {
                response = SmartSetCategory(category);
                if (response)
                {
                    response = SetChoice(choice);
                    if (response)
                    {
                        return true;
                    }
                    startUsingPitMenu();
                }
            }
            return false;
        }

        // Unit Test
        public void SetMenuDict(Dictionary<string, List<string>> dict)
        {
            menuLayout.Set(dict);
        }

        #endregion Public Methods

        #region Private Classes

        /// <summary>
        /// Virtualisation of the menu layout for the current vehicle
        /// </summary>
        private class MenuLayout
        {
            #region Private Fields

            private Dictionary<string, List<string>> menuDict =
                new Dictionary<string, List<string>>();

            private static readonly PitMenuController pmc = new PitMenuController();

            #endregion Private Fields

            #region Public Methods

            public void NewCar()
            {
                menuDict = new Dictionary<string, List<string>> { };
            }

            internal List<string> Get(string key)
            {
                if (menuDict.Count == 0)
                {
                    menuDict = pmc.GetMenuDict();
                }
                if (menuDict.Count > 0 && menuDict.TryGetValue(key, out List<string> value))
                {
                    return value;
                }
                return new List<string>();
            }

            internal List<string> GetKeys()
            {
                if (menuDict.Count == 0)
                {
                    menuDict = pmc.GetMenuDict();
                }
                return new List<string>(menuDict.Keys);
            }

            internal void Set(Dictionary<string, List<string>> unitTestDict)
            {
                menuDict = unitTestDict;
            }

            #endregion Public Methods
        }

        #endregion Private Classes
    }
}