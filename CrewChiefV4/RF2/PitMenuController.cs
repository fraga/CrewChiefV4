/*
Use HW controls (and reading the Pit Info buffer) to set the rFactor 2 Pit Menu
using TheIronWolf's rF2 Shared Memory Map plugin
https://github.com/TheIronWolfModding/rF2SharedMemoryMapPlugin

Author: Tony Whitley (sven.smiles@gmail.com)
*/
using CrewChiefV4;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PitMenuAPI
{
    /// <summary>
    /// PitMenuAPI consists PitMenuAbstractionLayer : PitMenuController : PitMenu
    /// </summary>
    public class PitMenuController : PitMenu
    {
        #region Public Fields

        public string[] genericTyreTypes = {
            "Supersoft",
            "Soft",
            "Medium",
            "Hard",
            "Intermediate",
            "Wet",
            "Monsoon"
    };

        #endregion Public Fields

        #region Private Fields

        /// <summary>
        /// Dictionary of all menu categories for the current vehicle
        /// The tyre categories have a list of all the tyre choices
        /// </summary>
        private static Dictionary<string, List<string>> shadowPitMenu;
        private static List<string> shadowPitMenuCats = new List<string> { };
        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Get a dictionary of all choices for all tyre/tire menu categories
        /// TIREs are a special case, we want all values. For the others we might
        /// want min and max.  It will take quite a time to find them though.
        /// </summary>
        /// <returns>
        /// Dictionary of all choices for all tyre/tire menu categories
        /// </returns>
        public static Dictionary<string, List<string>> GetMenuDict()
        {
            shadowPitMenu = new Dictionary<string, List<string>> { };
            shadowPitMenuCats = new List<string> { };
            string category;
            string choice;

            Log.Debug("GetMenuDict");
            if (startUsingPitMenu())
                {
                do
                {
                    category = GetCategory();
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        break;
                    }
                    shadowPitMenu[category] = new List<string>();
                    shadowPitMenuCats.Add(category);
                    if (category.Contains("TIRE"))
                    { // The only category that wraps round
                        do
                        {
                            choice = GetChoice();
                            shadowPitMenu[category].Add(choice);
                            nextChoice();
                        } while (!shadowPitMenu[category].Contains(GetChoice()));
                    }
                    CategoryDown();
                } while (!shadowPitMenu.ContainsKey(GetCategory()));
            }

            if (shadowPitMenu.Count < 2)
            {
                // return empty so this will be called again
                shadowPitMenu = new Dictionary<string, List<string>> {};
            }
            return shadowPitMenu;
        }
        /// <summary>
        /// Keep banging away until the menu choice changes
        /// </summary>
        /// <returns>the new choice</returns>
        private static string nextChoice()
        {
            string newChoice;
            string currentChoice = GetChoice();
            do
            {
                ChoiceInc();
                newChoice = GetChoice();
                if (newChoice == currentChoice)
                {
                    startUsingPitMenu();
                }
            }
            while (newChoice == currentChoice);
            return newChoice;
        }

        /// <summary>
        /// Take the shortest way to "category"
        /// </summary>
        /// <param name="category"> Pit Menu category</param>
        public static bool SmartSetCategory(string category)
        {
            if (shadowPitMenuCats.Count == 0)
            {
                if (GetMenuDict().Count == 0)
                {
                    return false;
                }
            }
            string currentCategory = GetCategory();
            if (category != currentCategory)
            {
                startUsingPitMenu();
                int origin = Array.IndexOf(shadowPitMenuCats.ToArray(), currentCategory);
                int target = Array.IndexOf(shadowPitMenuCats.ToArray(), category);

                for (int i = 1; i < shadowPitMenuCats.Count; i++)
                {
                    if (((origin + i) % shadowPitMenuCats.Count) == target)
                    {
                        //down
                        while (GetCategory() != category)
                        {
                            CategoryDown();
                        }
                        break;
                    }
                    if (((shadowPitMenuCats.Count + origin - i) % shadowPitMenuCats.Count) == target)
                    {
                        //up
                        while (GetCategory() != category)
                        {
                            CategoryUp();
                        }
                        break;
                    }
                }
            }
            return GetCategory() == category;
        }

        //////////////////////////////////////////////////////////////////////////
        // Menu Choices

        //////////////////////////////////////////////////////////////////////////
        // Fuel

        /// <summary>
        /// Read the fuel level to find if "Relative Fuel Strategy" is selected
        /// in Player.JSON (if it is there is a + before the fuel quantity)
        /// </summary>
        /// <returns>
        /// true: "Relative Fuel Strategy" is selected
        /// </returns>
        public bool RelativeFuelStrategy()
        {
            bool relativeFuelStrategy = false;
            Match match;
            Regex reggie = new Regex(@"(.*)/(.*)");
            if (SmartSetCategory("FUEL:"))
            {
                match = reggie.Match(GetChoice());
                if (match.Groups.Count == 3)
                {
                    if (match.Groups[1].Value.StartsWith("+"))
                    {
                        relativeFuelStrategy = true;
                    }
                }
            }
            return relativeFuelStrategy;
        }

        /// <summary>
        /// Read the fuel level in the Pit Menu display
        /// Player.JSON "Relative Fuel Strategy" affects the display
        /// "+ 1.6/2"	Gallons to ADD/laps "Relative Fuel Strategy":TRUE,
        /// "65/25"		Litres TOTAL/laps   "Relative Fuel Strategy":FALSE,
        /// </summary>
        /// <returns>
        /// Fuel level in litres
        /// -1 if parsing the number failed
        /// </returns>
        public int GetFuelLevel()
        {
            float current = -1;
            Match match;
            Regex reggie = new Regex(@"(.*)/(.*)");
            if (SmartSetCategory("FUEL:"))
            {
                match = reggie.Match(GetChoice());
                if (match.Groups.Count == 3)
                {
                    bool parsed = float.TryParse(match.Groups[1].Value, out current);
                    if (parsed)
                    {
                        if (match.Value.Contains("."))
                        {   // Gallons are displayed in 10ths
                            current = convertGallonsToLitres(current);
                        }
                    }
                }
            }
            return (int)current;
        }
        private float convertGallonsToLitres(float gallons)
        {
            float litresPerGallon = 3.78541f;
            return (float)Math.Round(gallons * litresPerGallon);
        }

        /// <summary>
        /// Set the fuel level in the Pit Menu display
        /// </summary>
        /// <param name="requiredFuel"> in litres (even if current units are (US?) gallons)</param>
        /// <returns>
        /// true if level set (or it reached max/min possible
        /// false if the level can't be read
        /// </returns>
        public bool SetFuelLevel(int requiredFuel)
        {
            int tryNo = 5;

            SmartSetCategory("FUEL:");
            int current = GetFuelLevel();

            if (current < 0)
            {
                return false; // Can't read value
            }

            // Adjust down if necessary
            while (current > requiredFuel)
            {
                ChoiceDec();
                int newLevel = GetFuelLevel();
                if (newLevel == current)
                { // Can't adjust further
                    if (tryNo-- < 0)
                    {
                        return false;
                    }
                    startUsingPitMenu();
                    SmartSetCategory("FUEL:");
                }
                else
                {
                    current = newLevel;
                }
            }
            // Adjust up to >= required level
            while (current < requiredFuel)
            {
                ChoiceInc();
                int newLevel = GetFuelLevel();
                if (newLevel == current)
                { // Can't adjust further
                    if (tryNo-- < 0)
                    {
                        return false;
                    }
                    startUsingPitMenu();
                    SmartSetCategory("FUEL:");
                }
                else
                {
                    current = newLevel;
                }
            }
            return true;
        }

        //////////////////////////////////////////////////////////////////////////
        // Tyres

        /// <summary>
        /// Get the names of tyres available in the menu. (Includes "No Change")
        /// </summary>
        /// <returns>
        /// List of the names of tyres available in the menu
        /// </returns>
        public List<string> GetTyreTypeNames()
        {
            List<string> result = new List<string> { "NO_TYRE" };
            foreach (var category in shadowPitMenu)
            {
                if (category.Key.Contains("TIRE"))
                {
                    result = category.Value;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        /// Get the choices of tyres to change - "FL FR RL RR", "F R" etc.
        /// </summary>
        /// <returns>
        /// List of the change options available in the menu
        /// e.g. {"F TIRES", "R TIRES"}
        /// </returns>
        public List<string> GetTyreChangeCategories()
        {
            List<string> result = new List<string>();
            if (shadowPitMenu.Count == 0)
            {
                if (GetMenuDict().Count == 0)
                {
                    return result;
                }
            }
            foreach (var category in shadowPitMenu.Keys)
            {
                if (category.Contains("TIRE"))
                {
                    result.Add(category);
                }
            }
            return result;
        }

        /// <summary>
        /// Set the type of tyre selected
        /// The name of the type or No Change
        /// </summary>
        /// <returns>
        /// true if successful
        /// </returns>

        public bool SetTyreType(string requiredType)
        {
            if (GetCategory().Contains("TIRE"))
            {
                string current = GetChoice();

                // Adjust  if necessary
                while (GetChoice() != requiredType)
                {
                    ChoiceInc();
                    string newType = GetChoice();
                    if (newType == current)
                    { // Didn't find it
                      //return false;
                    }
                }
                return true;
            }
            return false;
        }

#endregion Public Methods
    }
}