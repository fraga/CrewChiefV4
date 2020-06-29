using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

    enum PitManagerEvent
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
        TyreCompoundPrimary,
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

    class PitManager
    {
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // This gets very messy but you don't need to worry about it
        // It allows each game to set up a dictionary PM_event_dict containing
        // PitManagerEvent, actionHandler fn, responseHandler fn
        // for all the events handled by the game
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
        /// </summary>
        /// <param name="ev">PitManagerEvent</param>
        /// <returns>
        /// true if event was handled
        /// </returns>
        public bool EventHandler(PitManagerEvent ev)
        {
            bool result = false;
            var PM_event_dict = games_dict[CrewChief.gameDefinition.gameEnum];

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
                }
            }
            else
            {
                //TBD: default handler "Not available in this game"
            }
            return result;
        }

        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // More messy stuff
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
          { PME.TyreChangeAll,      _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example, PMEH.responseHandler_example) },
          { PME.AeroFrontSetToX,    _PM_event_tuple(_PM_event_helper, PMEH.actionHandler_example2, PMEH.responseHandler_example) },
        };

        ///////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// The event dictionary for...
        /// </summary>


    }
}
