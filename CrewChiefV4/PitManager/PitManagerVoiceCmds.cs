using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Xml;

namespace CrewChiefV4.PitManager
{
    using PME = PitManagerEvent;  // shorthand
    using SRE = SpeechRecogniser;

    public class PitManagerVoiceCmds : AbstractEvent
    {
        #region Private Fields

        private static readonly PitManager pmh = new PitManager();

        private static readonly Dictionary<PitManagerEvent, String[]> voiceCmds =
            new Dictionary<PitManagerEvent, String[]>
        {
            {PME.TyreChangeAll,           SRE.PIT_STOP_CHANGE_ALL_TYRES },
            {PME.TyreChangeNone,          SRE.PIT_STOP_CLEAR_TYRES },
            {PME.TyreChangeFront,         SRE.PIT_STOP_CHANGE_FRONT_TYRES },
            {PME.TyreChangeRear,          SRE.PIT_STOP_CHANGE_REAR_TYRES },
            {PME.TyreChangeLeft,          SRE.PIT_STOP_CHANGE_LEFT_SIDE_TYRES },
            {PME.TyreChangeRight,         SRE.PIT_STOP_CHANGE_RIGHT_SIDE_TYRES},
            {PME.TyreChangeLF,            SRE.PIT_STOP_CHANGE_FRONT_LEFT_TYRE },
            {PME.TyreChangeRF,            SRE.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE },
            {PME.TyreChangeLR,            SRE.PIT_STOP_CHANGE_REAR_LEFT_TYRE },
            {PME.TyreChangeRR,            SRE.PIT_STOP_CHANGE_REAR_RIGHT_TYRE },

            {PME.TyrePressure,            SRE.PIT_STOP_CHANGE_TYRE_PRESSURE },
            {PME.TyrePressureLF,          SRE.PIT_STOP_CHANGE_FRONT_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRF,          SRE.PIT_STOP_CHANGE_FRONT_RIGHT_TYRE_PRESSURE },
            {PME.TyrePressureLR,          SRE.PIT_STOP_CHANGE_REAR_LEFT_TYRE_PRESSURE },
            {PME.TyrePressureRR,          SRE.PIT_STOP_CHANGE_REAR_RIGHT_TYRE_PRESSURE },

            {PME.TyreCompoundHard,        SRE.PIT_STOP_HARD_TYRES },
            {PME.TyreCompoundMedium,      SRE.PIT_STOP_MEDIUM_TYRES },
            {PME.TyreCompoundSoft,        SRE.PIT_STOP_SOFT_TYRES },
            {PME.TyreCompoundSupersoft,   SRE.PIT_STOP_SUPERSOFT_TYRES },
            {PME.TyreCompoundUltrasoft,   SRE.PIT_STOP_ULTRASOFT_TYRES },
            {PME.TyreCompoundHypersoft,   SRE.PIT_STOP_HYPERSOFT_TYRES },
            {PME.TyreCompoundIntermediate,SRE.PIT_STOP_INTERMEDIATE_TYRES },
            {PME.TyreCompoundWet,         SRE.PIT_STOP_WET_TYRES },
            {PME.TyreCompoundMonsoon,     SRE.PIT_STOP_MONSOON_TYRES },
            {PME.TyreCompoundOption,      SRE.PIT_STOP_OPTION_TYRES },
            {PME.TyreCompoundPrime,       SRE.PIT_STOP_PRIME_TYRES },
            {PME.TyreCompoundAlternate,   SRE.PIT_STOP_ALTERNATE_TYRES },
            {PME.TyreCompoundNext,        SRE.PIT_STOP_NEXT_TYRE_COMPOUND },

            {PME.FuelAddXlitres,          SRE.PIT_STOP_ADD },
            {PME.FuelFillToXlitres,       SRE.PIT_STOP_FILL_TO },
            {PME.FuelFillToEnd,           SRE.PIT_STOP_FUEL_TO_THE_END },
            {PME.FuelNone,                SRE.PIT_STOP_DONT_REFUEL },
            //{PME.FuelNone,              SRE.PIT_STOP_CLEAR_FUEL },

            {PME.RepairAll,               SRE.PIT_STOP_FIX_ALL },          // rF2
            {PME.RepairNone,              SRE.PIT_STOP_FIX_NONE },         // rF2
            {PME.RepairFast,              SRE.PIT_STOP_FAST_REPAIR },        // iRacing
            {PME.RepairAllAero,           SRE.PIT_STOP_FIX_ALL_AERO },       // R3E
            {PME.RepairFrontAero,         SRE.PIT_STOP_FIX_FRONT_AERO },
            {PME.RepairRearAero,          SRE.PIT_STOP_FIX_REAR_AERO },
            {PME.RepairSuspension,        SRE.PIT_STOP_FIX_SUSPENSION },
            {PME.RepairSuspensionNone,    SRE.PIT_STOP_DONT_FIX_SUSPENSION },
            {PME.RepairBody,              SRE.PIT_STOP_FIX_BODY },         // rF2

            {PME.PenaltyServe,            SRE.PIT_STOP_SERVE_PENALTY },
            {PME.PenaltyServeNone,        SRE.PIT_STOP_DONT_SERVE_PENALTY },

            {PME.ClearAll,                SRE.PIT_STOP_CLEAR_ALL },

            // tbd {PME.AeroFrontPlusMinusX, SRE.PIT_STOP },     // tbd: would require added speech handling
            // tbd {PME.AeroRearPlusMinusX,  SRE.PIT_STOP },
            // tbd {PME.AeroFrontSetToX,  SRE.PIT_STOP },
            // tbd {PME.AeroRearSetToX,   SRE.PIT_STOP },

            // tbd {PME.GrillePlusMinusX, SRE.PIT_STOP },        // tbd: rF2
            // tbd {PME.GrilleSetToX,     SRE.PIT_STOP },
            // tbd {PME.WedgePlusMinusX,  SRE.PIT_STOP },
            // tbd {PME.WedgeSetToX,      SRE.PIT_STOP },
            // tbd {PME.TrackBarPlusMinusX,  SRE.PIT_STOP },
            // tbd {PME.TrackBarSetToX,   SRE.PIT_STOP },
            // tbd {PME.RubberLF,         SRE.PIT_STOP },
            // tbd {PME.RubberRF,         SRE.PIT_STOP },
            // tbd {PME.RubberLR,         SRE.PIT_STOP },
            // tbd {PME.RubberRR,         SRE.PIT_STOP },
            // tbd {PME.FenderL,          SRE.PIT_STOP },
            // tbd {PME.FenderR,          SRE.PIT_STOP },
            // tbd {PME.FlipUpL,          SRE.PIT_STOP },
            // tbd {PME.FlipUpR,          SRE.PIT_STOP },

            {PME.Tearoff,                 SRE.PIT_STOP_TEAROFF },    // iRacing
            {PME.TearOffNone,             SRE.PIT_STOP_CLEAR_WIND_SCREEN },
            };

