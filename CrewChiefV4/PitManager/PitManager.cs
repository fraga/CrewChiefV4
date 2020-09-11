using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using CrewChiefV4.Events;
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
    using PMEHrF2 = PitManagerEventHandlers_RF2;
    using PMEHiR = PitManagerEventHandlers_iRacing;
    using PMER = PitManagerResponseHandlers;

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
        TyrePressureLF,
        TyrePressureRF,
        TyrePressureLR,
        TyrePressureRR,

        TyreCompoundHard,
        TyreCompoundMedium,
        TyreCompoundSoft,
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
        FuelNone,

        RepairAll,              // rF2
        RepairNone,
        RepairFast,             // iRacing
        RepairAllAero,          // R3E
        RepairFrontAero,
        RepairRearAero,
        RepairSuspension,
        RepairSuspensionNone,
        RepairBody,             // rF2

        PenaltyServe,
        PenaltyServeNone,

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
        TearOffNone
    }

    public class PitManager
    {
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This gets very messy but you don't need to worry about it
        // It allows each game to set up a dictionary PM_event_dict containing
        // PitManagerEvent, actionHandler fn, responseHandler fn
        // for all the events handled by the game.
        // There's probably a neater way of doing it but it's beyond my C# skills.
        public struct PitManagerEventTableEntry
        {
            public delegate bool PitManagerEventAction_Delegate(string voiceMessage);

            public delegate bool PitManagerEventResponse_Delegate();

            public PitManagerEventAction_Delegate PitManagerEventAction;
            public PitManagerEventResponse_Delegate PitManagerEventResponse;
        }

        private class GamePitManagerDict : Dictionary<PME, PitManagerEventTableEntry>
        {
        }

        //-------------------------------------------------------------------------

        // Dictionary of games and their event dicts
        private readonly Dictionary<CrewChiefV4.GameEnum, GamePitManagerDict>
            games_dict = new Dictionary<CrewChiefV4.GameEnum, GamePitManagerDict>
        {
          {GameEnum.RF2_64BIT,  PM_event_dict_RF2},
#if IRACING
          {GameEnum.IRACING,    PM_event_dict_iRacing }
#endif
        };

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
        private static Object myLock = new Object();

        private static Thread executeThread = null;

        /// <summary>
        /// Used to initialise PM event handler the first time a command is
        /// issued in a session
        /// </summary>
        private static bool initialised = false;

        public void EventHandlerInit()
        {
            initialised = false;
        }

        public bool EventHandler(PitManagerEvent ev, string voiceMessage)
        {
            bool result = false;
            bool unitTest = false;
            GamePitManagerDict PM_event_dict;
            try
            {   // Use the event dict for the current game
                if (games_dict.ContainsKey(CrewChief.gameDefinition.gameEnum))
                {
                    PM_event_dict = games_dict[CrewChief.gameDefinition.gameEnum];
                }
                else
                {
                    //TBD: default handler "Pit menu control is not available in this game"
                    return result;
                }
            }
            catch
            {   // Running in Unit test
                PM_event_dict = games_dict[GameEnum.RF2_64BIT];
                unitTest = true;
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
                                    Console.WriteLine($"Pit Manager couldn't do {ev} for this vehicle");
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Pit Manager event error " + e.ToString());
                            }
                        }
                        else
                        {
                            //TBD: default handler "Not available in this game"
                            PitManagerResponseHandlers.PMrh_CantDoThat();
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
        /// <returns></returns>
        static public PitManagerEventTableEntry _PMet(PitManagerEventTableEntry existing,
              PitManagerEventTableEntry.PitManagerEventAction_Delegate actionHandler,
              PitManagerEventTableEntry.PitManagerEventResponse_Delegate responseHandler)
        {
            existing.PitManagerEventAction = new PitManagerEventTableEntry.PitManagerEventAction_Delegate(actionHandler);
            existing.PitManagerEventResponse = new PitManagerEventTableEntry.PitManagerEventResponse_Delegate(responseHandler);
            return existing;
        }

        /// <summary>
        /// Shorthand
        /// </summary>
        static private PitManagerEventTableEntry _PMeh = new PitManagerEventTableEntry();

        //-------------------------------------------------------------------------

        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for rF2
        /// </summary>
        private static readonly GamePitManagerDict PM_event_dict_RF2 = new GamePitManagerDict
        {
            //  The event                                      the fn that implements it        the fn that handles speech
            //                                                 (changes the pit menu)           response and any other outcomes
            {PME.Initialise,              _PMet(_PMeh, PMEHrF2.PMrF2eh_initialise,         PMER.PMrh_NoResponse) },
            {PME.Teardown,                _PMet(_PMeh, PMEHrF2.PMrF2eh_teardown,           PMER.PMrh_NoResponse) },
            {PME.PrepareToUseMenu,        _PMet(_PMeh, PMEHrF2.PMrF2eh_prepareToUseMenu,   PMER.PMrh_NoResponse) },
            {PME.TyreChangeAll,           _PMet(_PMeh, PMEHrF2.PMrF2eh_changeAllTyres,     PMER.PMrh_ChangeAllTyres) },
            {PME.TyreChangeNone,          _PMet(_PMeh, PMEHrF2.PMrF2eh_changeNoTyres,      PMER.PMrh_ChangeNoTyres) },
            {PME.TyreChangeFront,         _PMet(_PMeh, PMEHrF2.PMrF2eh_changeFrontTyres,   PMER.PMrh_ChangeFrontTyres) },
            {PME.TyreChangeRear,          _PMet(_PMeh, PMEHrF2.PMrF2eh_changeRearTyres,    PMER.PMrh_ChangeRearTyres) },
            {PME.TyreChangeLeft,          _PMet(_PMeh, PMEHrF2.PMrF2eh_changeLeftTyres,    PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRight,         _PMet(_PMeh, PMEHrF2.PMrF2eh_changeRightTyres,   PMER.PMrh_Acknowledge) },
            {PME.TyreChangeLF,            _PMet(_PMeh, PMEHrF2.PMrF2eh_changeFLTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRF,            _PMet(_PMeh, PMEHrF2.PMrF2eh_changeFRTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeLR,            _PMet(_PMeh, PMEHrF2.PMrF2eh_changeRLTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRR,            _PMet(_PMeh, PMEHrF2.PMrF2eh_changeRRTyre,       PMER.PMrh_Acknowledge) },

            {PME.TyrePressureLF,          _PMet(_PMeh, PMEHrF2.PMrF2eh_changeFLpressure,   PMER.PMrh_Acknowledge) },
            {PME.TyrePressureRF,          _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            {PME.TyrePressureLR,          _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            {PME.TyrePressureRR,          _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },

            {PME.TyreCompoundHard,        _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundHard,   PMER.PMrh_TyreCompoundHard) },
            {PME.TyreCompoundMedium,      _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundMedium, PMER.PMrh_TyreCompoundMedium) },
            {PME.TyreCompoundSoft,        _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundSoft,   PMER.PMrh_TyreCompoundSoft) },
            {PME.TyreCompoundIntermediate,_PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundWet,         _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundMonsoon,     _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundOption,      _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundOption, PMER.PMrh_TyreCompoundOption) },
            {PME.TyreCompoundPrime,       _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundPrime,  PMER.PMrh_TyreCompoundPrime) },
            {PME.TyreCompoundAlternate,   _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundAlternate, PMER.PMrh_TyreCompoundAlternate) },
            {PME.TyreCompoundNext,        _PMet(_PMeh, PMEHrF2.PMrF2eh_TyreCompoundNext,   PMER.PMrh_TyreCompoundNext) },

            {PME.FuelAddXlitres,          _PMet(_PMeh, PMEHrF2.PMrF2eh_FuelAddXlitres,     PMER.PMrh_NoResponse) },
            {PME.FuelFillToXlitres,       _PMet(_PMeh, PMEHrF2.PMrF2eh_FuelToXlitres,      PMER.PMrh_Acknowledge) },
            {PME.FuelFillToEnd,           _PMet(_PMeh, PMEHrF2.PMrF2eh_FuelToEnd,          PMER.PMrh_fuelToEnd) },
            {PME.FuelNone,                _PMet(_PMeh, PMEHrF2.PMrF2eh_FuelNone,           PMER.PMrh_noFuel) },

            {PME.RepairAll,               _PMet(_PMeh, PMEHrF2.PMrF2eh_RepairAll,          PMER.PMrh_Acknowledge) },
            {PME.RepairNone,              _PMet(_PMeh, PMEHrF2.PMrF2eh_RepairNone,         PMER.PMrh_Acknowledge) },
            //{PME.RepairFast,            _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },     // iRacing
            //{PME.RepairAllAero,         _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },     // R3E
            //{PME.RepairFrontAero,       _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairRearAero,        _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairSuspension,      _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairSuspensionNone,  _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_Acknowledge) },
            {PME.RepairBody,              _PMet(_PMeh, PMEHrF2.PMrF2eh_RepairBody,         PMER.PMrh_Acknowledge) },

            {PME.PenaltyServe,            _PMet(_PMeh, PMEHrF2.PMrF2eh_PenaltyServe,       PMER.PMrh_Acknowledge) },
            {PME.PenaltyServeNone,        _PMet(_PMeh, PMEHrF2.PMrF2eh_PenaltyServeNone,   PMER.PMrh_Acknowledge) },

            {PME.AeroFrontPlusMinusX,     _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroRearPlusMinusX,      _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroFrontSetToX,         _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroRearSetToX,          _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },

            {PME.GrillePlusMinusX,        _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) }, // rF2
            {PME.GrilleSetToX,            _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.WedgePlusMinusX,         _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.WedgeSetToX,             _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.TrackBarPlusMinusX,      _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.TrackBarSetToX,          _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberLF,                _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberRF,                _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberLR,                _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberRR,                _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.FenderL,                 _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.FenderR,                 _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.FlipUpL,                 _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
            {PME.FlipUpR,                 _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },

            //{PME.Tearoff,               _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) }, // iRacing
            //{PME.TearOffNone,           _PMet(_PMeh, PMEHrF2.PMrF2eh_example,            PMER.PMrh_CantDoThat) },
        };

#if IRACING
        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for iRacing
        /// </summary>
        private static readonly GamePitManagerDict PM_event_dict_iRacing = new GamePitManagerDict
        {
            //  The event                                      the fn that implements it        the fn that handles speech
            //                                                 (changes the pit menu)           response and any other outcomes
            {PME.Initialise,              _PMet(_PMeh, PMEHiR.PMiReh_initialise,         PMER.PMrh_NoResponse) },
            {PME.Teardown,                _PMet(_PMeh, PMEHiR.PMiReh_teardown,           PMER.PMrh_NoResponse) },
            {PME.PrepareToUseMenu,        _PMet(_PMeh, PMEHiR.PMiReh_prepareToUseMenu,   PMER.PMrh_NoResponse) },
            {PME.TyreChangeAll,           _PMet(_PMeh, PMEHiR.PMiReh_changeAllTyres,     PMER.PMrh_ChangeAllTyres) },
            {PME.TyreChangeNone,          _PMet(_PMeh, PMEHiR.PMiReh_changeNoTyres,      PMER.PMrh_ChangeNoTyres) },
            {PME.TyreChangeFront,         _PMet(_PMeh, PMEHiR.PMiReh_changeFrontTyres,   PMER.PMrh_ChangeFrontTyres) },
            {PME.TyreChangeRear,          _PMet(_PMeh, PMEHiR.PMiReh_changeRearTyres,    PMER.PMrh_ChangeRearTyres) },
            {PME.TyreChangeLeft,          _PMet(_PMeh, PMEHiR.PMiReh_changeLeftTyres,    PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRight,         _PMet(_PMeh, PMEHiR.PMiReh_changeRightTyres,   PMER.PMrh_Acknowledge) },
            {PME.TyreChangeLF,            _PMet(_PMeh, PMEHiR.PMiReh_changeFLTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRF,            _PMet(_PMeh, PMEHiR.PMiReh_changeFRTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeLR,            _PMet(_PMeh, PMEHiR.PMiReh_changeRLTyre,       PMER.PMrh_Acknowledge) },
            {PME.TyreChangeRR,            _PMet(_PMeh, PMEHiR.PMiReh_changeRRTyre,       PMER.PMrh_Acknowledge) },

            {PME.TyrePressureLF,          _PMet(_PMeh, PMEHiR.PMiReh_changeFLpressure,   PMER.PMrh_Acknowledge) },
            {PME.TyrePressureRF,          _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            {PME.TyrePressureLR,          _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            {PME.TyrePressureRR,          _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },

            {PME.TyreCompoundHard,        _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundHard,   PMER.PMrh_TyreCompoundHard) },
            {PME.TyreCompoundMedium,      _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundMedium, PMER.PMrh_TyreCompoundMedium) },
            {PME.TyreCompoundSoft,        _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundSoft,   PMER.PMrh_TyreCompoundSoft) },
            {PME.TyreCompoundIntermediate,_PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundWet,         _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundMonsoon,     _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundWet,    PMER.PMrh_TyreCompoundWet) },
            {PME.TyreCompoundOption,      _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundOption, PMER.PMrh_TyreCompoundOption) },
            {PME.TyreCompoundPrime,       _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundPrime,  PMER.PMrh_TyreCompoundPrime) },
            {PME.TyreCompoundAlternate,   _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundAlternate, PMER.PMrh_TyreCompoundAlternate) },
            {PME.TyreCompoundNext,        _PMet(_PMeh, PMEHiR.PMiReh_TyreCompoundNext,   PMER.PMrh_TyreCompoundNext) },

            {PME.FuelAddXlitres,          _PMet(_PMeh, PMEHiR.PMiReh_FuelAddXlitres,     PMER.PMrh_NoResponse) },
            {PME.FuelFillToXlitres,       _PMet(_PMeh, PMEHiR.PMiReh_FuelToXlitres,      PMER.PMrh_Acknowledge) },
            {PME.FuelFillToEnd,           _PMet(_PMeh, PMEHiR.PMiReh_FuelToEnd,          PMER.PMrh_fuelToEnd) },
            {PME.FuelNone,                _PMet(_PMeh, PMEHiR.PMiReh_FuelNone,           PMER.PMrh_noFuel) },

            {PME.RepairAll,               _PMet(_PMeh, PMEHiR.PMiReh_RepairAll,          PMER.PMrh_Acknowledge) },
            {PME.RepairNone,              _PMet(_PMeh, PMEHiR.PMiReh_RepairNone,         PMER.PMrh_Acknowledge) },
            //{PME.RepairFast,            _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },     // iRacing
            //{PME.RepairAllAero,         _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },     // R3E
            //{PME.RepairFrontAero,       _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairRearAero,        _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairSuspension,      _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            //{PME.RepairSuspensionNone,  _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_Acknowledge) },
            {PME.RepairBody,              _PMet(_PMeh, PMEHiR.PMiReh_RepairBody,         PMER.PMrh_Acknowledge) },

            {PME.PenaltyServe,            _PMet(_PMeh, PMEHiR.PMiReh_PenaltyServe,       PMER.PMrh_Acknowledge) },
            {PME.PenaltyServeNone,        _PMet(_PMeh, PMEHiR.PMiReh_PenaltyServeNone,   PMER.PMrh_Acknowledge) },

            {PME.AeroFrontPlusMinusX,     _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroRearPlusMinusX,      _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroFrontSetToX,         _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.AeroRearSetToX,          _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },

            {PME.GrillePlusMinusX,        _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) }, // rF2
            {PME.GrilleSetToX,            _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.WedgePlusMinusX,         _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.WedgeSetToX,             _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.TrackBarPlusMinusX,      _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.TrackBarSetToX,          _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberLF,                _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberRF,                _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberLR,                _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.RubberRR,                _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.FenderL,                 _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.FenderR,                 _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.FlipUpL,                 _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
            {PME.FlipUpR,                 _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },

            //{PME.Tearoff,               _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) }, // iRacing
            //{PME.TearOffNone,           _PMet(_PMeh, PMEHiR.PMiReh_example,            PMER.PMrh_CantDoThat) },
        };
#endif // IRACING
        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for...
        /// </summary>
    }
}