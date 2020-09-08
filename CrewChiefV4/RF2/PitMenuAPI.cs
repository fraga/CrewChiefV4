﻿/* Pit Menu API using TheIronWolf's rF2 Shared Memory Map plugin
https://github.com/TheIronWolfModding/rF2SharedMemoryMapPlugin

It provides functions to directly control the Pit Menu (selecting it,
moving up and down, changing the menu choice) and "smart" controls that
move to a specified category or a specified choice.

Author: Tony Whitley (sven.smiles@gmail.com)
*/

using System;
using System.Text;
using CrewChiefV4;
using rF2SharedMemory;
using rF2SharedMemory.rFactor2Data;

namespace PitMenuAPI
{
    /// <summary>
    /// PitMenuAPI consists PitMenuAbstractionLayer : PitMenuController : PitMenu
    /// </summary>
    public class PitMenu
    {
        #region Private Fields

        private static
            SendrF2HWControl sendHWControl = new SendrF2HWControl();
        private static
                MappedBuffer<rF2PitInfo> pitInfoBuffer = new MappedBuffer<rF2PitInfo>(
                    rFactor2Constants.MM_PITINFO_FILE_NAME,
                    false /*partial*/,
                    true /*skipUnchanged*/);

        private static rF2PitInfo pitInfo;
        private static bool Connected = false;

        // Shared memory scans slowly until the first control is received. It
        // returns to scanning slowly when it hasn't received a control for a while.
        private static int initialDelay = 230;

        // Delay in mS after sending a HW control to rFactor before sending another,
        // set by experiment
        // 20 works for category selection and tyres but fuel needs it slower
        private static int delay = 30;

        #endregion Private Fields

        #region Public Methods

        ///////////////////////////////////////////////////////////////////////////
        /// Setup
        ///
        /// <summary>
        /// Connect to the Shared Memory running in rFactor
        /// </summary>
        /// <returns>
        /// true if connected
        /// </returns>
        public static bool Connect()
        {
            if (!Connected)
            {
                Connected = sendHWControl.Connect();
                if (Connected)
                {
                    pitInfoBuffer.Connect();
                }
            }
            return Connected;
        }

        /// <summary>
        /// Disconnect from the Shared Memory running in rFactor
        /// tbd: :No references???
        /// </summary>
        public void Disconnect()
        {
            pitInfoBuffer.Disconnect();
            sendHWControl.Disconnect();
            Connected = false;
        }

        /// <summary>
        /// Shared memory is normally scanning slowly until a control is received
        /// so send the first control (to select the Pit Menu) with a longer delay
        /// </summary>
        public static bool startUsingPitMenu()
        {
            if (!Connected)
            {
                Connected = Connect();
            }
            if (Connected)
            {
                // Need to select the Pit Menu
                // If it is off ToggleMFDA will turn it on then ToggleMFDB will switch
                // to the Pit Menu
                // If it is showing MFDA ToggleMFDA will turn it off then ToggleMFDB
                // will show the Pit Menu
                // If it is showing MFD"x" ToggleMFDA will show MFDA then ToggleMFDB
                // will show the Pit Menu
                do
                {
                    sendHWControl.SendHWControl("ToggleMFDA", true);
                    System.Threading.Thread.Sleep(initialDelay);
                    sendHWControl.SendHWControl("ToggleMFDA", false);
                    System.Threading.Thread.Sleep(delay);
                    sendHWControl.SendHWControl("ToggleMFDB", true); // Select rFactor Pit Menu
                    System.Threading.Thread.Sleep(delay);
                    sendHWControl.SendHWControl("ToggleMFDB", false); // Select rFactor Pit Menu
                    System.Threading.Thread.Sleep(delay);
                }
                while (!(iSoftMatchCategory("TIRE") || iSoftMatchCategory("FUEL")));
            }
            return Connected;
        }

        /// <summary>
        /// Set the delay between sending each control
        /// After sending the first control in sequence the delay should be longer
        /// as the Shared Memory takes up to 200 mS to switch to its higher update
        /// rate.  After 200 mS without receiving any controls it returns to a
        /// 200 mS update.
        /// </summary>
        /// <param name="mS"></param>
        public void setDelay(int mS, int _initialDelay)
        {
            delay = mS;
            initialDelay = _initialDelay;
        }

        //////////////////////////////////////////////////////////////////////////
        /// Direct menu control
        //////////////////////////////////////////////////////////////////////////
        // Menu Categories
        /// <summary>
        /// Get the current Pit Menu category
        /// </summary>
        /// <returns>
        /// Name of the category
        /// </returns>
        public static string GetCategory()
        {
            pitInfoBuffer.GetMappedData(ref pitInfo);
            var catName = GetStringFromBytes(pitInfo.mPitMneu.mCategoryName);
            if (CrewChief.Debugging)
            {
                Console.WriteLine($"Pit menu category '{catName}'");
            }
            return catName;
        }

        /// <summary>
        /// Move up to the next category
        /// </summary>
        public static void CategoryUp()
        {
            sendControl("PitMenuUp");
        }

        /// <summary>
        /// Move down to the next category
        /// </summary>
        public static void CategoryDown()
        {
            sendControl("PitMenuDown");
        }

        //////////////////////////////////////////////////////////////////////////
        // Menu Choices
        /// <summary>
        /// Increment the current choice
        /// </summary>
        public static void ChoiceInc()
        {
            sendControl("PitMenuIncrementValue");
        }

        /// <summary>
        /// Decrement the current choice
        /// </summary>
        public static void ChoiceDec()
        {
            sendControl("PitMenuDecrementValue");
        }

        /// <summary>
        /// Get the text of the current choice
        /// </summary>
        /// <returns>string</returns>
        public static string GetChoice()
        {
            pitInfoBuffer.GetMappedData(ref pitInfo);
            var choiceStr = GetStringFromBytes(pitInfo.mPitMneu.mChoiceString);
            if (CrewChief.Debugging)
            {
                Console.WriteLine($"Pit menu choice '{choiceStr}'");
            }
            return choiceStr;
        }

        //////////////////////////////////////////////////////////////////////////
        /// "Smart" menu control - specify which category or choice and it will be
        /// selected
        //////////////////////////////////////////////////////////////////////////
        // Menu Categories
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
            int tryNo = 5;
            while (GetCategory() != category)
            {
                CategoryDown();
                if (GetCategory() == InitialCategory)
                {  // Wrapped around, category not found
                    if (tryNo-- < 0)
                    {
                        return false;
                    }
                    startUsingPitMenu();
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
            return iSoftMatchCategory(category);
        }

        /// <summary>
        /// Static version
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        private static bool iSoftMatchCategory(string category)  // tbd Can this be done more cleanly?
        {
            string InitialCategory = GetCategory();
            int tryNo = 3;
            while (!GetCategory().Contains(category))
            {
                CategoryDown();
                if (GetCategory() == InitialCategory)
                {  // Wrapped around, category not found
                    if (tryNo-- < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
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
                if (inc)
                    ChoiceInc();
                else
                    ChoiceDec();
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

        #endregion Public Methods

        #region Private Methods

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

        private static void sendControl(string control)
        {
            sendHWControl.SendHWControl(control, true);
            System.Threading.Thread.Sleep(delay);
            // Doesn't seem to be necessary to do "retVal false" too
            sendHWControl.SendHWControl(control, false);
            System.Threading.Thread.Sleep(delay);
        }

        #endregion Private Methods
    }
}