        private static float fuelCapacity = -1;
        private static float currentFuel = -1;
        // In the car (in real time)
        private static bool inCar = false;

        #endregion Private Fields

        #region Public Constructors

        public PitManagerVoiceCmds(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
            fuelCapacity = -1;
            currentFuel = -1;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// I think this is a list of the sessions when Pit Manager should be active.
        /// Player may want to use it in practice and qually
        /// </summary>
        public override List<SessionType> applicableSessionTypes
        {
            get
            {
                return new List<SessionType> {
                    SessionType.Practice,
                    SessionType.Qualify,
                    SessionType.Race,
                    SessionType.LonePractice };
            }
        }
        /// <summary>
        /// I think this is a list of the subset of phases of sessions when
        /// Pit Manager should be active.
        /// </summary>
        public override List<SessionPhase> applicableSessionPhases
        {
            get
            {
                return new List<SessionPhase> {
                    SessionPhase.Garage,
                    SessionPhase.Formation,
                    SessionPhase.Green,
                    SessionPhase.Countdown,
                    SessionPhase.FullCourseYellow };
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Respond to a voice command
        /// </summary>
        /// <param name="voiceMessage"></param>
        public override void respond(String voiceMessage)
        {
            // Check the Pit commands
            foreach (var cmd in voiceCmds)
            {
                if (SRE.ResultContains(voiceMessage, cmd.Value))
                {
                    if (inCar)
                    {
                        Log.Debug("Pit Manager voice command " + cmd.Value[0]);
                        pmh.EventHandler(cmd.Key, voiceMessage);
                        break;
                    }
                    else
                    {
                        PitManagerResponseHandlers.PMrh_CantDoThat(); // tbd
                        Log.Commentary("Not in car received Pit Manager voice command " + cmd.Value[0]);
                    }
                }
            }
        }

        /// <summary>
        /// reinitialise any state held by the event subtype
        /// </summary>
        public override void clearState()
        {
            fuelCapacity = -1;
            currentFuel = -1;
            pmh.EventHandlerInit();
        }

        /// <summary>
        /// Cleardown the event subtype
        /// </summary>
        public override void teardownState()
        {
            fuelCapacity = -1;
            currentFuel = -1;
            pmh.EventHandler(PME.Teardown, "");
        }

        public static float getFuelCapacity()
        {
            return fuelCapacity;
        }
        public static float getCurrentFuel()
        {
            return currentFuel;
        }

        public static bool isOnTrack()
        {
            return inCar;
        }

        #endregion Public Methods

        #region Protected Methods

        /// <summary>
        /// This is called on each 'tick' - the event subtype should
        /// place its logic in here including calls to audioPlayer.queueClip
        /// </summary>
        /// <param name="previousGameState"></param>
        /// <param name="currentGameState"></param>
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            Boolean autoFuelToEnd = UserSettings.GetUserSettings().getBoolean("rf2_enable_auto_fuel_to_end_of_race");

            inCar = currentGameState.inCar;
            if (!previousGameState.inCar && currentGameState.inCar)
            {
                pmh.EventHandlerInit();
                Log.Debug($"Car name {currentGameState.carName}");
                Log.Debug($"Track name {currentGameState.trackName}");
            }

            fuelCapacity = currentGameState.FuelData.FuelCapacity;
            currentFuel = currentGameState.FuelData.FuelLeft;
            if (inCar && autoFuelToEnd
                && (previousGameState != null
                    && (!previousGameState.PitData.InPitlane
                    && currentGameState.PitData.InPitlane)
                    && currentGameState.SessionData.SessionType == SessionType.Race
                    && currentGameState.SessionData.SessionRunningTime > 15
                    && !previousGameState.PitData.IsInGarage
                    && !currentGameState.PitData.JumpedToPits))
            {
                var litres = PitFuelling.fuelToEnd(fuelCapacity, currentFuel);
                if (CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT)
                {
                    PitManagerEventHandlers_RF2.rF2SetFuel(litres);
                }
            }
        }

        #endregion Protected Methods
    }

    /// <summary>
    /// Utility class to handle pit number commands
    /// </summary>
    internal static class PitNumberHandling
    {
        #region Private Fields

        private const float litresPerGallon = 3.78541f;
        private static readonly CrewChief crewChief = MainWindow.instance.crewChief;

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// Parse a non-zero number from the voice command
        /// </summary>
        /// <param name="_voiceMessage"></param>
        /// <returns>
        /// The number, 0 if the number couldn't be parsed
        /// </returns>
        static public int processNumber(string _voiceMessage)
        {
            int amount = 0;

            foreach (KeyValuePair<String[], int> entry in SpeechRecogniser.numberToNumber)
            {
                foreach (String numberStr in entry.Key)
                {
                    if (_voiceMessage.Contains(" " + numberStr))
                    {
                        amount = entry.Value;
                        Log.Debug("processed number " + amount.ToString());
                        break;
                    }
                }
            }
            if (amount == 0)
            {
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
            }
            return amount;
        }

        /// <summary>
        /// Parse the amount in litres by looking at the remainder of the voice
        /// command. Report the amount of fuel and the units
        /// </summary>
        /// <param name="amount"></param>
        /// The number
        /// <param name="_voiceMessage"></param>
        /// <returns>
        /// The number of litres
        /// </returns>
        static public int processLitresGallons(int amount, string _voiceMessage)
        {
            bool litres = true;
            if (SpeechRecogniser.ResultContains(_voiceMessage, SpeechRecogniser.LITERS))
            {
                litres = true;
            }
            else if (SpeechRecogniser.ResultContains(_voiceMessage, SpeechRecogniser.GALLONS))
            {
                litres = false;
            }
            else
            {
                Fuel fuelEvent = (Fuel)CrewChief.getEvent("Fuel");
                Log.Commentary("Got fuel request with no unit, assuming " + (fuelEvent.fuelReportsInGallon ? " gallons" : "litres"));
                if (fuelEvent.fuelReportsInGallon)
                {
                    litres = false;
                }
            }

            if (litres)
            {
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(
                    "iracing_add_fuel", 0,  // tbd: rename
                    messageFragments: PitManagerVoiceCmds.MessageContents(
                        AudioPlayer.folderAcknowlegeOK,
                        amount,
                        amount == 1 ? Fuel.folderLitre : Fuel.folderLitres)
                    ));
            }
            else
            {
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(
                    "iracing_add_fuel", 0,
                    messageFragments: PitManagerVoiceCmds.MessageContents(
                        AudioPlayer.folderAcknowlegeOK,
                        amount,
                        amount == 1 ? Fuel.folderGallon : Fuel.folderGallons)
                    ));
                amount = convertGallonsToLitres(amount);
            }
            return amount;
        }

