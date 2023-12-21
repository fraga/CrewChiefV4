/* Pit Menu API using TheIronWolf's rF2 Shared Memory Map plugin
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
        private MappedBuffer<rF2PitInfo> pitInfoBuffer = null;

        private static rF2PitInfo pitInfo;
        private bool Connected = false;

        // Shared memory scans slowly until the first control is received. It
        // returns to scanning slowly when it hasn't received a control for a while.
        private int initialDelay = 230;

        // Delay in mS after sending a HW control to rFactor before sending another,
        // set by experiment
        // 20 works for category selection and tyres but fuel needs it slower
        private int delay = 30;

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
        public bool Connect()
        {
            if (!this.Connected)
            {
                Connected = sendHWControl.Connect();
                if (Connected)
                {
                    pitInfoBuffer = new MappedBuffer<rF2PitInfo>(
                    rFactor2Constants.MM_PITINFO_FILE_NAME,
                    partial: false,
                    skipUnchanged: true);
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
        /// Switch the MFD to
        /// MFDA Standard (Sectors etc.)
        /// MFDB Pit Menu
        /// MFDC Vehicle Status (Tyres etc.)
        /// MFDD Driving Aids
        /// MFDE Extra Info (RPM, temps)
        /// MFDF Race Info (Clock, leader etc.)
        /// MFDG Standings (Race position)
        /// MFDH Penalties
        ///
        /// Shared memory is normally scanning slowly until a control is received
        /// so send the first control with a longer delay
        /// </summary>
        /// <param name="display"></param>
        /// <returns></returns>
        public bool switchMFD(string display = "MFDB")
        {
            if (!Connected)
            {
                Connected = Connect();
            }
            if (Connected)
            {
                // To select MFDB screen for example:
                // If the MFD is off ToggleMFDA will turn it on then ToggleMFDB will switch
                // to the Pit Menu
                // If it is showing MFDA ToggleMFDA will turn it off then ToggleMFDB
                // will show the Pit Menu
                // If it is showing MFD"x" ToggleMFDA will show MFDA then ToggleMFDB
                // will show the Pit Menu
                string notDisplay = display == "MFDA" ? "ToggleMFDB" : "ToggleMFDA";
                int countDown = 20; // Otherwise it can lock up here
                do
                {
                    sendHWControl.SendHWControl(notDisplay, true);
                    System.Threading.Thread.Sleep(initialDelay);
                    sendHWControl.SendHWControl(notDisplay, false);
                    System.Threading.Thread.Sleep(delay);
                    sendHWControl.SendHWControl("Toggle" + display, true); // Select required MFD
                    System.Threading.Thread.Sleep(delay);
                    sendHWControl.SendHWControl("Toggle" + display, false);
                    System.Threading.Thread.Sleep(delay);
                }
                while (!(iSoftMatchCategory("TIRE", "FUEL")) && countDown-- > 0);
            }
            return Connected;
        }

        public  bool startUsingPitMenu()
        {
            return switchMFD("MFDB");
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

        /// <summary>
        /// Send a Pit Request (which toggles)
        /// </summary>
        /// <returns>Successful</returns>
        public bool PitRequest()
        {
            if (!Connected)
            {
                Connected = Connect();
            }
            if (Connected)
            {
                sendHWControl.SendHWControl("PitRequest", true);
                Log.Commentary("PitRequest sent");
            }
            return Connected;
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
        public string GetCategory()
        {
            pitInfoBuffer.GetMappedData(ref pitInfo);
            var catName = GetStringFromBytes(pitInfo.mPitMneu.mCategoryName);
            Log.Verbose($"Pit menu category '{catName}'");
            return catName;
        }

        /// <summary>
        /// Move up to the next category
        /// </summary>
        public void CategoryUp()
        {
            sendControl("PitMenuUp");
            Log.Verbose("Pit menu category up");
        }

        /// <summary>
        /// Move down to the next category
        /// </summary>
        public void CategoryDown()
        {
            sendControl("PitMenuDown");
            Log.Verbose("Pit menu category down");
        }

        //////////////////////////////////////////////////////////////////////////
        // Menu Choices
        /// <summary>
        /// Increment the current choice
        /// </summary>
        public void ChoiceInc()
        {
            sendControl("PitMenuIncrementValue");
            Log.Verbose("Pit menu value inc");
        }

        /// <summary>
        /// Decrement the current choice
        /// </summary>
        public void ChoiceDec()
        {
            sendControl("PitMenuDecrementValue");
            Log.Verbose("Pit menu value dec");
        }

        /// <summary>
        /// Get the text of the current choice
        /// </summary>
        /// <returns>string</returns>
        public string GetChoice()
        {
            pitInfoBuffer.GetMappedData(ref pitInfo);
            var choiceStr = GetStringFromBytes(pitInfo.mPitMneu.mChoiceString);
            if (CrewChief.Debugging)
            {
                Log.Verbose($"Pit menu choice '{choiceStr}'");
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
        /// <param name="cat1">category to match</param>
        /// <param name="cat2">optional other category to match</param>
        /// <returns></returns>
        private bool iSoftMatchCategory(string cat1, string cat2 = "bleagh")  // tbd Can this be done more cleanly?
        {
            string InitialCategory = GetCategory();
            int tryNo = 3;
            while (!(GetCategory().Contains(cat1) || GetCategory().Contains(cat2)))
            {
                CategoryDown();
                if (GetCategory() == InitialCategory)
                {  // Wrapped around, category not found
#pragma warning disable S1066
                    if (tryNo-- < 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// SetChoice first tries an exact match, if that fails it accepts an
        /// entry that starts with the choice.  This extracts that complexity
        /// (Not certain it's necessary, an exact match looks OK but it was
        /// written with StartsWith...)
        /// </summary>
        /// <param name="choice"></param>
        /// <param name="startsWith"></param>
        /// <returns>
        /// false: Choice not found using the current comparison
        /// </returns>
        bool choiceCompare(string choice, bool startsWith)
        {
            return ((startsWith && GetChoice().StartsWith(choice)) ||
                (!startsWith && GetChoice() == choice));
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
            bool startsWith = false;
            while (!choiceCompare(choice, startsWith))
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
#pragma warning disable S2583 // Conditionally executed code should be reachable
                        if (startsWith)
#pragma warning restore S2583 // Conditionally executed code should be reachable
                        {
                            return false;
                        }
                        startsWith = true;
                        inc = false;
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

        private void sendControl(string control)
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