using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;

namespace CrewChiefV4.PitManager
{
    public class PitManagerVoiceCmds : AbstractEvent
    {
        #region Private Fields

        private static readonly PitManager pmh = new PitManager();

        private static float fuelCapacity = -1;
        private static float currentFuel = -1;
        // In the car (in real time)
        private static bool inCar;

        private static Boolean rf2AutoFuelToEnd = UserSettings.GetUserSettings().getBoolean("rf2_enable_auto_fuel_to_end_of_race");

        public static Boolean tyresAutoCleared;

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
            var cmd = PitManager.IsPitManagerCommand(voiceMessage);
            if (cmd != null)
            {
                if (inCar)
                {
                    Log.Debug("Pit Manager voice command " + cmd.Item2.SpeechRecognitionPhrases[0]);
                    pmh.EventHandler(cmd.Item1, voiceMessage);
                }
                else
                {
                    PitManagerResponseHandlers.PMrh_CantDoThat(); // tbd
                    Log.Commentary("Not in car received Pit Manager voice command " + cmd.Item2.SpeechRecognitionPhrases[0]);
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
            PitManagerEventHandlers_RF2.FuelVoiceCommand.Given = false;
            pmh.EventHandlerInit();
        }

        /// <summary>
        /// Cleardown the event subtype
        /// </summary>
        public override void teardownState()
        {
            fuelCapacity = -1;
            currentFuel = -1;
            pmh.EventHandler(PitManagerEvent.Teardown, "");
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

        public static void startOfRace()
        {
            CrewChief.getEvent("PitManagerVoiceCmds").respond("pitstop clear tyres");
            tyresAutoCleared = true;
    }
    #endregion Public Methods

    #region Protected Methods

    /// <summary>
    /// This is called on each 'tick' - the event subtype should
    /// place its logic in here including calls to audioPlayer.queueClip
    /// </summary>
    /// <param name="previousGameState"></param>
    /// <param name="currentGameState"></param>
    protected override void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            inCar = currentGameState.inCar;
            if (!previousGameState.inCar && currentGameState.inCar)
            {
                pmh.EventHandlerInit();
                Log.Debug($"Car name {currentGameState.carName}");
                Log.Debug($"Track name {currentGameState.trackName}");
            }

            fuelCapacity = currentGameState.FuelData.FuelCapacity;
            currentFuel = currentGameState.FuelData.FuelLeft;
            if (inCar
#pragma warning disable S2589
                && (previousGameState != null
#pragma warning restore S2589
                    && currentGameState.SessionData.SessionType == SessionType.Race
                    && currentGameState.SessionData.SessionRunningTime > 15
                    && !previousGameState.PitData.IsInGarage
                    && !currentGameState.PitData.JumpedToPits))
            {
                if (!previousGameState.PitData.InPitlane
                    && currentGameState.PitData.InPitlane)
                {
                    Log.Commentary("Entered pit lane");
                    if (rf2AutoFuelToEnd)
                    {
                        if (PitManagerEventHandlers_RF2.FuelVoiceCommand.Given)
                        {
                            Log.Warning("'rF2 auto refuelling' ignored as a pitstop fuel voice command has been given");
                            PitManagerEventHandlers_RF2.FuelVoiceCommand.Given = false;  // auto refuel next pitstop
                        }
                        else
                        {

                            if (CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT)
                            {
                                var litres = PitFuelling.fuelToEnd(fuelCapacity, currentFuel);
                                if (litres >= 0)
                                {
                                    PitManagerEventHandlers_RF2.SetFuel(litres);
                                }
                                // else couldn't calculate fuel required
                            }
                        }
                    }
                }
                else if (previousGameState.PitData.InPitlane
                    && !currentGameState.PitData.InPitlane)
                {
                    Log.Commentary("Left pit lane");
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
        public static int processNumber(string _voiceMessage)
        {
            float amount = NumberProcessing.SpokenNumberParser.Parse(_voiceMessage);

            if (amount < 1)
            {
                amount = 0;
                crewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
            }
            return (int)amount;
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
        public static int processLitresGallons(int amount, string _voiceMessage)
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

        private static int convertGallonsToLitres(int gallons)
        {
            return (int)Math.Ceiling(gallons * litresPerGallon);
        }

        #endregion Private Methods
    }

    static class PitFuelling
    {
        private static readonly CrewChief crewChief = MainWindow.instance.crewChief;

        private static float addAdditionalFuelLaps = UserSettings.GetUserSettings().getFloat("add_additional_fuel");

        private static Boolean baseCalculationsOnMaxConsumption = UserSettings.GetUserSettings().getBoolean("prefer_max_consumption_in_fuel_calculations");

        #region Public Methods
        /// <summary>
        /// Calculate how much fuel is needed to get to the end of the race
        /// </summary>
        /// <param name="fuelCapacity"></param>
        /// <param name="currentFuel"></param>
        /// <returns>
        /// +ve: Litres needed
        /// 0:   Enough fuel in car already
        /// -ve: Couldn't calculate fuel required
        /// </returns>
        public static int fuelToEnd(float fuelCapacity, float currentFuel)
        {
            int roundedLitresNeeded = -1;
            Fuel fuelEvent = (Fuel)CrewChief.getEvent("Fuel");
            float additionaLitresNeeded = fuelEvent.getAdditionalFuelToEndOfRace(true);

            Log.Fuel($"Laps of extra fuel: {addAdditionalFuelLaps}");
            Log.Fuel($"Fuel calculations based on max fuel consumption: {baseCalculationsOnMaxConsumption}");

            if (additionaLitresNeeded == float.MaxValue)
            {
                crewChief.audioPlayer.playMessage(new QueuedMessage(AudioPlayer.folderNoData, 0));
                Log.Fuel("Pit: couldn't calculate fuel needed");
                roundedLitresNeeded = -1;
            }
            else if (additionaLitresNeeded <= 0)
            {
                crewChief.audioPlayer.playMessage(new QueuedMessage(Fuel.folderPlentyOfFuel, 0));
                Log.Fuel("Pit: no fuel needed");
                roundedLitresNeeded = 0;
            }
            else if (additionaLitresNeeded > 0)
            {
                roundedLitresNeeded = (int)Math.Ceiling(additionaLitresNeeded);
                Log.Fuel($"Pit: auto refuel to the end of the race, need to add {roundedLitresNeeded} litres of fuel");
                if (roundedLitresNeeded > fuelCapacity - currentFuel)
                {
                    // if we have a known fuel capacity and this is less than the calculated amount of fuel we need, warn about it.
                    crewChief.audioPlayer.playMessage(new QueuedMessage(Fuel.folderWillNeedToStopAgain, 0, secondsDelay: 4));
                    Log.Fuel($"Pit: need {roundedLitresNeeded + currentFuel} but tank only holds {fuelCapacity} litres");
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