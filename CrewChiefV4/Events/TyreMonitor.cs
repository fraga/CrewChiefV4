﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.RaceRoom.RaceRoomData;
using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using CrewChiefV4.NumberProcessing;

namespace CrewChiefV4.Events
{
    class TyreMonitor : AbstractEvent
    {
        private Boolean delayResponses = UserSettings.GetUserSettings().getBoolean("enable_delayed_responses");
        private Boolean logStats = UserSettings.GetUserSettings().getBoolean("log_tyre_stats");

        // tyre temp messages...
        private String folderColdFrontTyres = "tyre_monitor/cold_front_tyres";
        private String folderColdRearTyres = "tyre_monitor/cold_rear_tyres";
        private String folderColdLeftTyres = "tyre_monitor/cold_left_tyres";
        private String folderColdRightTyres = "tyre_monitor/cold_right_tyres";
        private String folderColdTyresAllRound = "tyre_monitor/cold_tyres_all_round";

        private String folderHotLeftFrontTyre = "tyre_monitor/hot_left_front_tyre";
        private String folderHotLeftRearTyre = "tyre_monitor/hot_left_rear_tyre";
        private String folderHotRightFrontTyre = "tyre_monitor/hot_right_front_tyre";
        private String folderHotRightRearTyre = "tyre_monitor/hot_right_rear_tyre";
        private String folderHotFrontTyres = "tyre_monitor/hot_front_tyres";
        private String folderHotRearTyres = "tyre_monitor/hot_rear_tyres";
        private String folderHotLeftTyres = "tyre_monitor/hot_left_tyres";
        private String folderHotRightTyres = "tyre_monitor/hot_right_tyres";
        private String folderHotTyresAllRound = "tyre_monitor/hot_tyres_all_round";

        public static String folderLeftFront = "tyre_monitor/left_front";
        public static String folderRightFront = "tyre_monitor/right_front";
        public static String folderLeftRear = "tyre_monitor/left_rear";
        public static String folderRightRear = "tyre_monitor/right_rear";

        private String folderCookingLeftFrontTyre = "tyre_monitor/cooking_left_front_tyre";
        private String folderCookingLeftRearTyre = "tyre_monitor/cooking_left_rear_tyre";
        private String folderCookingRightFrontTyre = "tyre_monitor/cooking_right_front_tyre";
        private String folderCookingRightRearTyre = "tyre_monitor/cooking_right_rear_tyre";
        private String folderCookingFrontTyres = "tyre_monitor/cooking_front_tyres";
        private String folderCookingRearTyres = "tyre_monitor/cooking_rear_tyres";
        private String folderCookingLeftTyres = "tyre_monitor/cooking_left_tyres";
        private String folderCookingRightTyres = "tyre_monitor/cooking_right_tyres";
        private String folderCookingTyresAllRound = "tyre_monitor/cooking_tyres_all_round";

        private String folderGoodTyreTemps = "tyre_monitor/good_tyre_temps";


        // brake temp messages...
        private String folderColdFrontBrakes = "tyre_monitor/cold_front_brakes";
        private String folderColdRearBrakes = "tyre_monitor/cold_rear_brakes";
        private String folderHotFrontBrakes = "tyre_monitor/hot_front_brakes";
        private String folderHotRearBrakes = "tyre_monitor/hot_rear_brakes";
        private String folderCookingFrontBrakes = "tyre_monitor/hot_front_brakes";
        private String folderCookingRearBrakes = "tyre_monitor/hot_rear_brakes";

        private String folderColdBrakesAllRound = "tyre_monitor/cold_brakes_all_round";
        private String folderGoodBrakeTemps = "tyre_monitor/good_brake_temps";
        private String folderHotBrakesAllRound = "tyre_monitor/hot_brakes_all_round";
        private String folderCookingBrakesAllRound = "tyre_monitor/cooking_brakes_all_round";


        // tyre condition messages...
        private String folderKnackeredLeftFront = "tyre_monitor/knackered_left_front";
        private String folderKnackeredLeftRear = "tyre_monitor/knackered_left_rear";
        private String folderKnackeredRightFront = "tyre_monitor/knackered_right_front";
        private String folderKnackeredRightRear = "tyre_monitor/knackered_right_rear";
        private String folderKnackeredFronts = "tyre_monitor/knackered_fronts";
        private String folderKnackeredRears = "tyre_monitor/knackered_rears";
        private String folderKnackeredLefts = "tyre_monitor/knackered_lefts";
        private String folderKnackeredRights = "tyre_monitor/knackered_rights";
        private String folderKnackeredAllRound = "tyre_monitor/knackered_all_round";

        private String folderGoodWear = "tyre_monitor/good_wear";
        // same as above but filtered to remove sounds that only work when used in a voice command response
        private String folderGoodWearGeneral = "tyre_monitor/good_wear_general";

        private String folderMinorWearLeftFront = "tyre_monitor/minor_wear_left_front";
        private String folderMinorWearLeftRear = "tyre_monitor/minor_wear_left_rear";
        private String folderMinorWearRightFront = "tyre_monitor/minor_wear_right_front";
        private String folderMinorWearRightRear = "tyre_monitor/minor_wear_right_rear";
        private String folderMinorWearFronts = "tyre_monitor/minor_wear_fronts";
        private String folderMinorWearRears = "tyre_monitor/minor_wear_rears";
        private String folderMinorWearLefts = "tyre_monitor/minor_wear_lefts";
        private String folderMinorWearRights = "tyre_monitor/minor_wear_rights";
        private String folderMinorWearAllRound = "tyre_monitor/minor_wear_all_round";

        private String folderWornLeftFront = "tyre_monitor/worn_left_front";
        private String folderWornLeftRear = "tyre_monitor/worn_left_rear";
        private String folderWornRightFront = "tyre_monitor/worn_right_front";
        private String folderWornRightRear = "tyre_monitor/worn_right_rear";
        private String folderWornFronts = "tyre_monitor/worn_fronts";
        private String folderWornRears = "tyre_monitor/worn_rears";
        private String folderWornLefts = "tyre_monitor/worn_lefts";
        private String folderWornRights = "tyre_monitor/worn_rights";
        private String folderWornAllRound = "tyre_monitor/worn_all_round";

        public static String folderLapsOnCurrentTyresIntro = "tyre_monitor/laps_on_current_tyres_intro";
        public static String folderLapsOnCurrentTyresOutro = "tyre_monitor/laps_on_current_tyres_outro";
        public static String folderMinutesOnCurrentTyresIntro = "tyre_monitor/minutes_on_current_tyres_intro";
        public static String folderMinutesOnCurrentTyresOutro = "tyre_monitor/minutes_on_current_tyres_outro";

        public static String folderLockingFrontsForLapWarning = "tyre_monitor/locking_fronts_lap_warning";
        public static String folderLockingRearsForLapWarning = "tyre_monitor/locking_rears_lap_warning";
        public static String folderLockingLeftFrontForLapWarning = "tyre_monitor/locking_left_front_lap_warning";
        public static String folderLockingRightFrontForLapWarning = "tyre_monitor/locking_right_front_lap_warning";
        public static String folderLockingLeftRearForLapWarning = "tyre_monitor/locking_left_rear_lap_warning";
        public static String folderLockingRightRearForLapWarning = "tyre_monitor/locking_right_rear_lap_warning";

        public static String folderSpinningFrontsForLapWarning = "tyre_monitor/spinning_fronts_lap_warning";
        public static String folderSpinningRearsForLapWarning = "tyre_monitor/spinning_rears_lap_warning";
        public static String folderSpinningLeftFrontForLapWarning = "tyre_monitor/spinning_left_front_lap_warning";
        public static String folderSpinningRightFrontForLapWarning = "tyre_monitor/spinning_right_front_lap_warning";
        public static String folderSpinningLeftRearForLapWarning = "tyre_monitor/spinning_left_rear_lap_warning";
        public static String folderSpinningRightRearForLapWarning = "tyre_monitor/spinning_right_rear_lap_warning";

        private Boolean enableTyreTempWarnings = UserSettings.GetUserSettings().getBoolean("enable_tyre_temp_warnings");
        private Boolean enableBrakeTempWarnings = UserSettings.GetUserSettings().getBoolean("enable_brake_temp_warnings");
        private Boolean enableTyreWearWarnings = UserSettings.GetUserSettings().getBoolean("enable_tyre_wear_warnings");

        private Boolean useFahrenheit = UserSettings.GetUserSettings().getBoolean("use_fahrenheit");

        private Boolean enableWheelSpinWarnings = UserSettings.GetUserSettings().getBoolean("enable_wheel_spin_warnings");
        private Boolean enableBrakeLockWarnings = UserSettings.GetUserSettings().getBoolean("enable_brake_lock_warnings");

        private float initialTotalLapLockupThreshold = UserSettings.GetUserSettings().getFloat("cumulative_lap_lockup_warning_threshold");
        private float initialTotalWheelspinThreshold = UserSettings.GetUserSettings().getFloat("cumulative_lap_wheelspin_warning_threshold");

        private static String folderSpinningLeftFrontForCornerWarning = "tyre_monitor/spinning_left_front_corner_warning";
        private static String folderSpinningRightFrontForCornerWarning = "tyre_monitor/spinning_right_front_corner_warning";
        private static String folderSpinningFrontsForCornerWarning = "tyre_monitor/spinning_fronts_corner_warning";
        private static String folderSpinningLeftRearForCornerWarning = "tyre_monitor/spinning_left_rear_corner_warning";
        private static String folderSpinningRightRearForCornerWarning = "tyre_monitor/spinning_right_rear_corner_warning";
        private static String folderSpinningRearsForCornerWarning = "tyre_monitor/spinning_rears_corner_warning";

        private static String folderLockingLeftFrontForCornerWarning = "tyre_monitor/locking_left_front_corner_warning";
        private static String folderLockingRightFrontForCornerWarning = "tyre_monitor/locking_right_front_corner_warning";
        private static String folderLockingFrontsForCornerWarning = "tyre_monitor/locking_fronts_corner_warning";
        private static String folderLockingRearsForCornerWarning = "tyre_monitor/locking_rears_corner_warning";

        public static String folderHardTyres = "tyre_monitor/hards";
        public static String folderMediumTyres = "tyre_monitor/mediums";
        public static String folderSoftTyres = "tyre_monitor/softs";
        public static String folderSuperSoftTyres = "tyre_monitor/super_softs";
        public static String folderUltraSoftTyres = "tyre_monitor/ultra_softs";
        public static String folderHyperSoftTyres = "tyre_monitor/hyper_softs";
        public static String folderPrimaryTyres = "tyre_monitor/primaries";
        public static String folderAlternateTyres = "tyre_monitor/alternates";
        public static String folderPrimeTyres = "tyre_monitor/primes";
        public static String folderOptionTyres = "tyre_monitor/options";
        public static String folderWetTyres = "tyre_monitor/wets";
        public static String folderIntermediateTyres = "tyre_monitor/intermediates";
        public static String folderSlickTyres = "tyre_monitor/slicks";

        public static String folderAreAbout = "tyre_monitor/are_about";
        public static String folderFasterThan = "tyre_monitor/faster_than";

        // camber stuff:
        public static String folderAverageFrontsIMOSame = "tyre_monitor/average_front_inner_and_outer_same";
        public static String folderAverageRearsIMOSame = "tyre_monitor/average_rear_inner_and_outer_same";
        public static String folderAverageFrontInnerTempsIntro = "tyre_monitor/average_front_inner_temps_are";
        public static String folderAverageRearInnerTempsIntro = "tyre_monitor/average_rear_inner_temps_are";
        public static String folderCelsiusColderThanOuter = "tyre_monitor/celsius_colder_than_outer";
        public static String folderCelsiusColderThanOuters = "tyre_monitor/celsius_colder_than_outers"; // different sound for plural
        public static String folderCelsiusHotterThanOuter = "tyre_monitor/celsius_hotter_than_outer";
        public static String folderCelsiusHotterThanOuters = "tyre_monitor/celsius_hotter_than_outers"; // different sound for plural
        public static String folderLFIMOSame = "tyre_monitor/lf_inner_and_outer_are_same";
        public static String folderRFIMOSame = "tyre_monitor/rf_inner_and_outer_are_same";
        public static String folderLRIMOSame = "tyre_monitor/lr_inner_and_outer_are_same";
        public static String folderRRIMOSame = "tyre_monitor/rr_inner_and_outer_are_same";
        public static String folderLFInnerIsRunning = "tyre_monitor/lf_inner_is_running";
        public static String folderRFInnerIsRunning = "tyre_monitor/rf_inner_is_running";
        public static String folderLRInnerIsRunning = "tyre_monitor/lr_inner_is_running";
        public static String folderRRInnerIsRunning = "tyre_monitor/rr_inner_is_running";
        public static String folderNeedMoreNegativeCamber = "tyre_monitor/you_need_more_negative_camber";
        public static String folderNeedMorePositiveCamber = "tyre_monitor/you_need_more_positive_camber";
        public static String folderNeedMoreNegativeCamberOnFronts = "tyre_monitor/you_need_more_negative_camber_on_fronts";
        public static String folderNeedMorePostiveCamberOnFronts = "tyre_monitor/you_need_more_positive_camber_on_fronts";
        public static String folderNeedMoreNegativeCamberOnRears = "tyre_monitor/you_need_more_negative_camber_on_rears";
        public static String folderNeedMorePostiveCamberOnRears = "tyre_monitor/you_need_more_positive_camber_on_rears";
        public static String folderCamberOK = "tyre_monitor/camber_ok";

        // pressure stuff
        public static String folderLeftFrontPressureVeryHigh = "tyre_monitor/left_front_pressure_very_high";
        public static String folderLeftFrontPressureHigh = "tyre_monitor/left_front_pressure_high";
        public static String folderLeftFrontPressureOK = "tyre_monitor/left_front_pressure_ok";
        public static String folderLeftFrontPressureLow = "tyre_monitor/left_front_pressure_low";
        public static String folderLeftFrontPressureVeryLow = "tyre_monitor/left_front_pressure_very_low";
        public static String folderRightFrontPressureVeryHigh = "tyre_monitor/right_front_pressure_very_high";
        public static String folderRightFrontPressureHigh = "tyre_monitor/right_front_pressure_high";
        public static String folderRightFrontPressureOK = "tyre_monitor/right_front_pressure_ok";
        public static String folderRightFrontPressureLow = "tyre_monitor/right_front_pressure_low";
        public static String folderRightFrontPressureVeryLow = "tyre_monitor/right_front_pressure_very_low";
        public static String folderLeftRearPressureVeryHigh = "tyre_monitor/left_rear_pressure_very_high";
        public static String folderLeftRearPressureHigh = "tyre_monitor/left_rear_pressure_high";
        public static String folderLeftRearPressureOK = "tyre_monitor/left_rear_pressure_ok";
        public static String folderLeftRearPressureLow = "tyre_monitor/left_rear_pressure_low";
        public static String folderLeftRearPressureVeryLow = "tyre_monitor/left_rear_pressure_very_low";
        public static String folderRightRearPressureVeryHigh = "tyre_monitor/right_rear_pressure_very_high";
        public static String folderRightRearPressureHigh = "tyre_monitor/right_rear_pressure_high";
        public static String folderRightRearPressureOK = "tyre_monitor/right_rear_pressure_ok";
        public static String folderRightRearPressureLow = "tyre_monitor/right_rear_pressure_low";
        public static String folderRightRearPressureVeryLow = "tyre_monitor/right_rear_pressure_very_low";
        public static String folderFrontPressuresVeryHigh = "tyre_monitor/front_pressures_very_high";
        public static String folderFrontPressuresHigh = "tyre_monitor/front_pressures_high";
        public static String folderFrontPressuresOK = "tyre_monitor/front_pressures_ok";
        public static String folderFrontPressuresLow = "tyre_monitor/front_pressures_low";
        public static String folderFrontPressuresVeryLow = "tyre_monitor/front_pressures_very_low";
        public static String folderRearPressuresVeryHigh = "tyre_monitor/rear_pressures_very_high";
        public static String folderRearPressuresHigh = "tyre_monitor/rear_pressures_high";
        public static String folderRearPressuresOK = "tyre_monitor/rear_pressures_ok";
        public static String folderRearPressuresLow = "tyre_monitor/rear_pressures_low";
        public static String folderRearPressuresVeryLow = "tyre_monitor/rear_pressures_very_low";


