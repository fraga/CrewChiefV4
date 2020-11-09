/*
 * This monitor handles rally co-driver pacenotes.
 * 
 * Official website: thecrewchief.org 
 * License: MIT
 */
using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;

namespace CrewChiefV4.Events
{
    public class CoDriver : AbstractEvent
    {
        public enum CornerCallStyle
        {
            UNKNOWN,
            NUMBER_FIRST,
            DIRECTION_FIRST,
            DESCRIPTIVE,
            NUMBER_FIRST_REVERSED,
            DIRECTION_FIRST_REVERSED,
        }

        // for making pace note corrections, and eventually stage recce
        public enum Direction
        {
            LEFT,
            RIGHT,
            UNKNOWN
        }

        // Acknowledgement: Based on IDs partially documented in WorkerBee's RBR Pacenotes plugin.
        // IDs above 10000 are fake and still need figuring out.
        //
        // NOTE: coner_hairpin* and variants is special, it is not coming from the game.  CoDriver pack
        //       has to map it if needed.
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PacenoteType
        {
            // Weird naming is used to simplify sound reading.
            corner_1_left = 0,
            corner_square_left = 1,
            corner_3_left = 2,
            corner_4_left = 3,
            corner_5_left = 4,
            corner_6_left = 5,
            corner_6_right = 6,
            corner_5_right = 7,
            corner_4_right = 8,
            corner_3_right = 9,
            corner_square_right = 10,
            corner_1_right = 11,
            detail_twisty = 12,
            detail_distance_call = 13,
            detail_narrows = 14,
            detail_wideout = 15,
            detail_over_crest = 16,
            detail_ford = 17,
            detail_care = 18,
            detail_bump = 19,
            detail_jump = 20,
            detail_start = 21,
            detail_finish = 22,
            detail_split = 23,
            detail_end_of_track = 24,
            corner_flat_right = 25,
            corner_flat_left = 26,
            detail_bridge = 27,
            detail_go_straight = 28,
            detail_keep_right = 29,
            detail_keep_left = 30,
            detail_keep_middle = 31,
            detail_caution = 32,
            corner_2_left = 102,
            corner_2_right = 112,
            corner_left = 120,
            corner_right = 121,
            corner_right_into = 122,
            corner_left_into = 123,
            corner_right_left = 124,
            corner_left_right = 125,
            corner_right_around = 126,
            corner_left_around = 127,
            number_1 = 140,
            number_2 = 141,
            number_3 = 142,
            number_4 = 143,
            number_5 = 144,
            number_6 = 145,
            number_7 = 146,
            number_8 = 147,
            number_9 = 148,
            number_10 = 149,
            number_20 = 150,
            number_30 = 151,
            number_40 = 152,
            number_50 = 153,
            number_60 = 154,
            number_70 = 155,
            number_80 = 156,
            number_90 = 157,
            number_100 = 158,
            number_120 = 159,
            number_140 = 160,
            number_150 = 161,
            number_160 = 162,
            number_180 = 163,
            number_200 = 164,
            number_250 = 165,
            number_300 = 166,
            number_350 = 167,
            number_400 = 168,
            number_450 = 169,
            number_500 = 170,
            number_600 = 171,
            number_700 = 172,
            number_800 = 173,
            number_900 = 174,
            number_1000 = 175,
            detail_over_bridge = 200,
            detail_over_rails = 201,
            detail_over_railway = 202,
            detail_deep_cut = 211,
            detail_full_cut = 212,
            detail_keep_centre = 213,
            detail_full = 214,
            detail_go_full = 215,
            detail_flatout = 216,
            detail_brake = 217,
            detail_light_cut = 218,
            detail_handbrake = 219,
            detail_keep_in = 220,
            detail_keep_out = 221,
            detail_clip = 223,
            detail_take = 224,
            detail_from_left = 224,
            detail_from_right = 224,
            detail_minus = 230,
            detail_minusminus = 231,
            detail_plus = 232,
            detail_plus_plus = 233,
            detail_early = 234,
            detail_late = 235,
            detail_easy = 236,
            detail_much = 237,
            detail_many = 238,
            detail_very = 239,
            detail_hard = 240,
            detail_fast = 241,
            detail_slow = 242,
            detail_exact = 243,
            detail_slowing = 244,
            detail_directly = 245,
            detail_light = 246,
            detail_big = 247,
            detail_small = 248,
            detail_sharp = 249,
            detail_round = 250,
            detail_tight = 251,
            detail_slight = 252,
            detail_good = 253,
            detail_bad = 254,
            detail_narrow = 255,
            detail_wide = 256,
            detail_straight = 257,
            detail_extra = 258,
            detail_uphill = 260,
            detail_downhill = 261,
            detail_longlong = 263,
            detail_short = 264,
            detail_short_short = 265,
            detail_go_narrow = 266,
            detail_go_wide = 267,
            detai_slippery = 268,
            detail_slide = 269,
            detail_understeer = 270,
            detail_sideways = 271,
            detail_hook = 272,
            detail_draws_in = 273,
            detail_very_long = 274,
            detail_very_short = 275,
            detail_curbside = 276,
            detail_slippy = 277,
            detail_muddy = 290,
            detail_dirty = 291,
            detail_bumpy = 292,
            detail_cramped = 293,
            detail_positive = 294,
            detail_negative = 295,
            detail_dirt = 296,
            detail_opens = 298,
            detail_fakes = 299,
            detail_bumps = 300,
            detail_hidden = 301,
            detail_blind = 302,
            detail_double_caution = 303,
            detail_triple_caution = 304,
            detail_gravel = 320,
            detail_tarmac = 321,
            detail_concrete = 322,
            detail_cobbles = 323,
            detail_grit = 324,
            detail_snow = 325,
            detail_onsplit = 326,
            detail_icy = 327,
            detail_rubble = 328,
            detail_ice = 329,
            detail_loose_gravel = 330,
            detail_crest = 340,
            detail_hollow = 341,
            detail_camber = 342,
            detail_reverse_camber = 343,
            detail_hole = 344,
            detail_ruts = 345,
            detail_deepruts = 346,
            detail_border = 347,
            detail_edge = 348,
            detail_curb = 349,
            detail_ditch = 350,
            detail_junction = 351,
            detail_curve = 352,
            detail_turn = 353,
            detail_steep_drop = 354,
            detail_bad_camber = 355,
            detail_shoulder = 356,
            detail_steep_hill = 357,
            detail_steep_incline = 358,
            detail_steep_slope = 359,
            detail_snow_border = 360,
            detail_dip = 361,
            detail_drops = 362,
            detail_drops_left = 363,
            detail_drops_right = 364,
            detail_fork_left = 365,
            detail_fork_right = 366,
            detail_negative_camber = 367,
            detail_positive_camber = 368,
            detail_compression = 369,
            detail_fence = 370,
            detail_wall = 371,
            detail_house = 372,
            detail_tree = 373,
            detail_stump = 374,
            detail_mast = 375,
            detail_post = 376,
            detail_island = 377,
            detail_chicane = 378,
            detail_stone = 379,
            detail_rock = 380,
            detail_tunnel = 381,
            detail_road = 382,
            detail_walk = 383,
            detail_rails = 384,
            detail_sign = 385,
            detail_bush = 386,
            detail_path = 387,
            detail_water = 388,
            detail_puddle = 389,
            detail_netting = 390,
            detail_tape = 391,
            detail_left_entry_chicane = 392,
            detail_right_entry_chicane = 393,
            detail_tyres = 394,
            detail_spectators = 395,
            detail_marshalls = 396,
            detail_barrels = 397,
            detail_roundabout = 399,
            detail_through = 400,
            detail_after = 401,
            detail_near = 402,
            detail_on = 403,
            detail_until = 404,
            detail_at = 405,
            detail_before = 406,
            detail_over = 407,
            detail_in = 408,
            detail_behind = 409,
            detail_for = 410,
            detail_inside = 411,
            detail_outside = 412,
            detail_then = 413,
            detail_off = 414,
            detail_from = 415,
            detail_in_de = 416,
            detail_done = 430,
            detail_stop = 431,
            detail_line = 432,
            detail_lifts = 433,
            detail_wide_d_e = 434,
            detail_next_lap = 435,
            detail_take_exit = 436,
            detail_wooden_fence = 443,
            detail_onto_gravel = 540,
            detail_onto_tarmac = 541,
            detail_onto_concrete = 542,
            detail_onto_cobbles = 543,
            detail_onto_grit = 544,
            detail_onto_snow = 545,
            detail_wet = 546,
            detail_dry = 547,
            detail_damp = 548,
            detail_hold = 549,
            detail_take_speed = 550,
            detail_left_foot_braking = 551,
            detail_grip_off = 552,
            detail_grip = 553,
            detail_good_grip = 554,
            detail_to_sight_distance = 555,
            detail_split_time = 556,
            detail_entry = 557,
            detail_checkpoint = 558,
            detail_speed = 559,
            detail_tightens_to_6 = 2001,
            detail_tightens_to_5 = 2002,
            detail_tightens_to_4 = 2003,
            detail_tightens_to_3 = 2004,
            detail_tightens_to_2 = 2005,
            detail_tightens_to_1 = 2006,
            detail_tightens_to_acute = 2007,
            detail_to_6 = 2008,
            detail_to_5 = 2009,
            detail_to_4 = 2010,
            detail_to_3 = 2011,
            detail_to_2 = 2012,
            detail_to_1 = 2013,
            detail_to_acute = 2014,
            detail_tightens_late = 2015,
            detail_dont_cut_early = 2016,
            detail_dont_cut_late = 2017,
            detail_opens_tightens = 2018,
            detail_tightens_opens = 2019,
            detail_stay_out = 2020,
            detail_care_in = 2021,
            detail_care_out = 2022,
            detail_to_finish = 2105,
            detail_jump_flat = 2106,
            detail_jump_bind = 2107,
            detail_over_jump = 2108,
            detail_small_crest = 2109,
            detail_late_apex = 2023,
            detail_to_dip = 2024,
            detail_continues_over_crest = 2029,
            corner_left_acute = 2040,
            corner_right_acute = 2041,
            detail_empty_call = 4075,
            detail_caution_water = 4077,
            detail_onto = 4082,
            detail_into = 4083,
            detail_and = 4084,
            detail_thightens = 4088,
            detail_double_tightens = 4089,

            // _don't care?
            detail_place_holder = 10005,
            detail_callout_time = 10006,
            detail_callout_distance = 10007,
            detail_sound_file = 10008,
            detail_standard_call = 10009,
            detail_sound_index = 10010,
            detail_callout_adjust = 10012,
            unknown = 20000,

            // CC-only
            detail_through_gate = 40001,
            detail_big_jump = 40002,
            corner_open_hairpin_left = 40003,
            corner_open_hairpin_right = 40004,
            detail_widens = 40005,
            detail_logs_inside = 40006,
            detail_rocks_inside = 40007,
            detail_tree_inside = 40008,
            detail_logs_outside = 40009,
            detail_rocks_outside = 40010,
            detail_tree_outside = 40010
        }

        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PacenoteModifier : int
        {
            none = 0,
            detail_narrows = 1,
            detail_wideout = 2,
            detail_tightens = 4,
            detail_dont_cut = 32,
            detail_cut = 64,
            detail_double_tightens = 128,
            detail_opens = 256,
            detail_longlong = 512,
            detail_long = 1024,
            detail_minus = 2048,
            detail_plus = 4096,
            detail_maybe = 8192
        }

        private static readonly int[] distanceCallRanges = new int[]
        {
            30,
            40,
            50,
            60,
            70,
            80,
            100,
            120,
            140,
            150,
            160,
            180,
            200,
            250,
            300,
            350,
            400,
            450,
            500,
            600,
            700,
            800,
            900,
            1000
        };


