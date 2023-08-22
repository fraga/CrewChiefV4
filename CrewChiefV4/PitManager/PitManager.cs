using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A set of events that the driver or crew chief want
/// * Input from voice, button or CC(e.g.fuel calculation or Strategy Manager)
/// * Execution via game-specific functions
/// * Each has speech output.If the game doesn’t have the function then say so,
///   or if it’s not available (e.g.aero on non-aero car)
/// </summary>
namespace CrewChiefV4.PitManager
{
    using PME = PitManagerEvent;  // shorthand
    using SRE = SpeechRecogniser;

    /// <summary>
    /// All the events that Pit Manager handles
    /// Not all events can be handled by all games or all cars
    /// </summary>

    public enum PitManagerEvent
    {
        Initialise,
        Teardown,
        PrepareToUseMenu,

        TyreChangeAll,
        TyreChangeNone,
        TyreChangeFront,
        TyreChangeRear,
        TyreChangeLeft,
        TyreChangeRight,
        TyreChangeLF,
        TyreChangeRF,
        TyreChangeLR,
        TyreChangeRR,

        TyrePressure,
        TyrePressureFront,       // ACC
        TyrePressureRear,        // ACC
        TyrePressureLF,
        TyrePressureRF,
        TyrePressureLR,
        TyrePressureRR,

        TyreCompoundDry,
        TyreCompoundHard,
        TyreCompoundMedium,
        TyreCompoundSoft,
        TyreCompoundSupersoft,
        TyreCompoundUltrasoft,
        TyreCompoundHypersoft,
        TyreCompoundIntermediate,
        TyreCompoundWet,
        TyreCompoundMonsoon,
        TyreCompoundOption,
        TyreCompoundPrime,
        TyreCompoundAlternate,
        TyreCompoundNext,

        FuelAddXlitres,
        FuelFillToXlitres,
        FuelFillToEnd,
        Refuel,
        FuelNone,

        RepairAll,              // rF2
        RepairNone,
        RepairFast,             // iRacing
        RepairAllAero,          // R3E
        RepairAeroNone,
        RepairFrontAero,
        RepairRearAero,
        RepairSuspension,
        RepairSuspensionNone,
        RepairBody,             // rF2

        PenaltyServe,
        PenaltyServeNone,

        ClearAll,

        AeroFrontPlusMinusX,
        AeroRearPlusMinusX,
        AeroFrontSetToX,
        AeroRearSetToX,

        GrillePlusMinusX,       // rF2
        GrilleSetToX,
        WedgePlusMinusX,        // TBD: guessing actions for these
        WedgeSetToX,
        TrackBarPlusMinusX,
        TrackBarSetToX,
        RubberLF,
        RubberRF,
        RubberLR,
        RubberRR,
        FenderL,
        FenderR,
        FlipUpL,
        FlipUpR,

        Tearoff,                // iRacing
        TearOffNone,
        HowManyIncidentPoints,
        WhatsTheIncidentLimit,
        WhatMyIrating,
        WhatsMyLicenseClass,
        WhatsTheSof,

        WhatsThePitActions,     // R3E

        DisplaySectors,         // rF2 Multi-Function Display pages
        DisplayPitMenu,
        DisplayTyres,
        DisplayTemps,
        DisplayRaceInfo,
        DisplayStandings,
        DisplayPenalties,
        DisplayNext
    }

    public class PitManager
    {
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This gets very messy but you don't need to worry about it
        // It allows each game to set up a dictionary PM_event_dict containing
        // PitManagerEvent, actionHandler fn, responseHandler fn, SpeechRecognitionPhrases
        // for all the events handled by the game.
        // There's probably a neater way of doing it but it's beyond my C# skills.
        public struct PitManagerEventTableEntry
        {
            public delegate bool PitManagerEventAction_Delegate(string voiceMessage);

            public delegate bool PitManagerEventResponse_Delegate();

            public PitManagerEventAction_Delegate PitManagerEventAction;
            public PitManagerEventResponse_Delegate PitManagerEventResponse;
            public String[] SpeechRecognitionPhrases;
        }

        internal class GamePitManagerDict : Dictionary<PME, PitManagerEventTableEntry>
        {
        }

        //-------------------------------------------------------------------------

        // Dictionary of games and their event dicts
        private readonly Dictionary<GameEnum, GamePitManagerDict>
            games_dict = new Dictionary<GameEnum, GamePitManagerDict>
        {
          {GameEnum.RF2_64BIT,  PitManagerEventHandlers_RF2.PM_event_dict_RF2},
        };

        private static readonly Object myLock = new Object();

        private static Thread executeThread;

        /// <summary>
        /// Used to initialise PM event handler the first time a command is
        /// issued in a session
        /// </summary>
        private static bool initialised;