        // some finger-in-air target inner-outer temps percentage increase from outer to inner - 
        // if inner = 90 and outer = 85 then we have 100 * (90 - 85)/90 = 5.56
        float[] defaultTargetIODifference = { 3, 6 }; // inners should be min 3%, max 6% hotter that outers
        private static Dictionary<TyreType, float[]> idealCamberValues = new Dictionary<TyreType, float[]>() {
            {TyreType.R3E_2016_HARD, new float[] {2, 4}}, // these are the old R3E tyre model with core temps that change very little
            {TyreType.R3E_2016_MEDIUM, new float[] {2, 4}}, // these are the old R3E tyre model with core temps that change very little
            {TyreType.R3E_2016_SOFT, new float[] {1, 3}}, // these are the old R3E tyre model with core temps that change very little
            {TyreType.R3E_2017_HARD, new float[] {4, 7}},     // more recent tyre model
            {TyreType.R3E_2017_MEDIUM, new float[] {4, 7}},     // more recent tyre model
            {TyreType.R3E_2017_SOFT, new float[] {3, 6}},     // more recent tyre model
            {TyreType.Medium, new float[] {4, 7}},
            {TyreType.Soft, new float[] {3, 6}},
            {TyreType.Super_Soft, new float[] {3, 5}},
            {TyreType.Hyper_Soft, new float[] {2, 4}},
            {TyreType.Bias_Ply, new float[] {2, 4}}
        };

        private int imoSampleFrequency = 1000;  // milliseconds, externalise?
        private DateTime nextIMOSamplesDue = DateTime.MinValue;

        private LinkedList<float> leftFrontInnerSamples = new LinkedList<float>();
        private LinkedList<float> leftFrontMiddleSamples = new LinkedList<float>();
        private LinkedList<float> leftFrontOuterSamples = new LinkedList<float>();
        private LinkedList<float> rightFrontInnerSamples = new LinkedList<float>();
        private LinkedList<float> rightFrontMiddleSamples = new LinkedList<float>();
        private LinkedList<float> rightFrontOuterSamples = new LinkedList<float>();
        private LinkedList<float> leftRearInnerSamples = new LinkedList<float>();
        private LinkedList<float> leftRearMiddleSamples = new LinkedList<float>();
        private LinkedList<float> leftRearOuterSamples = new LinkedList<float>();
        private LinkedList<float> rightRearInnerSamples = new LinkedList<float>();
        private LinkedList<float> rightRearMiddleSamples = new LinkedList<float>();
        private LinkedList<float> rightRearOuterSamples = new LinkedList<float>();
        private float[] leftFrontAverageIMOLastLap;
        private float[] rightFrontAverageIMOLastLap;
        private float[] leftRearAverageIMOLastLap;
        private float[] rightRearAverageIMOLastLap;

        private int lapsIntoSessionBeforeTempMessage = 2;

        // check at start of which sector (1=s/f line)
        private int checkBrakesAtSector = 3;
        private float lastBrakeTempCheckSessionTime = -1.0f;
        private float secondsBetweenBrakeTempCheck = 120.0f;

        private Boolean reportedTyreWearForCurrentPitEntry;

        private Boolean reportedEstimatedTimeLeftOneThirdWear;
        private Boolean reportedEstimatedTimeLeftTwoThirdsWear;

        private float leftFrontWearPercent;
        private float rightFrontWearPercent;
        private float leftRearWearPercent;
        private float rightRearWearPercent;

        private int completedLaps;
        private int lapsInSession;
        private float timeInSession;
        private float timeElapsed;

        private CornerData currentTyreConditionStatus;

        private CornerData currentTyreTempStatus;

        private CornerData currentBrakeTempStatus;
        private CornerData peakBrakeTempStatus;

        private int lapsBetweenBrakeTempChecks = 1;
        private int lastBrakeTempCheckLap = 0;
        private int lapsBetweenTyreTempChecks = 1;
        private int lastTyreTempCheckLap = 0;
        private List<List<MessageFragment>> tyreTempMessagesPlayed = new List<List<MessageFragment>>();
        private List<List<MessageFragment>> brakeTempMessagesPlayed = new List<List<MessageFragment>>();

        private List<MessageFragment> lastTyreConditionMessage = null;

        private float peakBrakeTempForLap = 0;

        private float timeLeftFrontIsLockedForLap = 0;
        private float timeRightFrontIsLockedForLap = 0;
        private float timeLeftRearIsLockedForLap = 0;
        private float timeRightRearIsLockedForLap = 0;
        private float timeBothRearsAreLockedForLap = 0;
        private float timeLeftFrontIsSpinningForLap = 0;
        private float timeRightFrontIsSpinningForLap = 0;
        private float timeLeftRearIsSpinningForLap = 0;
        private float timeRightRearIsSpinningForLap = 0;

        // list of flags indicating locked wheels for previous ticks, used in conjunction with track landmarks only
        private int tickMillis = UserSettings.GetUserSettings().getInt("update_interval");
        // number of ticks a wheel has to be locked before a warning - default to 5 (0.5s) but recalulate based on tick length
        private int lockedTicksThreshold = 5;
        private float cornerEntryLockThreshold = 0.25f;  // lockups > 0.25 seconds are reported
        // check 3 seconds worth of historical ticks - again, default but recalulated
        private int ticksToCheckForLocking = 30;
        private float cornerExitSpinningThreshold = 0.3f; // wheelspin > 0.3 seconds are reported
        private List<Boolean> leftFrontLockedList = new List<Boolean>();
        private List<Boolean> rightFrontLockedList = new List<Boolean>();
        private List<Boolean> leftRearLockedList = new List<Boolean>();
        private List<Boolean> rightRearLockedList = new List<Boolean>();
        private String currentCornerName = null;
        private TimeSpan cornerLockWarningMaxFrequency = TimeSpan.FromSeconds(180);
        private TimeSpan cornerSpinningWarningMaxFrequency = TimeSpan.FromSeconds(120);

        private Dictionary<String, DateTime> cornerLockWarningsPlayed = new Dictionary<string, DateTime>();
        private Dictionary<String, DateTime> cornerSpinningWarningsPlayed = new Dictionary<string, DateTime>();

        private DateTime nextCornerSpecificSpinningCheck = DateTime.MaxValue;
        private float leftFrontExitStartWheelSpinTime = 0;
        private float rightFrontExitStartWheelSpinTime = 0;
        private float leftRearExitStartWheelSpinTime = 0;
        private float rightRearExitStartWheelSpinTime = 0;

        private Boolean enableCornerSpecificLockingAndSpinningChecks = false;

        private float leftFrontTyreTemp = 0;
        private float rightFrontTyreTemp = 0;
        private float leftRearTyreTemp = 0;
        private float rightRearTyreTemp = 0;

        private float leftFrontBrakeTemp = 0;
        private float rightFrontBrakeTemp = 0;
        private float leftRearBrakeTemp = 0;
        private float rightRearBrakeTemp = 0;

        private float totalLockupThresholdForNextLap;
        private float totalWheelspinThresholdForNextLap;

        private Boolean warnedOnLockingForLap = false;
        private Boolean warnedOnWheelspinForLap = false;

        private DateTime nextLockingAndSpinningCheck = DateTime.MinValue;

        private TimeSpan lockingAndSpinningCheckInterval = TimeSpan.FromSeconds(3);

        private Boolean lastBrakeTempCheckOK = true;
        private Boolean lastTyreTempCheckOK = true;

        private Dictionary<TyreType, float> playerClassSessionBestLapTimeByTyre = null;

        private int thisLapTyreConditionReportSector = 2;
        private int thisLapTyreTempReportSector = 3;

        private List<double> tyreLifeYPointsTime = new List<double>();
        private List<double> tyreLifeYPointsSectors = new List<double>();
        private List<double> tyreLifeXPointsLFWearBySector = new List<double>();
        private List<double> tyreLifeXPointsLFWearByTime = new List<double>();
        private List<double> tyreLifeXPointsRFWearBySector = new List<double>();
        private List<double> tyreLifeXPointsRFWearByTime = new List<double>();
        private List<double> tyreLifeXPointsLRWearBySector = new List<double>();
        private List<double> tyreLifeXPointsLRWearByTime = new List<double>();
        private List<double> tyreLifeXPointsRRWearBySector = new List<double>();
        private List<double> tyreLifeXPointsRRWearByTime = new List<double>();
        private float lastTyreLifeYPointTime = -1;

        // don't warn about cold brakes for these car classes. This is in addition to the 'oval' check - some car classes
        // (older stuff, road cars) will have brakes that never really get hot, resulting in lots of annoying messages.
        private CarData.CarClassEnum[] ignoreColdBrakesForClasses = new CarData.CarClassEnum[] {
            CarData.CarClassEnum.FORMULA_E_2018, CarData.CarClassEnum.FORMULA_E_2019, CarData.CarClassEnum.HISTORIC_TOURING_1, CarData.CarClassEnum.HISTORIC_TOURING_2, 
            CarData.CarClassEnum.Kart_1, CarData.CarClassEnum.Kart_2, CarData.CarClassEnum.KART_F1, 
            CarData.CarClassEnum.KART_JUNIOR, CarData.CarClassEnum.KART_X30_RENTAL, CarData.CarClassEnum.KART_X30_SENIOR,
            CarData.CarClassEnum.NSU_TT, CarData.CarClassEnum.ROAD_G, CarData.CarClassEnum.ROAD_F,
            CarData.CarClassEnum.ROAD_E, CarData.CarClassEnum.VINTAGE_GT_C, CarData.CarClassEnum.VINTAGE_GT_D,
            CarData.CarClassEnum.VINTAGE_INDY_65, CarData.CarClassEnum.VINTAGE_STOCK_CAR
        };

        public TyreMonitor(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
            this.totalLockupThresholdForNextLap = this.initialTotalLapLockupThreshold;
            this.totalWheelspinThresholdForNextLap = this.initialTotalWheelspinThreshold;
    }

        public static String getFolderForTyreType(TyreType tyreType)
        {
            switch (tyreType)
            {
                case TyreType.Hyper_Soft:
                    return folderHyperSoftTyres;
                case TyreType.Ultra_Soft:
                    return folderUltraSoftTyres;
                case TyreType.Super_Soft:
                    return folderSuperSoftTyres;
                case TyreType.Soft:
                case TyreType.R3E_2016_SOFT:
                case TyreType.R3E_2017_SOFT:
                    return folderSoftTyres;
                case TyreType.Medium:
                case TyreType.R3E_2016_MEDIUM:
                case TyreType.R3E_2017_MEDIUM:
                    return folderMediumTyres;
                case TyreType.Hard:
                case TyreType.R3E_2016_HARD:
                case TyreType.R3E_2017_HARD:
                    return folderHardTyres;
                case TyreType.Primary:
                    return folderPrimaryTyres;
                case TyreType.Alternate:
                    return folderAlternateTyres;
                case TyreType.Prime:
                    return folderPrimeTyres;
                case TyreType.Option:
                    return folderOptionTyres;
                case TyreType.Wet:
                    return folderWetTyres;
                case TyreType.Intermediate:
                    return folderIntermediateTyres;
                default:
                    return folderSlickTyres;
            }
        }

        public override void clearState()
        {
            reportedTyreWearForCurrentPitEntry = false;
            reportedEstimatedTimeLeftOneThirdWear = false;
            reportedEstimatedTimeLeftTwoThirdsWear = false;
            leftFrontWearPercent = 0;
            leftRearWearPercent = 0;
            rightFrontWearPercent = 0;
            rightRearWearPercent = 0;
            completedLaps = 0;
            lapsInSession = 0;
            timeInSession = 0;
            timeElapsed = 0;
            currentTyreConditionStatus = new CornerData();
            currentTyreTempStatus = new CornerData();
            currentBrakeTempStatus = new CornerData();
            peakBrakeTempStatus = new CornerData();
            tyreTempMessagesPlayed.Clear();
            brakeTempMessagesPlayed.Clear();
            lapsBetweenBrakeTempChecks = 1;
            lastBrakeTempCheckLap = 0;
            lapsBetweenTyreTempChecks = 1;
            lastTyreTempCheckLap = 0;
            lastTyreConditionMessage = null;
            peakBrakeTempForLap = 0;
            timeLeftFrontIsLockedForLap = 0;
            timeRightFrontIsLockedForLap = 0;
            timeLeftRearIsLockedForLap = 0;
            timeRightRearIsLockedForLap = 0;
            timeBothRearsAreLockedForLap = 0;
            timeLeftFrontIsSpinningForLap = 0;
            timeRightFrontIsSpinningForLap = 0;
            timeLeftRearIsSpinningForLap = 0;
            timeRightRearIsSpinningForLap = 0;
            totalLockupThresholdForNextLap = initialTotalLapLockupThreshold;
            totalWheelspinThresholdForNextLap = initialTotalWheelspinThreshold;
            warnedOnLockingForLap = false;
            warnedOnWheelspinForLap = false;
            nextLockingAndSpinningCheck = DateTime.MinValue;
            secondsBetweenBrakeTempCheck = 120.0f;
            leftFrontTyreTemp = 0;
            rightFrontTyreTemp = 0;
            leftRearTyreTemp = 0;
            rightRearTyreTemp = 0;

            leftFrontBrakeTemp = 0;
            rightFrontBrakeTemp = 0;
            leftRearBrakeTemp = 0;
            rightRearBrakeTemp = 0;

            lastBrakeTempCheckOK = true;
            lastTyreTempCheckOK = true;

            // want to check locking for 0.5 seconds
            lockedTicksThreshold = tickMillis == 0 ? 5 : (int)Math.Ceiling(cornerEntryLockThreshold / ((float)tickMillis / 1000f));
            ticksToCheckForLocking = tickMillis == 0 ? 30 : (int)Math.Ceiling(3.0f / ((float)tickMillis / 1000f));
            leftFrontLockedList.Clear();
            rightFrontLockedList.Clear();
            leftRearLockedList.Clear();
            rightRearLockedList.Clear();
            nextCornerSpecificSpinningCheck = DateTime.MaxValue;
            leftFrontExitStartWheelSpinTime = 0;
            rightFrontExitStartWheelSpinTime = 0;
            leftRearExitStartWheelSpinTime = 0;
            rightRearExitStartWheelSpinTime = 0;
            cornerLockWarningsPlayed.Clear();
            cornerSpinningWarningsPlayed.Clear();

            currentCornerName = null;
            enableCornerSpecificLockingAndSpinningChecks = false;
            playerClassSessionBestLapTimeByTyre = null;

            lastBrakeTempCheckSessionTime = -1.0f;

            tyreLifeYPointsTime.Clear();
            tyreLifeYPointsSectors.Clear();
            tyreLifeXPointsLFWearBySector.Clear();
            tyreLifeXPointsRFWearBySector.Clear();
            tyreLifeXPointsLRWearBySector.Clear();
            tyreLifeXPointsRRWearBySector.Clear();
            tyreLifeXPointsLFWearByTime.Clear();
            tyreLifeXPointsRFWearByTime.Clear();
            tyreLifeXPointsLRWearByTime.Clear();
            tyreLifeXPointsRRWearByTime.Clear();
            lastTyreLifeYPointTime = -1;

            leftFrontInnerSamples.Clear();
            leftFrontMiddleSamples.Clear();
            leftFrontOuterSamples.Clear();
            rightFrontInnerSamples.Clear();
            rightFrontMiddleSamples.Clear();
            rightFrontOuterSamples.Clear();
            leftRearInnerSamples.Clear();
            leftRearMiddleSamples.Clear();
            leftRearOuterSamples.Clear();
            rightRearInnerSamples.Clear();
            rightRearMiddleSamples.Clear();
            rightRearOuterSamples.Clear();
            leftFrontAverageIMOLastLap = null;
            rightFrontAverageIMOLastLap = null;
            leftRearAverageIMOLastLap = null;
            rightRearAverageIMOLastLap = null;
        }