        // It turns out that chainedNotes not only define "into" logic, they also affect distance calculation.
        // That means we may need to allow specifying those in the .json.
#if false
        public static HashSet<CoDriver.PacenoteType> chainedNotes = new HashSet<CoDriver.PacenoteType>()
        {
            CoDriver.PacenoteType.corner_1_left,
            CoDriver.PacenoteType.corner_square_left,
            CoDriver.PacenoteType.corner_3_left,
            CoDriver.PacenoteType.corner_4_left,
            CoDriver.PacenoteType.corner_5_left,
            CoDriver.PacenoteType.corner_6_left,
            CoDriver.PacenoteType.corner_6_right,
            CoDriver.PacenoteType.corner_5_right,
            CoDriver.PacenoteType.corner_4_right,
            CoDriver.PacenoteType.corner_3_right,
            CoDriver.PacenoteType.corner_square_right,
            CoDriver.PacenoteType.corner_1_right,
            CoDriver.PacenoteType.corner_flat_right,
            CoDriver.PacenoteType.corner_flat_left,
            CoDriver.PacenoteType.corner_2_left,
            CoDriver.PacenoteType.corner_2_right,
            CoDriver.PacenoteType.corner_left,
            CoDriver.PacenoteType.corner_right,
            CoDriver.PacenoteType.corner_right_into,
            CoDriver.PacenoteType.corner_left_into,
            CoDriver.PacenoteType.corner_right_left,
            CoDriver.PacenoteType.corner_left_right,
            CoDriver.PacenoteType.corner_right_around,
            CoDriver.PacenoteType.corner_left_around,
            CoDriver.PacenoteType.detail_care,
            CoDriver.PacenoteType.detail_caution,
            CoDriver.PacenoteType.detail_double_caution,
            CoDriver.PacenoteType.detail_triple_caution,
            CoDriver.PacenoteType.detail_caution_water,
            CoDriver.PacenoteType.detail_hole,
            CoDriver.PacenoteType.detail_ruts,
            CoDriver.PacenoteType.detail_deepruts,
            CoDriver.PacenoteType.detail_post,
            CoDriver.PacenoteType.detail_mast,
            CoDriver.PacenoteType.detail_wall,
            CoDriver.PacenoteType.detail_fence,
            CoDriver.PacenoteType.detail_house,
            CoDriver.PacenoteType.detail_island,
            CoDriver.PacenoteType.detail_chicane,
            CoDriver.PacenoteType.detail_tunnel,
            CoDriver.PacenoteType.detail_rails,
            CoDriver.PacenoteType.detail_path,
            CoDriver.PacenoteType.detail_road,
            CoDriver.PacenoteType.detail_walk,
            CoDriver.PacenoteType.detail_roundabout,
            CoDriver.PacenoteType.detail_wooden_fence,
            CoDriver.PacenoteType.detail_twisty,
            CoDriver.PacenoteType.detail_go_straight,
            CoDriver.PacenoteType.detail_jump,
            /*CoDriver.PacenoteType.detail_over_crest,*/
            CoDriver.PacenoteType.detail_crest,
            CoDriver.PacenoteType.detail_bridge,
            CoDriver.PacenoteType.detail_ford,
            CoDriver.PacenoteType.detail_bump,
            CoDriver.PacenoteType.detail_water,
            CoDriver.PacenoteType.detail_puddle,
            CoDriver.PacenoteType.detail_tree,
            CoDriver.PacenoteType.detail_stump,
            CoDriver.PacenoteType.detail_stone,
            CoDriver.PacenoteType.detail_rock,
            CoDriver.PacenoteType.detail_sign,
            CoDriver.PacenoteType.detail_bush,
            CoDriver.PacenoteType.detail_barrels,
            /*CoDriver.PacenoteType.detail_over_bridge,
            CoDriver.PacenoteType.detail_over_rails,
            CoDriver.PacenoteType.detail_over_railway,*/
            CoDriver.PacenoteType.detail_netting,
            CoDriver.PacenoteType.detail_tyres,
            CoDriver.PacenoteType.detail_spectators,
            CoDriver.PacenoteType.detail_marshalls,
            CoDriver.PacenoteType.detail_tape,
            CoDriver.PacenoteType.detail_junction,
            CoDriver.PacenoteType.detail_curve,
            CoDriver.PacenoteType.detail_turn,
            CoDriver.PacenoteType.detail_left_entry_chicane,
            CoDriver.PacenoteType.detail_right_entry_chicane,
            CoDriver.PacenoteType.detail_fork_left,
            CoDriver.PacenoteType.detail_fork_right
        };
#endif 

        // For chained notes, we can drop the 'into' and use 'over...' instead for these.
        private Dictionary<CoDriver.PacenoteType, CoDriver.PacenoteType> intoToOver = new Dictionary<CoDriver.PacenoteType, CoDriver.PacenoteType>()
        {
            { CoDriver.PacenoteType.detail_rails, CoDriver.PacenoteType.detail_over_rails },
            { CoDriver.PacenoteType.detail_jump, CoDriver.PacenoteType.detail_over_jump },
            { CoDriver.PacenoteType.detail_crest, CoDriver.PacenoteType.detail_over_crest },
            { CoDriver.PacenoteType.detail_bridge, CoDriver.PacenoteType.detail_over_bridge }
        };

        // this needs to be an ordered dictionary so we can look for more specific corner names (open hairpin) before less specific names
        // (hairpin) - as we iterate we expect to encounter more specific names first
        private OrderedDictionary possibleCornerCommands = new OrderedDictionary();
        
        private Dictionary<string[], PacenoteType> obstaclePacenoteTypes = new Dictionary<string[], PacenoteType>()
        {
            { SpeechRecogniser.RALLY_BAD_CAMBER, PacenoteType.detail_bad_camber },
            { SpeechRecogniser.RALLY_OVER_BRIDGE, PacenoteType.detail_over_bridge },
            { SpeechRecogniser.RALLY_BRIDGE, PacenoteType.detail_bridge },
            { SpeechRecogniser.RALLY_BUMPS, PacenoteType.detail_bumps },
            { SpeechRecogniser.RALLY_CONCRETE, PacenoteType.detail_concrete },
            { SpeechRecogniser.RALLY_OVER_CREST, PacenoteType.detail_over_crest },
            { SpeechRecogniser.RALLY_CREST, PacenoteType.detail_crest },
            { SpeechRecogniser.RALLY_FORD, PacenoteType.detail_ford },
            { SpeechRecogniser.RALLY_LOOSE_GRAVEL, PacenoteType.detail_loose_gravel },
            { SpeechRecogniser.RALLY_GRAVEL, PacenoteType.detail_gravel },
            { SpeechRecogniser.RALLY_SNOW, PacenoteType.detail_snow },
            { SpeechRecogniser.RALLY_SLIPPY, PacenoteType.detail_slippy},
            { SpeechRecogniser.RALLY_OVER_JUMP, PacenoteType.detail_over_jump },
            { SpeechRecogniser.RALLY_BIG_JUMP, PacenoteType.detail_big_jump }, 
            { SpeechRecogniser.RALLY_JUMP, PacenoteType.detail_jump },
            { SpeechRecogniser.RALLY_JUNCTION, PacenoteType.detail_junction },
            { SpeechRecogniser.RALLY_KEEP_IN, PacenoteType.detail_keep_in },
            { SpeechRecogniser.RALLY_KEEP_LEFT, PacenoteType.detail_keep_left },
            { SpeechRecogniser.RALLY_KEEP_MIDDLE, PacenoteType.detail_keep_middle },
            { SpeechRecogniser.RALLY_KEEP_OUT, PacenoteType.detail_keep_out },
            { SpeechRecogniser.RALLY_KEEP_RIGHT, PacenoteType.detail_keep_right },
            { SpeechRecogniser.RALLY_LEFT_ENTRY_CHICANE, PacenoteType.detail_left_entry_chicane },
            { SpeechRecogniser.RALLY_OPENS_THEN_TIGHTENS, PacenoteType.detail_opens_tightens },
            { SpeechRecogniser.RALLY_TIGHTENS_THEN_OPENS, PacenoteType.detail_tightens_opens },
            { SpeechRecogniser.RALLY_OPENS, PacenoteType.detail_opens },
            { SpeechRecogniser.RALLY_WIDENS, PacenoteType.detail_widens },
            { SpeechRecogniser.RALLY_OVER_RAILS, PacenoteType.detail_over_rails },
            { SpeechRecogniser.RALLY_RIGHT_ENTRY_CHICANE, PacenoteType.detail_right_entry_chicane },
            { SpeechRecogniser.RALLY_DEEP_RUTS, PacenoteType.detail_deepruts },
            { SpeechRecogniser.RALLY_RUTS, PacenoteType.detail_ruts },
            { SpeechRecogniser.RALLY_TARMAC, PacenoteType.detail_onto_tarmac },
            { SpeechRecogniser.RALLY_TUNNEL, PacenoteType.detail_tunnel },
            { SpeechRecogniser.RALLY_CARE, PacenoteType.detail_care },
            { SpeechRecogniser.RALLY_CAUTION, PacenoteType.detail_caution },
            { SpeechRecogniser.RALLY_DANGER, PacenoteType.detail_double_caution },
            { SpeechRecogniser.RALLY_THROUGH_GATE, PacenoteType.detail_through_gate },
            { SpeechRecogniser.RALLY_NARROWS, PacenoteType.detail_narrows },
            { SpeechRecogniser.RALLY_LOGS_INSIDE, PacenoteType.detail_logs_inside },
            { SpeechRecogniser.RALLY_ROCKS_INSIDE, PacenoteType.detail_rocks_inside },
            { SpeechRecogniser.RALLY_TREE_INSIDE, PacenoteType.detail_tree_inside },
            { SpeechRecogniser.RALLY_LOGS_OUTSIDE, PacenoteType.detail_logs_outside },
            { SpeechRecogniser.RALLY_ROCKS_OUTSIDE, PacenoteType.detail_rocks_outside },
            { SpeechRecogniser.RALLY_TREE_OUTSIDE, PacenoteType.detail_tree_outside },
            { SpeechRecogniser.RALLY_UPHILL, PacenoteType.detail_uphill },
            { SpeechRecogniser.RALLY_DOWNHILL, PacenoteType.detail_downhill },
            { SpeechRecogniser.RALLY_BRAKE, PacenoteType.detail_brake },
            { SpeechRecogniser.RALLY_GO_STRAIGHT, PacenoteType.detail_go_straight },
        };

        public class Terminology
        {
            public Dictionary<string, string> terminology = new Dictionary<string, string>();
        }

        public class Terminologies
        {
            public Dictionary<string, CoDriver.Terminology> termininologies = new Dictionary<string, CoDriver.Terminology>();
            public HashSet<string> chainedNotes = new HashSet<string>();
        }

        public class HistoricCall
        {
            public DateTime callTime = DateTime.MinValue;
            public float callDistance = 0;
            public PacenoteType callType = PacenoteType.unknown;
            public PacenoteModifier modifier = PacenoteModifier.none;
            public HistoricCall()
            {

            }
            public HistoricCall(PacenoteType callType, PacenoteModifier modifier, float callDistance, DateTime callTime)
            {
                this.callType = callType;
                this.callDistance = callDistance;
                this.callTime = callTime;
                this.modifier = modifier;
            }
            public override string ToString()
            {
                return callType.ToString() + ":" + modifier.ToString();
            }
        }

        private const string pacenotesFileName = "pacenotes.json";
        private const string correctionsFileName = "corrections.json";

        public static Terminologies terminologies = new Terminologies();
  
        private const string codriverFolderPrefix = "codriver_";
        public const string defaultCodriverId = "Jim Britton (default)";
        public static List<String> availableCodrivers = new List<String>();
        private static string selectedCodrvier = UserSettings.GetUserSettings().getString("codriver_name");
        public static CornerCallStyle cornerCallStyle = CornerCallStyle.UNKNOWN;

        private float rushedSpeedKmH = UserSettings.GetUserSettings().getFloat("codriver_rushed_speed");   // default 140 km/h
        private float chainedPacenoteThresholdMeters = UserSettings.GetUserSettings().getFloat("codriver_chained_pacenote_threshold_distance");  // default 30m
        private float lookaheadSecondsFromConfig = UserSettings.GetUserSettings().getFloat("codriver_lookahead_seconds");  // default 4s
        private float rushedLookaheadSeconds = UserSettings.GetUserSettings().getFloat("codriver_rushed_lookahead_seconds");  // default 2s
        private bool dynamicLookahead = UserSettings.GetUserSettings().getBoolean("codriver_dynamic_lookahead");
        private float minSpacingForAutoDistanceCall = UserSettings.GetUserSettings().getFloat("codriver_min_space_for_auto_distance_call");  // 40m
        private float earlierLaterStepSeconds = 0.5f;   // step used when moving calls forward or back

        private float lookaheadSecondsToUse;
        private const float maxLookaheadSeconds = 10f;
        private const float minLookaheadSeconds = 0.5f;

        private int lastProcessedPacenoteIdx = 0;
        private bool isLost = false;
        private float lastProcessedLapDist = -1.0f;

        private DateTime lastIntoSoundPlayed = DateTime.MinValue;

        // used when switching between descriptive and numeric corner calls
        private bool preferReversedNumbers = false;

        // Sound folders.
        private static string folderCodriverPrefix = "codriver/";
        private static string loadedCodriverPrefix = "";
        private static string startRecce = "acknowledge_start_recce";
        private static string endRecce = "acknowledge_end_recce";
        private static string correction = "correction";
        // if available, we use a codriver-specific version of these
        private string folderAcknowlegeOK = AudioPlayer.folderAcknowlegeOK;
        private string folderDidntUnderstand = AudioPlayer.folderDidntUnderstand;
        private string folderNo = AudioPlayer.folderNo;
        private string folderAcknowledgeStartRecce = folderCodriverPrefix + startRecce;
        private string folderAcknowledgeEndRecce = folderCodriverPrefix + endRecce;
        private string folderCorrection = folderCodriverPrefix + correction;

        // These are to be combined with the folderCodriverPrefix string.
        public static string folderFalseStart;
        public static string folderMicCheck;

        private DateTime lastRushedPacenoteTime = DateTime.MinValue;
        private int lastBatchFragmentCount = 0;

        private LinkedList<HistoricCall> historicCalls = new LinkedList<HistoricCall>();

        private List<CoDriverPacenote> correctionsForCurrentSession = new List<CoDriverPacenote>();

        private List<CoDriverPacenote> recePaceNotes = new List<CoDriverPacenote>();
        private bool inReceMode = false;
        private bool lastRecePacenoteWasDistance = false;

        // this is the set of pacenotes just added when in recce mode or just played when in normal mode.
        // It's used to locate the pace notes we need to remove and replace (recce mode) and provide a set
        // of pace notes to replay (normal mode when requesting that the chief repeats the last message)
        private List<CoDriverPacenote> lastPlayedOrAddedBatch = new List<CoDriverPacenote>();
        private DateTime lastPlayedBatchTime = DateTime.MinValue;

        public static string TOGGLE_RALLY_RECCE_MODE = "toggle_rally_recce_mode";

        private string lastStageName = "";

        // random rally helper function
        public static double GetClosestValueForDistanceCall(double distanceToNext)
        {
            var closestRange = 1000;
            var minDistance = Math.Abs(closestRange - distanceToNext);
            foreach (var r in CoDriver.distanceCallRanges)
            {
                var distToRange = Math.Abs(r - distanceToNext);
                if (distToRange < minDistance)
                {
                    minDistance = distToRange;
                    closestRange = r;
                }
            }
            return closestRange;
        }

