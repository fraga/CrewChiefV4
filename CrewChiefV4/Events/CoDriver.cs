﻿/*
 * This monitor handles rally co-driver pacenotes.
 * 
 * Official website: thecrewchief.org 
 * License: MIT
 */
using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            unknown = 20000
        }

        [Flags]
        public enum PacenoteModifier : int
        {
            none = 0,
            detail_narrows = 1,
            detail_wideout = 2,
            detail_tightens = 4,
            detail_dont_cut = 32,
            detail_cut = 64,
            detail_double_tightens = 128,
            detail_long = 1024,
            detail_maybe = 8192
        }

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

        public class Terminology
        {
            public Dictionary<string, string> terminology = new Dictionary<string, string>();
        }

        public class Terminologies
        {
            public Dictionary<string, CoDriver.Terminology> termininologies = new Dictionary<string, CoDriver.Terminology>();
            public HashSet<string> chainedNotes = new HashSet<string>();
        }

        public class HistoricCornerCall
        {
            public DateTime callTime = DateTime.MinValue;
            public float callDistance = 0;
            public PacenoteType callType = PacenoteType.unknown;
            public HistoricCornerCall()
            {

            }
            public HistoricCornerCall(PacenoteType callType, float callDistance, DateTime callTime)
            {
                this.callType = callType;
                this.callDistance = callDistance;
                this.callTime = callTime;
            }
        }

        public class PaceNoteCorrection
        {
            public float distance;
            public string pacenoteType;
        }
        
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

        private float lookaheadSecondsToUse;
        private const float maxLookaheadSeconds = 10f;
        private const float minLookaheadSeconds = 0.5f;

        private int lastProcessedPacenoteIdx = 0;
        private bool isLost = false;
        private float lastProcessedLapDist = -1.0f;

        // used when switching between descriptive and numeric corner calls
        private bool preferReversedNumbers = false;

        // Sound folders.
        private static string folderCodriverPrefix = "codriver/";
        private static string loadedCodriverPrefix = "";

        // These are to be combined with the folderCodriverPrefix string.
        public static string folderFalseStart;
        public static string folderMicCheck;

        private DateTime lastRushedPacenoteTime = DateTime.MinValue;
        private int lastBatchFragmentCount = 0;

        private LinkedList<HistoricCornerCall> historicCalls = new LinkedList<HistoricCornerCall>();

        private List<PaceNoteCorrection> correctionsForCurrentSession = new List<PaceNoteCorrection>();

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
    }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            var pgs = previousGameState;

            if (pgs == null)
                return;

            var cgs = currentGameState;
            var csd = currentGameState.SessionData;
            var psd = previousGameState.SessionData;

            this.ProcessRaceStart(cgs, csd, psd);
            this.ProcessPenalties(cgs, pgs);
            this.ProcessLost(cgs, pgs);
            this.ProcessPacenotes(cgs, csd);
        }

        private void ProcessLost(GameStateData cgs, GameStateData pgs)
        {
            if (cgs.SessionData.SessionPhase != SessionPhase.Green
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
                // load the corrections here for now
                LoadCorrections(cgs.SessionData.TrackDefinition.name);
            }
        }

        private void LoadCorrections(string trackName)
        {
            string correctionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", GameEnum.RBR.ToString(), trackName, "corrections.json");
            if (File.Exists(correctionsPath))
            {
                correctionsForCurrentSession = JsonConvert.DeserializeObject<List<PaceNoteCorrection>>(Utilities.GetFileContentsJsonWithComment(correctionsPath));
            }
            if (correctionsForCurrentSession == null)
            {
                // empty file
                correctionsForCurrentSession = new List<PaceNoteCorrection>();
            }
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
                var speed = cgs.PositionAndMotionData.CarSpeed;
                var readDist = cgs.PositionAndMotionData.DistanceRoundTrack + this.lookaheadSecondsToUse * speed;
#if DEBUG
                var reachedFinish = false;
#endif  // DEBUG

                var nextBatchDistance = this.FindNextBatchDistance(readDist, cgs, out var fragmentsInCurrBatch);

                while (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count
                    && readDist > cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance)
                {
                    if (this.ShouldIgnorePacenote(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote))
                    {
                        Console.WriteLine($"IGNORING PACENOTE: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
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
                        Console.WriteLine($"PLAYING PACENOTE: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                        if (Enum.TryParse<CoDriver.PacenoteType>("number_" + cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options, out var pacenote))
                            this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, cgs.Now), 0));
                        else
                            Console.WriteLine($"DISTANCE PARSE FAILED: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");

                    }
                    else
                    {
                        Console.WriteLine($"PLAYING PACENOTE: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  " +
                            $"with pacenote distance: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}  " +
                            $"at track distance {cgs.PositionAndMotionData.DistanceRoundTrack.ToString("0.000")}");
                        this.audioPlayer.playMessageImmediately(new QueuedMessage(GetPacenoteMessageID(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, cgs.Now), 0));
                    }

                    // Play modifiers.
                    var modifier = cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Modifier;
                    if (modifier != CoDriver.PacenoteModifier.none)
                    {
                        foreach (var mod in Utilities.GetEnumFlags(modifier))
                        {
                            if ((CoDriver.PacenoteModifier)mod != CoDriver.PacenoteModifier.none)
                            {
                                Console.WriteLine($"PLAYING MODIFIER PACENOTE: {mod}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                                if (Enum.TryParse<CoDriver.PacenoteModifier>(mod.ToString(), out var modChecked))
                                    this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(CoDriver.PacenoteType.unknown, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, cgs.Now, modChecked), 0));
                                else
                                    Console.WriteLine($"MODIFIER PARSE FAILED: {mod}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                            }
                        }
                    }

                    var prevNoteDist = cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance;

                    ++this.lastProcessedPacenoteIdx;

                    // If next call is one of the chained calls, play them.  All this might be too RBR specific, but if there
                    // will ever be other rally games added, all this can be tweaked based on game definition.
                    while (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count)
                    {
                        if (cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote == CoDriver.PacenoteType.detail_distance_call)
                        {
                            Console.WriteLine($"PLAYING CHAINED PACENOTE: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");
                            if (Enum.TryParse<CoDriver.PacenoteType>("number_" + cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options, out var pacenote))
                                this.audioPlayer.playMessageImmediately(new QueuedMessage(this.GetPacenoteMessageID(pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, cgs.Now), 0));
                            else
                                Console.WriteLine($"DISTANCE PARSE FAILED: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Options}  at: {cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance.ToString("0.000")}");

                            ++this.lastProcessedPacenoteIdx;

                            continue;
                        }
                        else if (Math.Abs(prevNoteDist - cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance) < this.chainedPacenoteThresholdMeters)
                        {
                            if (CoDriver.terminologies.chainedNotes.Contains(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote.ToString()))
                            {
                                Console.WriteLine($"PLAYING INSERTED CHAINED PACENOTE: {CoDriver.PacenoteType.detail_into}  at: {prevNoteDist.ToString("0.000")}");
                                foreach (var pacenoteMessageID in this.GetChainedPacenoteMessageIDs(cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Pacenote, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, cgs.Now))
                                    this.audioPlayer.playMessageImmediately(new QueuedMessage(pacenoteMessageID, 0));

                                // NOTE: Not sure if we want to advance prevNoteDist, don't for now.

                                ++this.lastProcessedPacenoteIdx;

                                continue;
                            }
                        }

                        break;
                    }
                }

#if DEBUG
                if (this.lastProcessedPacenoteIdx < cgs.CoDriverPacenotes.Count
                    && !reachedFinish)
                    Debug.Assert(nextBatchDistance == cgs.CoDriverPacenotes[this.lastProcessedPacenoteIdx].Distance);
#endif  // DEBUG
            }
        }

        private List<string> GetChainedPacenoteMessageIDs(PacenoteType pacenoteType, float mainPacenoteDist, float nextBatchDistance, int fragmentsInCurrBatch, float speed, DateTime now)
        {
            var IDs = new List<string>();
            var id = this.GetPacenoteMessageID(pacenoteType, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now);

            // first see if we have a compound "into_[whatever] sound, and if so use it:
            var idWithIntoPrefix = id.Insert(CoDriver.folderCodriverPrefix.Length, "cmp_into_");
            Console.WriteLine($"LOOKING FOR {idWithIntoPrefix}");
            if (SoundCache.availableSounds.Contains(idWithIntoPrefix))
            {
                Console.WriteLine($"PLAYING COMPOUND INSERTED PACENOTE: {idWithIntoPrefix}");
                IDs.Add(idWithIntoPrefix);
            }
            else if (intoToOver.ContainsKey(pacenoteType))
            {
                // otherwise see if we can transform into X to over X
                Console.WriteLine($"TRANSFORMING: into_{pacenoteType} TO over_{pacenoteType}");
                IDs.Add(this.GetPacenoteMessageID(intoToOver[pacenoteType], mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now));
            }
            else
            {
                // just add the into and the X messages
                Console.WriteLine("Falling back to INTO");
                IDs.Add(this.GetPacenoteMessageID(CoDriver.PacenoteType.detail_into, mainPacenoteDist, nextBatchDistance, fragmentsInCurrBatch, speed, now));
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
                if (this.ShouldIgnorePacenote(cgs.CoDriverPacenotes[preprocPacenoteIdx].Pacenote))
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

        public string GetPacenoteMessageID(CoDriver.PacenoteType pacenote, float distance, float nextBatchDistance, int fragmentsInCurrBatch, float carSpeed, DateTime now,
            CoDriver.PacenoteModifier modifier = PacenoteModifier.none)
        {
            PaceNoteCorrection correction = FindMatchingCorrection(pacenote, distance);
            if (correction != null)
            {
                PacenoteType correctedPacenote;
                if (Enum.TryParse<PacenoteType>(correction.pacenoteType, out correctedPacenote))
                {
                    Console.WriteLine($"CoDriver: Correcting {pacenote} to: {correctedPacenote}");
                    pacenote = correctedPacenote;
                }
            }

            var pacenoteStr = pacenote != PacenoteType.unknown ? pacenote.ToString() : modifier.ToString();
            var pacenoteID = CoDriver.folderCodriverPrefix + pacenoteStr;
            if (pacenoteStr.StartsWith("corner_"))
            {
                historicCalls.AddLast(new HistoricCornerCall(pacenote, distance, now));
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
                else
                    Console.WriteLine($"CoDriver: The sound: '{rushedPacenoteID}' is not available, reverting to: '{pacenoteID}'");
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

            if (mappedPacenoteStr != pacenoteStr)
                Console.WriteLine($"PACENOTE RE-MAPPED FROM: {pacenoteStr}  TO: {mappedPacenoteStr}");

            return mappedPacenoteStr;
        }

        public void PlayFinishMessage()
        {
            this.audioPlayer.playMessageImmediately(new QueuedMessage(CoDriver.folderCodriverPrefix + CoDriver.PacenoteType.detail_finish, 0));
        }

        private bool ShouldIgnorePacenote(CoDriver.PacenoteType pacenote)
        {
            switch (pacenote)
            {
                case CoDriver.PacenoteType.detail_start:
                case CoDriver.PacenoteType.detail_empty_call:
                case CoDriver.PacenoteType.detail_split:  // For now, ignore split/checkpoints, but eventually consider announcing something, maybe time, if not busy.
                    return true;
            }

            return false;
        }

        public override void respond(String voiceMessage)
        {
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_EARLIER_CALLS))
            {
                if (this.lookaheadSecondsToUse < CoDriver.maxLookaheadSeconds)
                {
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                    var newLookahead = this.lookaheadSecondsToUse + 0.5f;
                    Console.WriteLine("Increasing lookahead from " + this.lookaheadSecondsToUse.ToString("0.0") + " seconds to " + newLookahead.ToString("0.0") + " seconds.");
                    this.lookaheadSecondsToUse = newLookahead;
                }
                else
                {
                    // TODO: need to specific "no, bugger off" response?
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNo, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_LATER_CALLS))
            {
                if (this.lookaheadSecondsToUse > CoDriver.minLookaheadSeconds)
                {
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                    var newLookahead = this.lookaheadSecondsToUse - 0.5f;
                    Console.WriteLine("Decreasing lookahead from " + this.lookaheadSecondsToUse.ToString("0.0") + " seconds to " + newLookahead.ToString("0.0") + " seconds.");
                    this.lookaheadSecondsToUse = newLookahead;
                }
                else
                {
                    // TODO: need to specific "no, bugger off" response?
                    this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNo, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_DECRIPTIONS))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = CornerCallStyle.DESCRIPTIVE;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_NUMBER_FIRST))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = this.preferReversedNumbers ? CornerCallStyle.NUMBER_FIRST_REVERSED : CornerCallStyle.NUMBER_FIRST;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_CORNER_DIRECTION_FIRST))
            {
                this.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                CoDriver.cornerCallStyle = this.preferReversedNumbers ? CornerCallStyle.DIRECTION_FIRST_REVERSED : CornerCallStyle.DIRECTION_FIRST;
            }
            else if (voiceMessage.StartsWith(SpeechRecogniser.RALLY_CORRECTION))
            {
                ProcessCorrection(voiceMessage);
            }
        }

        private void ProcessCorrection(string voiceMessage)
        {
            if (historicCalls.Last == null || CrewChief.currentGameState == null
                || CrewChief.currentGameState.SessionData == null || CrewChief.currentGameState.SessionData.TrackDefinition == null)
            {
                Console.WriteLine("no pace note to correct");
                return;
            }
            string trackName = CrewChief.currentGameState.SessionData.TrackDefinition.name;
            CoDriver.Direction requestedDirection = Direction.UNKNOWN;
            if (voiceMessage.Contains(SpeechRecogniser.RALLY_LEFT))
            {
                requestedDirection = Direction.LEFT;
            }
            else if (voiceMessage.Contains(SpeechRecogniser.RALLY_RIGHT))
            {
                requestedDirection = Direction.RIGHT;
            }
            HistoricCornerCall callToCorrect = GetCornerCallToCorrect(requestedDirection);
            if (callToCorrect.callType != PacenoteType.unknown)
            {
                PacenoteType correctedPacenote = GetCorrectedPacenoteType(voiceMessage, callToCorrect.callType);
                if (correctedPacenote != PacenoteType.unknown)
                {
                    Console.WriteLine("Correcting existing pace note " + callToCorrect.callType + " at distance " + callToCorrect.callDistance + " to be " + correctedPacenote);
                    // now write the pacenote correction to some file
                    PaceNoteCorrection existingCorrection = FindMatchingCorrection(callToCorrect.callType, callToCorrect.callDistance);
                    if (existingCorrection != null)
                    {
                        existingCorrection.pacenoteType = correctedPacenote.ToString();
                    }
                    else
                    {
                        PaceNoteCorrection newCorrection = new PaceNoteCorrection();
                        newCorrection.distance = callToCorrect.callDistance;
                        newCorrection.pacenoteType = correctedPacenote.ToString();
                        correctionsForCurrentSession.Add(newCorrection);
                    }
                    WritePacenoteCorrections(trackName);
                }
            }            
        }

        private PaceNoteCorrection FindMatchingCorrection(PacenoteType pacenote, float distance)
        {
            if (pacenote.ToString().StartsWith("corner_"))
            foreach (PaceNoteCorrection paceNoteCorrection in this.correctionsForCurrentSession)
            {
                if (Math.Abs(paceNoteCorrection.distance - distance) < 5)
                {
                    return paceNoteCorrection;
                }
            }
            return null;
        }

        private void WritePacenoteCorrections(string trackName)
        {
            string correctionsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "CrewChiefV4", GameEnum.RBR.ToString(), trackName);
            Directory.CreateDirectory(correctionsPath);
            File.WriteAllText(Path.Combine(correctionsPath, "corrections.json"), JsonConvert.SerializeObject(this.correctionsForCurrentSession, Formatting.Indented));
        }

        private HistoricCornerCall GetCornerCallToCorrect(Direction requestedDirection)
        {
            // Count back through the historic calls to find the first one that we've passed and that matches the corner direction (if we specified one).
            // We also check that the corner call was made more than 0.4 seconds ago (do we need this?)
            // If we find a call to correct and it's for a corner > 200 metres behind us, assume we've not been able to find the appropriate call
            var historicCallNode = this.historicCalls.Last;
            while (historicCallNode != null
                && (historicCallNode.Value.callDistance > SpeechRecogniser.distanceWhenVoiceCommandStarted   // we've not reached this pacenote
                    || (SpeechRecogniser.timeVoiceCommandStarted - historicCallNode.Value.callTime).TotalSeconds < 0.4  // the pacenote call is too recent - we've not had time to drive the corner
                    || (requestedDirection != Direction.UNKNOWN && requestedDirection != GetDirectionFromPaceNote(historicCallNode.Value.callType)))) // this pacenote's direction is incorrect
            {
                historicCallNode = historicCallNode.Previous;
            }
            if (historicCallNode == null || SpeechRecogniser.distanceWhenVoiceCommandStarted - historicCallNode.Value.callDistance > 200 /* this pace note is for a corner 200m behind us*/)
            {
                Console.WriteLine("Unable to find a pacenote to correct");
                return null;
            }
            return historicCallNode.Value;
        }

        private Direction GetDirectionFromPaceNote(PacenoteType paceNote)
        {
            if (paceNote == PacenoteType.corner_1_left || paceNote == PacenoteType.corner_2_left || paceNote == PacenoteType.corner_3_left
                    || paceNote == PacenoteType.corner_4_left || paceNote == PacenoteType.corner_5_left || paceNote == PacenoteType.corner_6_left
                    || paceNote == PacenoteType.corner_flat_left || paceNote == PacenoteType.corner_left_acute)
                return Direction.LEFT;
            else if (paceNote == PacenoteType.corner_1_right || paceNote == PacenoteType.corner_2_right || paceNote == PacenoteType.corner_3_right
                    || paceNote == PacenoteType.corner_4_right || paceNote == PacenoteType.corner_5_right || paceNote == PacenoteType.corner_6_right
                    || paceNote == PacenoteType.corner_flat_right || paceNote == PacenoteType.corner_right_acute)
                return Direction.RIGHT;
            else
                return Direction.UNKNOWN;
        }

        private PacenoteType GetCorrectedPacenoteType(string voiceMessage, PacenoteType pacenoteToCorrect)
        {
            Direction direction = GetDirectionFromPaceNote(pacenoteToCorrect);
            PacenoteType correctedPaceNote = PacenoteType.unknown;
            if (direction != Direction.UNKNOWN)
            {
                bool reverseNumber = CoDriver.cornerCallStyle == CornerCallStyle.DIRECTION_FIRST_REVERSED || CoDriver.cornerCallStyle == CornerCallStyle.NUMBER_FIRST_REVERSED;
                if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_HAIRPIN))
                {
                    correctedPaceNote = direction == Direction.LEFT ? PacenoteType.corner_left_acute : PacenoteType.corner_right_acute;
                }
                if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_FLAT))
                {
                    correctedPaceNote = direction == Direction.LEFT ? PacenoteType.corner_flat_left : PacenoteType.corner_flat_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_1))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_6_left : PacenoteType.corner_1_left : reverseNumber ? PacenoteType.corner_6_right : PacenoteType.corner_1_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_2))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_5_left : PacenoteType.corner_2_left : reverseNumber ? PacenoteType.corner_5_right : PacenoteType.corner_2_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_3))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_4_left : PacenoteType.corner_3_left : reverseNumber ? PacenoteType.corner_4_right : PacenoteType.corner_3_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_4))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_3_left : PacenoteType.corner_4_left : reverseNumber ? PacenoteType.corner_3_right : PacenoteType.corner_4_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_5))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_2_left : PacenoteType.corner_5_left : reverseNumber ? PacenoteType.corner_2_right : PacenoteType.corner_5_right;
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RALLY_6))
                {
                    correctedPaceNote = direction == Direction.LEFT ? reverseNumber ? PacenoteType.corner_1_left : PacenoteType.corner_6_left : reverseNumber ? PacenoteType.corner_1_right : PacenoteType.corner_6_right;
                }
                // TODO: square
            }
            return correctedPaceNote;
        }
    }
}