        private Boolean isBrakeTempPeakForLap(float leftFront, float rightFront, float leftRear, float rightRear)
        {
            if (leftFront > peakBrakeTempForLap || rightFront > peakBrakeTempForLap ||
                leftRear > peakBrakeTempForLap || rightRear > peakBrakeTempForLap)
            {
                peakBrakeTempForLap = Math.Max(leftFront, Math.Max(rightFront, Math.Max(leftRear, rightRear)));
                return true;
            }
            return false;
        }

        private void logTyreStats(TyreData tyreData, int sectorNumber, int lapNumber)
        {
            // tyre stats debug code
            Console.WriteLine("-------------------------");
            Console.WriteLine("Lap " + (lapNumber + 1) + " sector " + sectorNumber + " tyre temps, Outer, middle, inner  |------|  inner, middle, outer");
            Console.WriteLine("Fronts:    " + Math.Round(tyreData.FrontLeft_LeftTemp, 2) +
                ", " + Math.Round(tyreData.FrontLeft_CenterTemp, 2) +
                ", " + Math.Round(tyreData.FrontLeft_RightTemp, 2) +
                "  |------|  " + Math.Round(tyreData.FrontRight_LeftTemp, 2) +
                ", " + Math.Round(tyreData.FrontRight_CenterTemp, 2) +
                ", " + Math.Round(tyreData.FrontRight_RightTemp, 2));
            Console.WriteLine("Rears:    " + Math.Round(tyreData.RearLeft_LeftTemp, 2) +
                ", " + Math.Round(tyreData.RearLeft_CenterTemp, 2) +
                ", " + Math.Round(tyreData.RearLeft_RightTemp, 2) +
                "  |------|  " + Math.Round(tyreData.RearRight_LeftTemp, 2) +
                ", " + Math.Round(tyreData.RearRight_CenterTemp, 2) +
                ", " + Math.Round(tyreData.RearRight_RightTemp, 2));

            Console.WriteLine("-------------------------");
            Console.WriteLine("Peak fronts:    " + Math.Round(tyreData.PeakFrontLeftTemperatureForLap, 2) +
                "  |------|  " + Math.Round(tyreData.PeakFrontRightTemperatureForLap, 2));
            Console.WriteLine("Peak rears:    " + Math.Round(tyreData.PeakRearLeftTemperatureForLap, 2) +
                "  |------|  " + Math.Round(tyreData.PeakRearRightTemperatureForLap, 2));

            Console.WriteLine("-------------------------");
            Console.WriteLine("Temperature interpretation:");
            foreach (var key in tyreData.TyreTempStatus.cornersForEachStatus)
            {
                Console.WriteLine("Status: " + key);
            }

            Console.WriteLine("-------------------------");
            Console.WriteLine("Wear, percentage  |------|  percentage");
            Console.WriteLine("Fronts:    " + Math.Round(tyreData.FrontLeftPercentWear, 2) +
                "  |------|  " + Math.Round(tyreData.FrontRightPercentWear, 2));
            Console.WriteLine("Rears:    " + Math.Round(tyreData.RearLeftPercentWear, 2) +
                "  |------|  " + Math.Round(tyreData.RearRightPercentWear, 2));
            Console.WriteLine("-------------------------");
            Console.WriteLine("Wear interpretation:");
            foreach (var key in tyreData.TyreConditionStatus.cornersForEachStatus)
            {
                Console.WriteLine("Status: " + key);
            }
            Console.WriteLine("-------------------------");
        }

        private void getIMOAveragesForLap()
        {
            if (leftFrontInnerSamples.Count > 0 && leftFrontMiddleSamples.Count > 0 && leftFrontOuterSamples.Count > 0)
            {
                leftFrontAverageIMOLastLap = new float[] { leftFrontInnerSamples.Average(), leftFrontMiddleSamples.Average(), leftFrontOuterSamples.Average() };
            }
            if (rightFrontInnerSamples.Count > 0 && rightFrontMiddleSamples.Count > 0 && rightFrontOuterSamples.Count > 0)
            {
                rightFrontAverageIMOLastLap = new float[] { rightFrontInnerSamples.Average(), rightFrontMiddleSamples.Average(), rightFrontOuterSamples.Average() };
            }
            if (leftRearInnerSamples.Count > 0 && leftRearMiddleSamples.Count > 0 && leftRearOuterSamples.Count > 0)
            {
                leftRearAverageIMOLastLap = new float[] { leftRearInnerSamples.Average(), leftRearMiddleSamples.Average(), leftRearOuterSamples.Average() };
            }
            if (rightRearInnerSamples.Count > 0 && rightRearMiddleSamples.Count > 0 && rightRearOuterSamples.Count > 0)
            {
                rightRearAverageIMOLastLap = new float[] { rightRearInnerSamples.Average(), rightRearMiddleSamples.Average(), rightRearOuterSamples.Average() };
            }
        }

        private void clearIMOData()
        {
            leftFrontInnerSamples.Clear();
            leftFrontMiddleSamples.Clear();
            leftFrontOuterSamples.Clear();
            rightFrontInnerSamples.Clear();
            rightFrontMiddleSamples.Clear();
            rightFrontOuterSamples.Clear();
            leftRearInnerSamples.Clear();
            leftRearMiddleSamples.Clear();
            leftRearOuterSamples.Clear();
            rightRearInnerSamples.Clear();
            rightRearMiddleSamples.Clear();
            rightRearOuterSamples.Clear();
        }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (GameStateData.onManualFormationLap || CrewChief.gameDefinition.gameEnum == GameEnum.PCARS3 /* no tyre useage in pCars3*/)
            {
                return;
            }
            playerClassSessionBestLapTimeByTyre = currentGameState.SessionData.PlayerClassSessionBestLapTimeByTyre;
            if (logStats && currentGameState.SessionData.IsNewSector)
            {
                logTyreStats(currentGameState.TyreData, currentGameState.SessionData.SectorNumber, currentGameState.SessionData.CompletedLaps);
            }
            if (currentGameState.SessionData.IsNewLap)
            {
                timeLeftFrontIsLockedForLap = 0;
                timeRightFrontIsLockedForLap = 0;
                timeLeftRearIsLockedForLap = 0;
                timeRightRearIsLockedForLap = 0;
                timeBothRearsAreLockedForLap = 0;
                timeLeftFrontIsSpinningForLap = 0;
                timeRightFrontIsSpinningForLap = 0;
                timeLeftRearIsSpinningForLap = 0;
                timeRightRearIsSpinningForLap = 0;
                leftFrontExitStartWheelSpinTime = 0;
                rightFrontExitStartWheelSpinTime = 0;
                leftRearExitStartWheelSpinTime = 0;
                rightRearExitStartWheelSpinTime = 0;
                if (warnedOnLockingForLap)
                {
                    totalLockupThresholdForNextLap = totalLockupThresholdForNextLap + 1;
                }
                else
                {
                    totalLockupThresholdForNextLap = initialTotalLapLockupThreshold;
                }
                if (warnedOnWheelspinForLap)
                {
                    totalWheelspinThresholdForNextLap = totalWheelspinThresholdForNextLap + 1;
                }
                else
                {
                    totalWheelspinThresholdForNextLap = initialTotalWheelspinThreshold;
                }
                warnedOnLockingForLap = false;
                warnedOnWheelspinForLap = false;

                leftFrontLockedList.Clear();
                rightFrontLockedList.Clear();
                leftRearLockedList.Clear();
                rightRearLockedList.Clear();
                currentCornerName = null;

                // corner specific locking and spinning checks only for cars and where we have data
                enableCornerSpecificLockingAndSpinningChecks = currentGameState.SessionData.TrackDefinition.trackLandmarks != null &&
                    currentGameState.SessionData.TrackDefinition.trackLandmarks.Count > 0 && currentGameState.carClass.carClassEnum != CarData.CarClassEnum.Kart_1 &&
                    currentGameState.carClass.carClassEnum != CarData.CarClassEnum.Kart_2 &&
                    currentGameState.carClass.carClassEnum != CarData.CarClassEnum.KART_F1 &&
                    currentGameState.carClass.carClassEnum != CarData.CarClassEnum.KART_JUNIOR;

                checkBrakesAtSector = Utilities.random.Next(1, 4);

                if (Utilities.random.Next(0, 2) == 0)
                {
                    thisLapTyreConditionReportSector = 2;
                    thisLapTyreTempReportSector = 3;
                }
                else
                {
                    thisLapTyreConditionReportSector = 3;
                    thisLapTyreTempReportSector = 2;
                }
                // just finished a lap, get IMO data
                getIMOAveragesForLap();
                clearIMOData();
            }
            else if (previousGameState != null && previousGameState.PitData.InPitlane && !currentGameState.PitData.InPitlane)
            {
                // just exited the pit, clear the lap's IMO temp data
                clearIMOData();
            }
            else if (!currentGameState.PitData.InPitlane && currentGameState.PositionAndMotionData.CarSpeed > 10 && currentGameState.Now > nextIMOSamplesDue)
            {
                // if we're not in the pitlane and a moving at a reasonable speed, collect IMO temp samples
                nextIMOSamplesDue = currentGameState.Now.AddMilliseconds(imoSampleFrequency);
                leftFrontInnerSamples.AddLast(currentGameState.TyreData.FrontLeft_RightTemp);
                leftFrontMiddleSamples.AddLast(currentGameState.TyreData.FrontLeft_CenterTemp);
                leftFrontOuterSamples.AddLast(currentGameState.TyreData.FrontLeft_LeftTemp);
                rightFrontInnerSamples.AddLast(currentGameState.TyreData.FrontRight_LeftTemp);
                rightFrontMiddleSamples.AddLast(currentGameState.TyreData.FrontRight_CenterTemp);
                rightFrontOuterSamples.AddLast(currentGameState.TyreData.FrontRight_RightTemp);
                leftRearInnerSamples.AddLast(currentGameState.TyreData.RearLeft_RightTemp);
                leftRearMiddleSamples.AddLast(currentGameState.TyreData.RearLeft_CenterTemp);
                leftRearOuterSamples.AddLast(currentGameState.TyreData.RearLeft_LeftTemp);
                rightRearInnerSamples.AddLast(currentGameState.TyreData.RearRight_LeftTemp);
                rightRearMiddleSamples.AddLast(currentGameState.TyreData.RearRight_CenterTemp);
                rightRearOuterSamples.AddLast(currentGameState.TyreData.RearRight_RightTemp);
            }