        #endregion Public Methods

        #region Private Methods

        static private int convertGallonsToLitres(int gallons)
        {
            return (int)Math.Ceiling(gallons * litresPerGallon);
        }

        #endregion Private Methods
    }

    static class PitFuelling
    {
        private static readonly CrewChief crewChief = MainWindow.instance.crewChief;

        #region Public Methods
        static public int fuelToEnd(float fuelCapacity, float currentFuel)
        {
            int roundedLitresNeeded = -1;
            Fuel fuelEvent = (Fuel)CrewChief.getEvent("Fuel");
            float additionaLitresNeeded = fuelEvent.getLitresToEndOfRace(true);

            if (additionaLitresNeeded == float.MaxValue)
            {
                crewChief.audioPlayer.playMessage(new QueuedMessage(AudioPlayer.folderNoData, 0));
                roundedLitresNeeded = 0;
            }
            else if (additionaLitresNeeded <= 0)
            {
                crewChief.audioPlayer.playMessage(new QueuedMessage(Fuel.folderPlentyOfFuel, 0));
                roundedLitresNeeded = 0;
            }
            else if (additionaLitresNeeded > 0)
            {
                roundedLitresNeeded = (int)Math.Ceiling(additionaLitresNeeded);
                Log.Commentary("Auto refuel to the end of the race, adding " + roundedLitresNeeded + " litres of fuel");
                if (roundedLitresNeeded > fuelCapacity - currentFuel)
                {
                    // if we have a known fuel capacity and this is less than the calculated amount of fuel we need, warn about it.
                    crewChief.audioPlayer.playMessage(new QueuedMessage(Fuel.folderWillNeedToStopAgain, 0, secondsDelay: 4));
                }
                else
                {
                    crewChief.audioPlayer.playMessage(new QueuedMessage(AudioPlayer.folderFuelToEnd, 0));
                }
            }
            return roundedLitresNeeded;
        }

        #endregion Public Methods
    }
}