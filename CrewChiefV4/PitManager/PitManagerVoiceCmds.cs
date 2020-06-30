using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.PitManager
{
    using PME = PitManagerEvent;  // shorthand

    public class PitManagerVoiceCmds
    {
        private static PitManager pmh = new PitManager();
        private static readonly Dictionary<PitManagerEvent, String[]> voiceCmds =
            new Dictionary<PitManagerEvent, String[]>
        {
            {PME.TyreChangeAll,     SpeechRecogniser.PIT_STOP_CHANGE_ALL_TYRES },
            {PME.TyreChangeNone,    SpeechRecogniser.PIT_STOP },
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

            {PME.TyreCompoundHard,  SpeechRecogniser.PIT_STOP },    //tbd:
            {PME.TyreCompoundMedium, SpeechRecogniser.PIT_STOP },
            {PME.TyreCompoundSoft,  SpeechRecogniser.PIT_STOP },
            {PME.TyreCompoundWet,   SpeechRecogniser.PIT_STOP },
            {PME.TyreCompoundPrimary, SpeechRecogniser.PIT_STOP },
            {PME.TyreCompoundAlternate, SpeechRecogniser.PIT_STOP },
            {PME.TyreCompoundNext,  SpeechRecogniser.PIT_STOP },

            {PME.FuelAddXlitres,    SpeechRecogniser.PIT_STOP },
            {PME.FuelFillToXlitres, SpeechRecogniser.PIT_STOP },
            {PME.FuelFillToEnd,     SpeechRecogniser.PIT_STOP },
            {PME.FuelNone,          SpeechRecogniser.PIT_STOP },

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

            {PME.GrillePlusMinusX,  SpeechRecogniser.PIT_STOP },    // rF2
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
        public static void respond(String voiceMessage)
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
    }
}
