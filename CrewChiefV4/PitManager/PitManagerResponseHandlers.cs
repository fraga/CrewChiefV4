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
    class PitManagerResponseHandlers
    {
        /// <summary>
        /// The common response handlers for all games.  Some games may have
        /// their own special cases.
        /// </summary>

        // tyre compound responses
        static private void playMessage(string folder)
        {
            if (true) //SoundCache.hasSingleSound(folder))
            {
                CrewChief.audioPlayer.playMessageImmediately(new QueuedMessage(folder, 0));
            }
            else
                CrewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));

        }
        static public bool PMrh_Acknowledge()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);
            return true;
        }
        static public bool PMrh_CantDoThat()
        {
            playMessage("mandatory_pit_stops/cant_do_that");
            return true;
        }

        static public bool PMrh_ChangeAllTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_all_tyres");
            return true;
        }
        static public bool PMrh_ChangeFrontTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_front_tyres");
            return true;
        }
        static public bool PMrh_ChangeRearTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_rear_tyres");
            return true;
        }
        static public bool PMrh_ChangeNoTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_no_tyres");
            return true;
        }

        static public bool PMrh_TyreCompoundHard()
        {
            playMessage("mandatory_pit_stops/confirm_hard_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundMedium()
        {
            playMessage("mandatory_pit_stops/confirm_medium_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundSoft()
        {
            playMessage("mandatory_pit_stops/confirm_soft_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundWet()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);     // tbd
            return true;
        }
        static public bool PMrh_TyreCompoundOption()
        {
            playMessage("mandatory_pit_stops/confirm_option_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundPrime()
        {
            playMessage("mandatory_pit_stops/confirm_prime_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundAlternate()
        {
            playMessage("mandatory_pit_stops/confirm_alternate_tyres");
            return true;
        }
        static public bool PMrh_TyreCompoundNext()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);
            return true;
        }
        static public bool PMrh_TyreCompoundNotAvailable()   // Warning response
        {
            playMessage("mandatory_pit_stops/confirm_requested_tyre_not_available");
            return true;
        }
        static public bool PMrh_fuelToEnd()
        {
            playMessage(AudioPlayer.folderFuelToEnd);
            return true;
        }
        static public bool PMrh_FuelAddXlitres()
        {
            playMessage(AudioPlayer.folderFuelToEnd); // tbd:
            return true;
        }
        static public bool PMrh_noFuel()
        {
            playMessage("mandatory_pit_stops/confirm_no_refuelling ");
            return true;
        }
    }
}