        public CoDriver(AudioPlayer audioPlayer)
        {
            // reload the settings
            CoDriver.cornerCallStyle = (CornerCallStyle)UserSettings.GetUserSettings().getInt("codriver_style");
            this.rushedSpeedKmH = UserSettings.GetUserSettings().getFloat("codriver_rushed_speed");
            this.chainedPacenoteThresholdMeters = UserSettings.GetUserSettings().getFloat("codriver_chained_pacenote_threshold_distance");
            this.lookaheadSecondsFromConfig = UserSettings.GetUserSettings().getFloat("codriver_lookahead_seconds");
            this.lookaheadSecondsToUse = this.lookaheadSecondsFromConfig;   // this can be changed on the fly

            this.rushedLookaheadSeconds = UserSettings.GetUserSettings().getFloat("codriver_rushed_lookahead_seconds");

            this.audioPlayer = audioPlayer;
            assemblePossibleCornerCommands();

            if (GlobalBehaviourSettings.racingType != CrewChief.RacingType.Rally)
            {
                Debug.WriteLine("Co-driver is not initialized because app is not running in a rally racing mode");
                return;
            }

            if (CoDriver.availableCodrivers.Count == 0)
            {
                CoDriver.availableCodrivers.Add(defaultCodriverId);
                try
                {
                    var soundsDirectory = new DirectoryInfo(AudioPlayer.soundFilesPathNoChiefOverride + "/voice");
                    var directories = soundsDirectory.GetDirectories();
                    foreach (var dir in directories)
                    {
                        if (dir.Name.StartsWith(codriverFolderPrefix) && dir.Name.Length > codriverFolderPrefix.Length)
                            CoDriver.availableCodrivers.Add(dir.Name.Substring(codriverFolderPrefix.Length));
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("No co-driver sound folders available");
                    return;
                }
            }

            var selectedCodriver = UserSettings.GetUserSettings().getString("codriver_name");
            if (!CoDriver.defaultCodriverId.Equals(selectedCodriver))
            {
                if (Directory.Exists(AudioPlayer.soundFilesPathNoChiefOverride + "/voice/codriver_" + selectedCodriver))
                {
                    Console.WriteLine("Using co-driver: " + selectedCodriver);
                    CoDriver.folderCodriverPrefix = "codriver_" + selectedCodriver + "/";
                    string codriverAcknowledgeOK = CoDriver.folderCodriverPrefix + "OK";
                    string codriverNo = CoDriver.folderCodriverPrefix + "no";
                    string codriverAcknowledgeDidntUnderstand = CoDriver.folderCodriverPrefix + "didnt_understand";
                    string codriverStartRecce = CoDriver.folderCodriverPrefix + startRecce;
                    string codriverEndRecce = CoDriver.folderCodriverPrefix + endRecce;
                    string codriverCorrection = CoDriver.folderCodriverPrefix + correction;
                    if (SoundCache.availableSounds.Contains(codriverAcknowledgeOK))
                    {
                        this.folderAcknowlegeOK = codriverAcknowledgeOK;
                    }
                    if (SoundCache.availableSounds.Contains(codriverAcknowledgeDidntUnderstand))
                    {
                        this.folderDidntUnderstand = codriverAcknowledgeDidntUnderstand;
                    }
                    if (SoundCache.availableSounds.Contains(codriverNo))
                    {
                        this.folderNo = codriverNo;
                    }
                    if (SoundCache.availableSounds.Contains(codriverStartRecce))
                    {
                        this.folderAcknowledgeStartRecce = codriverStartRecce;
                    }
                    if (SoundCache.availableSounds.Contains(codriverEndRecce))
                    {
                        this.folderAcknowledgeEndRecce = codriverEndRecce;
                    }
                    if (SoundCache.availableSounds.Contains(codriverCorrection))
                    {
                        this.folderCorrection = codriverCorrection;
                    }
                }
            }
            CoDriver.folderFalseStart = CoDriver.folderCodriverPrefix + "penalty_false_start";
            CoDriver.folderMicCheck = CoDriver.folderCodriverPrefix + "microphone_check";

            if (CoDriver.loadedCodriverPrefix != CoDriver.folderCodriverPrefix)
            {
                try
                {
                    var overridePath = System.IO.Path.Combine(Environment.GetFolderPath(
                        Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "terminologies.json");

                    if (File.Exists(overridePath))
                    {
                        Console.WriteLine($"Loading co-driver terminology from the user override file.  Current style selected: {CoDriver.cornerCallStyle}.");
                        CoDriver.terminologies = JsonConvert.DeserializeObject<Terminologies>(Utilities.GetFileContentsJsonWithComment(overridePath));
                    }
                    else
                    {
                        Console.WriteLine($"Loading co-driver terminology.  Current style selected: {CoDriver.cornerCallStyle}.");
                        CoDriver.terminologies = JsonConvert.DeserializeObject<Terminologies>(
                            Utilities.GetFileContentsJsonWithComment(AudioPlayer.soundFilesPathNoChiefOverride + @"\voice\" + folderCodriverPrefix.Substring(0, folderCodriverPrefix.Length - 1) + "\\terminologies.json"));
                    }

                    CoDriver.loadedCodriverPrefix = CoDriver.folderCodriverPrefix;

                    // Validate styles.
                    foreach (var t in CoDriver.terminologies.termininologies.Keys)
                    {
                        if (!Enum.TryParse<CoDriver.CornerCallStyle>(t, out var enumValue))
                            Console.WriteLine($"Corrupted terminology.  Value {t} is not a valid CoDriver.CornerCallStyle value.");
                    }

                    // Validate terminology note IDs.
                    foreach (var t in CoDriver.terminologies.termininologies.Values)
                    {
                        foreach (var id in t.terminology.Keys)
                        {
                            if (!Enum.TryParse<CoDriver.PacenoteType>(id, out var enumValue))
                                Console.WriteLine($"Corrupted terminology mapping.  Value {id} is not a valid CoDriver.PacenoteType value.");
                        }
                    }

                    // Validate chained note IDs.
                    foreach (var id in terminologies.chainedNotes)
                    {
                        if (!Enum.TryParse<CoDriver.PacenoteType>(id, out var enumValue))
                            Console.WriteLine($"Corrupted terminology chained note.  Value {id} is not a valid CoDriver.PacenoteType value.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to load terminologies for co-driver: {selectedCodriver}.  Terminology mappings, distance calling and 'into' chaining will not function correctly.  Exception: {e.Message}");
                }
            }

#if false
            var terminologies = new Terminologies();
            var term = new Terminology();
            term.terminology.Add(CoDriver.PacenoteType.corner_1_left.ToString(), "corner_hairpin_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_1_right.ToString(), "corner_hairpin_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_left.ToString(), "corner_1_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_right.ToString(), "corner_1_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_left.ToString(), "corner_2_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_right.ToString(), "corner_2_right");
            terminologies.termininologies.Add(CornerCallStyle.NUMBER_FIRST.ToString(), term);

            term = new Terminology();
            term.terminology.Add(CoDriver.PacenoteType.corner_1_left.ToString(), "corner_hairpin_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_1_right.ToString(), "corner_hairpin_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_left.ToString(), "corner_1_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_right.ToString(), "corner_1_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_left.ToString(), "corner_2_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_right.ToString(), "corner_2_right");
            terminologies.termininologies.Add(CornerCallStyle.DIRECTION_FIRST.ToString(), term);

            term = new Terminology();
            terminologies.termininologies.Add(CornerCallStyle.DESCRIPTIVE.ToString(), term);

            term = new Terminology();
            term.terminology.Add(CoDriver.PacenoteType.corner_1_left.ToString(), "corner_hairpin_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_1_right.ToString(), "corner_hairpin_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_left.ToString(), "corner_1_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_right.ToString(), "corner_1_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_left.ToString(), "corner_2_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_right.ToString(), "corner_2_right");
            terminologies.termininologies.Add(CornerCallStyle.NUMBER_FIRST_REVERSED.ToString(), term);

            term = new Terminology();
            term.terminology.Add(CoDriver.PacenoteType.corner_1_left.ToString(), "corner_hairpin_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_1_right.ToString(), "corner_hairpin_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_left.ToString(), "corner_1_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_2_right.ToString(), "corner_1_right");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_left.ToString(), "corner_2_left");
            term.terminology.Add(CoDriver.PacenoteType.corner_square_right.ToString(), "corner_2_right");
            terminologies.termininologies.Add(CornerCallStyle.DIRECTION_FIRST_REVERSED.ToString(), term);

            foreach (var msg in chainedNotes)
            {
                terminologies.chainedNotes.Add(msg.ToString());
            }

            using (StreamWriter file = File.CreateText(System.IO.Path.Combine(@"c:\temp\", "term.json")))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                serializer.Serialize(file, terminologies);
            }
#endif
    }

        private void assemblePossibleCornerCommands()
        {
            // Note that this needs to be assembled with the most specific commands first so we find, for example,
            // "open hairpin" before "hairpin"
            foreach (string direction in SpeechRecogniser.RALLY_LEFT)
            {
                foreach (string cornerType in SpeechRecogniser.RALLY_1)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_1_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_1_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_2)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_2_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_2_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_3)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_3_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_3_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_4)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_4_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_4_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_5)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_5_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_5_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_6)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_6_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_6_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_SQUARE)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_square_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_square_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_FLAT)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_flat_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_flat_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_OPEN_HAIRPIN)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_open_hairpin_left;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_open_hairpin_left;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_HAIRPIN)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_left_acute;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_left_acute;
                }
            }

            foreach (string direction in SpeechRecogniser.RALLY_RIGHT)
            {
                foreach (string cornerType in SpeechRecogniser.RALLY_1)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_1_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_1_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_2)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_2_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_2_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_3)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_3_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_3_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_4)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_4_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_4_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_5)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_5_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_5_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_6)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_6_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_6_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_SQUARE)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_square_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_square_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_FLAT)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_flat_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_flat_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_OPEN_HAIRPIN)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_open_hairpin_right;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_open_hairpin_right;
                }
                foreach (string cornerType in SpeechRecogniser.RALLY_HAIRPIN)
                {
                    possibleCornerCommands[cornerType + " " + direction] = PacenoteType.corner_right_acute;
                    possibleCornerCommands[direction + " " + cornerType] = PacenoteType.corner_right_acute;
                }
            }
        }
        
        public override List<CrewChief.RacingType> applicableRacingTypes
        {
            get { return new List<CrewChief.RacingType> { CrewChief.RacingType.Rally }; }
        }

        public override List<SessionType> applicableSessionTypes
        {
            get { return new List<SessionType> { SessionType.Race }; }
        }

        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Countdown, SessionPhase.Green, SessionPhase.Finished, SessionPhase.Checkered }; }
        }

        /*
         * IMPORTANT: This method is called twice - when the message becomes due, and immediately before playing it (which may have a 
         * delay caused by the length of the queue at the time). So be *very* careful when checking and updating local state in here.
         */
        public override bool isMessageStillValid(string eventSubType, GameStateData currentGameState, Dictionary<String, Object> validationData)
        {
            if (base.isMessageStillValid(eventSubType, currentGameState, validationData))
                return true;

            return false;
        }

        public override void clearState()
        {
            this.lastProcessedPacenoteIdx = 0;
            this.isLost = false;
            this.lastProcessedLapDist = -1.0f;

            // Reset to config setting, as ToUse one can be adjusted via SRE.
            this.lookaheadSecondsToUse = this.lookaheadSecondsFromConfig;
            this.preferReversedNumbers = CoDriver.cornerCallStyle == CornerCallStyle.DIRECTION_FIRST_REVERSED || CoDriver.cornerCallStyle == CornerCallStyle.NUMBER_FIRST_REVERSED;
            this.lastRushedPacenoteTime = DateTime.MinValue;
            this.lastBatchFragmentCount = 0;
            this.historicCalls.Clear();
            this.recePaceNotes.Clear();
        }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            var pgs = previousGameState;

            if (pgs == null)
                return;

            var cgs = currentGameState;
            var csd = currentGameState.SessionData;
            var psd = previousGameState.SessionData;
            if (csd.TrackDefinition != null && csd.TrackDefinition.name != null && csd.TrackDefinition.name != "")
            {
                lastStageName = csd.TrackDefinition.name;
            }

            if (this.inReceMode)
            {
                if (csd.SessionPhase == SessionPhase.Green && psd.SessionPhase == SessionPhase.Countdown)
                {
                    Console.WriteLine("Stage recce started");
                }
            }
            else
            {
                this.ProcessRaceStart(cgs, csd, psd);
                if (CrewChief.gameDefinition.gameEnum == GameEnum.RBR)
                {
                    this.ProcessPenalties(cgs, pgs);
                    this.ProcessLost(cgs, pgs);
                }
                this.ProcessPacenotes(cgs, csd);
            }
        }

        private void ProcessLost(GameStateData cgs, GameStateData pgs)
        {
            if (cgs.SessionData.SessionPhase != SessionPhase.Green || cgs.PositionAndMotionData.DistanceRoundTrack == 0
                || this.isLost)
                return;

            if (this.lastProcessedLapDist == -1.0)
            {
                this.lastProcessedLapDist = cgs.PositionAndMotionData.DistanceRoundTrack;
                return;
            }

            // If distance suddenly jumps drastically, we're likely lost.
            if (Math.Abs(this.lastProcessedLapDist - cgs.PositionAndMotionData.DistanceRoundTrack) > 500.0f)
            {
                Console.WriteLine("CoDriver: we seem to be lost, turning pacenotes off.");

                this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + "we_are_lost", 0));
                this.isLost = true;
                return;
            }

            this.lastProcessedLapDist = cgs.PositionAndMotionData.DistanceRoundTrack;
        }

        private void ProcessPenalties(GameStateData cgs, GameStateData pgs)
        {
            if (pgs.PenaltiesData.PenaltyCause == PenatiesData.DetailedPenaltyCause.NONE
                && cgs.PenaltiesData.PenaltyCause == PenatiesData.DetailedPenaltyCause.FALSE_START)
                this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + "penalty_false_start", 0));
        }

        private void ProcessRaceStart(GameStateData cgs, SessionData csd, SessionData psd)
        {
            if (csd.SessionPhase == SessionPhase.Countdown)
            {
                if (psd.SessionRunningTime < -3.0f
                    && csd.SessionRunningTime > -3.0f)
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + PacenoteType.number_3, 0));

                if (psd.SessionRunningTime < -2.0f
                    && csd.SessionRunningTime > -2.0f)
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + PacenoteType.number_2, 0));

                if (psd.SessionRunningTime < -1.0f
                    && csd.SessionRunningTime > -1.0f)
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + PacenoteType.number_1, 0));
            }

            if (psd.SessionPhase == SessionPhase.Countdown
                && csd.SessionPhase == SessionPhase.Green
                && cgs.PenaltiesData.PenaltyCause == PenatiesData.DetailedPenaltyCause.NONE)
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + "detail_go", 0));
                // load saved pace notes
                LoadRecePaceNotes(cgs);
                // load the corrections here for now
                LoadAndApplyCorrections(cgs.SessionData.TrackDefinition.name, cgs.CoDriverPacenotes);
            }
        }

        private void LoadAndApplyCorrections(string trackName, List<CoDriverPacenote> paceNotes)
        {
            // reset the corrections
            correctionsForCurrentSession = new List<CoDriverPacenote>();
            string pacenotesPath = GetPacenotesPath(trackName, true);
            if (Directory.Exists(pacenotesPath))
            {
                string correctionsFullFileName = Path.Combine(pacenotesPath, CoDriver.correctionsFileName);
                if (File.Exists(correctionsFullFileName))
                {
                    correctionsForCurrentSession = JsonConvert.DeserializeObject<List<CoDriverPacenote>>(Utilities.GetFileContentsJsonWithComment(correctionsFullFileName));
                    if (correctionsForCurrentSession == null)
                    {
                        // empty file, ensure the local var is initialised
                        correctionsForCurrentSession = new List<CoDriverPacenote>();
                    }
                }
            }
            // apply the corrections
            foreach (CoDriverPacenote correction in correctionsForCurrentSession)
            {
                bool appliedAsCorrection = false;
                foreach (CoDriverPacenote paceNote in paceNotes)
                {
                    bool logCorrectedPacenote = false;
                    if (Math.Abs(correction.Distance - paceNote.Distance) < 5)
                    {
                        // we've found a note to correct, if it's a corner and the correction is a corner or it's just
                        // a modifier, apply it. Otherwise just apply the distance
                        if (IsCorner(paceNote.Pacenote) && 
                            (IsCorner(correction.Pacenote) || (correction.Pacenote == PacenoteType.unknown && correction.Modifier != PacenoteModifier.none)))
                        {
                            if (correction.Pacenote != PacenoteType.unknown)
                            {
                                paceNote.Pacenote = correction.Pacenote;
                            }
                            if (correction.Modifier != PacenoteModifier.none)
                            {
                                paceNote.Modifier = correction.Modifier;
                            }
                            appliedAsCorrection = true;
                            logCorrectedPacenote = true;
                        }
                        if (correction.CorrectedDistance != null)
                        {
                            paceNote.Distance = (float)correction.CorrectedDistance;
                            appliedAsCorrection = true;
                            logCorrectedPacenote = true;
                        }
                        if (logCorrectedPacenote)
                        {
                            Console.WriteLine("Pacenote " + paceNote.ToString() + " corrected to " + correction.ToString());
                        }
                    }
                }
                if (!appliedAsCorrection)
                {
                    // look to insert this
                    for (int i = 0; i < paceNotes.Count; i++)
                    {
                        if (paceNotes[i].Pacenote == PacenoteType.detail_distance_call)
                        {
                            continue;
                        }
                        if (paceNotes[i].Distance > correction.Distance)
                        {
                            // we've found place to insert the new note
                            Console.WriteLine("Inserting pacenote " + correction.ToString());
                            paceNotes.Insert(i, correction);
                            break;
                        }
                    }
                }                
            }
        }

        private void LoadRecePaceNotes(GameStateData cgs)
        {
            if (CrewChief.gameDefinition.gameEnum == GameEnum.DIRT || CrewChief.gameDefinition.gameEnum == GameEnum.DIRT_2)
            {
                // if we're playing Dirt rally, there are no game-provided pace notes. The session clear should have taken care of this
                // but there are cases where it doesn't that I've still not debugged
                cgs.CoDriverPacenotes.Clear();
                correctionsForCurrentSession.Clear();
                historicCalls.Clear();
            }
            string pacenotesPath = GetPacenotesPath(cgs.SessionData.TrackDefinition.name, true);
            if (Directory.Exists(pacenotesPath))
            {
                string pacenotesFullFileName = Path.Combine(pacenotesPath, CoDriver.pacenotesFileName);
                if (File.Exists(pacenotesFullFileName))
                {
                    List<CoDriverPacenote> paceNotes = JsonConvert.DeserializeObject<List<CoDriverPacenote>>(Utilities.GetFileContentsJsonWithComment(pacenotesFullFileName));
                    if (paceNotes != null && paceNotes.Count > 0)
                    {
                        cgs.UseCrewchiefPaceNotes = true;
                        InsertDistanceData(paceNotes);
                        InsertFinish(paceNotes, cgs.SessionData.TrackDefinition.trackLength);
                        if (cgs.CoDriverPacenotes != null && cgs.CoDriverPacenotes.Count > 0)
                        {
                            Console.WriteLine("Replacing " + cgs.CoDriverPacenotes.Count + " game-provided stage notes with " + paceNotes.Count + " Crew Chief stage notes");
                        }
                        cgs.CoDriverPacenotes = paceNotes;
                    }
                }
            }
            this.lastProcessedPacenoteIdx = 0;
            this.isLost = false;
            this.lastProcessedLapDist = -1.0f;
        }

        private void InsertFinish(List<CoDriverPacenote> loadedPaceNotes, float trackLength)
        {
            int indexToInsertFinish = -1;
            for (int i=loadedPaceNotes.Count - 1; i>=0; i--)
            {
                CoDriverPacenote paceNote = loadedPaceNotes[i];
                if (paceNote.Pacenote == PacenoteType.detail_finish || paceNote.Pacenote == PacenoteType.detail_to_finish)
                {
                    return;
                }
                else if (indexToInsertFinish != -1 && paceNote.Distance <= trackLength)
                {
                    indexToInsertFinish = i + 1;
                    // allow the loop to continue here. We know where we want to insert the 'finish' pace note but continue iterating till we reach the start
                    // to ensure it's not in there already
                }
            }
            if (indexToInsertFinish > 0)
            {
                CoDriverPacenote finishPaceNote = new CoDriverPacenote();
                finishPaceNote.Distance = trackLength;
                finishPaceNote.Pacenote = PacenoteType.detail_to_finish;
                loadedPaceNotes.Insert(indexToInsertFinish, finishPaceNote);
            }
        }

        private void InsertDistanceData(List<CoDriverPacenote> loadedPaceNotes)
        {
            for (int i=1; i<loadedPaceNotes.Count; i++)
            {
                if (loadedPaceNotes[i].Pacenote == PacenoteType.detail_distance_call && loadedPaceNotes[i].Options == null)
                {
                    // this is an autogenerated distance placeholder.
                    // Get the distance from the previous pace note to the next proper pacenote
                    int nextIndex = i + 1;
                    while (nextIndex < loadedPaceNotes.Count)
                    {
                        CoDriverPacenote nextPacenote = loadedPaceNotes[nextIndex];
                        if (nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_centre
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_left
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_right
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_middle
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_out
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_keep_in
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_bumps
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_bump
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_ruts
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_deepruts
                            && nextPacenote.Pacenote != CoDriver.PacenoteType.detail_distance_call)
                        {
                            // next pace note is a real one so get the distance to call
                            float distanceToNext = loadedPaceNotes[i + 1].Distance - loadedPaceNotes[i].Distance;
                            if (distanceToNext >= minSpacingForAutoDistanceCall)
                            {
                                loadedPaceNotes[i].Options = CoDriver.GetClosestValueForDistanceCall(distanceToNext);
                            }
                            break;
                        }
                        nextIndex++;
                    }
                }
            }
        }

        // gets a (very) rough estimate of how slow the car might need to be as it travels through a slow corner.
        // This is used only to adjust the read-ahead distance when we're expecting the car to be slowing significantly.
        // The numbers here aren't intended to be exact or anything, they're just intended to allow the pace notes
        // after the corner to be delayed.
        private float GetSlowestExpectedSpeedForBatch(List<CoDriverPacenote> batch, float currentSpeed)
        {
            float slowestSpeed = currentSpeed;
            if (batch != null)
            {
                foreach (CoDriverPacenote paceNote in batch)
                {
                    switch (paceNote.Pacenote)
                    {
                        case PacenoteType.corner_1_left:
                        case PacenoteType.corner_1_right:
                        case PacenoteType.corner_left_acute:
                        case PacenoteType.corner_right_acute:
                            slowestSpeed = Math.Min(slowestSpeed, 15);
                            break;
                        case PacenoteType.corner_2_left:
                        case PacenoteType.corner_2_right:
                        case PacenoteType.corner_square_left:
                        case PacenoteType.corner_square_right:
                            slowestSpeed = Math.Min(slowestSpeed, 20);
                            break;
                        case PacenoteType.corner_3_left:
                        case PacenoteType.corner_3_right:
                        case PacenoteType.corner_open_hairpin_left:
                        case PacenoteType.corner_open_hairpin_right:
                            if (paceNote.Modifier.HasFlag(PacenoteModifier.detail_tightens))
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 25);
                            }
                            else if (paceNote.Modifier.HasFlag(PacenoteModifier.detail_double_tightens))
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 20);
                            }
                            else
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 30);
                            }
                            break;
                        case PacenoteType.corner_4_left:
                        case PacenoteType.corner_4_right:
                            if (paceNote.Modifier.HasFlag(PacenoteModifier.detail_tightens))
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 30);
                            }
                            else if (paceNote.Modifier.HasFlag(PacenoteModifier.detail_double_tightens))
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 20);
                            }
                            break;
                        default:
                            if (paceNote.Modifier.HasFlag(PacenoteModifier.detail_double_tightens))
                            {
                                slowestSpeed = Math.Min(slowestSpeed, 30);
                            }
                            break;
                    }
                }
            }
            return slowestSpeed;
        }

        private void ProcessPacenotes(GameStateData cgs, SessionData csd)
        {
            if (this.isLost)
                return;

            if (csd.SessionPhase == SessionPhase.Green
                && cgs.CoDriverPacenotes.Count > 0)
            {
                // NOTE: sometimes distance jumps significantly, that typically means we're lost on track.  Not sure we have to handle that though ("we're ***** lost" message?)

                // 4 secs of look ahead, by default.
                var currentSpeed = cgs.PositionAndMotionData.CarSpeed;
                // if our last batch is recent, use the expected slowest speed from it. Otherwise use our current speed.
                // readDist is recalculated on every iteration inside the loop
                float readDist;
                if (dynamicLookahead && cgs.Now < this.lastPlayedBatchTime.AddSeconds(3))
                {
                    // we've recently played a batch so use the expected speed from this batch
                    readDist = cgs.PositionAndMotionData.DistanceRoundTrack + 
                        this.lookaheadSecondsToUse * GetSlowestExpectedSpeedForBatch(this.lastPlayedOrAddedBatch, currentSpeed);
                }
                else
                {
                    readDist = cgs.PositionAndMotionData.DistanceRoundTrack + this.lookaheadSecondsToUse * currentSpeed;
                }
                var nextBatchDistance = this.FindNextBatchDistance(readDist, cgs, out var fragmentsInCurrBatch);
#if DEBUG
                var reachedFinish = false;
#endif  // DEBUG

                List<CoDriverPacenote> pacenotesInBatch = new List<CoDriverPacenote>();

                while (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count
                    && readDist > cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance)
                {
                    if (this.ShouldIgnorePacenote(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx]))
                    {
#if DEBUG
                        Console.WriteLine($"IGNORING PACENOTE: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
#endif  // DEBUG
                        ++this.lastProcessedPacenoteIdx;
                        continue;
                    }

                    // Finish is special: do not look ahead.
                    if (cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_finish
                        && cgs.PositionAndMotionData.DistanceRoundTrack < cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance)
                    {
#if DEBUG
                        reachedFinish = true;
#endif  // DEBUG
                        break;
                    }

                    // Play the main pacenote.
                    var mainPacenoteDist = cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance;
                    if (cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_distance_call)
                    {
                        // Distance call is not chained if "empty call" precedes it.
                        Console.WriteLine($"Playing pacenote: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                        if (Enum.TryParse<CoDriver.PacenoteType>("number_" + cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options, out var pacenote))
                            this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now), 0));
#if DEBUG
                        else

                            Console.WriteLine($"DISTANCE PARSE FAILED: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
#endif  // DEBUG
                    }
                    else
                    {
                        Console.WriteLine($"Playing pacenote: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  " +
                            $"with pacenote distance: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}  " +
                            $"at track distance {cgs.PositionAndMotionData.DistanceRoundTrack.ToString("0.000")}");
                        this.audioPlayer.playMessageImmediately(new QueuedMessage(GetPacenoteMessageID(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now), 0));
                        pacenotesInBatch.Add(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx]);
                    }

                    // Play modifiers.
                    playModifiers(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Modifier, cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance,
                        mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now);

                    var prevNoteDist = cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance;
                    var previousPacenoteType = cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote;

                    ++this.lastProcessedPacenoteIdx;

                    // If next call is one of the chained calls, play them.  All this might be too RBR specific, but if there
                    // will ever be other rally games added, all this can be tweaked based on game definition.
                    while (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count)
                    {
                        if (cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_distance_call)
                        {
                            Console.WriteLine($"Playing chained pacenote: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                            if (Enum.TryParse<CoDriver.PacenoteType>("number_" + cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options, out var pacenote))
                                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now), 0));
#if DEBUG
                            else
                                Console.WriteLine($"DISTANCE PARSE FAILED: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
#endif  // DEBUG
                            ++this.lastProcessedPacenoteIdx;
                            continue;
                        }
                        else if (Math.Abs(prevNoteDist - cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance) < this.chainedPacenoteThresholdMeters)
                        {
                            if (CoDriver.terminologies.chainedNotes.Contains(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote.ToString()))
                            {
                                Console.WriteLine($"Playing inserted chained pacenote: {CoDriver.PacenoteType.detail_into}  at: {prevNoteDist.ToString("0.000")}");
                                foreach (var pacenoteMessageID in this.GetChainedPacenoteMessageIDs(previousPacenoteType, cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote,
                                    mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now))
                                    this.audioPlayer.playMessageImmediately(new QueuedMessage(pacenoteMessageID, 0));

                                // play modifiers for this chained note
                                playModifiers(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Modifier, cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance,
                                    mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, currentSpeed, cgs.Now);

                                // NOTE: Not sure if we want to advance prevNoteDist, don't for now.
                                pacenotesInBatch.Add(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx]);
                                ++this.lastProcessedPacenoteIdx;

                                continue;
                            }
                        }
                        break;
                    }
                    // we now process the next note in the current set. Before we do, recalcuate the readDist and nextBatchDistance.
                    // This allows us to adjust read ahead distance to take into account the likelihood of the player having to slow
                    // significantly for a tight corner - currentSpeed may be much higher than the actual speed when he reaches a hairpin.
                    // This effectively delays calls after a slow corner to give the driver time to negotiate it before we make subsequent calls
                    if (dynamicLookahead)
                    {
                        readDist = cgs.PositionAndMotionData.DistanceRoundTrack + this.lookaheadSecondsToUse * GetSlowestExpectedSpeedForBatch(pacenotesInBatch, currentSpeed); ;
                        nextBatchDistance = this.FindNextBatchDistance(readDist, cgs, out fragmentsInCurrBatch);
                    }
                }

                if (pacenotesInBatch.Count > 0)
                {
                    this.lastPlayedOrAddedBatch.Clear();
                    this.lastPlayedOrAddedBatch.AddRange(pacenotesInBatch);
                    this.lastPlayedBatchTime = cgs.Now;
                }
