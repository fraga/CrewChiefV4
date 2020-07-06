﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using rF2SharedMemory;
using rF2SharedMemory.rFactor2Data;

namespace rF2SharedMemoryAPI
{
  public class PitMenuController
  {
    public string[] genericTyreTypes = {
      "Supersoft",
      "Soft",
      "Medium",
      "Hard",
      "Intermediate",
      "Wet",
      "Monsoon"
    };

    private
    SendrF2HWControl SendControl = new SendrF2HWControl();

    MappedBuffer<rF2PitInfo> pitInfoBuffer = new MappedBuffer<rF2PitInfo>(
      rFactor2Constants.MM_PITINFO_FILE_NAME,
      false /*partial*/,
      true /*skipUnchanged*/);
    rF2PitInfo pitInfo;
    string LastControl;
    bool Connected = false;

    // Delay in mS after sending a HW control to rFactor before sending another,
    // set by experiment
    // 20 works for category selection and tyres but fuel needs it slower
    int delay = 40;
    // Shared memory scans slowly until the first control is received. It
    // returns to scanning slowly when it hasn't received a control for a while.
    int initialDelay = 200;

    /// <summary>
    /// All the Pit Menu categories of tyres that rF2 selects from
    /// </summary>
    private readonly string[] tyres = {
            "RR TIRE:",
            "RL TIRE:",
            "FR TIRE:",
            "FL TIRE:",
            "R TIRES:",
            "F TIRES:",
            "RT TIRES:",
            "LF TIRES:"
        };

    /// <summary>
    /// Connect to the Shared Memory running in rFactor
    /// </summary>
    /// <returns>
    /// true if connected
    /// </returns>
    public bool Connect()
    {
      this.Connected = this.SendControl.Connect();
      if (this.Connected)
      {
        this.pitInfoBuffer.Connect();
      }
      return this.Connected;
    }

    /// <summary>
    /// Shared memory is normally scanning slowly until a control is received
    /// so send the first control (to select the Pit Menu) with a longer delay
    /// </summary>
    public void startUsingPitMenu()
    {
      // Need to select the Pit Menu
      this.SendControl.SendHWControl("ToggleMFDB", true); // Select rFactor Pit Menu
      System.Threading.Thread.Sleep(initialDelay);
      //this.SendControl.SendHWControl("ToggleMFDB", false); // Select rFactor Pit Menu
      System.Threading.Thread.Sleep(delay);
      // And it would be annoying to turn if off it was on so toggle it again.
      // There is no way of telling if it's being displayed or not and the menu
      // can be operated whether it is or not.
      this.SendControl.SendHWControl("ToggleMFDB", true); // Select rFactor Pit Menu
      System.Threading.Thread.Sleep(delay);
      //this.SendControl.SendHWControl("ToggleMFDB", false); // Select rFactor Pit Menu
    }
    //////////////////////////////////////////////////////////////////////////
    // Menu Categories
    /// <summary>
    /// Get the current Pit Menu category
    /// </summary>
    /// <returns>
    /// Name of the category
    /// </returns>
    public string GetCategory()
    {
      pitInfoBuffer.GetMappedData(ref pitInfo);
      var catName = GetStringFromBytes(this.pitInfo.mPitMneu.mCategoryName);
      //var choiceStr = GetStringFromBytes(this.pitInfo.mPitMneu.mChoiceString);
      return catName;
    }

    /// <summary>
    /// Move up or down to the next category
    /// </summary>
    /// <param name="up">
    /// true: up
    /// false: down
    /// </param>
    public void UpDownOne(bool up)
    {
      string cmd;
      if (up)
        cmd = "PitMenuUp";
      else
        cmd = "PitMenuDown";
      this.SendControl.SendHWControl(cmd, true);
      System.Threading.Thread.Sleep(delay);
      //this.SendControl.SendHWControl(cmd, false);
      System.Threading.Thread.Sleep(delay);
    }

    /// <summary>
    /// Set the Pit Menu category
    /// </summary>
    /// <param name="category">string</param>
    /// <returns>
    /// false: Category not found
    /// </returns>
    public bool SetCategory(string category)
    {
      string InitialCategory = GetCategory();
      while (GetCategory() != category)
      {
        UpDownOne(true);
        if (GetCategory() == InitialCategory)
        {  // Wrapped around, category not found
          return false;
        }
      }
      return true;
    }

