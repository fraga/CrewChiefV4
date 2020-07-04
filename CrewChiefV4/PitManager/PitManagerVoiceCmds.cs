using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.PitManager
{
    using PME = PitManagerEvent;  // shorthand
    using SRE = SpeechRecogniser;

    public class PitManagerVoiceCmds : AbstractEvent
    {
        private float fuelCapacity = -1;
        private float currentFuel = -1;
        private int roundedLitresNeeded = -1;

        private static readonly PitManager pmh = new PitManager();
        private static readonly Dictionary<PitManagerEvent, String[]> voiceCmds =
            new Dictionary<PitManagerEvent, String[]>
        {
            {PME.TyreChangeAll,     SRE.PIT_STOP_CHANGE_ALL_TYRES },
            {PME.TyreChangeNone,    SRE.PIT_STOP_CLEAR_TYRES },
            {PME.TyreChangeFront,   SRE.PIT_STOP_CHANGE_FRONT_TYRES },
            {PME.TyreChangeRear,    SRE.PIT_STOP_CHANGE_REAR_TYRES },
            {PME.TyreChangeLeft,    SRE.PIT_STOP_CHANGE_LEFT_SIDE_TYRES },
            {PME.TyreChangeRight,   SRE.PIT_STOP_CHANGE_RIGHT_SIDE_TYRES},
            {PME.TyreChangeLF,      SRE.PIT_STOP_CHANGE_FRONT_LEFT_TYRE },
            {PME.TyreChangeRF,      SRE.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE },
            {PME.TyreChangeLR,      SRE.PIT_STOP_CHANGE_REAR_LEFT_TYRE },
            {PME.TyreChangeRR,      SRE.PIT_STOP_CHANGE_REAR_RIGHT_TYRE },

            {PME.TyrePressureLF,    SRE.PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRF,    SRE.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE },
            {PME.TyrePressureLR,    SRE.PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRR,    SRE.PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE },

            {PME.TyreCompoundHard,  SRE.PIT_STOP_HARD_TYRES },
            {PME.TyreCompoundMedium, SRE.PIT_STOP_MEDIUM_TYRES },
            {PME.TyreCompoundSoft,  SRE.PIT_STOP_SOFT_TYRES },
            {PME.TyreCompoundWet,   SRE.PIT_STOP },        // tbd:
            {PME.TyreCompoundOption, SRE.PIT_STOP_OPTION_TYRES },
            {PME.TyreCompoundPrime, SRE.PIT_STOP_PRIME_TYRES },
            {PME.TyreCompoundAlternate, SRE.PIT_STOP_ALTERNATE_TYRES },
            {PME.TyreCompoundNext,  SRE.PIT_STOP_NEXT_TYRE_COMPOUND },

            {PME.FuelAddXlitres,    SRE.PIT_STOP_ADD },
            //{PME.FuelFillToXlitres, SRE.PIT_STOP },
            {PME.FuelFillToEnd,     SRE.PIT_STOP_FUEL_TO_THE_END },
            {PME.FuelNone,          SRE.PIT_STOP_DONT_REFUEL },

            {PME.RepairAll,         SRE.PIT_STOP },
            {PME.RepairNone,        SRE.PIT_STOP },
            {PME.RepairFast,        SRE.PIT_STOP },        // iRacing
            {PME.RepairAllAero,     SRE.PIT_STOP },        // R3E
            {PME.RepairFrontAero,   SRE.PIT_STOP },
            {PME.RepairRearAero,    SRE.PIT_STOP },
            {PME.RepairSuspension,  SRE.PIT_STOP },
            {PME.RepairSuspensionNone, SRE.PIT_STOP },
            {PME.RepairBody,        SRE.PIT_STOP },         // tbd: rF2

            {PME.PenaltyServe,      SRE.PIT_STOP },
            {PME.PenaltyServeNone,  SRE.PIT_STOP },

            {PME.AeroFrontPlusMinusX, SRE.PIT_STOP },
            {PME.AeroRearPlusMinusX, SRE.PIT_STOP },
            {PME.AeroFrontSetToX,   SRE.PIT_STOP },
            {PME.AeroRearSetToX,    SRE.PIT_STOP },

            {PME.GrillePlusMinusX,  SRE.PIT_STOP },        // tbd: rF2
            {PME.GrilleSetToX,      SRE.PIT_STOP },
            {PME.WedgePlusMinusX,   SRE.PIT_STOP },
            {PME.WedgeSetToX,       SRE.PIT_STOP },
            {PME.TrackBarPlusMinusX, SRE.PIT_STOP },
            {PME.TrackBarSetToX,    SRE.PIT_STOP },
            {PME.RubberLF,          SRE.PIT_STOP },
            {PME.RubberRF,          SRE.PIT_STOP },
            {PME.RubberLR,          SRE.PIT_STOP },
            {PME.RubberRR,          SRE.PIT_STOP },
            {PME.FenderL,           SRE.PIT_STOP },
            {PME.FenderR,           SRE.PIT_STOP },
            {PME.FlipUpL,           SRE.PIT_STOP },
            {PME.FlipUpR,           SRE.PIT_STOP },

            {PME.Tearoff,           SRE.PIT_STOP_TEAROFF },    // iRacing
            {PME.TearOffNone,       SRE.PIT_STOP_CLEAR_WIND_SCREEN },
            };
        public PitManagerVoiceCmds(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
            this.fuelCapacity = -1;
            this.currentFuel = -1;
        }
        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Garage, SessionPhase.Green, SessionPhase.Countdown, SessionPhase.FullCourseYellow }; }
        }

        public override void respond(String voiceMessage)
        {
            foreach (var cmd in voiceCmds)
            {
                if (SRE.ResultContains(voiceMessage, cmd.Value))
                {
                    pmh.EventHandler(cmd.Key);
                    break;
                }

            }
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            Boolean autoFuelToEnd = true;   // tbd: UserSettings.GetUserSettings().getBoolean("iracing_enable_auto_fuel_to_end_of_race");

            fuelCapacity = currentGameState.FuelData.FuelCapacity;
            currentFuel = currentGameState.FuelData.FuelLeft;
            if (autoFuelToEnd)
            {
                if (previousGameState != null && !previousGameState.PitData.InPitlane && currentGameState.PitData.InPitlane
                    && currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionRunningTime > 15
                    && !previousGameState.PitData.IsInGarage && !currentGameState.PitData.JumpedToPits)
                {
                    Fuel fuelEvent = (Fuel)CrewChief.getEvent("Fuel");
                    float litresNeeded = fuelEvent.getLitresToEndOfRace(true);

                    if (litresNeeded == float.MaxValue)
                    {
                        audioPlayer.playMessage(new QueuedMessage(AudioPlayer.folderNoData, 0));
                    }
                    else if (litresNeeded <= 0)
                    {
                        audioPlayer.playMessage(new QueuedMessage(Fuel.folderPlentyOfFuel, 0));
                    }
                    else if (litresNeeded > 0)
                    {
                        roundedLitresNeeded = (int)Math.Ceiling(litresNeeded);
                        //tbd:  EventHandler(roundedLitresNeeded);
                        // AddFuel(roundedLitresNeeded);
                        Console.WriteLine("Auto refuel to the end of the race, adding " + roundedLitresNeeded + " liters of fuel");
                        if (roundedLitresNeeded > fuelCapacity - currentFuel)
                        {
                            // if we have a known fuel capacity and this is less than the calculated amount of fuel we need, warn about it.
                            audioPlayer.playMessage(new QueuedMessage(Fuel.folderWillNeedToStopAgain, 0, secondsDelay: 4, abstractEvent: this));
                        }
                        else
                        {
                            audioPlayer.playMessage(new QueuedMessage(AudioPlayer.folderFuelToEnd, 0));
                        }
                    }
                }
            }
        }
        public override void clearState()
        {
            this.fuelCapacity = -1;
            this.currentFuel = -1;
        }

        public bool responseHandler_acknowledge()
        {
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
            return true;
        }
    }
}