#if DEBUG
                if (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count
                    && !reachedFinish)
                    Debug.Assert(nextBatchDistance == cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance);
#endif  // DEBUG
            }
        }

        private void playModifiers(CoDriver.PacenoteModifier modifier, float distance, float mainPacenoteDist, float nextBatchDistance, int fragmentsInCurrBatch, float speed, DateTime now)
        {
            if (modifier != CoDriver.PacenoteModifier.none)
            {
                foreach (var mod in Utilities.GetEnumFlags(modifier))
                {
                    if ((CoDriver.PacenoteModifier)mod != CoDriver.PacenoteModifier.none)
                    {
                        Console.WriteLine($"Playing modifier for pacenote: {mod}  at: {distance.ToString("0.000")}");
                        if (Enum.TryParse<CoDriver.PacenoteModifier>(mod.ToString(), out var modChecked))
                            this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(CoDriver.PacenoteType.unknown, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now, modChecked), 0));
#if DEBUG
                        else
                            Console.WriteLine($"MODIFIER PARSE FAILED: {mod}  at: {distance.ToString("0.000")}");
#endif  // DEBUG
                    }
                }
            }
        }

        // prevent too many 'into' sounds being stacked up in a single block of messages and block some specific combinations
        private bool canUseChaining(DateTime now, PacenoteType previousPacenoteType, PacenoteType pacenoteType)
        {
            if (IsCorner(previousPacenoteType) && pacenoteType == PacenoteType.detail_junction)
            {
                // don't allow "into junction" after a corner call as the corner will probably be the junction
                return false;
            }
            if (IsCorner(previousPacenoteType) && 
                (pacenoteType == PacenoteType.detail_ruts || pacenoteType == PacenoteType.detail_bumps || pacenoteType == PacenoteType.detail_bumpy || pacenoteType == PacenoteType.detail_deepruts))
            {
                // don't allow "into ruts" after a corner call as the ruts / bumps will probably be at the corner
                return false;
            }
            return (now - lastIntoSoundPlayed).TotalSeconds > 2;
        }

        private List<string> GetChainedPacenoteMessageIDs(PacenoteType previousPaceNoteType, PacenoteType pacenoteType, float mainPacenoteDist, 
            float nextBatchDistance, int fragmentsInCurrBatch, float speed, DateTime now)
        {
            var IDs = new List<string>();
            var id = this.GetPacenoteMessageID(pacenoteType, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now);

            if (canUseChaining(now, previousPaceNoteType, pacenoteType))
            {

                // first see if we have a compound "into_[whatever] sound, and if so use it:
                var idWithIntoPrefix = id.Insert(CoDriver.folderCodriverPrefix.Length, "cmp_into_");
#if DEBUG
                Console.WriteLine($"LOOKING FOR {idWithIntoPrefix}");
#endif  // DEBUG
                if (SoundCache.availableSounds.Contains(idWithIntoPrefix))
                {
#if DEBUG
                    Console.WriteLine($"PLAYING COMPOUND INSERTED PACENOTE: {idWithIntoPrefix}");
#endif  // DEBUG
                    IDs.Add(idWithIntoPrefix);
                }
                else if (intoToOver.ContainsKey(pacenoteType))
                {
                    // otherwise see if we can transform into X to over X
#if DEBUG
                    Console.WriteLine($"TRANSFORMING: into_{pacenoteType} TO over_{pacenoteType}");
#endif  // DEBUG
                    IDs.Add(this.GetPacenoteMessageID(intoToOver[pacenoteType], mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now));
                }
                else
                {
                    // just add the into and the X messages
#if DEBUG
                    Console.WriteLine("Falling back to INTO");
#endif  // DEBUG
                    IDs.Add(this.GetPacenoteMessageID(CoDriver.PacenoteType.detail_into, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now));
                    IDs.Add(id);
                }
                lastIntoSoundPlayed = now;
            }
            else
            {
                IDs.Add(id);
            }

            return IDs;
        }

        public float FindNextBatchDistance(float readDist, GameStateData cgs, out int fragmentsInCurrBatch)
        {
            var preprocPacenoteIdx = this.lastProcessedPacenoteIdx;
            var reachedFinish = false;

            fragmentsInCurrBatch = 0;

            while (preprocPacenoteIdx < cgs.CoDriverPacenotes.Count
                && readDist > cgs.CoDriverPacenotes[preprocPacenoteIdx].Distance)
            {
                if (this.ShouldIgnorePacenote(cgs.CoDriverPacenotes[preprocPacenoteIdx]))
                {
                    ++preprocPacenoteIdx;
                    continue;
                }

                // Finish is special: do not look ahead.
                if (cgs.CoDriverPacenotes[preprocPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_finish
                    && cgs.PositionAndMotionData.DistanceRoundTrack < cgs.CoDriverPacenotes[preprocPacenoteIdx].Distance)
                {
                    reachedFinish = true;
                    break;
                }

                var prevNoteDist = cgs.CoDriverPacenotes[preprocPacenoteIdx].Distance;

                // Handle modifiers.
                var modifier = cgs.CoDriverPacenotes[preprocPacenoteIdx].Modifier;
                if (modifier != CoDriver.PacenoteModifier.none)
                {
                    foreach (var mod in Utilities.GetEnumFlags(modifier))
                    {
                        if ((CoDriver.PacenoteModifier)mod != CoDriver.PacenoteModifier.none)
                        {
                            if (Enum.TryParse<CoDriver.PacenoteModifier>(mod.ToString(), out var modChecked))
                                ++fragmentsInCurrBatch;
                        }
                    }
                }

                ++preprocPacenoteIdx;
                ++fragmentsInCurrBatch;

                // Skip chained calls.
                while (preprocPacenoteIdx < cgs.CoDriverPacenotes.Count)
                {
                    if (cgs.CoDriverPacenotes[preprocPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_distance_call)
                    {
                        ++preprocPacenoteIdx;
                        ++fragmentsInCurrBatch;
                        continue;
                    }
                    else if (Math.Abs(prevNoteDist - cgs.CoDriverPacenotes[preprocPacenoteIdx].Distance) < this.chainedPacenoteThresholdMeters)
                    {
                        if (CoDriver.terminologies.chainedNotes.Contains(cgs.CoDriverPacenotes[preprocPacenoteIdx].Pacenote.ToString()))
                        {
                            ++preprocPacenoteIdx;

                            // Count chained pace notes as 2 fragments, as they are typically consist of 'over X' or 'into X'
                            fragmentsInCurrBatch += 2;
                            continue;
                        }
                    }

                    break;
                }
            }

            if (preprocPacenoteIdx < cgs.CoDriverPacenotes.Count
                && !reachedFinish)
                return cgs.CoDriverPacenotes[preprocPacenoteIdx].Distance;

            return -1.0f;
        }

        private string GetMessageID(PacenoteType pacenote, PacenoteModifier modifier)
        {
            var pacenoteStr = pacenote != PacenoteType.unknown ? pacenote.ToString() : modifier.ToString();
            var pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStr;
            if (pacenoteStr.StartsWith("corner_"))
            {
                var pacenoteStrRemapped = this.RemapPerChosenTerminology(pacenoteStr, pacenote);
                if (CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.NUMBER_FIRST)
                    pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStrRemapped;
                else if (CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.DIRECTION_FIRST)
                {
                    pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStrRemapped;

                    var reversedPacenoteString = pacenoteID + "_reversed";
                    if (SoundCache.availableSounds.Contains(reversedPacenoteString))
                        pacenoteID = reversedPacenoteString;
                    else
                        Console.WriteLine($"CoDriver: The sound: '{reversedPacenoteString}' is not available, reverting to: '{pacenoteID}'");
                }
                else if (CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.DESCRIPTIVE)
                {
                    pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStrRemapped;

                    var descriptivePacenoteString = pacenoteID + "_descriptive";
                    if (SoundCache.availableSounds.Contains(descriptivePacenoteString))
                        pacenoteID = descriptivePacenoteString;
                    else
                        Console.WriteLine($"CoDriver: The sound: '{descriptivePacenoteString}' is not available, reverting to: '{pacenoteID}'");
                }
                else if (CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.NUMBER_FIRST_REVERSED
                    || CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.DIRECTION_FIRST_REVERSED)
                {
                    // Invert 1:6 -> 6:1.
                    if (pacenoteStrRemapped == "corner_1_left")
                        pacenoteStrRemapped = "corner_6_left";
                    else if (pacenoteStrRemapped == "corner_2_left")
                        pacenoteStrRemapped = "corner_5_left";
                    else if (pacenoteStrRemapped == "corner_3_left")
                        pacenoteStrRemapped = "corner_4_left";
                    else if (pacenoteStrRemapped == "corner_4_left")
                        pacenoteStrRemapped = "corner_3_left";
                    else if (pacenoteStrRemapped == "corner_5_left")
                        pacenoteStrRemapped = "corner_2_left";
                    else if (pacenoteStrRemapped == "corner_6_left")
                        pacenoteStrRemapped = "corner_1_left";
                    else if (pacenoteStrRemapped == "corner_1_right")
                        pacenoteStrRemapped = "corner_6_right";
                    else if (pacenoteStrRemapped == "corner_2_right")
                        pacenoteStrRemapped = "corner_5_right";
                    else if (pacenoteStrRemapped == "corner_3_right")
                        pacenoteStrRemapped = "corner_4_right";
                    else if (pacenoteStrRemapped == "corner_4_right")
                        pacenoteStrRemapped = "corner_3_right";
                    else if (pacenoteStrRemapped == "corner_5_right")
                        pacenoteStrRemapped = "corner_2_right";
                    else if (pacenoteStrRemapped == "corner_6_right")
                        pacenoteStrRemapped = "corner_1_right";

                    if (CoDriver.cornerCallStyle == CoDriver.CornerCallStyle.DIRECTION_FIRST_REVERSED)
                    {
                        pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStrRemapped;

                        var reversedPacenoteString = pacenoteID + "_reversed";
                        if (SoundCache.availableSounds.Contains(reversedPacenoteString))
                            pacenoteID = reversedPacenoteString;
                        else
                            Console.WriteLine($"CoDriver: The sound: '{reversedPacenoteString}' is not available, reverting to: '{pacenoteID}'");
                    }
                    else
                        pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStrRemapped;
                }
            }
            return pacenoteID;
        }

        public string GetPacenoteMessageID(CoDriver.PacenoteType pacenote, float distance, float nextBatchDistance, int fragmentsInCurrBatch, float carSpeed, DateTime now,
            CoDriver.PacenoteModifier modifier = PacenoteModifier.none)
        {
            var pacenoteStr = pacenote != PacenoteType.unknown ? pacenote.ToString() : modifier.ToString();
            
            historicCalls.AddLast(new HistoricCall(pacenote, modifier, distance, now));

            var pacenoteID = GetMessageID(pacenote, modifier);
            
            // TODO: handle relaxed vs regular, and potentially handle complex messages (into glued).
            var distToNextBatch = Math.Abs(distance - nextBatchDistance);

            // For each additional fragment (phrase) in the current batch, increase the threshold to rushed messages.
            var fragmentThreshold = (fragmentsInCurrBatch - 1) * 0.2f;
            var rushedThresholdDistMeters = (this.rushedLookaheadSeconds + fragmentThreshold) * carSpeed;
            var rushedThresholdDistNoFragmentsMeters = this.rushedLookaheadSeconds * carSpeed;

            // rushed pace notes are a bit sticky - if we played a rushed pace note less than 1 second ago, the subsequent note should be rushed
            // even if we'd otherwise not rush it
            bool rushBecauseLastWasRushed = this.lastRushedPacenoteTime.AddSeconds(3.0 + (this.lastBatchFragmentCount - 1) * 0.3) > now;
            // always check if we're legitimately rushing
            bool rushBecauseSpeedOrQueueLength = (this.rushedLookaheadSeconds > 0.01f  // Below consider disabled
                      && distToNextBatch < rushedThresholdDistMeters)
                || carSpeed > rushedSpeedKmH / 3.6f; // above 140km/h

            if (rushBecauseLastWasRushed || rushBecauseSpeedOrQueueLength)
            {
                var rushedPacenoteID = pacenoteID + "_rushed";
                if (SoundCache.availableSounds.Contains(rushedPacenoteID))
                {
                    var reason = "Stickiness";
                    if (rushBecauseSpeedOrQueueLength)
                    {
                        reason = "Speed";
                        if (distToNextBatch < rushedThresholdDistNoFragmentsMeters)
                            reason = "DistNoFragments";
                        else if (distToNextBatch < rushedThresholdDistMeters)
                            reason = "Dist";
                    }

                    Console.WriteLine($"CoDriver: Using rushed version of sound: '{pacenoteID}'  carSpeed: {(carSpeed * 3.6).ToString("0.0")}km/h"
                        + $"  distanceToNextBatch: {distToNextBatch.ToString("0.0")}m  fragmentsInCurrBatch: {fragmentsInCurrBatch}  "
                        + $"  fragmentAddedThreshold: {(fragmentThreshold * carSpeed).ToString("0.0")}m"
                        + $"  rushedThresholdDistMeters: {rushedThresholdDistMeters.ToString("0.0")}m  Reason: {reason}");

                    pacenoteID = rushedPacenoteID;
                    // only update the last rushed pacenote time is this was a legitimately rushed pace note
                    if (rushBecauseSpeedOrQueueLength)
                    {
                        this.lastRushedPacenoteTime = now;
                        this.lastBatchFragmentCount = fragmentsInCurrBatch;
                    }
                }
#if DEBUG
                else
                    Console.WriteLine($"CoDriver: The sound: '{rushedPacenoteID}' is not available, reverting to: '{pacenoteID}'");
#endif  // DEBUG
            }

            return pacenoteID;
        }

        private string RemapPerChosenTerminology(string pacenoteStr, CoDriver.PacenoteType pacenote)
        {
            var mappedPacenoteStr = pacenoteStr;
            // Each style can have multiple terminologies, or at least one.  For now, match Janne's pack, but eventually it could be one more json file with terminology definitions.
            if (CoDriver.terminologies.termininologies.TryGetValue(CoDriver.cornerCallStyle.ToString(), out var terminology))
            {
                if (terminology.terminology.TryGetValue(pacenote.ToString(), out var mappedPacenote))
                    mappedPacenoteStr = mappedPacenote;
            }

#if DEBUG
            if (mappedPacenoteStr != pacenoteStr)
                Console.WriteLine($"PACENOTE RE-MAPPED FROM: {pacenoteStr}  TO: {mappedPacenoteStr}");
#endif  // DEBUG
            return mappedPacenoteStr;
        }

        public void PlayFinishMessage()
        {
            this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + CoDriver.PacenoteType.detail_finish, 0));
        }

        private bool ShouldIgnorePacenote(CoDriverPacenote pacenote)
        {
            if (pacenote == null)
            {
                return true;
            }
            switch (pacenote.Pacenote)
            {
                case CoDriver.PacenoteType.detail_start:
                case CoDriver.PacenoteType.detail_empty_call:
                case CoDriver.PacenoteType.detail_split:  // For now, ignore split/checkpoints, but eventually consider announcing something, maybe time, if not busy.
                    return true;
                case CoDriver.PacenoteType.detail_distance_call:
                    return pacenote.Options == null;
            }
            return false;
        }

        public override void respond(String voiceMessage)
        {
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_EARLIER_CALLS))
            {
                if (this.lookaheadSecondsToUse + earlierLaterStepSeconds <= CoDriver.maxLookaheadSeconds)
                {
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                    var newLookahead = this.lookaheadSecondsToUse + earlierLaterStepSeconds;
                    Console.WriteLine("Increasing lookahead from " + this.lookaheadSecondsToUse.ToString("0.0") + " seconds to " + newLookahead.ToString("0.0") + " seconds.");
                    this.lookaheadSecondsToUse = newLookahead;
                }
                else
                {
                    // TODO: need to specific "no, bugger off" response?
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderNo, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_LATER_CALLS))
            {
                if (this.lookaheadSecondsToUse - earlierLaterStepSeconds >= CoDriver.minLookaheadSeconds)
                {
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                    var newLookahead = this.lookaheadSecondsToUse - earlierLaterStepSeconds;
                    Console.WriteLine("Decreasing lookahead from " + this.lookaheadSecondsToUse.ToString("0.0") + " seconds to " + newLookahead.ToString("0.0") + " seconds.");
                    this.lookaheadSecondsToUse = newLookahead;
                }
                else
                {
                    // TODO: need to specific "no, bugger off" response?
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderNo, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_DECRIPTIONS))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = CornerCallStyle.DESCRIPTIVE;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_NUMBER_FIRST))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = this.preferReversedNumbers ? CornerCallStyle.NUMBER_FIRST_REVERSED : CornerCallStyle.NUMBER_FIRST;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_DIRECTION_FIRST))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = this.preferReversedNumbers ? CornerCallStyle.DIRECTION_FIRST_REVERSED : CornerCallStyle.DIRECTION_FIRST;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_START_RECORDING_STAGE_NOTES))
            {
                if (!this.inReceMode)
                {
                    this.inReceMode = true;
                    this.recePaceNotes.Clear();
                    if (CrewChief.currentGameState != null)
                    {
                        CrewChief.currentGameState.CoDriverPacenotes.Clear();
                    }
                }
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowledgeStartRecce, 0));
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_FINISH_RECORDING_STAGE_NOTES))
            {
                if (this.inReceMode)
                {
                    this.inReceMode = false;
                    if (this.recePaceNotes != null && this.recePaceNotes.Count > 0)
                    {
                        WriteRecePacenotes(lastStageName);
                    }
                    // weird bug: after finishing stage recce mid-stage, the app spews loads of pace note messages
                    
                    if (CrewChief.currentGameState != null)
                    {
                        this.lastProcessedPacenoteIdx = CrewChief.currentGameState.CoDriverPacenotes.Count - 1;
                        CrewChief.currentGameState.CoDriverPacenotes.Clear();
                    }
                    this.recePaceNotes.Clear();
                }
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowledgeEndRecce, 0));
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { TOGGLE_RALLY_RECCE_MODE }))
            {
                if (!this.inReceMode)
                {
                    this.inReceMode = true;
                    this.recePaceNotes.Clear();if (CrewChief.currentGameState != null)
                    {
                        CrewChief.currentGameState.CoDriverPacenotes.Clear();
                    }
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowledgeStartRecce, 0));
                }
                else
                {
                    this.inReceMode = false;
                    if (this.recePaceNotes != null && this.recePaceNotes.Count > 0)
                    {
                        WriteRecePacenotes(lastStageName);
                        // weird bug: after finishing stage recce mid-stage, the app spews loads of pace note messages
                        if (CrewChief.currentGameState != null)
                        {
                            this.lastProcessedPacenoteIdx = CrewChief.currentGameState.CoDriverPacenotes.Count - 1;
                            CrewChief.currentGameState.CoDriverPacenotes.Clear();
                        }
                        this.recePaceNotes.Clear();
                    }
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowledgeEndRecce, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORRECTION))
            {
                if (this.inReceMode)
                {
                    // save the previously added batch from the recce notes - these will be re-added if the correction doesn't
                    // insert any replacements
                    List<CoDriverPacenote> deletedPacenotes = new List<CoDriverPacenote>();
                    // remove the previously added batch from the recce notes
                    foreach (CoDriverPacenote pacenote in this.lastPlayedOrAddedBatch)
                    {
                        Console.WriteLine("Recce correction, removing pace note " + pacenote);
                        this.recePaceNotes.Remove(pacenote);
                        deletedPacenotes.Add(pacenote);
                    }
                    // remove the correction bit from the voice command
                    foreach (string correctionFragment in SpeechRecogniser.RALLY_CORRECTION)
                    {
                        if (voiceMessage.StartsWith(correctionFragment))
                        {
                            voiceMessage = voiceMessage.Remove(0, correctionFragment.Length).Trim();
                            break;
                        }
                    }
                    // now process this as a regular recce note
                    if (ProcessRecePaceNote(voiceMessage))
                    {
                        if (UserSettings.GetUserSettings().getBoolean("confirm_recce_pace_notes"))
                        {
                            this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderCorrection, 0));
                            ReplayLastPacenotesBatch(false);
                        }
                    }
                    else
                    {
                        // got 'correction' but no actual correction, reinstate the removed notes and say "eh?"
                        this.recePaceNotes.AddRange(deletedPacenotes);
                        Console.WriteLine("Voice message \"Correction, " + voiceMessage + "\" didn't produce any pace notes");
                        this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
                    }
                }
                else
                {
                    // regular non-recce correction
                    ProcessCorrection(voiceMessage);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_INSERT))
            {
                ProcessInsert(voiceMessage);
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.REPEAT_LAST_MESSAGE))
            {
                ReplayLastPacenotesBatch(false);
            }
            else if (this.inReceMode)
            {
                bool addedPacenote = ProcessRecePaceNote(voiceMessage);
                if (addedPacenote)
                {
                    if (UserSettings.GetUserSettings().getBoolean("confirm_recce_pace_notes"))
                    {
                        ReplayLastPacenotesBatch(true);
                    }
                }
                else
                {
                    Console.WriteLine("Voice message \"" + voiceMessage + "\" didn't produce any pace notes");
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
                }
            }
        }

        private void ReplayLastPacenotesBatch(bool addAcknowledge)
        {
            if (this.lastPlayedOrAddedBatch.Count > 0)
            {
                List<MessageFragment> confirmationFragments = new List<MessageFragment>();
                if (addAcknowledge)
                {
                    confirmationFragments.Add(MessageFragment.Text(AudioPlayer.folderAcknowlegeOK));
                }
                foreach (CoDriverPacenote pacenote in this.lastPlayedOrAddedBatch)
                {
                    if (pacenote.Pacenote != PacenoteType.detail_distance_call && pacenote.Pacenote != PacenoteType.unknown)
                    {
                        string sound = GetMessageID(pacenote.Pacenote, PacenoteModifier.none);
                        if (SoundCache.availableSounds.Contains(sound))
                        {
                            confirmationFragments.Add(MessageFragment.Text(sound));
                        }
                        else
                        {
                            Console.WriteLine("Successfully identified pace note modifier " + sound + " but can't find a sound for this");
                        }
                    }
                    if (pacenote.Modifier != PacenoteModifier.none)
                    {
                        foreach (var mod in Utilities.GetEnumFlags(pacenote.Modifier))
                        {
                            if ((CoDriver.PacenoteModifier)mod != CoDriver.PacenoteModifier.none)
                            {
                                if (Enum.TryParse<CoDriver.PacenoteModifier>(mod.ToString(), out var modChecked))
                                {
                                    string sound = GetMessageID(PacenoteType.unknown, modChecked);
                                    if (SoundCache.availableSounds.Contains(sound))
                                    {
                                        confirmationFragments.Add(MessageFragment.Text(sound));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Successfully identified pace note fragment " + sound + " but can't find a sound for this");
                                    }
                                }
                            }
                        }
                    }
                }
                this.audioPlayer.playMessageImmediately(new QueuedMessage("pacenote confirmation", 0, confirmationFragments));
            }
            else if (addAcknowledge)
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDidntUnderstand, 0));
            }
        }

        private void WriteRecePacenotes(string trackName)
        {
            string pacenotesPath = GetPacenotesPath(trackName, false);
            Directory.CreateDirectory(pacenotesPath);
            RenameExistingPacenotesFile(trackName);
            File.WriteAllText(Path.Combine(pacenotesPath, CoDriver.pacenotesFileName), JsonConvert.SerializeObject(this.recePaceNotes, Formatting.Indented));
        }

        // we allow ambiguous Dirt track names when loading but not when saving
        private string GetPacenotesPath(string trackName, bool allowAmbiguousPath)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", CrewChief.gameDefinition.gameEnum.ToString(), trackName);
            if (allowAmbiguousPath && (CrewChief.gameDefinition.gameEnum == GameEnum.DIRT || CrewChief.gameDefinition.gameEnum == GameEnum.DIRT_2))
            {
                // special case for dirt / dirt 2, try and load the stage name with the x and z positions and fall back to the ambiguous name if it's not there
                if (!Directory.Exists(path) && trackName.Contains("^"))
                {
                    string ambiguousTrackName = trackName.Split('^')[0];
                    string ambiguousPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", CrewChief.gameDefinition.gameEnum.ToString(), ambiguousTrackName);
                    if (Directory.Exists(ambiguousPath))
                    {
                        Console.WriteLine("Warning: using ambiguous track name " + ambiguousTrackName + " instead of full track name " + trackName);
                        return ambiguousPath;
                    }
                }
            }
            return path;
        }

        private void RenameExistingPacenotesFile(string trackName)
        {
            string pacenotesPath = GetPacenotesPath(trackName, false);
            string existingPacenotesFullPath = Path.Combine(pacenotesPath, CoDriver.pacenotesFileName);
            if (File.Exists(existingPacenotesFullPath))
            {
                string newFileName;
                int i = 1;
                do
                {
                    string filenameWithNoExtension = Path.GetFileNameWithoutExtension(CoDriver.pacenotesFileName);
                    string filenameExtension = Path.GetExtension(CoDriver.pacenotesFileName);
                    newFileName = Path.Combine(pacenotesPath, filenameWithNoExtension + "_" + i + filenameExtension);
                    i++;
                }
                while (File.Exists(newFileName));
                File.Move(existingPacenotesFullPath, newFileName);
            }
        }

        private bool ProcessRecePaceNote(string voiceMessage)
        {
            Console.WriteLine("Got stage recce voice message \"" + voiceMessage + "\"");
            float currentDistance = CrewChief.currentGameState == null ? 0 : CrewChief.currentGameState.PositionAndMotionData.DistanceRoundTrack;
            float distance = MainWindow.voiceOption == MainWindow.VoiceOptionEnum.ALWAYS_ON ?
                currentDistance : SpeechRecogniser.distanceWhenVoiceCommandStarted;
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_DISTANCE))
            {
                if (lastRecePacenoteWasDistance)
                {
                    Console.WriteLine("Skipping distance pacenote as we can't have 2 of these consecutively");
                    return true;
                }
                // manually add a distance call - the actual distance will be resolved on playback
                this.recePaceNotes.Add(new CoDriverPacenote { Pacenote = PacenoteType.detail_distance_call, Distance = distance });
                lastRecePacenoteWasDistance = true;
                return true;
            }
            else
            {
                // we're assuming that the pace note command is made *after* the obstacle / corner, so create the notes then use the created
                // notes to estimate how long the obstacle / corner is (i.e. the stage distance when the obstacle starts), and set that into the notes
                List<CoDriverPacenote> paceNotesToAdd = GetPacenotesFromVoiceCommand(voiceMessage);
                // one special case (eeewww). We missed a modifier during our previous corner call and have made a new command which is just "don't cut"
                // or something. In this case we attempt to insert that modifier into the last corner call
                if (paceNotesToAdd.Count == 0 && ContainsCornerModifier(voiceMessage))
                {
                    AppendModifierToLastCorner(voiceMessage);
                }
                else
                {
                    float distanceAtStartOfObstacle = distance - EstimateObstacleLength(paceNotesToAdd);
                    foreach (CoDriverPacenote paceNote in paceNotesToAdd)
                    {
                        paceNote.Distance = distanceAtStartOfObstacle;
                    }
                    // after each block of pace notes there'll be a distance placeholder. We always add this but the decision as to whether it'll be
                    // read is made on playback.
                    if (!lastRecePacenoteWasDistance)
                    {
                        // auto generate an optional distance note
                        paceNotesToAdd.Add(new CoDriverPacenote { Pacenote = PacenoteType.detail_distance_call, Distance = distance + 20 }); // the distance call needs to come some way after the obstacle is finished
                    }
                    this.recePaceNotes.AddRange(paceNotesToAdd);
                    // store these so we can remove them if we get a 'correction' call
                    this.lastPlayedOrAddedBatch.Clear();
                    this.lastPlayedOrAddedBatch.AddRange(paceNotesToAdd);
                    this.lastPlayedBatchTime = CrewChief.currentGameState.Now;
                }
                lastRecePacenoteWasDistance = false;
                return this.lastPlayedOrAddedBatch.Count > 0;
            }
        }

        private void AppendModifierToLastCorner(string voiceMessage)
        {
            Tuple<PacenoteType, PacenoteModifier> modifier = GetCornerPacenoteTypeWithModifier(new MutableString(voiceMessage), true);
            if (modifier.Item2 != PacenoteModifier.none)
            {
                // we have a modifier from the call, so see if we have a corner in the last batch and apply the modifier to it
                foreach (CoDriverPacenote paceNote in this.lastPlayedOrAddedBatch)
                {
                    if (IsCorner(paceNote.Pacenote))
                    {
                        // update the modifier
                        paceNote.Modifier = paceNote.Modifier | modifier.Item2;
                        break;
                    }                        
                }
            }
        }

        // gets the longest obstacle length from the batch (estimated)
        private float EstimateObstacleLength(List<CoDriverPacenote> paceNotesInBatch)
        {
            float distance = 10;    // any non-corner obstacle is assumed to be 10 metres from the start point to where the player makes the pace note command
            foreach (CoDriverPacenote paceNote in paceNotesInBatch)
            {
                bool isVeryLong = paceNote.Modifier.ToString().Contains("longlong");
                bool isLong = paceNote.Modifier.ToString().Contains("long") || paceNote.Modifier.ToString().Contains("tighens");
                switch (paceNote.Pacenote)
                {
                    case PacenoteType.corner_1_left:
                    case PacenoteType.corner_1_right:
                    case PacenoteType.corner_2_left:
                    case PacenoteType.corner_2_right:
                    case PacenoteType.corner_square_left:
                    case PacenoteType.corner_square_right:
                        // short corners
                        distance = Math.Max(distance, isVeryLong ? 60 : isLong ? 40 : 20);
                        break;
                    case PacenoteType.corner_3_left:
                    case PacenoteType.corner_3_right:
                        distance = Math.Max(distance, isVeryLong ? 90 : isLong ? 60 : 30);
                        break;
                    case PacenoteType.corner_open_hairpin_left:
                    case PacenoteType.corner_open_hairpin_right:
                        distance = Math.Max(distance, isVeryLong ? 140 : isLong ? 100 : 60);
                        break;
                    case PacenoteType.corner_4_left:
                    case PacenoteType.corner_4_right:
                    case PacenoteType.corner_left_acute:
                    case PacenoteType.corner_right_acute:
                    case PacenoteType.detail_left_entry_chicane:
                    case PacenoteType.detail_right_entry_chicane:
                        distance = Math.Max(distance, isVeryLong ? 120 : isLong ? 80 : 50);
                        break;
                    case PacenoteType.corner_5_left:
                    case PacenoteType.corner_5_right:// long corners
                        distance = Math.Max(distance, isVeryLong ? 150 : isLong ? 100 : 50);
                        break;
                    case PacenoteType.corner_6_left:
                    case PacenoteType.corner_6_right:
                    case PacenoteType.corner_flat_left:
                    case PacenoteType.corner_flat_right:
                        // long corners
                        distance = Math.Max(distance, isVeryLong ? 200 : isLong ? 100 : 50);
                        break;
                    case PacenoteType.detail_tunnel:
                        distance = Math.Max(distance, 50);
                        break;
                        // other cases?
                }
            }
            return distance;
        }

        private void ProcessInsert(string voiceMessage)
        {
            Console.WriteLine("Got insert voice message \"" + voiceMessage + "\"");
            if (CrewChief.currentGameState == null || CrewChief.currentGameState.SessionData == null || CrewChief.currentGameState.SessionData.TrackDefinition == null)
            {
                return;
            }
            string trackName = CrewChief.currentGameState.SessionData.TrackDefinition.name;
            float distance = MainWindow.voiceOption == MainWindow.VoiceOptionEnum.ALWAYS_ON ? 
                CrewChief.currentGameState.PositionAndMotionData.DistanceRoundTrack : SpeechRecogniser.distanceWhenVoiceCommandStarted;
            // remove the insert bit from the voice command
            foreach (string insertFragment in SpeechRecogniser.RALLY_INSERT)
            {
                if (voiceMessage.StartsWith(insertFragment))
                {
                    voiceMessage = voiceMessage.Remove(0, insertFragment.Length);
                    break;
                }
            }
            List<CoDriverPacenote> insertedNotes = GetPacenotesFromVoiceCommand(voiceMessage);
            float distanceAtStartOfObstacle = distance - EstimateObstacleLength(insertedNotes);
            foreach (CoDriverPacenote paceNote in insertedNotes)
            {
                paceNote.Distance = distanceAtStartOfObstacle;
            }
            if (insertedNotes.Count > 0)
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                correctionsForCurrentSession.AddRange(insertedNotes);
                WritePacenoteCorrections(trackName);
            }
        }

        // get the pace note (or sometimes multiple pace notes) from a single voice command
        private List<CoDriverPacenote> GetPacenotesFromVoiceCommand(string voiceMessage)
        {
            List<CoDriverPacenote> paceNotes = new List<CoDriverPacenote>();
            // as we parse the command we want to consume the recognised text, so wrap this in our little helper class
            MutableString voiceMessageWrapper = new MutableString(voiceMessage);

            // first see if we have a corner - we assume there can only be 1 corner for each command
            Tuple<PacenoteType, PacenoteModifier> cornerWithModifier = GetCornerPacenoteTypeWithModifier(voiceMessageWrapper, false);
            if (cornerWithModifier.Item1 != PacenoteType.unknown)
            {
                paceNotes.Add(new CoDriverPacenote() { Pacenote = cornerWithModifier.Item1, Modifier = cornerWithModifier.Item2, RawVoiceCommand = voiceMessage });
            }
            // reset the cursor in the remaining (uneaten) voice message and extract the other obstacle calls
            voiceMessageWrapper.ResetCursor();
            foreach (Tuple<PacenoteType, PacenoteModifier> obstacle in GetObstaclePacenoteTypesWithModifiers(voiceMessageWrapper))
            {
                if (obstacle.Item1 == PacenoteType.detail_care || obstacle.Item1 == PacenoteType.detail_caution || obstacle.Item1 == PacenoteType.detail_double_caution)
                {
                    // special case for danger / care / caution - ensure it's played first
                    paceNotes.Insert(0, new CoDriverPacenote() { Pacenote = obstacle.Item1, Modifier = obstacle.Item2, RawVoiceCommand = voiceMessage });
                }
                else
                {
                    paceNotes.Add(new CoDriverPacenote() { Pacenote = obstacle.Item1, Modifier = obstacle.Item2, RawVoiceCommand = voiceMessage });
                }
            }
            // if we've not been able to work out what's been said here, create an empty pace note to hold the misunderstood raw voice command
            if (paceNotes.Count == 0)
            {
                paceNotes.Add(new CoDriverPacenote() { RawVoiceCommand = voiceMessage, UnprocessedVoiceCommandText = voiceMessage });
            }
            else
            {
                string uneatenVoiceCommanFragments = voiceMessageWrapper.GetUnprocessedCommandText();
                foreach (CoDriverPacenote paceNote in paceNotes)
                {
                    paceNote.UnprocessedVoiceCommandText = uneatenVoiceCommanFragments;
                }
            }
            return paceNotes;
        }

        private void ProcessCorrection(string voiceMessage)
        {
            Console.WriteLine("Got correction voice message \"" + voiceMessage + "\"");
            if (historicCalls.Last == null || CrewChief.currentGameState == null
                || CrewChief.currentGameState.SessionData == null || CrewChief.currentGameState.SessionData.TrackDefinition == null)
            {
                Console.WriteLine("no pace note to correct");
                return;
            }
            string trackName = CrewChief.currentGameState.SessionData.TrackDefinition.name;
            CoDriver.Direction requestedDirection = Direction.UNKNOWN;
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_LEFT))
            {
                requestedDirection = Direction.LEFT;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_RIGHT))
            {
                requestedDirection = Direction.RIGHT;
            }
            List<HistoricCall> callsToCorrect = GetCallsToCorrect(requestedDirection);
            if (callsToCorrect != null && callsToCorrect.Count > 0)
            {
                // we might change the voice message to replace some contents, so stash it first so we can add the raw message to the note
                string rawVoiceMessage = voiceMessage;
                // check if we're moving a call:
                bool moveEarlier = SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_EARLIER);
                bool moveLater = SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_LATER);
                // remove that text from the voice command
                foreach (string correctionWord in SpeechRecogniser.RALLY_EARLIER)
                {
                    voiceMessage = voiceMessage.Replace(correctionWord, "").Trim();
                }
                foreach (string correctionWord in SpeechRecogniser.RALLY_LATER)
                {
                    voiceMessage = voiceMessage.Replace(correctionWord, "").Trim();
                }
                bool correctionIncludesCornerModifier = ContainsCornerModifier(voiceMessage);
                bool appliedCorrection = false;
                foreach (HistoricCall callToCorrect in callsToCorrect)
                {
                    // special case for corner corrections
                    // we might have just called "correction don't cut" here, so our PacenoteType from the correction will be unknown so check for modifier as well as corner
                    if (IsCorner(callToCorrect.callType) || correctionIncludesCornerModifier)
                    {
                        Direction direction = GetDirectionFromPaceNote(callToCorrect.callType);
                        if (requestedDirection == Direction.UNKNOWN)
                        {
                            // if no direction was specified, add it to the command so the parser can work
                            foreach (string directionWord in direction == Direction.LEFT ? SpeechRecogniser.RALLY_LEFT : SpeechRecogniser.RALLY_RIGHT)
                            {
                                foreach (string correctionWord in SpeechRecogniser.RALLY_CORRECTION)
                                {
                                    voiceMessage = voiceMessage.Replace(correctionWord.ToLower(), directionWord.ToLower());
                                }
                            }
                        }
                        Tuple<PacenoteType, PacenoteModifier> cornerWithModifier = GetCornerPacenoteTypeWithModifier(new MutableString(voiceMessage), true);
                        // check we've been able to derive a call type, modifier or move directive
                        if (cornerWithModifier.Item1 == PacenoteType.unknown && cornerWithModifier.Item2 == PacenoteModifier.none && !moveEarlier && !moveLater)
                        {
                            Console.WriteLine("Unable to create a usable correction from " + rawVoiceMessage);
                        }
                        else
                        {
                            appliedCorrection = true;
                            CreateCorrection(callToCorrect, cornerWithModifier.Item1, cornerWithModifier.Item2, moveEarlier, moveLater, rawVoiceMessage);
                        }
                    }
                    else
                    {
                        // also move non-corner calls if we have a move request
                        if (moveEarlier || moveLater)
                        {
                            appliedCorrection = true;
                            CreateCorrection(callToCorrect, callToCorrect.callType, callToCorrect.modifier, moveEarlier, moveLater, rawVoiceMessage);
                        }
                    }
                }
                if (appliedCorrection)
                {
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.folderAcknowlegeOK, 0));
                    WritePacenoteCorrections(trackName);
                }
            }
        }

        private void CreateCorrection(HistoricCall callToCorrect, PacenoteType pacenoteType, PacenoteModifier pacenoteModifier, bool moveEarlier, bool moveLater, string rawVoiceMessage)
        {
            Console.WriteLine("Correcting existing pace note " + callToCorrect.ToString() + " at distance " + callToCorrect.callDistance + " to be " + pacenoteType + ":" + pacenoteModifier);

            CoDriverPacenote existingCorrection = FindMatchingCorrection(callToCorrect.callType, callToCorrect.callDistance);
            // we might be correcting distance, but this is optional
            float? correctedDistance = null;
            if (existingCorrection != null)
            {
                if (moveEarlier)
                {
                    correctedDistance = existingCorrection.CorrectedDistance == null ? existingCorrection.Distance - 50 : existingCorrection.CorrectedDistance - 50;
                }
                else if (moveLater)
                {
                    correctedDistance = existingCorrection.CorrectedDistance == null ? existingCorrection.Distance + 50 : existingCorrection.CorrectedDistance + 50;
                }
                existingCorrection.Pacenote = pacenoteType;
                existingCorrection.Modifier = pacenoteModifier;
                existingCorrection.CorrectedDistance = correctedDistance;
                existingCorrection.RawVoiceCommand = rawVoiceMessage;
            }
            else
            {
                if (moveEarlier)
                {
                    correctedDistance = callToCorrect.callDistance - 50;
                }
                else if (moveLater)
                {
                    correctedDistance = callToCorrect.callDistance + 50;
                }
                correctionsForCurrentSession.Add(new CoDriverPacenote()
                {
                    Distance = callToCorrect.callDistance,
                    CorrectedDistance = correctedDistance,
                    Pacenote = pacenoteType,
                    Modifier = pacenoteModifier,
                    RawVoiceCommand = rawVoiceMessage
                });
            }
        }
        
        private CoDriverPacenote FindMatchingCorrection(PacenoteType pacenote, float distance)
        {
            if (pacenote.ToString().StartsWith("corner_"))
            foreach (CoDriverPacenote paceNoteCorrection in this.correctionsForCurrentSession)
            {
                if (Math.Abs(paceNoteCorrection.Distance - distance) < 5)
                {
                    return paceNoteCorrection;
                }
            }
            return null;
        }

        private void WritePacenoteCorrections(string trackName)
        {
            string pacenotesPath = GetPacenotesPath(trackName, false);
            Directory.CreateDirectory(pacenotesPath);
            File.WriteAllText(Path.Combine(pacenotesPath, CoDriver.correctionsFileName), JsonConvert.SerializeObject(this.correctionsForCurrentSession, Formatting.Indented));
        }

        private List<HistoricCall> GetCallsToCorrect(Direction requestedDirection)
        {
            // Count back through the historic calls to find the first one that we've passed and that matches the corner direction (if we specified one).
            // If we find a call to correct and it's for a corner > 200 metres behind us, assume we've not been able to find the appropriate call
            var historicCallNode = this.historicCalls.Last;
            float distance = MainWindow.voiceOption == MainWindow.VoiceOptionEnum.ALWAYS_ON ?
                    CrewChief.currentGameState.PositionAndMotionData.DistanceRoundTrack : SpeechRecogniser.distanceWhenVoiceCommandStarted;
            while (historicCallNode != null
                && (historicCallNode.Value.callDistance > distance   // we've not reached this pacenote
                    || (requestedDirection != Direction.UNKNOWN && requestedDirection != GetDirectionFromPaceNote(historicCallNode.Value.callType)))) // this pacenote's direction is incorrect
            {
                historicCallNode = historicCallNode.Previous;
            }
            if (historicCallNode == null || distance - historicCallNode.Value.callDistance > 400 /* this pace note is for an obstacle / corner 400m behind us*/)
            {
                Console.WriteLine("Unable to find a pacenote to correct");
                return null;
            }

            // at this point we have the first historic call we want to correct, get this and the other calls made at the same distance
            List<HistoricCall> historicCalls = new List<HistoricCall>();
            float distanceOfFirstCorrectedCall = historicCallNode.Value.callDistance;
            historicCalls.Add(historicCallNode.Value);
            var preceedingCallNode = historicCallNode.Previous;
            while (preceedingCallNode != null)
            {
                if (preceedingCallNode.Value.callDistance == distanceOfFirstCorrectedCall)
                {
                    historicCalls.Add(preceedingCallNode.Value);
                    preceedingCallNode = preceedingCallNode.Previous;
                }
                else
                {
                    break;
                }
            }
            return historicCalls;
        }

        private Direction GetDirectionFromPaceNote(PacenoteType paceNote)
        {
            if (paceNote.ToString().ToLower().Contains("left"))
                return Direction.LEFT;
            else if (paceNote.ToString().ToLower().Contains("right"))
                return Direction.RIGHT;
            else
                return Direction.UNKNOWN;
        }
        
        private Tuple<PacenoteType, PacenoteModifier> GetCornerPacenoteTypeWithModifier(MutableString voiceMessageWrapper, bool allowModifierOnly)
        {
            Tuple<PacenoteType, PacenoteModifier> result = new Tuple<PacenoteType, PacenoteModifier>(PacenoteType.unknown, PacenoteModifier.none);
            bool gotCornerType = false;
            foreach (string key in possibleCornerCommands.Keys)
            {
                if (voiceMessageWrapper.FindAndRemove(key, false, true))
                {
                    result = new Tuple<PacenoteType, PacenoteModifier> ((PacenoteType)possibleCornerCommands[key], GetModifier(voiceMessageWrapper));
                    gotCornerType = true;
                    break;
                }
            }
            if (!gotCornerType && allowModifierOnly)
            {
                result = new Tuple<PacenoteType, PacenoteModifier>(PacenoteType.unknown, GetModifier(voiceMessageWrapper));
            }
            return result;
        }

        private HashSet<Tuple<PacenoteType, PacenoteModifier>> GetObstaclePacenoteTypesWithModifiers(MutableString voiceMessageWrapper)
        {
            HashSet<Tuple<PacenoteType, PacenoteModifier>> matches = new HashSet<Tuple<PacenoteType, PacenoteModifier>>();
            foreach (string[] command in SpeechRecogniser.RallyObstacleCommands)
            {
                if (voiceMessageWrapper.FindAndRemove(command, false, true))
                {
                    matches.Add(new Tuple<PacenoteType, PacenoteModifier>(obstaclePacenoteTypes[command], GetModifier(voiceMessageWrapper)));
                }
            }
            return matches;
        }

        // this is quite specific - we only want modifiers that come after the position in the string
        // where we got our previous command
        private PacenoteModifier GetModifier(MutableString voiceMessageWrapper)
        {
            PacenoteModifier modifier = PacenoteModifier.none;
            bool allowCut = true;
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_TIGHTENS_BAD, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_double_tightens;
            }
            if (!voiceMessageWrapper.ContainsAny(SpeechRecogniser.RALLY_TIGHTENS_THEN_OPENS, true)
                && !voiceMessageWrapper.ContainsAny(SpeechRecogniser.RALLY_OPENS_THEN_TIGHTENS, true)
                && voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_TIGHTENS, true, false))
            {
                // additional check here - we don't want this to trigger for "tightens then opens" / "opens then tightens"
                modifier = modifier | PacenoteModifier.detail_tightens;
            }
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_DONT_CUT, true, false))
            {
                allowCut = false;   // hack... cases where multiple notes are stacked in a single command can have cut and don't cut
                                    // This doesn't really work because the ordering is lost so we end up with "don't cut, cut"
                modifier = modifier | PacenoteModifier.detail_dont_cut;
            }
            if (allowCut && voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_CUT, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_cut;
            }
            if (!voiceMessageWrapper.ContainsAny(SpeechRecogniser.RALLY_TIGHTENS_THEN_OPENS, true)
                && !voiceMessageWrapper.ContainsAny(SpeechRecogniser.RALLY_OPENS_THEN_TIGHTENS, true)
                && voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_OPENS, true, false))
            {
                // additional check here - we don't want this to trigger for "tightens then opens" / "opens then tightens"
                modifier = modifier | PacenoteModifier.detail_opens;
            }
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_LONGLONG, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_longlong;
            }
            else if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_LONG, true, false)) /*long OR very long, not both*/
            {
                modifier = modifier | PacenoteModifier.detail_long;
            }
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_WIDENS, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_wideout;
            }
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_MAYBE, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_maybe;
            }
            if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_PLUS, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_plus;
            }
            else if (voiceMessageWrapper.FindAndRemove(SpeechRecogniser.RALLY_MINUS, true, false))
            {
                modifier = modifier | PacenoteModifier.detail_minus;
            }
            return modifier;
        }

        private bool IsCorner(PacenoteType pacenoteType)
        {
            return pacenoteType.ToString().Contains("corner");
        }

        private bool ContainsCornerModifier(string voiceMessage)
        {
            return SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_DONT_CUT)
                || SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CUT)
                || SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_TIGHTENS)
                || SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_TIGHTENS_BAD)
                || SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_WIDENS);
        }
    }

    class MutableString
    {
        string contents;
        int cursorPosition = 0;
        public MutableString(string contents)
        {
            this.contents = contents;
        }

        public string GetString()
        {
            return contents;
        }

        public void ResetCursor()
        {
            this.cursorPosition = 0;
        }
        
        // return true is the text is in our internal string, and remove it from the string.
        // If fromCursorPosition is true, we start the search from where our cursor is. If 
        // updateCursor is true we move the cursor to the start of the located string.
        //
        // When looking for an obstacle or corner, we want to search the whole string and move the cursor
        // to the position after the match. Then we search for associated modifiers from the cursor position
        // but as there can be multiple modifiers, we keep the cursor at the same place so we can be sure to
        // find all the modifiers after the corner or obstacle.
        //
        // This logic means that a corner will have all subsequent modifiers applied to it, even when the command
        // goes corner->modifier1->obstacle->modifier2. This is a bit of an edge case.
        public bool FindAndRemove(string text, bool fromCursorPosition, bool updateCursor)
        {
            if (fromCursorPosition && this.cursorPosition >= this.contents.Length)
            {
                return false;
            }
            string substringToSearch = fromCursorPosition ? this.contents.Substring(cursorPosition) : this.contents;
            int matchIndex = substringToSearch.IndexOf(text);
            if (matchIndex != -1)
            {
                int startPoint = fromCursorPosition ? this.cursorPosition + matchIndex : matchIndex;
                if (updateCursor)
                {
                    this.cursorPosition = startPoint;
                }
                this.contents = this.contents.Remove(startPoint, text.Length).Trim();
                return true;
            }
            return false;
        }

        public bool FindAndRemove(string[] text, bool fromCursorPosition, bool updateCursorPosition)
        {
            foreach (string item in text)
            {
                if (FindAndRemove(item, fromCursorPosition, updateCursorPosition))
                {
                    return true;
                }
            }
            return false;
        }

        public bool ContainsAny(string[] text, bool fromCursorPosition)
        {
            if (fromCursorPosition && cursorPosition >= this.contents.Length)
            {
                return false;
            }
            foreach (string item in text)
            {
                int startIndex = fromCursorPosition ? this.cursorPosition : 0;
                if (this.contents.Substring(startIndex).Contains(item))
                {
                    return true;
                }
            }
            return false;
        }

        // get any text fragments that haven't been successfully processed, or null if all the message was processed.
        public string GetUnprocessedCommandText()
        {
            string uneatenText = this.contents.Trim();
            if (uneatenText.Length > 0)
            {
                Console.WriteLine("Voice command fragments left over: \"" + contents + "\"");
                return uneatenText;
            }
            else
            {
                return null;
            }
        }
    }
}