    /// <summary>
    /// Select a category that includes "category"
    /// </summary>
    /// <param name="category"></param>
    /// <returns>
    /// True: category found
    /// </returns>
    public bool SoftMatchCategory(string category)
    {
      string InitialCategory = GetCategory();
      while (!GetCategory().Contains(category))
      {
        UpDownOne(true);
        if (GetCategory() == InitialCategory)
        {  // Wrapped around, category not found
          return false;
        }
      }
      return true;
    }

    //////////////////////////////////////////////////////////////////////////
    // Menu Choices
    /// <summary>
    /// Increment or decrement the current choice
    /// </summary>
    /// <param name="inc">
    /// true: inc
    /// false: dec
    /// </param>
    public void IncDecOne(bool inc)
    {
      string cmd;
      if (inc)
        cmd = "PitMenuIncrementValue";
      else
        cmd = "PitMenuDecrementValue";
      this.SendControl.SendHWControl(cmd, true);
      System.Threading.Thread.Sleep(delay);
      //this.SendControl.SendHWControl(cmd, false);
      System.Threading.Thread.Sleep(delay);
    }

    /// <summary>
    /// Get the text of the current choice
    /// </summary>
    /// <returns>string</returns>
    public string GetChoice()
    {
      pitInfoBuffer.GetMappedData(ref pitInfo);
      var choiceStr = GetStringFromBytes(this.pitInfo.mPitMneu.mChoiceString);
      return choiceStr;
    }

    /// <summary>
    /// Set the current choice
    /// </summary>
    /// <param name="choice">string</param>
    /// <returns>
    /// false: Choice not found
    /// </returns>
    public bool SetChoice(string choice)
    {
      string LastChoice = GetChoice();
      bool inc = true;
      while (!GetChoice().StartsWith(choice))
      {
        IncDecOne(inc);
        if (GetChoice() == LastChoice)
        {
          if (inc)
          { // Go the other way
            inc = false;
          }
          else
          {
            return false;
          }
        }
        LastChoice = GetChoice();
      }
      return true;
    }

    //////////////////////////////////////////////////////////////////////////
    // Fuel

    /// <summary>
    /// Read the fuel level in the Pit Menu display
    /// Player.JSON needs to be set "Relative Fuel Strategy":FALSE,
    /// "+ 1.6/2"	Gallons to ADD/laps "Relative Fuel Strategy":TRUE,
    /// "65/25"		Litres TOTAL/laps   "Relative Fuel Strategy":FALSE,
    /// </summary>
    /// <returns>
    /// Fuel level in current units (litres or (US?) gallons)
    /// -1 if parsing the number failed
    /// -2 if Relative Fuel Strategy true
    /// </returns>
    public int GetFuelLevel()
    {
      Int16 current = -1;
      Match match; // = Regex.Match(input, pattern);
      Regex reggie = new Regex(@"(.*)/(.*)");
      // if (this.GetCategory() == "FUEL:")
      match = reggie.Match(GetChoice());
      if (match.Groups.Count == 3)
      {
        bool parsed = Int16.TryParse(match.Groups[1].Value, out current);
        if (parsed)
        {
          if (match.Groups[1].Value.StartsWith("+"))
          {
            current = -2;
          }
        }
      }
      return current;
    }

