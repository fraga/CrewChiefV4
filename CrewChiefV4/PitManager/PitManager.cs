using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using CrewChiefV4.Events;

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
    using PMEH = PitManagerEventHandlers_RF2;

    /// <summary>
    /// All the events that Pit Manager handles
    /// Not all events can be handled by all games or all cars
    /// </summary>

    public enum PitManagerEvent
    {
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

        TyrePressureLF,
        TyrePressureRF,
        TyrePressureLR,
        TyrePressureRR,

        TyreCompoundHard,
        TyreCompoundMedium,
        TyreCompoundSoft,
        TyreCompoundWet,
        TyreCompoundPrime,
        TyreCompoundAlternate,
        TyreCompoundNext,

        FuelAddXlitres,
        FuelFillToXlitres,
        FuelFillToEnd,
        FuelNone,

        RepairAll,
        RepairNone,
        RepairFast,             // iRacing
        RepairAllAero,          // R3E
        RepairFrontAero,
        RepairRearAero,
        RepairSuspension,
        RepairSuspensionNone,

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
            public delegate bool PitManagerEventAction_Delegate();
            public delegate bool PitManagerEventResponse_Delegate();

            public PitManagerEventAction_Delegate PitManagerEventAction;
            public PitManagerEventResponse_Delegate PitManagerEventResponse;
        }
        class GamePitManagerDict : Dictionary<PME, PitManagerEventTableEntry> { }
        //-------------------------------------------------------------------------

        // Dictionary of games and their event dicts
        private readonly Dictionary<CrewChiefV4.GameEnum, GamePitManagerDict>
            games_dict = new Dictionary<CrewChiefV4.GameEnum, GamePitManagerDict>
        {
          {GameEnum.RF2_64BIT,  PM_event_dict_RF2}
        };

        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The Event handler
        /// The event may come from a voice command, a command macro or
        /// internally (e.g."Fill to end" Property
        /// </summary>
        /// <param name="ev">PitManagerEvent</param>
        /// <returns>
        /// true if event was handled
        /// </returns>
        public bool EventHandler(PitManagerEvent ev)
        {
            bool result = false;
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
            }

            if (PM_event_dict.ContainsKey(ev))
            {
                result = PM_event_dict[ev].PitManagerEventAction.Invoke();
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
                //Alternatively event dicts for all games have all events
                //and the response handler does the warning.
            }
            return result;
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // More messy stuff to set up the dictionary
        static public PitManagerEventTableEntry _PM_event_tuple(PitManagerEventTableEntry existing,
              PitManagerEventTableEntry.PitManagerEventAction_Delegate actionHandler,
              PitManagerEventTableEntry.PitManagerEventResponse_Delegate responseHandler)
        {
            existing.PitManagerEventAction = new PitManagerEventTableEntry.PitManagerEventAction_Delegate(actionHandler);
            existing.PitManagerEventResponse = new PitManagerEventTableEntry.PitManagerEventResponse_Delegate(responseHandler);
            return existing;
        }

        static public PitManagerEventTableEntry _PM_event_helper = new PitManagerEventTableEntry();
        //-------------------------------------------------------------------------


        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for rF2
        /// </summary>
        static readonly GamePitManagerDict PM_event_dict_RF2 = new GamePitManagerDict
        {
            //  The event                                               the fn that implements it  the fn that handles speech
            //                                                          (changes the pit menu)     response and any other outcomes
            {PME.TyreChangeAll,     _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeNone,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeFront,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeRear,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeLeft,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeRight,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeLF,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeRF,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeLR,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreChangeRR,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.TyrePressureLF,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyrePressureRF,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyrePressureLR,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyrePressureRR,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.TyreCompoundHard,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },    //tbd:
            {PME.TyreCompoundMedium, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreCompoundSoft,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreCompoundWet,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreCompoundPrime, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreCompoundAlternate, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TyreCompoundNext,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.FuelAddXlitres,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FuelFillToXlitres, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FuelFillToEnd,     _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_fuelToEnd) },
            {PME.FuelNone,          _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.RepairAll,         _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.RepairNone,        _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            //{PME.RepairFast,        _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },        // iRacing
            //{PME.RepairAllAero,     _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },        // R3E
            //{PME.RepairFrontAero,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            //{PME.RepairRearAero,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            //{PME.RepairSuspension,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            //{PME.RepairSuspensionNone, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.PenaltyServe,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.PenaltyServeNone,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.AeroFrontPlusMinusX, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.AeroRearPlusMinusX, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.AeroFrontSetToX,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.AeroRearSetToX,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            {PME.GrillePlusMinusX,  _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },    // rF2
            {PME.GrilleSetToX,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.WedgePlusMinusX,   _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.WedgeSetToX,       _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TrackBarPlusMinusX, _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.TrackBarSetToX,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.RubberLF,          _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.RubberRF,          _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.RubberLR,          _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.RubberRR,          _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FenderL,           _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FenderR,           _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FlipUpL,           _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
            {PME.FlipUpR,           _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },

            //{PME.Tearoff,           _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },    // iRacing
            //{PME.TearOffNone,       _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
        };

        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for...
        /// </summary>


    }
}
