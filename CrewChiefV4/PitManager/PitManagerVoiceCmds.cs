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
        private const float litresPerGallon = 3.78541f;
        private int amount = 0;

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
            {PME.TyreCompoundWet,   SRE.PIT_STOP_WET_TYRES },        // tbd:
            {PME.TyreCompoundOption, SRE.PIT_STOP_OPTION_TYRES },
            {PME.TyreCompoundPrime, SRE.PIT_STOP_PRIME_TYRES },
            {PME.TyreCompoundAlternate, SRE.PIT_STOP_ALTERNATE_TYRES },
            {PME.TyreCompoundNext,  SRE.PIT_STOP_NEXT_TYRE_COMPOUND },

            {PME.FuelAddXlitres,    SRE.PIT_STOP_ADD },
            //{PME.FuelFillToXlitres, SRE.PIT_STOP },               // tbd: would require added speech handling
            {PME.FuelFillToEnd,     SRE.PIT_STOP_FUEL_TO_THE_END },
            {PME.FuelNone,          SRE.PIT_STOP_DONT_REFUEL },

            // tbd {PME.RepairAll,         SRE.PIT_STOP },
            {PME.RepairNone,        SRE.PIT_STOP_CLEAR_ALL },
            {PME.RepairFast,        SRE.PIT_STOP_FAST_REPAIR },        // iRacing
            {PME.RepairAllAero,     SRE.PIT_STOP_FIX_ALL_AERO },       // R3E
            {PME.RepairFrontAero,   SRE.PIT_STOP_FIX_FRONT_AERO },
            {PME.RepairRearAero,    SRE.PIT_STOP_FIX_REAR_AERO },
            {PME.RepairSuspension,  SRE.PIT_STOP_FIX_SUSPENSION },
            {PME.RepairSuspensionNone, SRE.PIT_STOP_DONT_FIX_SUSPENSION },
            {PME.RepairBody,        SRE.PIT_STOP_FIX_BODY },         // rF2

            {PME.PenaltyServe,      SRE.PIT_STOP_SERVE_PENALTY },
            {PME.PenaltyServeNone,  SRE.PIT_STOP_DONT_SERVE_PENALTY },

            // tbd {PME.AeroFrontPlusMinusX, SRE.PIT_STOP },     // tbd: would require added speech handling
            // tbd {PME.AeroRearPlusMinusX, SRE.PIT_STOP },
            // tbd {PME.AeroFrontSetToX,   SRE.PIT_STOP },
            // tbd {PME.AeroRearSetToX,    SRE.PIT_STOP },

            // tbd {PME.GrillePlusMinusX,  SRE.PIT_STOP },        // tbd: rF2
            // tbd {PME.GrilleSetToX,      SRE.PIT_STOP },
            // tbd {PME.WedgePlusMinusX,   SRE.PIT_STOP },
            // tbd {PME.WedgeSetToX,       SRE.PIT_STOP },
            // tbd {PME.TrackBarPlusMinusX, SRE.PIT_STOP },
            // tbd {PME.TrackBarSetToX,    SRE.PIT_STOP },
            // tbd {PME.RubberLF,          SRE.PIT_STOP },
            // tbd {PME.RubberRF,          SRE.PIT_STOP },
            // tbd {PME.RubberLR,          SRE.PIT_STOP },
            // tbd {PME.RubberRR,          SRE.PIT_STOP },
            // tbd {PME.FenderL,           SRE.PIT_STOP },
            // tbd {PME.FenderR,           SRE.PIT_STOP },
            // tbd {PME.FlipUpL,           SRE.PIT_STOP },
            // tbd {PME.FlipUpR,           SRE.PIT_STOP },

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

        static private bool AddFuel(int amount)
        {
            if (CrewChief.Debugging)
                Console.WriteLine("Pit Manager add fuel voice command +" +
                    amount.ToString() + " litres");
            PitManagerEventHandlers_RF2.amountHandler(amount);
            pmh.EventHandler(PME.FuelAddXlitres);
            return false; // Couldn't do it?
        }

        public override void respond(String voiceMessage)
        {
            amount = 0;
            // Check for "Add X... first
            if (SRE.ResultContains(voiceMessage, SRE.PIT_STOP_ADD))
            {
                foreach (KeyValuePair<String[], int> entry in SpeechRecogniser.numberToNumber)
                {
                    foreach (String numberStr in entry.Key)
                    {
                        if (voiceMessage.Contains(" " + numberStr + " "))
                        {
                            amount = entry.Value;
                            if (CrewChief.Debugging)
                                Console.WriteLine("Pit stop add " + amount.ToString());
                            break;
                        }
                    }
                }
                if (amount == 0)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
                    return;
                }
                if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.LITERS))
                {
                    AddFuel(amount);
                    audioPlayer.playMessageImmediately(new QueuedMessage("iracing_add_fuel", 0,
                        messageFragments: MessageContents(AudioPlayer.folderAcknowlegeOK, amount, amount == 1 ? Fuel.folderLitre : Fuel.folderLitres)));
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.GALLONS))
                {
                    AddFuel(convertGallonsToLitres(amount));
                    audioPlayer.playMessageImmediately(new QueuedMessage("iracing_add_fuel", 0,
                        messageFragments: MessageContents(AudioPlayer.folderAcknowlegeOK, amount, amount == 1 ? Fuel.folderGallon : Fuel.folderGallons)));
                }
                else
                {
                    Fuel fuelEvent = (Fuel)CrewChief.getEvent("Fuel");
                    Console.WriteLine("Got fuel request with no unit, assuming " + (fuelEvent.fuelReportsInGallon ? " gallons" : "litres"));
                    if (!fuelEvent.fuelReportsInGallon)
                    {
                        AddFuel(amount);
                        audioPlayer.playMessageImmediately(new QueuedMessage("iracing_add_fuel", 0,
                            messageFragments: MessageContents(AudioPlayer.folderAcknowlegeOK, amount, amount == 1 ? Fuel.folderLitre : Fuel.folderLitres)));
                    }
                    else
                    {
                        AddFuel(convertGallonsToLitres(amount));
                        audioPlayer.playMessageImmediately(new QueuedMessage("iracing_add_fuel", 0,
                            messageFragments: MessageContents(AudioPlayer.folderAcknowlegeOK, amount, amount == 1 ? Fuel.folderGallon : Fuel.folderGallons)));
                    }
                }
            }
            else
            {
                // Check the Pit commands
                foreach (var cmd in voiceCmds)
                {
                    if (SRE.ResultContains(voiceMessage, cmd.Value))
                    {
                        if (CrewChief.Debugging)
                            Console.WriteLine("Pit Manager voice command " + cmd.Value[0]);
                        pmh.EventHandler(cmd.Key);
                        break;
                    }
                }
            }
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            Boolean autoFuelToEnd = UserSettings.GetUserSettings().getBoolean("iracing_enable_auto_fuel_to_end_of_race");

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
                        AddFuel(roundedLitresNeeded);
                        Console.WriteLine("Auto refuel to the end of the race, adding " + roundedLitresNeeded + " litres of fuel");
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

        private int convertGallonsToLitres(int gallons)
        {
            return (int)Math.Ceiling(gallons * litresPerGallon);
        }
    }
}