    /// <summary>
    /// Set the fuel level in the Pit Menu display
    /// Player.JSON needs to be set "Relative Fuel Strategy":FALSE,
    /// </summary>
    /// <param name="requiredFuel"> in current units (litres or (US?) gallons)</param>
    /// <returns>
    /// true if level set (or it reached max/min possible
    /// false if the level can't be read
    /// </returns>
    public bool SetFuelLevel(int requiredFuel)
    {
      this.SetCategory("FUEL:");
      int current = GetFuelLevel();
      if (current < 0)
      {
        return false; // Can't read value
      }

      // Adjust down if necessary
      while (current > requiredFuel)
      {
        IncDecOne(false);
        int newLevel = GetFuelLevel();
        if (newLevel == current)
        { // Can't adjust further
          break;
        }
        else
        {
          current = newLevel;
        }
      }
      // Adjust up to >= required level
      while (current < requiredFuel)
      {
        IncDecOne(true);
        int newLevel = GetFuelLevel();
        if (newLevel == current)
        { // Can't adjust further
          break;
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
    /// Get the type of tyre selected
    /// </summary>
    /// <returns>
    /// Supersoft
    /// Soft
    /// Medium
    /// Hard
    /// Intermediate
    /// Wet
    /// Monsoon
    /// No Change
    /// </returns>
    public string GetGenericTyreType()
    {
      //if (this.GetCategory().Contains("TIRE"))
      string current = GetChoice();
      string result = "NO_TYRE";
      foreach (var genericTyreType in tyreDict)
      {
        if (genericTyreType.Value.Contains(current))
        {
          result = genericTyreType.Key;
          break;
        }
      }
      return result;
    }

    /// <summary>
    /// Get the names of tyres available in the menu. (Includes "No Change")
    /// </summary>
    /// <returns>
    /// List of the names of tyres available in the menu
    /// </returns>
    public List<string> GetTyreTypeNames()
    {
      List<string> result = new List<string>();
      if (this.SoftMatchCategory("TIRE"))
      {
        string current = GetChoice();
        while (result == null || !result.Contains(current))
        {
          result.Add(current);
          IncDecOne(true);
          current = GetChoice();
        }
      }
      else
      {
        result = new List<string> { "NO_TYRE" };
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
      string InitialCategory = GetCategory();
      do
      {
        if (this.GetCategory().Contains("TIRE"))
        {
          result.Add(GetCategory());
        }
        UpDownOne(true);
      }
      while (GetCategory() != InitialCategory);
      return result;
    }

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
    // tbd: maybe the client should be able to update the lists?
    private static readonly Dictionary<string, List<string>> tyreDict = new Dictionary<string, List<string>>() {
            { "Supersoft", new List <string> {"supersoft" , "soft", "s310", "slick" } },
            { "Soft", new List <string> {"soft", "s310", "slick" } },
            { "Medium", new List <string> { "medium", "default", "slick" } },
            { "Hard", new List <string> {"hard", "p310", "endur", "medium", "default", "slick" } },
            { "Intermediate", new List <string> { "intermediate", "wet", "rain", "all-weather" } },
            { "Wet", new List <string> {"wet", "rain", "monsoon", "intermediate", "all-weather" } },
            { "Monsoon", new List <string> {"monsoon", "wet", "rain", "intermediate", "all-weather" } },
            { "No Change", new List <string> {"no change"} }
        };
    public Dictionary<string, string> translateTyreTypes(List<string> inMenu)
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


    /// <summary>
    /// Set the type of tyre selected
    /// The name of the type or No Change
    /// </summary>
    /// <returns>
    /// true if successful
    /// </returns>
    public bool SetTyreType(string requiredType)
    {
      if (this.GetCategory().Contains("TIRE"))
      {
        string current = GetChoice();

        // Adjust  if necessary
        while (GetChoice() != requiredType)
        {
          IncDecOne(true);
          string newType = GetChoice();
          if (newType == current)
          { // Didn't find it
            return false;
          }
        }
        return true;
      }
      return false;
    }

    /// <summary>
    /// Set the Generic type of tyre selected
    /// The name of the type or No Change
    /// </summary>
    /// <returns>
    /// true if successful
    /// </returns>
    public bool SetGenericTyreType(string requiredType)
    {
            // tbd: return SetTyreType(ttDict(requiredType));
            return false;
    }

    //////////////////////////////////////////////////////////////////////////
    // Testing
    /// <summary>
    /// Set the delay between sending each control
    /// </summary>
    /// <param name="mS"></param>
    public void setDelay(int mS, int initialDelay)
    {
      this.delay = mS;
      this.initialDelay = initialDelay;
    }
    //////////////////////////////////////////////////////////////////////////
    // Utils
    private static string GetStringFromBytes(byte[] bytes)
    {
      if (bytes == null)
        return "";

      var nullIdx = Array.IndexOf(bytes, (byte)0);

      return nullIdx >= 0
        ? Encoding.Default.GetString(bytes, 0, nullIdx)
        : Encoding.Default.GetString(bytes);
    }
  }
}
