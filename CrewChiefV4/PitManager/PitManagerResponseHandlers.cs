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
    class PitManagerResponseHandlers
    {
        /// <summary>
        /// The common response handlers for all games.  Some games may have
        /// their own special cases.
        /// </summary>

        // tyre compound responses
        private const string folderConfirmSoftTyres = "mandatory_pit_stops/confirm_soft_tyres";
        private const string folderConfirmMediumTyres = "mandatory_pit_stops/confirm_medium_tyres";
        private const string folderConfirmHardTyres = "mandatory_pit_stops/confirm_hard_tyres";
        private const string folderConfirmPrimeTyres = "mandatory_pit_stops/confirm_prime_tyres";
        private const string folderConfirmOptionTyres = "mandatory_pit_stops/confirm_option_tyres";
        private const string folderConfirmAlternateTyres = "mandatory_pit_stops/confirm_alternate_tyres";
        private const string folderRequestedTyreNotAvailable = "mandatory_pit_stops/confirm_requested_tyre_not_available";
        static private void playMessage(string folder)
        {
            if (SoundCache.hasSingleSound(folder))
            {
                CrewChief.audioPlayer.playMessageImmediately(new QueuedMessage(folder, 0));
            }

        }
        static public bool responseHandler_example()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);
            return true;
        }
        static public bool responseHandler_TyreCompoundHard()
        {
            // if (SoundCache.hasSingleSound(folderConfirmHardTyres))
                playMessage(folderConfirmHardTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundMedium()
        {
            playMessage(folderConfirmMediumTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundSoft()
        {
            playMessage(folderConfirmSoftTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundWet()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);     // tbd
            return true;
        }
        static public bool responseHandler_TyreCompoundOption()
        {
            playMessage(folderConfirmOptionTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundPrime()
        {
            playMessage(folderConfirmPrimeTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundAlternate()
        {
            playMessage(folderConfirmAlternateTyres);
            return true;
        }
        static public bool responseHandler_TyreCompoundNext()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);
            return true;
        }
        static public bool responseHandler_TyreCompoundNotAvailable()   // Warning response
        {
            playMessage(folderRequestedTyreNotAvailable);
            return true;
        }
        static public bool responseHandler_fuelToEnd()
        {
            playMessage(AudioPlayer.folderFuelToEnd);
            return true;
        }
    }
}
