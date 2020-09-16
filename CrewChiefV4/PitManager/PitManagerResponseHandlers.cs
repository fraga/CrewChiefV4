using CrewChiefV4.Audio;

namespace CrewChiefV4.PitManager
{
    static internal class PitManagerResponseHandlers
    {
        /// <summary>
        /// The common response handlers for all games.  Some games may have
        /// their own special cases.
        /// </summary>
        private static readonly CrewChief crewChief = MainWindow.instance.crewChief;


        #region Public Methods

#pragma warning disable S3400 // Methods should not return constants
        static public bool PMrh_NoResponse()
#pragma warning restore S3400 // Methods should not return constants
        {
            return true;
        }
        static public bool PMrh_Acknowledge()
        {
            playMessage(AudioPlayer.folderAcknowlegeOK);
            return true;
        }

        static public bool PMrh_CantChangeThose()
        {
            playMessage("mandatory_pit_stops/cant_change_those");
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

        static public bool PMrh_ChangeLeftTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_left_side_tyres");
            return true;
        }

        static public bool PMrh_ChangeRightTyres()
        {
            playMessage("mandatory_pit_stops/confirm_change_right_side_tyres");
            return true;
        }

        static public bool PMrh_ChangeFrontLeftTyre()
        {
            playMessage("mandatory_pit_stops/confirm_change_front_left_only");
            return true;
        }

        static public bool PMrh_ChangeFrontRightTyre()
        {
            playMessage("mandatory_pit_stops/confirm_change_front_right_only");
            return true;
        }

        static public bool PMrh_ChangeRearLeftTyre()
        {
            playMessage("mandatory_pit_stops/confirm_change_rear_left_only");
            return true;
        }

        static public bool PMrh_ChangeRearRightTyre()
        {
            playMessage("mandatory_pit_stops/confirm_change_rear_right_only");
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

        static public bool PMrh_TyreCompoundIntermediate()
        {
            playMessage("mandatory_pit_stops/confirm_intermediate_tyres");
            return true;
        }

        static public bool PMrh_TyreCompoundWet()
        {
            playMessage("mandatory_pit_stops/confirm_wet_tyres");
            return true;
        }

        static public bool PMrh_TyreCompoundMonsoon()
        {
            playMessage("mandatory_pit_stops/confirm_monsoon_tyres");
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
            playMessage("mandatory_pit_stops/confirm_next_tyre_compound");
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

#pragma warning disable S4144 // Methods should not have identical implementations
        static public bool PMrh_FuelAddXlitres()
#pragma warning restore S4144 // Methods should not have identical implementations
        {
            playMessage(AudioPlayer.folderFuelToEnd); // tbd:
            return true;
        }

        static public bool PMrh_noFuel()
        {
            playMessage("mandatory_pit_stops/confirm_no_refuelling");
            return true;
        }

        static public bool PMrh_RepairAll()
        {
            playMessage("mandatory_pit_stops/confirm_fix_all");
            return true;
        }

        static public bool PMrh_RepairNone()
        {
            playMessage("mandatory_pit_stops/confirm_fix_nothing");
            return true;
        }

        static public bool PMrh_RepairBody()
        {
            playMessage("mandatory_pit_stops/confirm_fix_body");
            return true;
        }

        static public bool PMrh_ServePenalty()
        {
            playMessage("mandatory_pit_stops/will_be_serving_penalty");
            return true;
        }

        static public bool PMrh_DontServePenalty()
        {
            playMessage("mandatory_pit_stops/will_not_be_serving_penalty");
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        // tyre compound responses
        static private void playMessage(string folder)
        {
            if (true) //SoundCache.hasSingleSound(folder))
            {
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(folder, 0));
            }
            else
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
        }

        #endregion Private Methods
    }
}