            enableWheelSpinWarnings = enableWheelSpinWarnings && GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.LOCKING_AND_SPINNING);
            enableBrakeLockWarnings = enableBrakeLockWarnings && GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.LOCKING_AND_SPINNING);
            enableTyreWearWarnings = enableTyreWearWarnings && GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.TYRE_WEAR) && !Penalties.playerMustPitThisLap;
            enableTyreTempWarnings = enableTyreTempWarnings && GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.TYRE_TEMPS) && !Penalties.playerMustPitThisLap;
            enableBrakeTempWarnings = enableBrakeTempWarnings && GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.BRAKE_TEMPS) && !Penalties.playerMustPitThisLap;

            if (previousGameState != null && currentGameState.Ticks > previousGameState.Ticks)
            {
                addLockingAndSpinningData(currentGameState.TyreData, currentGameState.PitData.InPitlane,
                    hasWheelMissingOrPuncture(currentGameState), previousGameState.Ticks, currentGameState.Ticks,
                    currentGameState.carClass.allMembersAreFWD);
            }

            // cumulative locking and spinning checks
            Boolean playedCumulativeLockingMessage = false;
            Boolean playedCumulativeSpinningMessage = false;
            if (currentGameState.Now > nextLockingAndSpinningCheck)
            {
                if (!hasWheelMissingOrPuncture(currentGameState) && !currentGameState.PitData.InPitlane)
                {
                    if (enableBrakeLockWarnings)
                    {
                        playedCumulativeLockingMessage = checkLocking(currentGameState.carClass.allMembersAreFWD);
                    }
                    if (enableWheelSpinWarnings)
                    {
                        playedCumulativeSpinningMessage = checkWheelSpinning(currentGameState.carClass.allMembersAreFWD, currentGameState.carClass.allMembersAreRWD);
                    }
                }
                nextLockingAndSpinningCheck = currentGameState.Now.Add(lockingAndSpinningCheckInterval);
            }
            // corner-specific locking and spinning checks
            // skip these for race sector1 lap1 where wheelspin and locking are expected
            if (enableCornerSpecificLockingAndSpinningChecks &&
                    (currentGameState.SessionData.SessionType != SessionType.Race ||
                     currentGameState.SessionData.CompletedLaps > 0 ||
                     currentGameState.SessionData.SectorNumber != 1))
            {
                if (currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark != null)
                {
                    // we've just hit an apex (midpoint) so count back the locked wheel ticks
                    currentCornerName = currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark;
                    // only moan about locking if we've not just moaned about it for the lap
                    if (!playedCumulativeLockingMessage)
                    {
                        WheelsLockedEnum cornerLocking = getCornerEntryLocking(lockedTicksThreshold);
                        if (cornerLocking != WheelsLockedEnum.NONE)
                        {
                            // if we moaned about this corner recently, don't moan again
                            DateTime lastMoanTime = DateTime.MinValue;
                            DateTime timeLastWarnedAboutThisCorner = cornerLockWarningsPlayed.TryGetValue(currentCornerName, out lastMoanTime) ? lastMoanTime : DateTime.MinValue;
                            if (currentGameState.Now > timeLastWarnedAboutThisCorner + cornerLockWarningMaxFrequency)
                            {
                                // not moaned about this for a while, so moan away
                                cornerLockWarningsPlayed[currentCornerName] = currentGameState.Now;
                                Console.WriteLine("Locking " + cornerLocking + " into " + currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark);
                                int delay = Utilities.random.Next(4, 8);
                                switch (cornerLocking)
                                {
                                    case WheelsLockedEnum.FRONTS:
                                        audioPlayer.playMessage(new QueuedMessage("corner_locking", delay + 10, secondsDelay: delay,
                                            messageFragments: MessageContents(folderLockingFrontsForCornerWarning, "corners/" + currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark),
                                            abstractEvent: this, priority: 0));
                                        break;
                                    case WheelsLockedEnum.LEFT_FRONT:
                                        audioPlayer.playMessage(new QueuedMessage("corner_locking", delay + 10, secondsDelay: delay,
                                            messageFragments: MessageContents(folderLockingLeftFrontForCornerWarning, "corners/" + currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark),
                                            abstractEvent: this, priority: 0));
                                        break;
                                    case WheelsLockedEnum.RIGHT_FRONT:
                                        audioPlayer.playMessage(new QueuedMessage("corner_locking", delay + 10, secondsDelay: delay,
                                            messageFragments: MessageContents(folderLockingRightFrontForCornerWarning, "corners/" + currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark),
                                            abstractEvent: this, priority: 0));
                                        break;
                                    case WheelsLockedEnum.REARS:
                                        audioPlayer.playMessage(new QueuedMessage("corner_locking", delay + 10, secondsDelay: delay,
                                            messageFragments: MessageContents(folderLockingRearsForCornerWarning, "corners/" + currentGameState.SessionData.trackLandmarksTiming.atMidPointOfLandmark),
                                            abstractEvent: this, priority: 0));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    leftFrontLockedList.Clear();
                    rightFrontLockedList.Clear();
                    leftRearLockedList.Clear();
                    rightRearLockedList.Clear();

                    // and reset the wheelspin counters
                    nextCornerSpecificSpinningCheck = currentGameState.Now + TimeSpan.FromSeconds(3);
                    leftFrontExitStartWheelSpinTime = timeLeftFrontIsSpinningForLap;
                    rightFrontExitStartWheelSpinTime = timeRightFrontIsSpinningForLap;
                    leftRearExitStartWheelSpinTime = timeLeftRearIsSpinningForLap;
                    rightRearExitStartWheelSpinTime = timeRightRearIsSpinningForLap;
                }
                else if (currentGameState.Now > nextCornerSpecificSpinningCheck)
                {
                    // now moan about wheelspin
                    nextCornerSpecificSpinningCheck = DateTime.MaxValue;
                    if (!playedCumulativeSpinningMessage && currentCornerName != null)
                    {
                        DateTime lastWarningTime = DateTime.MinValue;
                        DateTime timeLastWarnedAboutThisCorner = cornerSpinningWarningsPlayed.TryGetValue(currentCornerName, out lastWarningTime) ? lastWarningTime : DateTime.MinValue;
                        if (currentGameState.Now > timeLastWarnedAboutThisCorner + cornerSpinningWarningMaxFrequency)
                        {
                            float leftFrontCornerSpecificWheelSpinTime = timeLeftFrontIsSpinningForLap - leftFrontExitStartWheelSpinTime;
                            float rightFrontCornerSpecificWheelSpinTime = timeRightFrontIsSpinningForLap - rightFrontExitStartWheelSpinTime;
                            float leftRearCornerSpecificWheelSpinTime = timeLeftRearIsSpinningForLap - leftRearExitStartWheelSpinTime;
                            float rightRearCornerSpecificWheelSpinTime = timeRightRearIsSpinningForLap - rightRearExitStartWheelSpinTime;
                            int delay = Utilities.random.Next(4, 8);
                            if (!currentGameState.carClass.allMembersAreRWD &&
                                leftFrontCornerSpecificWheelSpinTime > cornerExitSpinningThreshold && rightFrontCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning fronts out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningFrontsForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                            else if (!currentGameState.carClass.allMembersAreFWD &&
                                leftRearCornerSpecificWheelSpinTime > cornerExitSpinningThreshold && rightRearCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning rears out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningRearsForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                            else if (!currentGameState.carClass.allMembersAreRWD &&
                                leftFrontCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning left front out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningLeftFrontForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                            else if (!currentGameState.carClass.allMembersAreRWD &&
                                rightFrontCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning right front out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningRightFrontForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                            else if (!currentGameState.carClass.allMembersAreFWD &&
                                leftRearCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning left rear out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningLeftRearForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                            else if (!currentGameState.carClass.allMembersAreFWD &&
                                rightRearCornerSpecificWheelSpinTime > cornerExitSpinningThreshold)
                            {
                                Console.WriteLine("Spinning right rear out of " + currentCornerName);
                                audioPlayer.playMessage(new QueuedMessage("corner_spinning", delay + 10, secondsDelay: delay,
                                    messageFragments: MessageContents(folderSpinningRightRearForCornerWarning, "corners/" + currentCornerName), abstractEvent: this, priority: 0));
                                cornerSpinningWarningsPlayed[currentCornerName] = currentGameState.Now;
                            }
                        }
                    }
                }
            }

            leftFrontBrakeTemp = currentGameState.TyreData.LeftFrontBrakeTemp;
            rightFrontBrakeTemp = currentGameState.TyreData.RightFrontBrakeTemp;
            leftRearBrakeTemp = currentGameState.TyreData.LeftRearBrakeTemp;
            rightRearBrakeTemp = currentGameState.TyreData.RightRearBrakeTemp;
            leftFrontTyreTemp = currentGameState.TyreData.FrontLeft_CenterTemp;
            rightFrontTyreTemp = currentGameState.TyreData.FrontRight_CenterTemp;
            leftRearTyreTemp = currentGameState.TyreData.RearLeft_CenterTemp;
            rightRearTyreTemp = currentGameState.TyreData.RearRight_CenterTemp;

            currentTyreTempStatus = currentGameState.TyreData.TyreTempStatus;
            currentBrakeTempStatus = currentGameState.TyreData.BrakeTempStatus;

            if (isBrakeTempPeakForLap(currentGameState.TyreData.LeftFrontBrakeTemp,
                currentGameState.TyreData.RightFrontBrakeTemp, currentGameState.TyreData.LeftRearBrakeTemp,
                currentGameState.TyreData.RightRearBrakeTemp))
            {
                peakBrakeTempStatus = currentGameState.TyreData.BrakeTempStatus;
            }

            completedLaps = currentGameState.SessionData.CompletedLaps;
            lapsInSession = currentGameState.SessionData.SessionNumberOfLaps;
            timeInSession = currentGameState.SessionData.SessionTotalRunTime;
            timeElapsed = currentGameState.SessionData.SessionRunningTime;

            if (enableTyreTempWarnings && !currentGameState.SessionData.LeaderHasFinishedRace &&
                !currentGameState.PitData.InPitlane &&
                currentGameState.SessionData.CompletedLaps >= lapsIntoSessionBeforeTempMessage &&
                completedLaps >= lastTyreTempCheckLap + lapsBetweenTyreTempChecks &&
                currentGameState.SessionData.IsNewSector && currentGameState.SessionData.SectorNumber == thisLapTyreTempReportSector)
            {
                reportCurrentTyreTempStatus(false);
                lastTyreTempCheckLap = completedLaps;
            }

            if (!currentGameState.SessionData.LeaderHasFinishedRace &&
                    ((checkBrakesAtSector == 1 && currentGameState.SessionData.IsNewLap && completedLaps >= lastBrakeTempCheckLap + lapsBetweenBrakeTempChecks) ||
                    ((currentGameState.SessionData.IsNewSector && currentGameState.SessionData.SectorNumber == checkBrakesAtSector && completedLaps >= lastBrakeTempCheckLap + lapsBetweenBrakeTempChecks))) &&
                    (lastBrakeTempCheckSessionTime == -1.0f || ((currentGameState.SessionData.SessionRunningTime - lastBrakeTempCheckSessionTime) > secondsBetweenBrakeTempCheck)))
            {
                if (!currentGameState.PitData.InPitlane && currentGameState.SessionData.CompletedLaps >= lapsIntoSessionBeforeTempMessage)
                {
                    if (enableBrakeTempWarnings && !GlobalBehaviourSettings.useOvalLogic)
                    {
                        reportBrakeTempStatus(false, true);
                        lastBrakeTempCheckSessionTime = currentGameState.SessionData.SessionRunningTime;
                        lastBrakeTempCheckLap = completedLaps;
                    }
                }
                peakBrakeTempForLap = 0;
            }

            // only do tyre wear stuff if tyre wear is active
            currentTyreConditionStatus = currentGameState.TyreData.TyreConditionStatus;
            if (currentGameState.TyreData.TyreWearActive)
            {
                leftFrontWearPercent = currentGameState.TyreData.FrontLeftPercentWear;
                leftRearWearPercent = currentGameState.TyreData.RearLeftPercentWear;
                rightFrontWearPercent = currentGameState.TyreData.FrontRightPercentWear;
                rightRearWearPercent = currentGameState.TyreData.RearRightPercentWear;

                if (currentGameState.PitData.InPitlane && !currentGameState.SessionData.LeaderHasFinishedRace)
                {
                    if (currentGameState.SessionData.SessionType == SessionType.Race && enableTyreWearWarnings && !reportedTyreWearForCurrentPitEntry)
                    {
                        //reportCurrentTyreConditionStatus(false, true);
                        // sounds shit...
                        reportedTyreWearForCurrentPitEntry = true;
                    }
                }
                else
                {
                    reportedTyreWearForCurrentPitEntry = false;
                }
                if (currentGameState.SessionData.IsNewSector && currentGameState.SessionData.SectorNumber == thisLapTyreConditionReportSector
                    && !currentGameState.PitData.InPitlane && enableTyreWearWarnings && !currentGameState.SessionData.LeaderHasFinishedRace)
                {
                    reportCurrentTyreConditionStatus(false, false, delayResponses, false);
                }
                if (!currentGameState.PitData.InPitlane && enableTyreWearWarnings && !currentGameState.SessionData.LeaderHasFinishedRace &&
                    currentGameState.SessionData.SessionType == SessionType.Race)
                {
                    if (!reportedEstimatedTimeLeftOneThirdWear)
                    {
                        reportedEstimatedTimeLeftOneThirdWear = reportEstimatedTyreLife(33, currentGameState.SessionData.SessionRunningTime,
                            currentGameState.SessionData.SessionTotalRunTime, currentGameState.SessionData.CompletedLaps,
                            currentGameState.SessionData.SessionNumberOfLaps);
                    }
                    else if (!reportedEstimatedTimeLeftTwoThirdsWear)
                    {
                        reportedEstimatedTimeLeftTwoThirdsWear = reportEstimatedTyreLife(66, currentGameState.SessionData.SessionRunningTime,
                            currentGameState.SessionData.SessionTotalRunTime, currentGameState.SessionData.CompletedLaps,
                            currentGameState.SessionData.SessionNumberOfLaps);
                        if (reportedEstimatedTimeLeftTwoThirdsWear)
                        {
                            reportedEstimatedTimeLeftOneThirdWear = true;
                        }
                    }
                }
                // if the tyre wear has actually decreased, reset the 'reportdEstimatedTyreWear flag - assume this means the tyres have been changed
                if (currentGameState.SessionData.JustGoneGreen ||
                    (previousGameState != null && (currentGameState.TyreData.FrontLeftPercentWear < previousGameState.TyreData.FrontLeftPercentWear ||
                     currentGameState.TyreData.FrontRightPercentWear < previousGameState.TyreData.FrontRightPercentWear ||
                     currentGameState.TyreData.RearRightPercentWear < previousGameState.TyreData.RearRightPercentWear ||
                     currentGameState.TyreData.RearLeftPercentWear < previousGameState.TyreData.RearLeftPercentWear)))
                {
                    reportedEstimatedTimeLeftOneThirdWear = false;
                    reportedEstimatedTimeLeftTwoThirdsWear = false;

                    tyreLifeYPointsTime.Clear();
                    tyreLifeYPointsSectors.Clear();
                    tyreLifeXPointsLFWearBySector.Clear();
                    tyreLifeXPointsRFWearBySector.Clear();
                    tyreLifeXPointsLRWearBySector.Clear();
                    tyreLifeXPointsRRWearBySector.Clear();
                    tyreLifeXPointsLFWearByTime.Clear();
                    tyreLifeXPointsRFWearByTime.Clear();
                    tyreLifeXPointsLRWearByTime.Clear();
                    tyreLifeXPointsRRWearByTime.Clear();
                }
                else if (currentGameState.PositionAndMotionData.CarSpeed > 1)
                {
                    if (currentGameState.SessionData.IsNewSector)
                    {
                        // add some data if we're in a new sector
                        if (tyreLifeYPointsSectors.Count == 0)
                        {
                            // as we might have changed tyres, use the number of laps complete here to scale this
                            if (currentGameState.SessionData.SectorNumber == 2)
                            {
                                // special case here - our first measurement is for the sector1 end
                                tyreLifeYPointsSectors.Add(1 + (currentGameState.SessionData.CompletedLaps * 3));
                            }
                            else
                            {
                                tyreLifeYPointsSectors.Add(currentGameState.SessionData.CompletedLaps * 3);
                            }
                        }
                        else
                        {
                            tyreLifeYPointsSectors.Add((double)tyreLifeYPointsSectors[tyreLifeYPointsSectors.Count - 1] + 1);
                        }
                        tyreLifeXPointsLFWearBySector.Add((double)currentGameState.TyreData.FrontLeftPercentWear);
                        tyreLifeXPointsRFWearBySector.Add((double)currentGameState.TyreData.FrontRightPercentWear);
                        tyreLifeXPointsLRWearBySector.Add((double)currentGameState.TyreData.RearLeftPercentWear);
                        tyreLifeXPointsRRWearBySector.Add((double)currentGameState.TyreData.RearRightPercentWear);
                    }
                    if (lastTyreLifeYPointTime == -1)
                    {
                        tyreLifeYPointsTime.Add((double)currentGameState.SessionData.SessionRunningTime);
                        lastTyreLifeYPointTime = currentGameState.SessionData.SessionRunningTime;
                        tyreLifeXPointsLFWearByTime.Add((double)currentGameState.TyreData.FrontLeftPercentWear);
                        tyreLifeXPointsRFWearByTime.Add((double)currentGameState.TyreData.FrontRightPercentWear);
                        tyreLifeXPointsLRWearByTime.Add((double)currentGameState.TyreData.RearLeftPercentWear);
                        tyreLifeXPointsRRWearByTime.Add((double)currentGameState.TyreData.RearRightPercentWear);
                    }
                    else
                    {
                        float timeDiff = currentGameState.SessionData.SessionRunningTime - lastTyreLifeYPointTime;
                        if (timeDiff > 20)
                        {
                            tyreLifeYPointsTime.Add((double)currentGameState.SessionData.SessionRunningTime);
                            lastTyreLifeYPointTime = currentGameState.SessionData.SessionRunningTime;
                            tyreLifeXPointsLFWearByTime.Add((double)currentGameState.TyreData.FrontLeftPercentWear);
                            tyreLifeXPointsRFWearByTime.Add((double)currentGameState.TyreData.FrontRightPercentWear);
                            tyreLifeXPointsLRWearByTime.Add((double)currentGameState.TyreData.RearLeftPercentWear);
                            tyreLifeXPointsRRWearByTime.Add((double)currentGameState.TyreData.RearRightPercentWear);
                        }
                    }
                }
            }
        }

        public void reportCurrentTyreTempStatus(Boolean playImmediately)
        {
            // if all exactly zero then it is unlikely tire temps are supplied
            if (leftFrontTyreTemp == 0 && rightFrontTyreTemp == 0 && leftRearTyreTemp == 0 && rightRearTyreTemp == 0)
                return;

            // for FWD classes we don't care about rear tyres
            Boolean includeRears = playImmediately || !CrewChief.currentGameState.carClass.allMembersAreFWD;

            List<MessageFragment> messageContents = new List<MessageFragment>();
            addTyreTempWarningMessages(currentTyreTempStatus.getCornersForStatus(TyreTemp.COLD), TyreTemp.COLD, messageContents, includeRears);
            addTyreTempWarningMessages(currentTyreTempStatus.getCornersForStatus(TyreTemp.HOT), TyreTemp.HOT, messageContents, includeRears);
            addTyreTempWarningMessages(currentTyreTempStatus.getCornersForStatus(TyreTemp.COOKING), TyreTemp.COOKING, messageContents, includeRears);

            if (messageContents.Count == 0)
            {
                if (playImmediately || !lastTyreTempCheckOK)
                {
                    lastTyreTempCheckOK = true;
                    messageContents.Add(MessageFragment.Text(folderGoodTyreTemps));
                }
            }
            else
            {
                lastTyreTempCheckOK = false;
            }
            if (messageContents.Count > 0)
            {
                List<MessageFragment> lastTyreTempMessage = tyreTempMessagesPlayed.Count > 0 ? tyreTempMessagesPlayed.Last() : null;
                if (playImmediately)
                {
                    tyreTempMessagesPlayed.Add(messageContents);
                    audioPlayer.playMessageImmediately(new QueuedMessage("tyre_temps", 0, messageFragments: messageContents));
                }
                else if (lastTyreTempMessage == null || !messagesHaveSameContent(lastTyreTempMessage, messageContents))
                {
                    tyreTempMessagesPlayed.Add(messageContents);
                    Console.WriteLine("Tyre temp warning, temps : " + String.Join(", ", messageContents));
                    audioPlayer.playMessage(new QueuedMessage("tyre_temps", 0, messageContents, secondsDelay: Utilities.random.Next(0, 10), abstractEvent: this, priority: 5));
                    if (isBouncing(tyreTempMessagesPlayed, 3))
                    {
                        lapsBetweenTyreTempChecks++;
                        Console.WriteLine("This tyre temp message has already been played recently, increasing check interval");
                    }
                }
            }
        }

        public void reportBrakeTempStatus(Boolean playImmediately, Boolean peak)
        {
            // for FWD classes we don't care about rear tyres
            Boolean includeRears = playImmediately || !CrewChief.currentGameState.carClass.allMembersAreFWD;
            List<MessageFragment> messageContents = new List<MessageFragment>();
            if (peak)
            {
                if (!GlobalBehaviourSettings.useOvalLogic)
                {
                    addBrakeTempWarningMessages(peakBrakeTempStatus.getCornersForStatus(BrakeTemp.COLD), BrakeTemp.COLD, messageContents, includeRears);
                }
                addBrakeTempWarningMessages(peakBrakeTempStatus.getCornersForStatus(BrakeTemp.HOT), BrakeTemp.HOT, messageContents, includeRears);
                addBrakeTempWarningMessages(peakBrakeTempStatus.getCornersForStatus(BrakeTemp.COOKING), BrakeTemp.COOKING, messageContents, includeRears);
            }
            else
            {
                if (!GlobalBehaviourSettings.useOvalLogic)
                {
                    addBrakeTempWarningMessages(currentBrakeTempStatus.getCornersForStatus(BrakeTemp.COLD), BrakeTemp.COLD, messageContents, includeRears);
                }
                addBrakeTempWarningMessages(currentBrakeTempStatus.getCornersForStatus(BrakeTemp.HOT), BrakeTemp.HOT, messageContents, includeRears);
                addBrakeTempWarningMessages(currentBrakeTempStatus.getCornersForStatus(BrakeTemp.COOKING), BrakeTemp.COOKING, messageContents, includeRears);
            }
            if (messageContents.Count == 0)
            {
                if (playImmediately || !lastBrakeTempCheckOK)
                {
                    lastBrakeTempCheckOK = true;
                    messageContents.Add(MessageFragment.Text(folderGoodBrakeTemps));
                }
                else
                {
                    return;
                }
            }
            else
            {
                lastBrakeTempCheckOK = false;
            }

            if (messageContents.Count > 0)
            {
                List<MessageFragment> lastBrakeTempMessage = brakeTempMessagesPlayed.Count > 0 ? brakeTempMessagesPlayed.Last() : null;
                if (playImmediately)
                {
                    brakeTempMessagesPlayed.Add(messageContents);
                    audioPlayer.playMessageImmediately(new QueuedMessage("brake_temps", 0, messageFragments: messageContents));
                }
                else if (lastBrakeTempMessage == null || !messagesHaveSameContent(lastBrakeTempMessage, messageContents))
                {
                    brakeTempMessagesPlayed.Add(messageContents);
                    audioPlayer.playMessage(new QueuedMessage("brake_temps", 0, messageFragments: messageContents, secondsDelay: Utilities.random.Next(0, 10), abstractEvent: this, priority: 5));
                    if (isBouncing(brakeTempMessagesPlayed, 3))
                    {
                        lapsBetweenBrakeTempChecks++;
                        secondsBetweenBrakeTempCheck += 120f;
                        Console.WriteLine("This brake temp message has already been played recently, increasing check interval");
                    }
                }
            }
        }

        // check if this set of messages has already been played recently (in the last messagesToCheck times).
        private Boolean isBouncing(List<List<MessageFragment>> messages, int messagesToCheck)
        {
            if (messages.Count > 2)
            {
                int messagesChecked = 0;
                List<MessageFragment> thisSet = messages.Last();
                for (int i = messages.Count - 2; i >= 0; i--)
                {
                    List<MessageFragment> earlierSet = messages[i];
                    if (messagesHaveSameContent(thisSet, earlierSet))
                    {
                        return true;
                    }
                    messagesChecked++;
                    if (messagesChecked >= messagesToCheck)
                    {
                        break;
                    }
                }
            }
            return false;
        }

        private void reportCurrentTyreConditionStatus(Boolean playImmediately, Boolean playEvenIfUnchanged, Boolean allowDelayedResponse, Boolean tyreStatusRequestOnly)
        {
            List<MessageFragment> messageContents = new List<MessageFragment>();
            addTyreConditionWarningMessages(currentTyreConditionStatus.getCornersForStatus(TyreCondition.MINOR_WEAR), TyreCondition.MINOR_WEAR, messageContents);
            addTyreConditionWarningMessages(currentTyreConditionStatus.getCornersForStatus(TyreCondition.MAJOR_WEAR), TyreCondition.MAJOR_WEAR, messageContents);
            addTyreConditionWarningMessages(currentTyreConditionStatus.getCornersForStatus(TyreCondition.WORN_OUT), TyreCondition.WORN_OUT, messageContents);
            Boolean wearIsGood = false;
            if (messageContents.Count == 0)
            {
                messageContents.Add(MessageFragment.Text(tyreStatusRequestOnly ? folderGoodWear : folderGoodWearGeneral));
                wearIsGood = true;
            }

            if (playImmediately)
            {
                // might be a "stand by..." response
                if (allowDelayedResponse && Utilities.random.Next(10) >= 2)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderStandBy, 0));
                    int secondsDelay = Math.Max(5, Utilities.random.Next(11));
                    audioPlayer.pauseQueue(secondsDelay);
                    audioPlayer.playDelayedImmediateMessage(new QueuedMessage("tyre_condition", 0, messageFragments: messageContents, secondsDelay: secondsDelay));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("tyre_condition", 0, messageFragments: messageContents));
                }
            }
            else if (playEvenIfUnchanged ||
                (lastTyreConditionMessage != null && !messagesHaveSameContent(lastTyreConditionMessage, messageContents) && !wearIsGood))
            {
                audioPlayer.playMessage(new QueuedMessage("tyre_condition", 0, messageFragments: messageContents, secondsDelay: Utilities.random.Next(0, 10), abstractEvent: this, priority: 5));
            }
            lastTyreConditionMessage = messageContents;
        }

        private void playEstimatedTyreLifeMinutes(int minutesRemainingOnTheseTyres, Boolean immediate)
        {
            if (immediate)
            {
                if (minutesRemainingOnTheseTyres <= 1)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderKnackeredAllRound, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("minutes_on_current_tyres", 0,
                        messageFragments: MessageContents(folderMinutesOnCurrentTyresIntro, minutesRemainingOnTheseTyres, folderMinutesOnCurrentTyresOutro)));
                }
            }
            else if (minutesRemainingOnTheseTyres > 1 && minutesRemainingOnTheseTyres <= 4 + (timeInSession - timeElapsed) / 60)
            {
                audioPlayer.playMessage(new QueuedMessage("minutes_on_current_tyres", 0,
                    messageFragments: MessageContents(folderMinutesOnCurrentTyresIntro, minutesRemainingOnTheseTyres, folderMinutesOnCurrentTyresOutro), abstractEvent: this, priority: 5));
            }
        }

        private void playEstimatedTypeLifeLaps(int lapsRemainingOnTheseTyres, Boolean immediate)
        {
            if (immediate)
            {
                if (lapsRemainingOnTheseTyres <= 1)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderKnackeredAllRound, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("laps_on_current_tyres", 0,
                        messageFragments: MessageContents(
                            folderLapsOnCurrentTyresIntro,
                            MessageFragment.Integer(lapsRemainingOnTheseTyres, MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)),
                            folderLapsOnCurrentTyresOutro)));
                }
            }
            else if (lapsRemainingOnTheseTyres > 1 && lapsRemainingOnTheseTyres <= 2 + lapsInSession - completedLaps)
            {
                audioPlayer.playMessage(new QueuedMessage("laps_on_current_tyres", 0,
                    messageFragments: MessageContents(
                        folderLapsOnCurrentTyresIntro,
                        MessageFragment.Integer(lapsRemainingOnTheseTyres, MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)),
                        folderLapsOnCurrentTyresOutro),
                    abstractEvent: this, priority: 5));
            }
        }

        private int getRemainingTyreLife(float sessionRunningTime, Tuple<CornerData.Corners, float> maxWearPercent)
        {
            // if the maxWear is quite low (<20%) the quadratic estimate is quite inaccurate - use linear in this case?
            if (lapsInSession > 0 || timeInSession == 0)
            {
                double[] x_data;
                switch (maxWearPercent.Item1)
                {
                    case CornerData.Corners.FRONT_LEFT:
                        x_data = tyreLifeXPointsLFWearBySector.ToArray();
                        break;
                    case CornerData.Corners.FRONT_RIGHT:
                        x_data = tyreLifeXPointsRFWearBySector.ToArray();
                        break;
                    case CornerData.Corners.REAR_LEFT:
                        x_data = tyreLifeXPointsLRWearBySector.ToArray();
                        break;
                    default:
                        x_data = tyreLifeXPointsRRWearBySector.ToArray();
                        break;
                }
                if (x_data.Length < 5 || tyreLifeYPointsSectors.Count != x_data.Length)
                {
                    return -1;
                }
                int sectorCountAtFullWear = (int)Utilities.getYEstimate(x_data, tyreLifeYPointsSectors.ToArray(), 97, 2);
                // we know how many more sectors we expect to complete, so just divide it by 3
                return (sectorCountAtFullWear / 3) - completedLaps;
            }
            else
            {
                double[] x_data;
                switch (maxWearPercent.Item1)
                {
                    case CornerData.Corners.FRONT_LEFT:
                        x_data = tyreLifeXPointsLFWearByTime.ToArray();
                        break;
                    case CornerData.Corners.FRONT_RIGHT:
                        x_data = tyreLifeXPointsRFWearByTime.ToArray();
                        break;
                    case CornerData.Corners.REAR_LEFT:
                        x_data = tyreLifeXPointsLRWearByTime.ToArray();
                        break;
                    default:
                        x_data = tyreLifeXPointsRRWearByTime.ToArray();
                        break;
                }
                if (x_data.Length < 5 || tyreLifeYPointsTime.Count != x_data.Length)
                {
                    return -1;
                }
                double expectedSessionTimeAtFullWear = Utilities.getYEstimate(x_data, tyreLifeYPointsTime.ToArray(), 95, 2);
                return (int)Math.Ceiling((expectedSessionTimeAtFullWear - sessionRunningTime) / 60);
            }
        }

        private Boolean reportEstimatedTyreLife(float maxWearThreshold, float sessionRunningTime, float sessionTotalRunTime,
            int lapsCompleted, int sessionTotalLaps)
        {
            Tuple<CornerData.Corners, float> maxWearPercent = getMaxWearPercent();
            if (maxWearPercent.Item2 >= maxWearThreshold)
            {
                int tyreLifeRemaining = getRemainingTyreLife(sessionRunningTime, maxWearPercent);
                if (tyreLifeRemaining != -1)
                {
                    if (lapsInSession > 0 || timeInSession == 0)
                    {
                        // only announce this if the estimate is close to or smaller than race distance
                        int sessionLapsRemaining = sessionTotalLaps - lapsCompleted;
                        if (tyreLifeRemaining - sessionLapsRemaining < 2)
                        {
                            playEstimatedTypeLifeLaps(tyreLifeRemaining, false);
                        }
                    }
                    else
                    {
                        float sessionTimeRemaining = (sessionTotalRunTime - sessionRunningTime) / 60;
                        // only announce this if the estimate is close to or smaller than race distance
                        if (tyreLifeRemaining - sessionTimeRemaining < 2)
                        {
                            playEstimatedTyreLifeMinutes(tyreLifeRemaining, false);
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private Tuple<CornerData.Corners, float> getMaxWearPercent()
        {
            if (GlobalBehaviourSettings.useOvalLogic)
            {
                if (rightFrontWearPercent > rightRearWearPercent)
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.FRONT_RIGHT, rightFrontWearPercent);
                }
                else
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.REAR_RIGHT, rightRearWearPercent);
                }
            }
            else
            {
                if (rightFrontWearPercent > rightRearWearPercent && rightFrontWearPercent > leftFrontWearPercent && rightFrontWearPercent > leftRearWearPercent)
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.FRONT_RIGHT, rightFrontWearPercent);
                }
                else if (rightRearWearPercent > rightFrontWearPercent && rightRearWearPercent > leftFrontWearPercent && rightRearWearPercent > leftRearWearPercent)
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.REAR_RIGHT, rightRearWearPercent);
                }
                else if (leftFrontWearPercent > rightRearWearPercent && leftFrontWearPercent > rightFrontWearPercent && leftFrontWearPercent > leftRearWearPercent)
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.FRONT_LEFT, leftFrontWearPercent);
                }
                else
                {
                    return new Tuple<CornerData.Corners, float>(CornerData.Corners.REAR_LEFT, leftRearWearPercent);
                }
            }
        }

        private void reportCurrentTyreTemps()
        {
            if (leftFrontTyreTemp == 0 && rightFrontTyreTemp == 0 && leftRearTyreTemp == 0 && rightRearTyreTemp == 0)
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
            }
            else
            {
                audioPlayer.playMessageImmediately(new QueuedMessage("tyre_temps", 0,
                    messageFragments: MessageContents(folderLeftFront, convertTemp(leftFrontTyreTemp), folderRightFront, convertTemp(rightFrontTyreTemp),
                                                       folderLeftRear, convertTemp(leftRearTyreTemp), folderRightRear, convertTemp(rightRearTyreTemp), getTempUnit())));
            }

        }

        private void reportCurrentBrakeTemps()
        {
            if (leftFrontBrakeTemp == 0 && rightFrontBrakeTemp == 0 && leftRearBrakeTemp == 0 && rightRearBrakeTemp == 0)
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
            }
            else
            {
                audioPlayer.playMessageImmediately(new QueuedMessage("brake_temps", 0,
                    messageFragments: MessageContents(folderLeftFront, convertTemp(leftFrontBrakeTemp, 10), folderRightFront, convertTemp(rightFrontBrakeTemp, 10),
                                                      folderLeftRear, convertTemp(leftRearBrakeTemp, 10), folderRightRear, convertTemp(rightRearBrakeTemp, 10), getTempUnit())));
            }

        }

        public override void respond(string voiceMessage)
        {
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_TYRE_TEMPS))
            {
                if (currentTyreTempStatus != null)
                {
                    reportCurrentTyreTempStatus(true);
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_TYRE_TEMPS))
            {
                reportCurrentTyreTemps();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_BRAKE_TEMPS))
            {
                if (currentBrakeTempStatus != null)
                {
                    reportBrakeTempStatus(true, true);
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_BRAKE_TEMPS))
            {
                reportCurrentBrakeTemps();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_TYRE_WEAR))
            {
                Boolean forStatusReport = !SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_TYRE_WEAR);
                if (CrewChief.gameDefinition.gameEnum != GameEnum.IRACING && currentTyreConditionStatus != null)
                {
                    reportCurrentTyreConditionStatus(true, true, delayResponses, true);
                }
                else if (!forStatusReport)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_THE_RELATIVE_TYRE_PERFORMANCES))
            {
                List<TyrePerformanceContainer> tyrePerformances = getRelativeTyrePerformance();
                foreach (TyrePerformanceContainer tyrePeformance in tyrePerformances)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("tyre_comparison_" + tyrePeformance.type1 + "-" + tyrePeformance.type2, 0,
                        messageFragments: MessageContents(getFolderForTyreType(tyrePeformance.type1), folderAreAbout,
                                        TimeSpanWrapper.FromSeconds(tyrePeformance.bestLapDelta, Precision.AUTO_GAPS),
                                        folderFasterThan, getFolderForTyreType(tyrePeformance.type2))));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.CAR_STATUS) ||
                SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.STATUS))
            {
                if (currentTyreConditionStatus != null)
                {
                    reportCurrentTyreConditionStatus(true, true, false, false);
                }
                if (currentTyreTempStatus != null)
                {
                    reportCurrentTyreTempStatus(true);
                }
                if (currentBrakeTempStatus != null)
                {
                    reportBrakeTempStatus(true, true);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_LONG_WILL_THESE_TYRES_LAST))
            {
                if (CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    Tuple<CornerData.Corners, float> maxWearPercent = getMaxWearPercent();
                    Console.WriteLine("Tyre life estimate, wear active = " + CrewChief.currentGameState.TyreData.TyreWearActive);
                    Console.WriteLine("Max wear is " + maxWearPercent.Item2 + "% on " + maxWearPercent.Item1);
                    if (CrewChief.gameDefinition.gameEnum == GameEnum.IRACING)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                    }
                    else if (maxWearPercent.Item2 < 1)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderGoodWear, 0));
                    }
                    else if (maxWearPercent.Item2 < 5)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                    }
                    else
                    {
                        int remaining = CrewChief.currentGameState == null ? -1 : getRemainingTyreLife(CrewChief.currentGameState.SessionData.SessionRunningTime, maxWearPercent);
                        if (remaining != -1)
                        {
                            if (lapsInSession > 0 || timeInSession == 0)
                            {
                                playEstimatedTypeLifeLaps(remaining, true);
                            }
                            else
                            {
                                playEstimatedTyreLifeMinutes(remaining, true);
                            }
                        }
                        else
                        {
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                        }
                    }
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_LEFT_FRONT_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(CrewChief.currentGameState.TyreData.FrontLeft_RightTemp, CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp);
                    int absoluteDiff = (int)Math.Round(CrewChief.currentGameState.TyreData.FrontLeft_RightTemp - CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderLFIMOSame, folderLFInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_LEFT_FRONT_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(leftFrontAverageIMOLastLap[0], leftFrontAverageIMOLastLap[2]);
                    int absoluteDiff = (int)Math.Round(leftFrontAverageIMOLastLap[0] - leftFrontAverageIMOLastLap[2]);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderLFIMOSame, folderLFInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_RIGHT_FRONT_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(CrewChief.currentGameState.TyreData.FrontRight_LeftTemp, CrewChief.currentGameState.TyreData.FrontRight_RightTemp);
                    int absoluteDiff = (int)Math.Round(CrewChief.currentGameState.TyreData.FrontRight_LeftTemp - CrewChief.currentGameState.TyreData.FrontRight_RightTemp);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderRFIMOSame, folderRFInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_RIGHT_FRONT_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(rightFrontAverageIMOLastLap[0], rightFrontAverageIMOLastLap[2]);
                    int absoluteDiff = (int)Math.Round(rightFrontAverageIMOLastLap[0] - rightFrontAverageIMOLastLap[2]);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderRFIMOSame, folderRFInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_LEFT_REAR_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(CrewChief.currentGameState.TyreData.RearLeft_RightTemp, CrewChief.currentGameState.TyreData.RearLeft_LeftTemp);
                    int absoluteDiff = (int)Math.Round(CrewChief.currentGameState.TyreData.RearLeft_RightTemp - CrewChief.currentGameState.TyreData.RearLeft_LeftTemp);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderLRIMOSame, folderLRInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_LEFT_REAR_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(leftRearAverageIMOLastLap[0], leftRearAverageIMOLastLap[2]);
                    int absoluteDiff = (int)Math.Round(leftRearAverageIMOLastLap[0] - leftRearAverageIMOLastLap[2]);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderLRIMOSame, folderLRInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_RIGHT_REAR_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(CrewChief.currentGameState.TyreData.RearRight_LeftTemp, CrewChief.currentGameState.TyreData.RearRight_RightTemp);
                    int absoluteDiff = (int)Math.Round(CrewChief.currentGameState.TyreData.RearRight_LeftTemp - CrewChief.currentGameState.TyreData.RearRight_RightTemp);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderRRIMOSame, folderRRInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_RIGHT_REAR_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float percentageDiff = getPercentageDiff(rightRearAverageIMOLastLap[0], rightRearAverageIMOLastLap[2]);
                    int absoluteDiff = (int)Math.Round(rightRearAverageIMOLastLap[0] - rightRearAverageIMOLastLap[2]);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderRRIMOSame, folderRRInnerIsRunning, folderCelsiusHotterThanOuter, folderCelsiusColderThanOuter, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_FRONT_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float averageInner = (CrewChief.currentGameState.TyreData.FrontLeft_RightTemp + CrewChief.currentGameState.TyreData.FrontRight_LeftTemp) / 2f;
                    float averageOuter = (CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp + CrewChief.currentGameState.TyreData.FrontRight_RightTemp) / 2f;
                    float percentageDiff = getPercentageDiff(averageInner, averageOuter);
                    int absoluteDiff = (int)Math.Round(averageInner - averageOuter);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderAverageFrontsIMOSame, folderAverageFrontInnerTempsIntro, folderCelsiusHotterThanOuters, folderCelsiusColderThanOuters, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_FRONT_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float averageInner = (leftFrontAverageIMOLastLap[0] + rightFrontAverageIMOLastLap[0]) / 2f;
                    float averageOuter = (leftFrontAverageIMOLastLap[2] + leftFrontAverageIMOLastLap[2]) / 2f;
                    float percentageDiff = getPercentageDiff(averageInner, averageOuter);
                    int absoluteDiff = (int)Math.Round(averageInner - averageOuter);
                    Console.WriteLine("camber diff from average is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderAverageFrontsIMOSame, folderAverageFrontInnerTempsIntro, folderCelsiusHotterThanOuters, folderCelsiusColderThanOuters, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_REAR_CAMBER_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float averageInner = (CrewChief.currentGameState.TyreData.RearLeft_RightTemp + CrewChief.currentGameState.TyreData.RearRight_LeftTemp) / 2f;
                    float averageOuter = (CrewChief.currentGameState.TyreData.RearLeft_LeftTemp + CrewChief.currentGameState.TyreData.RearRight_RightTemp) / 2f;
                    float percentageDiff = getPercentageDiff(averageInner, averageOuter);
                    int absoluteDiff = (int)Math.Round(averageInner - averageOuter);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderAverageRearsIMOSame, folderAverageRearInnerTempsIntro, folderCelsiusHotterThanOuters, folderCelsiusColderThanOuters, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOWS_MY_REAR_CAMBER))
            {
                if (!hasUsableIMOTempData(true))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    float averageInner = (leftRearAverageIMOLastLap[0] + rightRearAverageIMOLastLap[0]) / 2f;
                    float averageOuter = (leftRearAverageIMOLastLap[2] + leftRearAverageIMOLastLap[2]) / 2f;
                    float percentageDiff = getPercentageDiff(averageInner, averageOuter);
                    int absoluteDiff = (int)Math.Round(averageInner - averageOuter);
                    Console.WriteLine("camber diff is " + absoluteDiff + " (" + percentageDiff + ") %");
                    playCamberMessage(percentageDiff, absoluteDiff, folderAverageRearsIMOSame, folderAverageRearInnerTempsIntro, folderCelsiusHotterThanOuters, folderCelsiusColderThanOuters, delayResponses);
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_TYRE_PRESSURES_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(getPressureMessage(CornerData.Corners.FRONT_LEFT, false), getPressureMessage(CornerData.Corners.FRONT_RIGHT, false),
                        getPressureMessage(CornerData.Corners.REAR_LEFT, false), getPressureMessage(CornerData.Corners.REAR_RIGHT, false)));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_TYRE_PRESSURES))
            {
                if (!hasUsableIMOTempData(true) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(getPressureMessage(CornerData.Corners.FRONT_LEFT, true), getPressureMessage(CornerData.Corners.FRONT_RIGHT, true),
                        getPressureMessage(CornerData.Corners.REAR_LEFT, true), getPressureMessage(CornerData.Corners.REAR_RIGHT, true)));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_FRONT_TYRE_PRESSURES_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(getPressureMessage(CornerData.Corners.FRONT_LEFT, false), getPressureMessage(CornerData.Corners.FRONT_RIGHT, false), null, null));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_FRONT_TYRE_PRESSURES))
            {
                if (!hasUsableIMOTempData(true) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(getPressureMessage(CornerData.Corners.FRONT_LEFT, true), getPressureMessage(CornerData.Corners.FRONT_RIGHT, true), null, null));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_REAR_TYRE_PRESSURES_RIGHT_NOW))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(null, null, getPressureMessage(CornerData.Corners.REAR_LEFT, false), getPressureMessage(CornerData.Corners.REAR_RIGHT, false)));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_ARE_MY_REAR_TYRE_PRESSURES))
            {
                if (!hasUsableIMOTempData(true) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    playPressureMessage(delayResponses,
                        aggregatePressureMessages(null, null, getPressureMessage(CornerData.Corners.REAR_LEFT, true), getPressureMessage(CornerData.Corners.REAR_RIGHT, true)));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_LEFT_FRONT_SURFACE_TEMPS))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("left front surface temps", 0, MessageContents(
                        convertTemp(CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.FrontLeft_CenterTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.FrontLeft_RightTemp, 1), 
                        getTempUnit())));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_RIGHT_FRONT_SURFACE_TEMPS))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("right front surface temps", 0, MessageContents(
                        convertTemp(CrewChief.currentGameState.TyreData.FrontRight_RightTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.FrontRight_CenterTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.FrontRight_LeftTemp, 1),
                        getTempUnit())));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_LEFT_REAR_SURFACE_TEMPS))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("left rear surface temps", 0, MessageContents(
                        convertTemp(CrewChief.currentGameState.TyreData.RearLeft_LeftTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.RearLeft_CenterTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.RearLeft_RightTemp, 1),
                        getTempUnit())));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_ARE_MY_RIGHT_REAR_SURFACE_TEMPS))
            {
                if (!hasUsableIMOTempData(false) || CrewChief.currentGameState == null)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("right front surface temps", 0, MessageContents(
                        convertTemp(CrewChief.currentGameState.TyreData.RearRight_RightTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.RearRight_CenterTemp, 1),
                        convertTemp(CrewChief.currentGameState.TyreData.RearRight_LeftTemp, 1),
                        getTempUnit())));
                }
            }
        }

        private void playPressureMessage(Boolean allowDelayedResponse, string folder)
        {
            playPressureMessage(allowDelayedResponse, new String[] {folder});
        }

        private void playPressureMessage(Boolean allowDelayedResponse, string[] folders)
        {
            List<MessageFragment> fragments = new List<MessageFragment>();
            Array.ForEach(folders, f => fragments.Add(MessageFragment.Text(f)));
            QueuedMessage message = new QueuedMessage("pressures", 0, fragments);
            if (delayResponses && Utilities.random.Next(10) >= 2 && SoundCache.availableSounds.Contains(AudioPlayer.folderStandBy))
            {
                this.audioPlayer.pauseQueueAndPlayDelayedImmediateMessage(message, 5 /*lowerDelayBoundInclusive*/, 7 /*upperDelayBound*/);
            }
            else
            {
                this.audioPlayer.playMessageImmediately(message);
            }
        }

        private String getPressureMessage(CornerData.Corners corners, Boolean useAverageData)
        {

            float percentDiff;
            float averageEdgeTemp;
            float middleTemp;
            switch (corners)
            {
                case CornerData.Corners.FRONT_LEFT:
                    averageEdgeTemp = useAverageData ? leftFrontAverageIMOLastLap[0] + leftFrontAverageIMOLastLap[2] / 2 :
                        (CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp + CrewChief.currentGameState.TyreData.FrontLeft_RightTemp) / 2;
                    middleTemp = useAverageData ? leftFrontAverageIMOLastLap[1] :
                        CrewChief.currentGameState.TyreData.FrontLeft_CenterTemp;
                    percentDiff = 100 * (middleTemp - averageEdgeTemp) / middleTemp;
                    if (percentDiff > 10)
                    {
                        return folderLeftFrontPressureVeryHigh;
                    }
                    else if (percentDiff > 4)
                    {
                        return folderLeftFrontPressureHigh;
                    }
                    else if (percentDiff < 4)
                    {
                        return folderLeftFrontPressureLow;
                    }
                    else if (percentDiff < 10)
                    {
                        return folderLeftFrontPressureVeryLow;
                    }
                    else
                    {
                        return folderLeftFrontPressureOK;
                    }
                case CornerData.Corners.FRONT_RIGHT:
                    averageEdgeTemp = useAverageData ? rightFrontAverageIMOLastLap[0] + rightFrontAverageIMOLastLap[2] / 2 :
                        (CrewChief.currentGameState.TyreData.FrontRight_LeftTemp + CrewChief.currentGameState.TyreData.FrontRight_RightTemp) / 2;
                    middleTemp = useAverageData ? leftFrontAverageIMOLastLap[1] :
                        CrewChief.currentGameState.TyreData.FrontLeft_CenterTemp;
                    percentDiff = 100 * (middleTemp - averageEdgeTemp) / middleTemp;
                    if (percentDiff > 10)
                    {
                        return folderRightFrontPressureVeryHigh;
                    }
                    else if (percentDiff > 4)
                    {
                        return folderRightFrontPressureHigh;
                    }
                    else if (percentDiff < 4)
                    {
                        return folderRightFrontPressureLow;
                    }
                    else if (percentDiff < 10)
                    {
                        return folderRightFrontPressureVeryLow;
                    }
                    else
                    {
                        return folderRightFrontPressureOK;
                    }
                case CornerData.Corners.REAR_LEFT:
                    averageEdgeTemp = useAverageData ? leftRearAverageIMOLastLap[0] + leftRearAverageIMOLastLap[2] / 2 :
                        (CrewChief.currentGameState.TyreData.RearLeft_LeftTemp + CrewChief.currentGameState.TyreData.RearLeft_RightTemp) / 2;
                    middleTemp = useAverageData ? leftFrontAverageIMOLastLap[1] :
                        CrewChief.currentGameState.TyreData.FrontLeft_CenterTemp;
                    percentDiff = 100 * (middleTemp - averageEdgeTemp) / middleTemp;
                    if (percentDiff > 10)
                    {
                        return folderLeftRearPressureVeryHigh;
                    }
                    else if (percentDiff > 4)
                    {
                        return folderLeftRearPressureHigh;
                    }
                    else if (percentDiff < 4)
                    {
                        return folderLeftRearPressureLow;
                    }
                    else if (percentDiff < 10)
                    {
                        return folderLeftRearPressureVeryLow;
                    }
                    else
                    {
                        return folderLeftRearPressureOK;
                    }
                case CornerData.Corners.REAR_RIGHT:
                    averageEdgeTemp = useAverageData ? rightRearAverageIMOLastLap[0] + rightRearAverageIMOLastLap[2] / 2 :
                        (CrewChief.currentGameState.TyreData.RearRight_LeftTemp + CrewChief.currentGameState.TyreData.RearRight_RightTemp) / 2;
                    middleTemp = useAverageData ? leftFrontAverageIMOLastLap[1] :
                        CrewChief.currentGameState.TyreData.FrontLeft_CenterTemp;
                    percentDiff = 100 * (middleTemp - averageEdgeTemp) / middleTemp;
                    if (percentDiff > 10)
                    {
                        return folderRightRearPressureVeryHigh;
                    }
                    else if (percentDiff > 4)
                    {
                        return folderRightRearPressureHigh;
                    }
                    else if (percentDiff < 4)
                    {
                        return folderRightRearPressureLow;
                    }
                    else if (percentDiff < 10)
                    {
                        return folderRightRearPressureVeryLow;
                    }
                    else
                    {
                        return folderRightRearPressureOK;
                    }
                default:
                    return null;
            }
        }

        private void playCamberMessage(float percentageDiff, int absoluteDiff, String folderSame, String folderDiffIntro, String folderHotterThanOutersOutro,
            String folderColderThanOutersOutro, Boolean allowDelayedResponse)
        {
            float[] targets = getTargetCamberPercentDiff();
            float minTarget = targets[0];
            float maxTarget = targets[1];
            QueuedMessage message;
            if (Math.Abs(absoluteDiff) <= 1)
            {
                message = new QueuedMessage("imo_diff", 0, messageFragments: MessageContents(folderSame, folderNeedMoreNegativeCamber));
            }
            else if (percentageDiff < -1)
            {
                message = new QueuedMessage("imo_diff", 0, messageFragments: MessageContents(folderDiffIntro, absoluteDiff * -1, folderColderThanOutersOutro, folderNeedMoreNegativeCamber));
            }
            else if (percentageDiff > maxTarget)
            {
                message = new QueuedMessage("imo_diff", 0, messageFragments: MessageContents(folderDiffIntro, absoluteDiff, folderHotterThanOutersOutro, folderNeedMorePositiveCamber));
            }
            else if (percentageDiff < minTarget)
            {
                message = new QueuedMessage("imo_diff", 0, messageFragments: MessageContents(folderDiffIntro, absoluteDiff, folderHotterThanOutersOutro, folderNeedMoreNegativeCamber));
            }
            else
            {
                message = new QueuedMessage("imo_diff", 0, messageFragments: MessageContents(folderDiffIntro, absoluteDiff, folderHotterThanOutersOutro, folderCamberOK));
            }
            if (delayResponses && Utilities.random.Next(10) >= 2 && SoundCache.availableSounds.Contains(AudioPlayer.folderStandBy))
            {
                this.audioPlayer.pauseQueueAndPlayDelayedImmediateMessage(message, 5 /*lowerDelayBoundInclusive*/, 7 /*upperDelayBound*/);
            }
            else
            {
                this.audioPlayer.playMessageImmediately(message);
            }
        }

        private float getPercentageDiff(float inner, float outer)
        {
            return 100 * (inner - outer) / outer;
        }

        private float[] getTargetCamberPercentDiff()
        {
            float[] target;
            if (TyreMonitor.idealCamberValues.TryGetValue(CrewChief.currentGameState.TyreData.FrontLeftTyreType, out target))
            {
                return target;
            }
            else
            {
                return defaultTargetIODifference;
            }
        }

        private Boolean hasUsableIMOTempData(Boolean useAverageData)
        {
            return CrewChief.gameDefinition.gameEnum != GameEnum.IRACING && CrewChief.gameDefinition.gameEnum != GameEnum.F1_2018 && CrewChief.gameDefinition.gameEnum != GameEnum.F1_2019 && CrewChief.gameDefinition.gameEnum != GameEnum.F1_2020 &&
                ((useAverageData && leftFrontAverageIMOLastLap != null && rightFrontAverageIMOLastLap != null && leftRearAverageIMOLastLap != null && rightRearAverageIMOLastLap != null) ||
                (!useAverageData && CrewChief.currentGameState != null && CrewChief.currentGameState.TyreData != null && CrewChief.currentGameState.TyreData.FrontLeft_LeftTemp > 30));
        }

        private void addTyreTempWarningMessages(CornerData.Corners corners, TyreTemp tyreTemp, List<MessageFragment> messageContents, Boolean includeRears)
        {
            switch (corners)
            {
                case CornerData.Corners.ALL:
                    switch (tyreTemp)
                    {
                        case TyreTemp.COLD:
                            messageContents.Add(MessageFragment.Text(folderColdTyresAllRound));
                            break;
                        case TyreTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotTyresAllRound));
                            break;
                        case TyreTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingTyresAllRound));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONTS:
                    switch (tyreTemp)
                    {
                        case TyreTemp.COLD:
                            messageContents.Add(MessageFragment.Text(folderColdFrontTyres));
                            break;
                        case TyreTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotFrontTyres));
                            break;
                        case TyreTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingFrontTyres));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REARS:
                    if (includeRears)
                    {
                        switch (tyreTemp)
                        {
                            case TyreTemp.COLD:
                                messageContents.Add(MessageFragment.Text(folderColdRearTyres));
                                break;
                            case TyreTemp.HOT:
                                messageContents.Add(MessageFragment.Text(folderHotRearTyres));
                                break;
                            case TyreTemp.COOKING:
                                messageContents.Add(MessageFragment.Text(folderCookingRearTyres));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case CornerData.Corners.LEFTS:
                    if (!GlobalBehaviourSettings.useOvalLogic)
                    {
                        switch (tyreTemp)
                        {
                            case TyreTemp.COLD:
                                messageContents.Add(MessageFragment.Text(folderColdLeftTyres));
                                break;
                            case TyreTemp.HOT:
                                messageContents.Add(MessageFragment.Text(folderHotLeftTyres));
                                break;
                            case TyreTemp.COOKING:
                                messageContents.Add(MessageFragment.Text(folderCookingLeftTyres));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case CornerData.Corners.RIGHTS:
                    switch (tyreTemp)
                    {
                        case TyreTemp.COLD:
                            messageContents.Add(MessageFragment.Text(folderColdRightTyres));
                            break;
                        case TyreTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotRightTyres));
                            break;
                        case TyreTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingRightTyres));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONT_LEFT:
                    if (!GlobalBehaviourSettings.useOvalLogic)
                    {
                        switch (tyreTemp)
                        {
                            case TyreTemp.HOT:
                                messageContents.Add(MessageFragment.Text(folderHotLeftFrontTyre));
                                break;
                            case TyreTemp.COOKING:
                                messageContents.Add(MessageFragment.Text(folderCookingLeftFrontTyre));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case CornerData.Corners.FRONT_RIGHT:
                    switch (tyreTemp)
                    {
                        case TyreTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotRightFrontTyre));
                            break;
                        case TyreTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingRightFrontTyre));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REAR_LEFT:
                    if (includeRears)
                    {
                        if (!GlobalBehaviourSettings.useOvalLogic)
                        {
                            switch (tyreTemp)
                            {
                                case TyreTemp.HOT:
                                    messageContents.Add(MessageFragment.Text(folderHotLeftRearTyre));
                                    break;
                                case TyreTemp.COOKING:
                                    messageContents.Add(MessageFragment.Text(folderCookingLeftRearTyre));
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    break;
                case CornerData.Corners.REAR_RIGHT:
                    if (includeRears)
                    {
                        switch (tyreTemp)
                        {
                            case TyreTemp.HOT:
                                messageContents.Add(MessageFragment.Text(folderHotRightRearTyre));
                                break;
                            case TyreTemp.COOKING:
                                messageContents.Add(MessageFragment.Text(folderCookingRightRearTyre));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
            }
        }

        private void addTyreConditionWarningMessages(CornerData.Corners corners, TyreCondition tyreCondition, List<MessageFragment> messageContents)
        {
            switch (corners)
            {
                case CornerData.Corners.ALL:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearAllRound));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornAllRound));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredAllRound));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONTS:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearFronts));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornFronts));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredFronts));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REARS:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearRears));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornRears));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredRears));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.LEFTS:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearLefts));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornLefts));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredLefts));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.RIGHTS:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearRights));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornRights));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredRights));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONT_LEFT:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearLeftFront));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornLeftFront));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredLeftFront));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONT_RIGHT:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearRightFront));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornRightFront));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredRightFront));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REAR_LEFT:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearLeftRear));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornLeftRear));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredLeftRear));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REAR_RIGHT:
                    switch (tyreCondition)
                    {
                        case TyreCondition.MINOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderMinorWearRightRear));
                            break;
                        case TyreCondition.MAJOR_WEAR:
                            messageContents.Add(MessageFragment.Text(folderWornRightRear));
                            break;
                        case TyreCondition.WORN_OUT:
                            messageContents.Add(MessageFragment.Text(folderKnackeredRightRear));
                            break;
                        default:
                            break;
                    }
                    break;
            }
        }

        private void addBrakeTempWarningMessages(CornerData.Corners corners, BrakeTemp brakeTemp, List<MessageFragment> messageContents, Boolean includeRears)
        {
            switch (corners)
            {
                case CornerData.Corners.ALL:
                    switch (brakeTemp)
                    {
                        case BrakeTemp.COLD:
                            if (!ignoreColdBrakesForClasses.Contains(CrewChief.currentGameState.carClass.carClassEnum))
                            {
                                messageContents.Add(MessageFragment.Text(folderColdBrakesAllRound));
                            }
                            break;
                        case BrakeTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotBrakesAllRound));
                            break;
                        case BrakeTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingBrakesAllRound));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.FRONTS:
                    switch (brakeTemp)
                    {
                        case BrakeTemp.COLD:
                            if (!ignoreColdBrakesForClasses.Contains(CrewChief.currentGameState.carClass.carClassEnum))
                            {
                                messageContents.Add(MessageFragment.Text(folderColdFrontBrakes));
                            }
                            break;
                        case BrakeTemp.HOT:
                            messageContents.Add(MessageFragment.Text(folderHotFrontBrakes));
                            break;
                        case BrakeTemp.COOKING:
                            messageContents.Add(MessageFragment.Text(folderCookingFrontBrakes));
                            break;
                        default:
                            break;
                    }
                    break;
                case CornerData.Corners.REARS:
                    if (includeRears)
                    {
                        switch (brakeTemp)
                        {
                            case BrakeTemp.COLD:
                                if (!ignoreColdBrakesForClasses.Contains(CrewChief.currentGameState.carClass.carClassEnum))
                                {
                                    messageContents.Add(MessageFragment.Text(folderColdRearBrakes));
                                }
                                break;
                            case BrakeTemp.HOT:
                                messageContents.Add(MessageFragment.Text(folderHotRearBrakes));
                                break;
                            case BrakeTemp.COOKING:
                                messageContents.Add(MessageFragment.Text(folderCookingRearBrakes));
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        private void addLockingAndSpinningData(TyreData tyreData, Boolean inPitLane, Boolean hasWheelMissingOrPuncture,
            long previousTicks, long currentTicks, Boolean isFWD)
        {
            if (hasWheelMissingOrPuncture || inPitLane)
            {
                return;
            }
            if (enableBrakeLockWarnings)
            {
                if (enableCornerSpecificLockingAndSpinningChecks)
                {
                    leftFrontLockedList.Add(tyreData.LeftFrontIsLocked);
                    rightFrontLockedList.Add(tyreData.RightFrontIsLocked);
                    leftRearLockedList.Add(tyreData.LeftRearIsLocked);
                    rightRearLockedList.Add(tyreData.RightRearIsLocked);
                }
                if (tyreData.LeftFrontIsLocked)
                {
                    timeLeftFrontIsLockedForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
                if (tyreData.RightFrontIsLocked)
                {
                    timeRightFrontIsLockedForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
                if (isFWD)
                {
                    if (tyreData.LeftRearIsLocked && tyreData.RightRearIsLocked)
                    {
                        timeBothRearsAreLockedForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                    }
                }
                else
                {
                    if (tyreData.LeftRearIsLocked)
                    {
                        timeLeftRearIsLockedForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                    }
                    if (tyreData.RightRearIsLocked)
                    {
                        timeRightRearIsLockedForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                    }
                }
            }
            if (enableWheelSpinWarnings)
            {
                if (tyreData.LeftFrontIsSpinning)
                {
                    timeLeftFrontIsSpinningForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
                if (tyreData.RightFrontIsSpinning)
                {
                    timeRightFrontIsSpinningForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
                if (tyreData.LeftRearIsSpinning)
                {
                    timeLeftRearIsSpinningForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
                if (tyreData.RightRearIsSpinning)
                {
                    timeRightRearIsSpinningForLap += (float)(currentTicks - previousTicks) / (float)TimeSpan.TicksPerSecond;
                }
            }
        }

        private Boolean hasWheelMissingOrPuncture(GameStateData currentState)
        {
            return !(currentState.TyreData.LeftFrontAttached && currentState.TyreData.RightFrontAttached &&
                    currentState.TyreData.LeftRearAttached && currentState.TyreData.RightRearAttached &&
                    DamageReporting.getPuncture(currentState.TyreData) == CornerData.Corners.NONE);
        }
        
        private CornerData.Corners getMostLockedWheelForLap()
        {
            var wheelMostLocked = CornerData.Corners.NONE;
            var timeMostLocked = 0.0f;

            if (this.timeLeftFrontIsLockedForLap > timeMostLocked)
            {
                timeMostLocked = this.timeLeftFrontIsLockedForLap;
                wheelMostLocked = CornerData.Corners.FRONT_LEFT;
            }

            if (this.timeRightFrontIsLockedForLap > timeMostLocked)
            {
                timeMostLocked = this.timeRightFrontIsLockedForLap;
                wheelMostLocked = CornerData.Corners.FRONT_RIGHT;
            }

            if (this.timeLeftRearIsLockedForLap > timeMostLocked)
            {
                timeMostLocked = this.timeLeftRearIsLockedForLap;
                wheelMostLocked = CornerData.Corners.REAR_LEFT;
            }

            if (this.timeRightRearIsLockedForLap > timeMostLocked)
                wheelMostLocked = CornerData.Corners.REAR_RIGHT;

            return wheelMostLocked;
        }

        private Boolean checkLocking(Boolean isFWD)
        {
            Boolean playedMessage = false;
            int messageDelay = Utilities.random.Next(0, 5);
            if (!warnedOnLockingForLap)
            {
                var mostLockedWheel = this.getMostLockedWheelForLap();
                if (mostLockedWheel == CornerData.Corners.FRONT_LEFT
                    && timeLeftFrontIsLockedForLap > totalLockupThresholdForNextLap)
                {
                    warnedOnLockingForLap = true;
                    if (timeRightFrontIsLockedForLap > totalLockupThresholdForNextLap / 2)
                    {
                        // lots of left front locking, some right front locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingFrontsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just left front locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingLeftFrontForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if (mostLockedWheel == CornerData.Corners.FRONT_RIGHT
                    && timeRightFrontIsLockedForLap > totalLockupThresholdForNextLap)
                {
                    warnedOnLockingForLap = true;
                    if (timeLeftFrontIsLockedForLap > totalLockupThresholdForNextLap / 2)
                    {
                        // lots of right front locking, some left front locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingFrontsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just right front locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingRightFrontForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if ((mostLockedWheel == CornerData.Corners.REAR_LEFT || mostLockedWheel == CornerData.Corners.REAR_RIGHT)
                    && isFWD
                    && timeBothRearsAreLockedForLap > totalLockupThresholdForNextLap)
                {
                    warnedOnLockingForLap = true;
                    audioPlayer.playMessage(new QueuedMessage(folderLockingRearsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                    playedMessage = true;
                }
                else if (mostLockedWheel == CornerData.Corners.REAR_LEFT
                    && !isFWD
                    && timeLeftRearIsLockedForLap > totalLockupThresholdForNextLap)
                {
                    warnedOnLockingForLap = true;
                    if (timeRightRearIsLockedForLap > totalLockupThresholdForNextLap / 2)
                    {
                        // lots of left rear locking, some right rear locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingRearsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just left rear locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingLeftRearForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if (mostLockedWheel == CornerData.Corners.REAR_RIGHT
                    && !isFWD
                    && timeRightRearIsLockedForLap > totalLockupThresholdForNextLap)
                {
                    warnedOnLockingForLap = true;
                    if (timeLeftRearIsLockedForLap > totalLockupThresholdForNextLap / 2)
                    {
                        // lots of right rear locking, some left rear locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingRearsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just right rear locking
                        audioPlayer.playMessage(new QueuedMessage(folderLockingRightRearForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
            }
            return playedMessage;
        }

        private CornerData.Corners getMostSpinningWheelForLap()
        {
            var wheelMostSpinning = CornerData.Corners.NONE;
            var timeMostSpinning = 0.0f;

            if (this.timeLeftFrontIsSpinningForLap > timeMostSpinning)
            {
                timeMostSpinning = this.timeLeftFrontIsSpinningForLap;
                wheelMostSpinning = CornerData.Corners.FRONT_LEFT;
            }

            if (this.timeRightFrontIsSpinningForLap > timeMostSpinning)
            {
                timeMostSpinning = this.timeRightFrontIsSpinningForLap;
                wheelMostSpinning = CornerData.Corners.FRONT_RIGHT;
            }

            if (this.timeLeftRearIsSpinningForLap > timeMostSpinning)
            {
                timeMostSpinning = this.timeLeftRearIsSpinningForLap;
                wheelMostSpinning = CornerData.Corners.REAR_LEFT;
            }

            if (this.timeRightRearIsSpinningForLap > timeMostSpinning)
                wheelMostSpinning = CornerData.Corners.REAR_RIGHT;

            return wheelMostSpinning;
        }

        private Boolean checkWheelSpinning(Boolean isFWD, Boolean isRWD)
        {
            Boolean playedMessage = false;
            int messageDelay = Utilities.random.Next(0, 5);
            if (!warnedOnWheelspinForLap)
            {
                var mostSpinningWheel = this.getMostSpinningWheelForLap();
                if (mostSpinningWheel == CornerData.Corners.FRONT_LEFT
                    && !isRWD
                    && timeLeftFrontIsSpinningForLap > totalWheelspinThresholdForNextLap)
                {
                    warnedOnWheelspinForLap = true;
                    if (timeRightFrontIsSpinningForLap > totalWheelspinThresholdForNextLap / 2)
                    {
                        // lots of left front spinning, some right front spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningFrontsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just left front spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningLeftFrontForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if (mostSpinningWheel == CornerData.Corners.FRONT_RIGHT
                    && !isRWD 
                    && timeRightFrontIsSpinningForLap > totalWheelspinThresholdForNextLap)
                {
                    warnedOnWheelspinForLap = true;
                    if (timeLeftFrontIsSpinningForLap > totalWheelspinThresholdForNextLap / 2)
                    {
                        // lots of right front spinning, some left front spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningFrontsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just right front spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningRightFrontForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if (mostSpinningWheel == CornerData.Corners.REAR_LEFT
                    && !isFWD 
                    && timeLeftRearIsSpinningForLap > totalWheelspinThresholdForNextLap)
                {
                    warnedOnWheelspinForLap = true;
                    if (timeRightRearIsSpinningForLap > totalWheelspinThresholdForNextLap / 2)
                    {
                        // lots of left rear spinning, some right rear spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningRearsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just left rear spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningLeftRearForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
                else if (mostSpinningWheel == CornerData.Corners.REAR_RIGHT
                    && !isFWD
                    && timeRightRearIsSpinningForLap > totalWheelspinThresholdForNextLap)
                {
                    warnedOnWheelspinForLap = true;
                    if (timeLeftRearIsSpinningForLap > totalWheelspinThresholdForNextLap / 2)
                    {
                        // lots of right rear spinning, some left rear spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningRearsForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                    else
                    {
                        // just right rear spinning
                        audioPlayer.playMessage(new QueuedMessage(folderSpinningRightRearForLapWarning, messageDelay + 6, secondsDelay: messageDelay, abstractEvent: this, priority: 0));
                        playedMessage = true;
                    }
                }
            }
            return playedMessage;
        }

        private WheelsLockedEnum getCornerEntryLocking(int minTicksLocked)
        {
            int minTicksPartiallyLocked = (int)Math.Ceiling((float)minTicksLocked / 2f);
            Boolean leftFrontLocking = false;
            Boolean rightFrontLocking = false;
            Boolean leftFrontPartiallyLocking = false;
            Boolean rightFrontPartiallyLocking = false;
            Boolean leftRearLocking = false;
            Boolean rightRearLocking = false;
            Boolean leftRearPartiallyLocking = false;
            Boolean rightRearPartiallyLocking = false;
            int lockCount = 0;
            for (int i = leftFrontLockedList.Count - 1; i >= ticksToCheckForLocking && i >= 0; i--)
            {
                if (leftFrontLockedList[i])
                {
                    lockCount++;
                }
            }
            leftFrontLocking = lockCount >= minTicksLocked;
            leftFrontPartiallyLocking = lockCount >= minTicksPartiallyLocked;
            lockCount = 0;
            for (int i = rightFrontLockedList.Count - 1; i >= ticksToCheckForLocking && i >= 0; i--)
            {
                if (rightFrontLockedList[i])
                {
                    lockCount++;
                }
            }
            rightFrontLocking = lockCount >= minTicksLocked;
            rightFrontPartiallyLocking = lockCount >= minTicksPartiallyLocked;
            lockCount = 0;
            for (int i = leftRearLockedList.Count - 1; i >= ticksToCheckForLocking && i >= 0; i--)
            {
                if (leftRearLockedList[i])
                {
                    lockCount++;
                }
            }
            leftRearLocking = lockCount >= minTicksLocked;
            leftRearPartiallyLocking = lockCount >= minTicksPartiallyLocked;
            lockCount = 0;
            for (int i = rightRearLockedList.Count - 1; i >= ticksToCheckForLocking && i >= 0; i--)
            {
                if (rightRearLockedList[i])
                {
                    lockCount++;
                }
            }
            rightRearLocking = lockCount >= minTicksLocked;
            rightRearPartiallyLocking = lockCount >= minTicksPartiallyLocked;
            if (leftFrontLocking && rightFrontLocking)
            {
                return WheelsLockedEnum.FRONTS;
            }
            if (leftFrontLocking)
            {
                return WheelsLockedEnum.LEFT_FRONT;
            }
            if (rightFrontLocking)
            {
                return WheelsLockedEnum.RIGHT_FRONT;
            }
            // for rears we need to be a bit careful - if one is above the threshold and the other is above half the threshold report it
            if ((leftRearLocking && rightRearPartiallyLocking) || (rightRearLocking && leftRearPartiallyLocking))
            {
                return WheelsLockedEnum.REARS;
            }
            return WheelsLockedEnum.NONE;
        }

        private List<TyrePerformanceContainer> getRelativeTyrePerformance()
        {
            List<TyrePerformanceContainer> performanceData = new List<TyrePerformanceContainer>();
            // don't bother unless we have 2 or more tyre types in the data:
            if (this.playerClassSessionBestLapTimeByTyre != null && this.playerClassSessionBestLapTimeByTyre.Count > 1)
            {
                // get a TyrePeformanceContainer for each pair of tyres we have data for:
                foreach (TyreType tyreType1 in Enum.GetValues(typeof(TyreType)))
                {
                    float type1BestLap = -1.0f;
                    if (this.playerClassSessionBestLapTimeByTyre.TryGetValue(tyreType1, out type1BestLap))
                    {
                        // now get all the other tyre types best lap to compare it to
                        foreach (TyreType tyreType2 in Enum.GetValues(typeof(TyreType)))
                        {
                            float type2BestLap = -1.0f;
                            if (tyreType1 != tyreType2 && this.playerClassSessionBestLapTimeByTyre.TryGetValue(tyreType2, out type2BestLap) &&
                                !comparisonAlreadyPresent(performanceData, tyreType1, tyreType2))
                            {
                                if (type1BestLap > type2BestLap)
                                {
                                    performanceData.Add(new TyrePerformanceContainer(tyreType2, tyreType1, type1BestLap - type2BestLap));
                                }
                                else if (type1BestLap < type2BestLap)
                                {
                                    performanceData.Add(new TyrePerformanceContainer(tyreType1, tyreType2, type2BestLap - type1BestLap));
                                }
                                break;
                            }
                        }
                    }
                }
            }
            return performanceData;
        }

        private Boolean comparisonAlreadyPresent(List<TyrePerformanceContainer> performanceData, TyreType tyreType1, TyreType tyreType2)
        {
            foreach (TyrePerformanceContainer container in performanceData)
            {
                if ((container.type1 == tyreType1 && container.type2 == tyreType2) ||
                    (container.type2 == tyreType1 && container.type1 == tyreType2))
                {
                    return true;
                }
            }
            return false;
        }

        private enum WheelsLockedEnum
        {
            // note that we don't have separate values for left and right rear locking because it's common for FWD cars to cock 
            // a wheel on turn-in so we only care of both rears are locking
            NONE, FRONTS, LEFT_FRONT, RIGHT_FRONT, REARS
        }

        private string[] aggregatePressureMessages(string frontLeft, string frontRight, string rearLeft, string rearRight)
        {
            List<string> aggregated = new List<string>();
            if (frontLeft != null && frontRight != null)
            {
                if (frontLeft == folderLeftFrontPressureVeryHigh && frontRight == folderRightFrontPressureVeryHigh)
                {
                    aggregated.Add(folderFrontPressuresVeryHigh);
                }
                else if (frontLeft == folderLeftFrontPressureHigh && frontRight == folderRightFrontPressureHigh)
                {
                    aggregated.Add(folderFrontPressuresHigh);
                }
                else if (frontLeft == folderLeftFrontPressureOK && frontRight == folderRightFrontPressureOK)
                {
                    aggregated.Add(folderFrontPressuresOK);
                }
                else if (frontLeft == folderLeftFrontPressureLow && frontRight == folderRightFrontPressureLow)
                {
                    aggregated.Add(folderFrontPressuresLow);
                }
                else if (frontLeft == folderLeftFrontPressureVeryLow && frontRight == folderRightFrontPressureVeryLow)
                {
                    aggregated.Add(folderFrontPressuresVeryLow);
                }
                else
                {
                    aggregated.Add(frontLeft);
                    aggregated.Add(frontRight);
                }
            }

            if (rearLeft != null && rearRight != null)
            {
                if (rearLeft == folderLeftRearPressureVeryHigh && rearRight == folderRightRearPressureVeryHigh)
                {
                    aggregated.Add(folderRearPressuresVeryHigh);
                }
                else if (rearLeft == folderLeftRearPressureHigh && rearRight == folderRightRearPressureHigh)
                {
                    aggregated.Add(folderRearPressuresHigh);
                }
                else if (rearLeft == folderLeftRearPressureOK && rearRight == folderRightRearPressureOK)
                {
                    aggregated.Add(folderRearPressuresOK);
                }
                else if (rearLeft == folderLeftRearPressureLow && rearRight == folderRightRearPressureLow)
                {
                    aggregated.Add(folderRearPressuresLow);
                }
                else if (rearLeft == folderLeftRearPressureVeryLow && rearRight == folderRightRearPressureVeryLow)
                {
                    aggregated.Add(folderRearPressuresVeryLow);
                }
                else
                {
                    aggregated.Add(rearLeft);
                    aggregated.Add(rearRight);
                }
            }
            return aggregated.ToArray();
        }
    }
    class TyrePerformanceContainer
    {
        public TyreType type1;
        public TyreType type2;
        public float bestLapDelta;
        public TyrePerformanceContainer(TyreType type1, TyreType type2, float bestLapDelta)
        {
            this.type1 = type1;
            this.type2 = type2;
            this.bestLapDelta = bestLapDelta;
        }
    }
}