        private static GamePitManagerDict PM_event_dict;
        private static bool unitTest;
        public PitManager()
        {
            try
            {   // Use the event dict for the current game
                if (games_dict.TryGetValue(CrewChief.gameDefinition.gameEnum, out GamePitManagerDict value))
                {
                    PM_event_dict = value;
                }
                else
                {
                    //TBD: default handler "Pit menu control is not available in this game"
                    PM_event_dict = null;
                }
            }
            catch
            {   // Running in Unit test
                PM_event_dict = games_dict[GameEnum.RF2_64BIT];
                unitTest = true;
            }
        }
        public void EventHandlerInit()
        {
            initialised = false;
        }

        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The Event handler
        /// The event may come from a voice command, a command macro or
        /// internally (e.g."Fill to end" Property)
        /// </summary>
        /// <param name="ev">PitManagerEvent</param>
        /// <param name="voiceMessage"> The voice command</param>
        /// <returns>
        /// true if event was handled
        /// </returns>
        public bool EventHandler(PitManagerEvent ev, string voiceMessage)
        {
            bool result = false;
            if (PM_event_dict == null)
            {
                return result;
            }    

            if (!unitTest)
            {
                lock (myLock)
                {
                    // run this in a new thread as it may take a while to complete its work
                    ThreadManager.UnregisterTemporaryThread(executeThread);
                    executeThread = new Thread(() =>
                    {
                        if (PM_event_dict.ContainsKey(ev))
                        {
                            try
                            {
                                if (!initialised && ev != PME.Teardown)
                                {
                                    PM_event_dict[PitManagerEvent.Initialise].PitManagerEventAction.Invoke("");
                                    initialised = true;
                                }
                                if (initialised)
                                {
                                    PM_event_dict[PitManagerEvent.PrepareToUseMenu].PitManagerEventAction.Invoke("");
                                    result = PM_event_dict[ev].PitManagerEventAction.Invoke(voiceMessage);
                                    if (result)
                                    {
                                        result = PM_event_dict[ev].PitManagerEventResponse.Invoke();
                                        if (PitManagerVoiceCmds.tyresAutoCleared)
                                        {   // Pit menu tyre change cleared at start of race
                                            PitManagerVoiceCmds.tyresAutoCleared = false;
                                            // Restore the MFD
                                            PM_event_dict[PME.DisplayRaceInfo].PitManagerEventAction.Invoke(voiceMessage);
                                        }
                                    }
                                    else
                                    {
                                        //TBD: default handler "Couldn't do event for this vehicle"
                                        // e.g. change aero on non-aero car, option not in menu,
                                        // fuel a car
                                        PitManagerResponseHandlers.PMrh_CantDoThat();
                                        Log.Commentary($"Pit Manager couldn't do {ev} for this vehicle");
                                    }
                                }
                                // else TearDown when not started up
                            }
                            catch (Exception e)
                            {
                                Log.Exception(e, "Pit Manager event error");
                            }
                        }
                        else
                        {
                            //TBD: default handler "Not available in this game"
                            PitManagerResponseHandlers.PMrh_CantDoThat();
                            Log.Commentary($"Pit Manager couldn't do {ev} in this game");
                            //Alternatively event dicts for all games have all events
                            //and the response handler does the warning.
                        }
                    });
                    executeThread.Name = "PitManager.executeThread";
                    ThreadManager.RegisterTemporaryThread(executeThread);
                    executeThread.Start();
                }
            }
            else
            {
                if (PM_event_dict.ContainsKey(ev))
                {
                    if (!initialised)
                    {
                        PM_event_dict[PitManagerEvent.Initialise].PitManagerEventAction.Invoke("");
                        initialised = true;
                    }
                    PM_event_dict[PitManagerEvent.PrepareToUseMenu].PitManagerEventAction.Invoke("");
                    result = PM_event_dict[ev].PitManagerEventAction.Invoke(voiceMessage);
                    if (result)
                    {
                        result = PM_event_dict[ev].PitManagerEventResponse.Invoke();
                    }
                    else
                    {
                        //TBD: default handler "Couldn't do event for this vehicle"
                        // e.g. change aero on non-aero car, option not in menu
                    }
                }
                else
                {
                    //TBD: default handler "Not available in this game"
                    PitManagerResponseHandlers.PMrh_CantDoThat();
                    //Alternatively event dicts for all games have all events
                    //and the response handler does the warning.
                }
            }
            return result;
        }

        public static Tuple<PME, PitManagerEventTableEntry> IsPitManagerCommand(String voiceMessage)
        {
            if (PM_event_dict != null)
            {
                // Check the Pit commands
                foreach (var cmd in PM_event_dict)
                {
                    if (cmd.Value.SpeechRecognitionPhrases != null &&
                        SRE.ResultContains(voiceMessage, cmd.Value.SpeechRecognitionPhrases))
                    {
                        return Tuple.Create(cmd.Key, cmd.Value);
                    }
                }
            }
            return null;
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // More messy stuff to set up the dictionary
        // Again, there's probably a neater way of doing it but it's beyond my C# skills.
        /// <summary>
        /// Helper fn to create GamePitManagerDict entry
        /// - "PM_event" : (actionHandler, responseHandler)
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="actionHandler"></param>
        /// <param name="responseHandler"></param>
        /// <param name="SpeechRecognitionPhrases"></param>
        /// <returns></returns>
        public static PitManagerEventTableEntry _PMet(PitManagerEventTableEntry existing,
              PitManagerEventTableEntry.PitManagerEventAction_Delegate actionHandler,
              PitManagerEventTableEntry.PitManagerEventResponse_Delegate responseHandler,
              String[] SpeechRecognitionPhrases)
        {
            existing.PitManagerEventAction = actionHandler;
            existing.PitManagerEventResponse = responseHandler;
            existing.SpeechRecognitionPhrases = SpeechRecognitionPhrases;
            return existing;
        }

        /// <summary>
        /// Shorthand
        /// </summary>
        internal static PitManagerEventTableEntry PMeh = new PitManagerEventTableEntry();
    }
}