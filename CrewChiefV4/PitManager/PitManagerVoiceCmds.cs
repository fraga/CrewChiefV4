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

    public class PitManagerVoiceCmds : AbstractEvent
    {
        private float fuelCapacity = -1;
        private float currentFuel = -1;
        private int roundedLitresNeeded = -1;

        private static readonly PitManager pmh = new PitManager();
        private static readonly Dictionary<PitManagerEvent, String[]> voiceCmds =
            new Dictionary<PitManagerEvent, String[]>
        {
            {PME.TyreChangeAll,     SpeechRecogniser.PIT_STOP_CHANGE_ALL_TYRES },
            {PME.TyreChangeNone,    SpeechRecogniser.PIT_STOP_CLEAR_TYRES },
            {PME.TyreChangeFront,   SpeechRecogniser.PIT_STOP_CHANGE_FRONT_TYRES },
            {PME.TyreChangeRear,    SpeechRecogniser.PIT_STOP_CHANGE_REAR_TYRES },
            {PME.TyreChangeLeft,    SpeechRecogniser.PIT_STOP_CHANGE_LEFT_SIDE_TYRES },
            {PME.TyreChangeRight,   SpeechRecogniser.PIT_STOP_CHANGE_RIGHT_SIDE_TYRES},
            {PME.TyreChangeLF,      SpeechRecogniser.PIT_STOP_CHANGE_FRONT_LEFT_TYRE },
            {PME.TyreChangeRF,      SpeechRecogniser.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE },
            {PME.TyreChangeLR,      SpeechRecogniser.PIT_STOP_CHANGE_REAR_LEFT_TYRE },
            {PME.TyreChangeRR,      SpeechRecogniser.PIT_STOP_CHANGE_REAR_RIGHT_TYRE },

            {PME.TyrePressureLF,    SpeechRecogniser.PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRF,    SpeechRecogniser.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE },
            {PME.TyrePressureLR,    SpeechRecogniser.PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRR,    SpeechRecogniser.PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE },

            {PME.TyreCompoundHard,  SpeechRecogniser.PIT_STOP_HARD_TYRES },
            {PME.TyreCompoundMedium, SpeechRecogniser.PIT_STOP_MEDIUM_TYRES },
            {PME.TyreCompoundSoft,  SpeechRecogniser.PIT_STOP_SOFT_TYRES },
            //{PME.TyreCompoundWet,   SpeechRecogniser.PIT_STOP },        // tbd:
            {PME.TyreCompoundPrime, SpeechRecogniser.PIT_STOP_PRIME_TYRES },
            {PME.TyreCompoundAlternate, SpeechRecogniser.PIT_STOP_ALTERNATE_TYRES },
            {PME.TyreCompoundNext,  SpeechRecogniser.PIT_STOP_NEXT_TYRE_COMPOUND },

            {PME.FuelAddXlitres,    SpeechRecogniser.PIT_STOP_ADD },
            //{PME.FuelFillToXlitres, SpeechRecogniser.PIT_STOP },
            {PME.FuelFillToEnd,     SpeechRecogniser.PIT_STOP_FUEL_TO_THE_END },
            {PME.FuelNone,          SpeechRecogniser.PIT_STOP_DONT_REFUEL },

            {PME.RepairAll,         SpeechRecogniser.PIT_STOP },
            {PME.RepairNone,        SpeechRecogniser.PIT_STOP },
            {PME.RepairFast,        SpeechRecogniser.PIT_STOP },        // iRacing
            {PME.RepairAllAero,     SpeechRecogniser.PIT_STOP },        // R3E
            {PME.RepairFrontAero,   SpeechRecogniser.PIT_STOP },
            {PME.RepairRearAero,    SpeechRecogniser.PIT_STOP },
            {PME.RepairSuspension,  SpeechRecogniser.PIT_STOP },
            {PME.RepairSuspensionNone, SpeechRecogniser.PIT_STOP },

            {PME.PenaltyServe,      SpeechRecogniser.PIT_STOP },
            {PME.PenaltyServeNone,  SpeechRecogniser.PIT_STOP },

            {PME.AeroFrontPlusMinusX, SpeechRecogniser.PIT_STOP },
            {PME.AeroRearPlusMinusX, SpeechRecogniser.PIT_STOP },
            {PME.AeroFrontSetToX,   SpeechRecogniser.PIT_STOP },
            {PME.AeroRearSetToX,    SpeechRecogniser.PIT_STOP },

            {PME.GrillePlusMinusX,  SpeechRecogniser.PIT_STOP },        // tbd: rF2
            {PME.GrilleSetToX,      SpeechRecogniser.PIT_STOP },
            {PME.WedgePlusMinusX,   SpeechRecogniser.PIT_STOP },
            {PME.WedgeSetToX,       SpeechRecogniser.PIT_STOP },
            {PME.TrackBarPlusMinusX, SpeechRecogniser.PIT_STOP },
            {PME.TrackBarSetToX,    SpeechRecogniser.PIT_STOP },
            {PME.RubberLF,          SpeechRecogniser.PIT_STOP },
            {PME.RubberRF,          SpeechRecogniser.PIT_STOP },
            {PME.RubberLR,          SpeechRecogniser.PIT_STOP },
            {PME.RubberRR,          SpeechRecogniser.PIT_STOP },
            {PME.FenderL,           SpeechRecogniser.PIT_STOP },
            {PME.FenderR,           SpeechRecogniser.PIT_STOP },
            {PME.FlipUpL,           SpeechRecogniser.PIT_STOP },
            {PME.FlipUpR,           SpeechRecogniser.PIT_STOP },

            {PME.Tearoff,           SpeechRecogniser.PIT_STOP_TEAROFF },    // iRacing
            {PME.TearOffNone,       SpeechRecogniser.PIT_STOP_CLEAR_WIND_SCREEN },
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
                if (SpeechRecogniser.ResultContains(voiceMessage, cmd.Value))
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
