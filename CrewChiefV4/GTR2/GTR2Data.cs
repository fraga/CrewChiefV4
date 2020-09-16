﻿/*
GTR2 internal state mapping structures.  Allows access to native C++ structs from C#.
Must be kept in sync with Include\GTR2State.h.

See: MainForm.MainUpdate for sample on how to marshall from native in memory struct.

Author: The Iron Wolf (vleonavicius@hotmail.com)
Website: thecrewchief.org
*/
using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

// CC specific: Mark more common unused members with JsonIgnore for reduced trace sizes.

namespace GTR2SharedMemory
{
    // Marshalled types:
    // C++                 C#
    // char          ->    byte
    // unsigned char ->    byte
    // signed char   ->    sbyte
    // bool          ->    byte
    // long          ->    int
    // unsigned long ->    uint
    // short         ->    short
    // unsigned short ->   ushort
    // ULONGLONG     ->    Int64
    public class GTR2Constants
    {
        public const string MM_TELEMETRY_FILE_NAME = "$GTR2CrewChief_Telemetry$";
        public const string MM_SCORING_FILE_NAME = "$GTR2CrewChief_Scoring$";
        public const string MM_PITINFO_FILE_NAME = "$GTR2CrewChief_PitInfo$";
        public const string MM_EXTENDED_FILE_NAME = "$GTR2CrewChief_Extended$";

        public const string MM_HWCONTROL_FILE_NAME = "$GTR2CrewChief_HWControl$";
        public const int MM_HWCONTROL_LAYOUT_VERSION = 1;

        public const int MM_WEATHER_CONTROL_LAYOUT_VERSION = 1;

        public const int MAX_MAPPED_VEHICLES = 104;
        public const int MAX_MAPPED_IDS = 512;
        public const int MAX_STATUS_MSG_LEN = 128;
        public const int MAX_RULES_INSTRUCTION_MSG_LEN = 96;
        public const int MAX_HWCONTROL_NAME_LEN = 96;
        public const string GTR2_PROCESS_NAME = "gtr2";

        public const byte RowX = 0;
        public const byte RowY = 1;
        public const byte RowZ = 2;

        // 0 Before session has begun
        // 1 Reconnaissance laps (race only)
        // 2 Grid walk-through (race only)
        // 3 Formation lap (race only)
        // 4 Starting-light countdown has begun (race only)
        // 5 Green flag
        // 6 Full course yellow / safety car
        // 7 Session stopped
        // 8 Session over
        public enum GTR2GamePhase
        {
            Garage = 0,
  WarmUp = 1,
  GridWalk = 2,
  Formation = 3,
  Countdown = 4,
  GreenFlag = 5,
  FullCourseYellow = 6,
  SessionStopped = 7,
  SessionOver = 8
}

        // Yellow flag states (applies to full-course only)
        // -1 Invalid
        //  0 None
        //  1 Pending
        //  2 Pits closed
        //  3 Pit lead lap
        //  4 Pits open
        //  5 Last lap
        //  6 Resume
        //  7 Race halt (not currently used)
        public enum GTR2YellowFlagState
        {
            Invalid = -1,
  NoFlag = 0,
  Pending = 1,
  PitClosed = 2,
  PitLeadLap = 3,
  PitOpen = 4,
  LastLap = 5,
  Resume = 6,
  RaceHalt = 7
}

        public enum GTR2SurfaceType
        {
            Dry = 0,
  Wet = 1,
  Grass = 2,
  Dirt = 3,
  Gravel = 4,
  Kerb = 5
}

        // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
        public enum GTR2Sector
        {
            Sector3 = 0,
  Sector1 = 1,
  Sector2 = 2
}

        // 0=none, 1=finished, 2=dnf, 3=dq
        public enum GTR2FinishStatus
        {
            None = 0,
  Finished = 1,
  Dnf = 2,
  Dq = 3
}

        // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote
        public enum GTR2Control
        {
            Nobody = -1,
  Player = 0,
  AI = 1,
  Remote = 2
}

        // wheel info (front left, front right, rear left, rear right)
        public enum GTR2WheelIndex
        {
            FrontLeft = 0,
  FrontRight = 1,
  RearLeft = 2,
  RearRight = 3
}

        // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
        public enum GTR2PitState
        {
            None = 0,
  Request = 1,
  Entering = 2,
  Stopped = 3,
  Exiting = 4
}

    }

    namespace GTR2Data
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GTR2Vec3
        {
            public double x, y, z;
        }

        /////////////////////////////////////
        // Based on TelemWheelV2
        ////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Wheel
        {
            [JsonIgnore] public float mRotation;             // radians/sec
            [JsonIgnore] public float mSuspensionDeflection; // meters
            [JsonIgnore] public float mRideHeight;           // meters
            [JsonIgnore] public float mTireLoad;             // Newtons
            [JsonIgnore] public float mLateralForce;         // Newtons
            [JsonIgnore] public float mGripFract;            // an approximation of what fraction of the contact patch is sliding
            public float mBrakeTemp;            // Celsius
            public float mPressure;             // kPa
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] mTemperature;       // Celsius, left/center/right (not to be confused with inside/center/outside!)

            public float mWear;           // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            [JsonIgnore] public byte[] mTerrainName;                                  // the material prefixes from the TDF file
            public byte mSurfaceType; // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip
            public byte mFlat;                 // whether tire is flat
            public byte mDetached;             // whether wheel is detached

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            [JsonIgnore] byte[] mExpansion;                                           // for future use
        };

        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TelemInfoV2, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        struct GTR2VehicleTelemetry
        {
            // Time
            float mDeltaTime;      // time since last update (seconds)
            long mLapNumber;       // current lap number
            float mLapStartET;     // time this lap was started
            char mVehicleName[64]; // current vehicle name
            char mTrackName[64];   // current track name

            // Position and derivatives
            GTR2Vec3 mPos;        // world position in meters
            GTR2Vec3 mLocalVel;   // velocity (meters/sec) in local vehicle coordinates
            GTR2Vec3 mLocalAccel; // acceleration (meters/sec^2) in local vehicle coordinates

            // Orientation and derivatives
            GTR2Vec3 mOriX; // top row of orientation matrix (also converts local vehicle vectors into world X using dot product)
            GTR2Vec3 mOriY; // mid row of orientation matrix (also converts local vehicle vectors into world Y using dot product)
            GTR2Vec3 mOriZ; // bot row of orientation matrix (also converts local vehicle vectors into world Z using dot product)
            GTR2Vec3 mLocalRot;      // rotation (radians/sec) in local vehicle coordinates
            GTR2Vec3 mLocalRotAccel; // rotational acceleration (radians/sec^2) in local vehicle coordinates

            // Vehicle status
            long mGear;             // -1=reverse, 0=neutral, 1+=forward gears
            float mEngineRPM;       // engine RPM
            float mEngineWaterTemp; // Celsius
            float mEngineOilTemp;   // Celsius
            float mClutchRPM;       // clutch RPM

            // Driver input
            float mUnfilteredThrottle; // ranges  0.0-1.0
            float mUnfilteredBrake;    // ranges  0.0-1.0
            float mUnfilteredSteering; // ranges -1.0-1.0 (left to right)
            float mUnfilteredClutch;   // ranges  0.0-1.0

            // Misc
            float mSteeringArmForce; // force on steering arms

            // state/damage info
            float mFuel;                    // amount of fuel (liters)
            float mEngineMaxRPM;            // rev limit
            unsigned char mScheduledStops;  // number of scheduled pitstops
            bool mOverheating;              // whether overheating icon is shown
            bool mDetached;                 // whether any parts (besides wheels) have been detached
            unsigned char mDentSeverity[8]; // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
            float mLastImpactET;            // time of last impact
            float mLastImpactMagnitude;     // magnitude of last impact
            GTR2Vec3 mLastImpactPos;        // location of last impact

            // Future use
            unsigned char mExpansion[64];

            // keeping this at the end of the structure to make it easier to replace in future versions
            GTR2Wheel mWheel[4]; // wheel info (front left, front right, rear left, rear right)
        };

        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to ScoringInfoV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        struct GTR2ScoringInfo
        {
            char mTrackName[64]; // current track name
            long mSession;       // current session
            float mCurrentET;    // current time
            float mEndET;        // ending time
            long mMaxLaps;       // maximum laps
            float mLapDist;      // distance around track

            // MM_NOT_USED
            // char *mResultsStream;          // results stream additions since last update (newline-delimited and
            // NULL-terminated)
            // MM_NEW
            unsigned char pointer1[4];

            long mNumVehicles; // current number of vehicles
                               // Game phases:
                               // 0 Before session has begun
                               // 1 Reconnaissance laps (race only)
                               // 2 Grid walk-through (race only)
                               // 3 Formation lap (race only)
                               // 4 Starting-light countdown has begun (race only)
                               // 5 Green flag
                               // 6 Full course yellow / safety car
                               // 7 Session stopped
                               // 8 Session over
            unsigned char mGamePhase;

            // Yellow flag states (applies to full-course only)
            // -1 Invalid
            //  0 None
            //  1 Pending
            //  2 Pits closed
            //  3 Pit lead lap
            //  4 Pits open
            //  5 Last lap
            //  6 Resume
            //  7 Race halt (not currently used)
            signed char mYellowFlagState;

            signed char mSectorFlag[3];  // whether there are any local yellows at the moment in each sector (not sure if sector 0
                                         // is first or last, so test)
            unsigned char mStartLight;   // start light frame (number depends on track)
            unsigned char mNumRedLights; // number of red lights in start sequence
            bool mInRealtime;            // in realtime as opposed to at the monitor
            char mPlayerName[32];        // player name (including possible multiplayer override)
            char mPlrFileName[64];       // may be encoded to be a legal filename

            // weather
            float mDarkCloud;      // cloud darkness? 0.0-1.0
            float mRaining;        // raining severity 0.0-1.0
            float mAmbientTemp;    // temperature (Celsius)
            float mTrackTemp;      // temperature (Celsius)
            GTR2Vec3 mWind;        // wind speed
            float mOnPathWetness;  // on main path 0.0-1.0
            float mOffPathWetness; // on main path 0.0-1.0

            // Future use
            unsigned char mExpansion[256];

            // MM_NOT_USED
            // VehicleScoringInfoV2 *mVehicle;  // array of vehicle scoring info's
            // MM_NEW
            unsigned char pointer2[4];
        };
        static_assert(sizeof(GTR2ScoringInfo) == sizeof(ScoringInfoV2),
                      "GTR2ScoringInfo and ScoringInfoV2 structures are out of sync");

        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to VehicleScoringInfoV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        struct GTR2VehicleScoring
        {
            char mDriverName[32];      // driver name
            char mVehicleName[64];     // vehicle name
            short mTotalLaps;          // laps completed
            signed char mSector;       // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
            signed char mFinishStatus; // 0=none, 1=finished, 2=dnf, 3=dq
            float mLapDist;            // current distance around track
            float mPathLateral;        // lateral position with respect to *very approximate* "center" path
            float mTrackEdge;          // track edge (w.r.t. "center" path) on same side of track as vehicle

            float mBestSector1; // best sector 1
            float mBestSector2; // best sector 2 (plus sector 1)
            float mBestLapTime; // best lap time
            float mLastSector1; // last sector 1
            float mLastSector2; // last sector 2 (plus sector 1)
            float mLastLapTime; // last lap time
            float mCurSector1;  // current sector 1 if valid
            float mCurSector2;  // current sector 2 (plus sector 1) if valid
                                // no current laptime because it instantly becomes "last"

            short mNumPitstops;     // number of pitstops made
            short mNumPenalties;    // number of outstanding penalties
            bool mIsPlayer;         // is this the player's vehicle
            signed char mControl;   // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote,
                                    // 3=replay (shouldn't get this)
            bool mInPits;           // between pit entrance and pit exit (not always accurate for remote vehicles)
            unsigned char mPlace;   // 1-based position
            char mVehicleClass[32]; // vehicle class

            // Dash Indicators
            float mTimeBehindNext;   // time behind vehicle in next higher place
            long mLapsBehindNext;    // laps behind vehicle in next higher place
            float mTimeBehindLeader; // time behind leader
            long mLapsBehindLeader;  // laps behind leader
            float mLapStartET;       // time this lap was started

            // Position and derivatives
            GTR2Vec3 mPos;        // world position in meters
            GTR2Vec3 mLocalVel;   // velocity (meters/sec) in local vehicle coordinates
            GTR2Vec3 mLocalAccel; // acceleration (meters/sec^2) in local vehicle coordinates

            // Orientation and derivatives
            GTR2Vec3 mOriX; // top row of orientation matrix (also converts local vehicle vectors into world X using dot product)
            GTR2Vec3 mOriY; // mid row of orientation matrix (also converts local vehicle vectors into world Y using dot product)
            GTR2Vec3 mOriZ; // bot row of orientation matrix (also converts local vehicle vectors into world Z using dot product)
            GTR2Vec3 mLocalRot;      // rotation (radians/sec) in local vehicle coordinates
            GTR2Vec3 mLocalRotAccel; // rotational acceleration (radians/sec^2) in local vehicle coordinates

            // Future use
            unsigned char mExpansion[128];
        };
        static_assert(sizeof(GTR2VehicleScoring) == sizeof(VehicleScoringInfoV2),
                      "GTR2VehicleScoring and VehicleScoringInfoV01 structures are out of sync");

        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to GraphicsInfoV02, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        struct GTR2GraphicsInfo
        {
            GTR2Vec3 mCamPos; // camera position
            GTR2Vec3 mCamOri; // camera orientation
            HWND mHWND;       // app handle

            float mAmbientRed;
            float mAmbientGreen;
            float mAmbientBlue;
        };
        static_assert(sizeof(GTR2GraphicsInfo) == sizeof(GraphicsInfoV2),
                      "GTR2GraphicsInfo and GraphicsInfoV2 structures are out of sync");

        ///////////////////////////////////////////
        // Mapped wrapper structures
        ///////////////////////////////////////////

        struct GTR2MappedBufferVersionBlock
        {
            unsigned long mVersionUpdateBegin; // Incremented right before buffer is written to.
            unsigned long mVersionUpdateEnd;   // Incremented after buffer write is done.
        };

        struct GTR2MappedBufferHeader
        {
            static int const MAX_MAPPED_VEHICLES = 104;
        };

        struct GTR2MappedBufferHeaderWithSize : public GTR2MappedBufferHeader
{
  int mBytesUpdatedHint; // How many bytes of the structure were written during the last update.
                         // 0 means unknown (whole buffer should be considered as updated).
    };

    struct GTR2Telemetry : public GTR2MappedBufferHeader
{
  GTR2VehicleTelemetry mPlayerTelemetry;
};

struct GTR2Scoring : public GTR2MappedBufferHeaderWithSize
{
  GTR2ScoringInfo mScoringInfo;
GTR2VehicleScoring mVehicles[GTR2MappedBufferHeader::MAX_MAPPED_VEHICLES];
};

// Note: not versioned due to high referesh rate and no need for consistent buffer view.
struct GTR2ForceFeedback : public GTR2MappedBufferHeader
{
  double mForceValue; // Current FFB value reported via InternalsPlugin::ForceFeedback.
};

// Note: not versioned due to high referesh rate and no need for consistent buffer view.
struct GTR2Graphics : public GTR2MappedBufferHeader
{
  GTR2GraphicsInfo mGraphicsInfo;
};

struct GTR2TrackedDamage
{
    double mMaxImpactMagnitude; // Max impact magnitude.  Tracked on every telemetry update, and reset on visit to pits or
                                // Session restart.
    double mAccumulatedImpactMagnitude; // Accumulated impact magnitude.  Tracked on every telemetry update, and reset on
                                        // visit to pits or Session restart.
};

struct GTR2VehScoringCapture
{
    // VehicleScoringInfoV01 members:
    unsigned char mPlace;
    bool mIsPlayer;
    signed char mFinishStatus; // 0=none, 1=finished, 2=dnf, 3=dq
};

struct GTR2SessionTransitionCapture
{
    // ScoringInfoV01 members:
    unsigned char mGamePhase;
    long mSession;

    // VehicleScoringInfoV01 members:
    long mNumScoringVehicles;
    GTR2VehScoringCapture mScoringVehicles[GTR2MappedBufferHeader::MAX_MAPPED_VEHICLES];
};

struct GTR2Extended : public GTR2MappedBufferHeader
{
  static int const MAX_MAPPED_IDS = 512;
static int const MAX_STATUS_MSG_LEN = 128;
static int const MAX_RULES_INSTRUCTION_MSG_LEN = 96;

char mVersion[12]; // API version

// Damage tracking for each vehicle (indexed by mID % GTR2Extended::MAX_MAPPED_IDS):
GTR2TrackedDamage mTrackedDamages[GTR2Extended::MAX_MAPPED_IDS];

// Function call based flags:
bool mInRealtimeFC; // in realtime as opposed to at the monitor (reported via last EnterRealtime/ExitRealtime calls).
bool mMultimediaThreadStarted; // multimedia thread started (reported via ThreadStarted/ThreadStopped calls).
bool mSimulationThreadStarted; // simulation thread started (reported via ThreadStarted/ThreadStopped calls).

bool mSessionStarted;           // True if Session Started was called.
ULONGLONG mTicksSessionStarted; // Ticks when session started.
ULONGLONG mTicksSessionEnded;   // Ticks when session ended.

// FUTURE: It might be worth to keep the whole scoring capture as a separate double buffer instead of this.
GTR2SessionTransitionCapture
  mSessionTransitionCapture; // Contains partial internals capture at session transition time.

// Direct Memory access stuff
bool mDirectMemoryAccessEnabled;

ULONGLONG mTicksStatusMessageUpdated; // Ticks when status message was updated;
char mStatusMessage[GTR2Extended::MAX_STATUS_MSG_LEN];

ULONGLONG mTicksLastHistoryMessageUpdated; // Ticks when last message history message was updated;
char mLastHistoryMessage[GTR2Extended::MAX_STATUS_MSG_LEN];

float mCurrentPitSpeedLimit; // speed limit m/s.

ULONGLONG mTicksLSIPhaseMessageUpdated; // Ticks when last LSI phase message was updated.
char mLSIPhaseMessage[GTR2Extended::MAX_RULES_INSTRUCTION_MSG_LEN];

ULONGLONG mTicksLSIPitStateMessageUpdated; // Ticks when last LSI pit state message was updated.
char mLSIPitStateMessage[GTR2Extended::MAX_RULES_INSTRUCTION_MSG_LEN];

ULONGLONG mTicksLSIOrderInstructionMessageUpdated; // Ticks when last LSI order instruction message was updated.
char mLSIOrderInstructionMessage[GTR2Extended::MAX_RULES_INSTRUCTION_MSG_LEN];

ULONGLONG mTicksLSIRulesInstructionMessageUpdated; // Ticks when last FCY rules message was updated.  Currently, only
                                                   // SCR plugin sets that.
char mLSIRulesInstructionMessage[GTR2Extended::MAX_RULES_INSTRUCTION_MSG_LEN];

long mUnsubscribedBuffersMask; // Currently active UnsbscribedBuffersMask value.  This will be allowed for clients to
                               // write to in the future, but not yet.

bool mHWControlInputEnabled;      // HWControl input buffer is enabled.
bool mPluginControlInputEnabled;  // Plugin Control input buffer is enabled.
};

struct GTR2MappedInputBufferHeader : public GTR2MappedBufferHeader
{
  long mLayoutVersion;
};

struct GTR2HWControl : public GTR2MappedInputBufferHeader
{
  static int const MAX_HWCONTROL_NAME_LEN = 96;

// Version supported by the _current_ plugin.
static long const SUPPORTED_LAYOUT_VERSION = 1L;

char mControlName[GTR2HWControl::MAX_HWCONTROL_NAME_LEN];
float mfRetVal;
};

struct GTR2PluginControl : public GTR2MappedInputBufferHeader
{
  // Version supported by the _current_ plugin.
  static long const SUPPORTED_LAYOUT_VERSION = 1L;

// Note: turning Scoring update on cannot be requested
long mRequestEnableBuffersMask;
bool mRequestHWControlInput;
};

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Wheel
        {
            [JsonIgnore] public double mSuspensionDeflection;                         // meters
            [JsonIgnore] public double mRideHeight;                                   // meters
            [JsonIgnore] public double mSuspForce;                                    // pushrod load in Newtons
            public double mBrakeTemp;                                    // Celsius
            [JsonIgnore] public double mBrakePressure;                                // currently 0.0-1.0, depending on driver input and brake balance; will convert to true brake pressure (kPa) in future

            public double mRotation;                                     // radians/sec
            [JsonIgnore] public double mLateralPatchVel;                              // lateral velocity at contact patch
            [JsonIgnore] public double mLongitudinalPatchVel;                         // longitudinal velocity at contact patch
            [JsonIgnore] public double mLateralGroundVel;                             // lateral velocity at contact patch
            [JsonIgnore] public double mLongitudinalGroundVel;                        // longitudinal velocity at contact patch
            [JsonIgnore] public double mCamber;                                       // radians (positive is left for left-side wheels, right for right-side wheels)
            [JsonIgnore] public double mLateralForce;                                 // Newtons
            [JsonIgnore] public double mLongitudinalForce;                            // Newtons
            [JsonIgnore] public double mTireLoad;                                     // Newtons

            [JsonIgnore] public double mGripFract;                                    // an approximation of what fraction of the contact patch is sliding
            public double mPressure;                                     // kPa (tire pressure)
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public double[] mTemperature;                                // Kelvin (subtract 273.15 to get Celsius), left/center/right (not to be confused with inside/center/outside!)
            public double mWear;                                         // wear (0.0-1.0, fraction of maximum) ... this is not necessarily proportional with grip loss
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            [JsonIgnore] public byte[] mTerrainName;                                  // the material prefixes from the TDF file
            public byte mSurfaceType;                                    // 0=dry, 1=wet, 2=grass, 3=dirt, 4=gravel, 5=rumblestrip, 6=special
            public byte mFlat;                                           // whether tire is flat
            public byte mDetached;                                       // whether wheel is detached
            public byte mStaticUndeflectedRadius;                        // tire radius in centimeters

            [JsonIgnore] public double mVerticalTireDeflection;                       // how much is tire deflected from its (speed-sensitive) radius
            [JsonIgnore] public double mWheelYLocation;                               // wheel's y location relative to vehicle y location
            [JsonIgnore] public double mToe;                                          // current toe angle w.r.t. the vehicle

            [JsonIgnore] public double mTireCarcassTemperature;                       // rough average of temperature samples from carcass (Kelvin)
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            [JsonIgnore] public double[] mTireInnerLayerTemperature;                  // rough average of temperature samples from innermost layer of rubber (before carcass) (Kelvin)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]
            [JsonIgnore] byte[] mExpansion;                                           // for future use
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2VehicleTelemetry
        {
            // Time
            public int mID;                                              // slot ID (note that it can be re-used in multiplayer after someone leaves)
            [JsonIgnore] public double mDeltaTime;                                    // time since last update (seconds)
            public double mElapsedTime;                                  // game session time
            [JsonIgnore] public int mLapNumber;                                       // current lap number
            [JsonIgnore] public double mLapStartET;                                   // time this lap was started
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            [JsonIgnore] public byte[] mVehicleName;                                  // current vehicle name
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            [JsonIgnore] public byte[] mTrackName;                                    // current track name

            // Position and derivatives
            public GTR2Vec3 mPos;                                         // world position in meters
            public GTR2Vec3 mLocalVel;                                    // velocity (meters/sec) in local vehicle coordinates
            [JsonIgnore] public GTR2Vec3 mLocalAccel;                                  // acceleration (meters/sec^2) in local vehicle coordinates

            // Orientation and derivatives
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public GTR2Vec3[] mOri;                                       // rows of orientation matrix (use TelemQuat conversions if desired), also converts local
                                                                         // vehicle vectors into world X, Y, or Z using dot product of rows 0, 1, or 2 respectively
            [JsonIgnore] public GTR2Vec3 mLocalRot;                                    // rotation (radians/sec) in local vehicle coordinates
            [JsonIgnore] public GTR2Vec3 mLocalRotAccel;                               // rotational acceleration (radians/sec^2) in local vehicle coordinates

            // Vehicle status
            public int mGear;                                            // -1=reverse, 0=neutral, 1+=forward gears
            public double mEngineRPM;                                    // engine RPM
            public double mEngineWaterTemp;                              // Celsius
            public double mEngineOilTemp;                                // Celsius
            [JsonIgnore] public double mClutchRPM;                                    // clutch RPM

            // Driver input
            public double mUnfilteredThrottle;                           // ranges  0.0-1.0
            public double mUnfilteredBrake;                              // ranges  0.0-1.0
            [JsonIgnore] public double mUnfilteredSteering;                           // ranges -1.0-1.0 (left to right)
            public double mUnfilteredClutch;                             // ranges  0.0-1.0

            // Filtered input (various adjustments for rev or speed limiting, TC, ABS?, speed sensitive steering, clutch work for semi-automatic shifting, etc.)
            [JsonIgnore] public double mFilteredThrottle;                             // ranges  0.0-1.0
            [JsonIgnore] public double mFilteredBrake;                                // ranges  0.0-1.0
            [JsonIgnore] public double mFilteredSteering;                             // ranges -1.0-1.0 (left to right)
            [JsonIgnore] public double mFilteredClutch;                               // ranges  0.0-1.0

            // Misc
            [JsonIgnore] public double mSteeringShaftTorque;                          // torque around steering shaft (used to be mSteeringArmForce, but that is not necessarily accurate for feedback purposes)
            [JsonIgnore] public double mFront3rdDeflection;                           // deflection at front 3rd spring
            [JsonIgnore] public double mRear3rdDeflection;                            // deflection at rear 3rd spring

            // Aerodynamics
            [JsonIgnore] public double mFrontWingHeight;                              // front wing height
            [JsonIgnore] public double mFrontRideHeight;                              // front ride height
            [JsonIgnore] public double mRearRideHeight;                               // rear ride height
            [JsonIgnore] public double mDrag;                                         // drag
            [JsonIgnore] public double mFrontDownforce;                               // front downforce
            [JsonIgnore] public double mRearDownforce;                                // rear downforce

            // State/damage info
            public double mFuel;                                         // amount of fuel (liters)
            public double mEngineMaxRPM;                                 // rev limit
            public byte mScheduledStops;                                 // number of scheduled pitstops
            public byte mOverheating;                                    // whether overheating icon is shown
            public byte mDetached;                                       // whether any parts (besides wheels) have been detached
            [JsonIgnore] public byte mHeadlights;                                     // whether headlights are on
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] mDentSeverity;                                 // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
            public double mLastImpactET;                                 // time of last impact
            [JsonIgnore] public double mLastImpactMagnitude;                          // magnitude of last impact
            [JsonIgnore] public GTR2Vec3 mLastImpactPos;                               // location of last impact

            // Expanded
            [JsonIgnore] public double mEngineTorque;                                 // current engine torque (including additive torque) (used to be mEngineTq, but there's little reason to abbreviate it)
            [JsonIgnore] public int mCurrentSector;                                   // the current sector (zero-based) with the pitlane stored in the sign bit (example: entering pits from third sector gives 0x80000002)
            public byte mSpeedLimiter;                                   // whether speed limiter is on
            [JsonIgnore] public byte mMaxGears;                                       // maximum forward gears
            public byte mFrontTireCompoundIndex;                         // index within brand
            [JsonIgnore] public byte mRearTireCompoundIndex;                          // index within brand
            [JsonIgnore] public double mFuelCapacity;                                 // capacity in liters
            [JsonIgnore] public byte mFrontFlapActivated;                             // whether front flap is activated
            public byte mRearFlapActivated;                              // whether rear flap is activated
            public byte mRearFlapLegalStatus;                            // 0=disallowed, 1=criteria detected but not allowed quite yet, 2=allowed
            [JsonIgnore] public byte mIgnitionStarter;                                // 0=off 1=ignition 2=ignition+starter

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] mFrontTireCompoundName;                        // name of front tire compound
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 18)]
            [JsonIgnore] public byte[] mRearTireCompoundName;                         // name of rear tire compound

            public byte mSpeedLimiterAvailable;                          // whether speed limiter is available
            [JsonIgnore] public byte mAntiStallActivated;                             // whether (hard) anti-stall is activated
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            [JsonIgnore] public byte[] mUnused;                                       //
            [JsonIgnore] public float mVisualSteeringWheelRange;                      // the *visual* steering wheel range

            [JsonIgnore] public double mRearBrakeBias;                                // fraction of brakes on rear
            [JsonIgnore] public double mTurboBoostPressure;                           // current turbo boost pressure if available
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            [JsonIgnore] public float[] mPhysicsToGraphicsOffset;                     // offset from static CG to graphical center
            [JsonIgnore] public float mPhysicalSteeringWheelRange;                    // the *physical* steering wheel range

            // Future use
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 152)]
            [JsonIgnore] public byte[] mExpansion;                                    // for future use (note that the slot ID has been moved to mID above)

            // keeping this at the end of the structure to make it easier to replace in future versions
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4)]
            public GTR2Wheel[] mWheels;                                   // wheel info (front left, front right, rear left, rear right)
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2ScoringInfo
        {
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] mTrackName;                                    // current track name
            public int mSession;                                         // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
            public double mCurrentET;                                    // current time
            public double mEndET;                                        // ending time
            public int mMaxLaps;                                         // maximum laps
            public double mLapDist;                                      // distance around track
                                                                         // MM_NOT_USED
                                                                         //char *mResultsStream;                                                   // results stream additions since last update (newline-delimited and NULL-terminated)
                                                                         // MM_NEW
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] pointer1;

            public int mNumVehicles;                                     // current number of vehicles

            // Game phases:
            // 0 Before session has begun
            // 1 Reconnaissance laps (race only)
            // 2 Grid walk-through (race only)
            // 3 Formation lap (race only)
            // 4 Starting-light countdown has begun (race only)
            // 5 Green flag
            // 6 Full course yellow / safety car
            // 7 Session stopped
            // 8 Session over
            // 9 Paused (tag.2015.09.14 - this is new, and indicates that this is a heartbeat call to the plugin)
            public byte mGamePhase;

            // Yellow flag states (applies to full-course only)
            // -1 Invalid
            //  0 None
            //  1 Pending
            //  2 Pits closed
            //  3 Pit lead lap
            //  4 Pits open
            //  5 Last lap
            //  6 Resume
            //  7 Race halt (not currently used)
            public sbyte mYellowFlagState;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public sbyte[] mSectorFlag;                                  // whether there are any local yellows at the moment in each sector (not sure if sector 0 is first or last, so test)
            [JsonIgnore] public byte mStartLight;                                     // start light frame (number depends on track)
            [JsonIgnore] public byte mNumRedLights;                                   // number of red lights in start sequence
            public byte mInRealtime;                                     // in realtime as opposed to at the monitor
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            [JsonIgnore] public byte[] mPlayerName;                                   // player name (including possible multiplayer override)
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            [JsonIgnore] public byte[] mPlrFileName;                                  // may be encoded to be a legal filename

            // weather
            [JsonIgnore] public double mDarkCloud;                                    // cloud darkness? 0.0-1.0
            public double mRaining;                                      // raining severity 0.0-1.0
            public double mAmbientTemp;                                  // temperature (Celsius)
            public double mTrackTemp;                                    // temperature (Celsius)
            public GTR2Vec3 mWind;                                        // wind speed
            [JsonIgnore] public double mMinPathWetness;                               // minimum wetness on main path 0.0-1.0
            [JsonIgnore] public double mMaxPathWetness;                               // maximum wetness on main path 0.0-1.0

            // multiplayer
            public byte mGameMode;                                       // 1 = server, 2 = client, 3 = server and client
            [JsonIgnore] public byte mIsPasswordProtected;                            // is the server password protected
            [JsonIgnore] public ushort mServerPort;                                   // the port of the server (if on a server)
            [JsonIgnore] public uint mServerPublicIP;                                 // the public IP address of the server (if on a server)
            [JsonIgnore] public int mMaxPlayers;                                      // maximum number of vehicles that can be in the session
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            [JsonIgnore] public byte[] mServerName;                                   // name of the server
            [JsonIgnore] public float mStartET;                                       // start time (seconds since midnight) of the event

            [JsonIgnore] public double mAvgPathWetness;                               // average wetness on main path 0.0-1.0

            // Future use
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 200)]
            [JsonIgnore] public byte[] mExpansion;

            // MM_NOT_USED
            // keeping this at the end of the structure to make it easier to replace in future versions
            // VehicleScoringInfoV01 *mVehicle;                                       // array of vehicle scoring info's
            // MM_NEW
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] pointer2;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2VehicleScoring
        {
            public int mID;                                              // slot ID (note that it can be re-used in multiplayer after someone leaves)
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mDriverName;                                   // driver name
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 64)]
            [JsonIgnore] public byte[] mVehicleName;                                  // vehicle name
            public short mTotalLaps;                                     // laps completed
            public sbyte mSector;                                        // 0=sector3, 1=sector1, 2=sector2 (don't ask why)
            public sbyte mFinishStatus;                                  // 0=none, 1=finished, 2=dnf, 3=dq
            public double mLapDist;                                      // current distance around track
            public double mPathLateral;                                  // lateral position with respect to *very approximate* "center" path
            public double mTrackEdge;                                    // track edge (w.r.t. "center" path) on same side of track as vehicle

            public double mBestSector1;                                  // best sector 1
            public double mBestSector2;                                  // best sector 2 (plus sector 1)
            public double mBestLapTime;                                  // best lap time
            public double mLastSector1;                                  // last sector 1
            public double mLastSector2;                                  // last sector 2 (plus sector 1)
            public double mLastLapTime;                                  // last lap time
            public double mCurSector1;                                   // current sector 1 if valid
            public double mCurSector2;                                   // current sector 2 (plus sector 1) if valid
                                                                         // no current laptime because it instantly becomes "last"

            public short mNumPitstops;                                   // number of pitstops made
            public short mNumPenalties;                                  // number of outstanding penalties
            public byte mIsPlayer;                                       // is this the player's vehicle

            public sbyte mControl;                                       // who's in control: -1=nobody (shouldn't get this), 0=local player, 1=local AI, 2=remote, 3=replay (shouldn't get this)
            public byte mInPits;                                         // between pit entrance and pit exit (not always accurate for remote vehicles)
            public byte mPlace;                                          // 1-based position
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mVehicleClass;                                 // vehicle class

            // Dash Indicators
            public double mTimeBehindNext;                               // time behind vehicle in next higher place
            [JsonIgnore] public int mLapsBehindNext;                                  // laps behind vehicle in next higher place
            [JsonIgnore] public double mTimeBehindLeader;                             // time behind leader
            [JsonIgnore] public int mLapsBehindLeader;                                // laps behind leader
            public double mLapStartET;                                   // time this lap was started

            // Position and derivatives
            [JsonIgnore] public GTR2Vec3 mPos;                                         // world position in meters
            public GTR2Vec3 mLocalVel;                                    // velocity (meters/sec) in local vehicle coordinates
            public GTR2Vec3 mLocalAccel;                                  // acceleration (meters/sec^2) in local vehicle coordinates

            // Orientation and derivatives
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            [JsonIgnore] public GTR2Vec3[] mOri;                                       // rows of orientation matrix (use TelemQuat conversions if desired), also converts local
                                                                                      // vehicle vectors into world X, Y, or Z using dot product of rows 0, 1, or 2 respectively
            [JsonIgnore] public GTR2Vec3 mLocalRot;                                    // rotation (radians/sec) in local vehicle coordinates
            [JsonIgnore] public GTR2Vec3 mLocalRotAccel;                               // rotational acceleration (radians/sec^2) in local vehicle coordinates

            // tag.2012.03.01 - stopped casting some of these so variables now have names and mExpansion has shrunk, overall size and old data locations should be same
            [JsonIgnore] public byte mHeadlights;                                     // status of headlights
            public byte mPitState;                                       // 0=none, 1=request, 2=entering, 3=stopped, 4=exiting
            [JsonIgnore] public byte mServerScored;                                   // whether this vehicle is being scored by server (could be off in qualifying or racing heats)
            [JsonIgnore] public byte mIndividualPhase;                                // game phases (described below) plus 9=after formation, 10=under yellow, 11=under blue (not used)

            [JsonIgnore] public int mQualification;                                   // 1-based, can be -1 when invalid

            [JsonIgnore] public double mTimeIntoLap;                                  // estimated time into lap
            [JsonIgnore] public double mEstimatedLapTime;                             // estimated laptime used for 'time behind' and 'time into lap' (note: this may changed based on vehicle and setup!?)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 24)]
            [JsonIgnore] public byte[] mPitGroup;                                     // pit group (same as team name unless pit is shared)
            public byte mFlag;                                           // primary flag being shown to vehicle (currently only 0=green or 6=blue)
            [JsonIgnore] public byte mUnderYellow;                                    // whether this car has taken a full-course caution flag at the start/finish line
            public byte mCountLapFlag;                                   // 0 = do not count lap or time, 1 = count lap but not time, 2 = count lap and time
            public byte mInGarageStall;                                  // appears to be within the correct garage stall

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16)]
            [JsonIgnore] public byte[] mUpgradePack;                                  // Coded upgrades

            public float mPitLapDist;                                    // location of pit in terms of lap distance

            [JsonIgnore] public float mBestLapSector1;                                // sector 1 time from best lap (not necessarily the best sector 1 time)
            [JsonIgnore] public float mBestLapSector2;                                // sector 2 time from best lap (not necessarily the best sector 2 time)

            // Future use
            // tag.2012.04.06 - SEE ABOVE!
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 48)]
            [JsonIgnore] public byte[] mExpansion;                                    // for future use
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2PhysicsOptions
        {
            [JsonIgnore] public byte mTractionControl;                                // 0 (off) - 3 (high)
            [JsonIgnore] public byte mAntiLockBrakes;                                 // 0 (off) - 2 (high)
            [JsonIgnore] public byte mStabilityControl;                               // 0 (off) - 2 (high)
            [JsonIgnore] public byte mAutoShift;                                      // 0 (off), 1 (upshifts), 2 (downshifts), 3 (all)
            [JsonIgnore] public byte mAutoClutch;                                     // 0 (off), 1 (on)
            public byte mInvulnerable;                                   // 0 (off), 1 (on)
            [JsonIgnore] public byte mOppositeLock;                                   // 0 (off), 1 (on)
            [JsonIgnore] public byte mSteeringHelp;                                   // 0 (off) - 3 (high)
            [JsonIgnore] public byte mBrakingHelp;                                    // 0 (off) - 2 (high)
            [JsonIgnore] public byte mSpinRecovery;                                   // 0 (off), 1 (on)
            [JsonIgnore] public byte mAutoPit;                                        // 0 (off), 1 (on)
            [JsonIgnore] public byte mAutoLift;                                       // 0 (off), 1 (on)
            [JsonIgnore] public byte mAutoBlip;                                       // 0 (off), 1 (on)

            public byte mFuelMult;                                       // fuel multiplier (0x-7x)
            [JsonIgnore] public byte mTireMult;                                       // tire wear multiplier (0x-7x)
            [JsonIgnore] public byte mMechFail;                                       // mechanical failure setting; 0 (off), 1 (normal), 2 (timescaled)
            [JsonIgnore] public byte mAllowPitcrewPush;                               // 0 (off), 1 (on)
            [JsonIgnore] public byte mRepeatShifts;                                   // accidental repeat shift prevention (0-5; see PLR file)
            [JsonIgnore] public byte mHoldClutch;                                     // for auto-shifters at start of race: 0 (off), 1 (on)
            [JsonIgnore] public byte mAutoReverse;                                    // 0 (off), 1 (on)
            [JsonIgnore] public byte mAlternateNeutral;                               // Whether shifting up and down simultaneously equals neutral

            // tag.2014.06.09 - yes these are new, but no they don't change the size of the structure nor the address of the other variables in it (because we're just using the existing padding)
            [JsonIgnore] public byte mAIControl;                                      // Whether player vehicle is currently under AI control
            [JsonIgnore] public byte mUnused1;                                        //
            [JsonIgnore] public byte mUnused2;                                        //

            [JsonIgnore] public float mManualShiftOverrideTime;                       // time before auto-shifting can resume after recent manual shift
            [JsonIgnore] public float mAutoShiftOverrideTime;                         // time before manual shifting can resume after recent auto shift
            [JsonIgnore] public float mSpeedSensitiveSteering;                        // 0.0 (off) - 1.0
            [JsonIgnore] public float mSteerRatioSpeed;                               // speed (m/s) under which lock gets expanded to full
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesCommandV01, except where noted by MM_NEW/MM_NOT_USED comments.  Renamed to match plugin convention.
        //////////////////////////////////////////////////////////////////////////////////////////
        public enum GTR2TrackRulesCommand
        {
            AddFromTrack = 0,             // crossed s/f line for first time after full-course yellow was called
            AddFromPit,                   // exited pit during full-course yellow
            AddFromUndq,                  // during a full-course yellow, the admin reversed a disqualification
            RemoveToPit,                  // entered pit during full-course yellow
            RemoveToDnf,                  // vehicle DNF'd during full-course yellow
            RemoveToDq,                   // vehicle DQ'd during full-course yellow
            RemoveToUnloaded,             // vehicle unloaded (possibly kicked out or banned) during full-course yellow
            MoveToBack,                   // misbehavior during full-course yellow, resulting in the penalty of being moved to the back of their current line
            LongestTime,                  // misbehavior during full-course yellow, resulting in the penalty of being moved to the back of the longest line
                                          //------------------
            Maximum                       // should be last
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesActionV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GTR2TrackRulesAction
        {
            // input only
            [JsonIgnore] public GTR2TrackRulesCommand mCommand;                        // recommended action
            [JsonIgnore] public int mID;                                              // slot ID if applicable
            [JsonIgnore] public double mET;                                           // elapsed time that event occurred, if applicable
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesColumnV01, except where noted by MM_NEW/MM_NOT_USED comments.  Renamed to match plugin convention.
        //////////////////////////////////////////////////////////////////////////////////////////
        public enum GTR2TrackRulesColumn
        {
            LeftLane = 0,                  // left (inside)
            MidLefLane,                    // mid-left
            MiddleLane,                    // middle
            MidrRghtLane,                  // mid-right
            RightLane,                     // right (outside)
                                           //------------------
            MaxLanes,                      // should be after the valid static lane choices
                                           //------------------
            Invalid = MaxLanes,            // currently invalid (hasn't crossed line or in pits/garage)
            FreeChoice,                    // free choice (dynamically chosen by driver)
            Pending,                       // depends on another participant's free choice (dynamically set after another driver chooses)
                                           //------------------
            Maximum                        // should be last
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesParticipantV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2TrackRulesParticipant
        {
            // input only
            public int mID;                                              // slot ID
            [JsonIgnore] public short mFrozenOrder;                                   // 0-based place when caution came out (not valid for formation laps)
            public short mPlace;                                         // 1-based place (typically used for the initialization of the formation lap track order)
            [JsonIgnore] public float mYellowSeverity;                                // a rating of how much this vehicle is contributing to a yellow flag (the sum of all vehicles is compared to TrackRulesV01::mSafetyCarThreshold)
            [JsonIgnore] public double mCurrentRelativeDistance;                      // equal to ( ( ScoringInfoV01::mLapDist * this->mRelativeLaps ) + VehicleScoringInfoV01::mLapDist )

            // input/output
            public int mRelativeLaps;                                    // current formation/caution laps relative to safety car (should generally be zero except when safety car crosses s/f line); this can be decremented to implement 'wave around' or 'beneficiary rule' (a.k.a. 'lucky dog' or 'free pass')
            public GTR2TrackRulesColumn mColumnAssignment;                // which column (line/lane) that participant is supposed to be in
            public int mPositionAssignment;                              // 0-based position within column (line/lane) that participant is supposed to be located at (-1 is invalid)
            public byte mPitsOpen;                                       // whether the rules allow this particular vehicle to enter pits right now (input is 2=false or 3=true; if you want to edit it, set to 0=false or 1=true)
            [JsonIgnore] public byte mUpToSpeed;                                      // while in the frozen order, this flag indicates whether the vehicle can be followed (this should be false for somebody who has temporarily spun and hasn't gotten back up to speed yet)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 2)]
            [JsonIgnore] public byte[] mUnused;                                       //

            [JsonIgnore] public double mGoalRelativeDistance;                         // calculated based on where the leader is, and adjusted by the desired column spacing and the column/position assignments

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] mMessage;                                      // a message for this participant to explain what is going on (untranslated; it will get run through translator on client machines)

            // future expansion
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 192)]
            [JsonIgnore] public byte[] mExpansion;
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesStageV01, except where noted by MM_NEW/MM_NOT_USED comments.  Renamed to match plugin convention.
        //////////////////////////////////////////////////////////////////////////////////////////
        public enum GTR2TrackRulesStage
        {
            FormationInit = 0,           // initialization of the formation lap
            FormationUpdate,             // update of the formation lap
            Normal,                      // normal (non-yellow) update
            CautionInit,                 // initialization of a full-course yellow
            CautionUpdate,               // update of a full-course yellow
                                         //------------------
            Maximum                      // should be last
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to TrackRulesV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2TrackRules
        {
            // input only
            [JsonIgnore] public double mCurrentET;                                    // current time
            public GTR2TrackRulesStage mStage;                            // current stage
            public GTR2TrackRulesColumn mPoleColumn;                      // column assignment where pole position seems to be located
            [JsonIgnore] public int mNumActions;                                      // number of recent actions

            // MM_NOT_USED
            // TrackRulesActionV01 *mAction;                                          // array of recent actions
            // MM_NEW
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] pointer1;

            public int mNumParticipants;                                 // number of participants (vehicles)

            [JsonIgnore] public byte mYellowFlagDetected;                             // whether yellow flag was requested or sum of participant mYellowSeverity's exceeds mSafetyCarThreshold
            [JsonIgnore] public byte mYellowFlagLapsWasOverridden;                    // whether mYellowFlagLaps (below) is an admin request (0=no 1=yes 2=clear yellow)

            [JsonIgnore] public byte mSafetyCarExists;                                // whether safety car even exists
            public byte mSafetyCarActive;                                // whether safety car is active
            public int mSafetyCarLaps;                                   // number of laps
            [JsonIgnore] public float mSafetyCarThreshold;                            // the threshold at which a safety car is called out (compared to the sum of TrackRulesParticipantV01::mYellowSeverity for each vehicle)
            public double mSafetyCarLapDist;                             // safety car lap distance

            [JsonIgnore] public float mSafetyCarLapDistAtStart;                       // where the safety car starts from
            public float mPitLaneStartDist;                              // where the waypoint branch to the pits breaks off (this may not be perfectly accurate)
            [JsonIgnore] public float mTeleportLapDist;                               // the front of the teleport locations (a useful first guess as to where to throw the green flag)

            // future input expansion
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]
            [JsonIgnore] public byte[] mInputExpansion;

            // input/output
            [JsonIgnore] public sbyte mYellowFlagState;                               // see ScoringInfoV01 for values
            [JsonIgnore] public short mYellowFlagLaps;                                // suggested number of laps to run under yellow (may be passed in with admin command)

            [JsonIgnore] public int mSafetyCarInstruction;                            // 0=no change, 1=go active, 2=head for pits
            public float mSafetyCarSpeed;                                // maximum speed at which to drive
            [JsonIgnore] public float mSafetyCarMinimumSpacing;                       // minimum spacing behind safety car (-1 to indicate no limit)
            [JsonIgnore] public float mSafetyCarMaximumSpacing;                       // maximum spacing behind safety car (-1 to indicate no limit)

            [JsonIgnore] public float mMinimumColumnSpacing;                          // minimum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)
            [JsonIgnore] public float mMaximumColumnSpacing;                          // maximum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)

            [JsonIgnore] public float mMinimumSpeed;                                  // minimum speed that anybody should be driving (-1 to indicate no limit)
            [JsonIgnore] public float mMaximumSpeed;                                  // maximum speed that anybody should be driving (-1 to indicate no limit)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 96)]
            public byte[] mMessage;                                      // a message for everybody to explain what is going on (which will get run through translator on client machines)

            // MM_NOT_USED
            // TrackRulesParticipantV01 *mParticipant;                                // array of partipants (vehicles)
            // MM_NEW
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            [JsonIgnore] public byte[] pointer2;

            // future input/output expansion
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]
            [JsonIgnore] public byte[] mInputOutputExpansion;
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to PitMenuV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2PitMenu
        {
            public int mCategoryIndex;                                   // index of the current category

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mCategoryName;                                 // name of the current category (untranslated)
            public int mChoiceIndex;                                     // index of the current choice (within the current category)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] mChoiceString;                                 // name of the current choice (may have some translated words)
            public int mNumChoices;                                      // total number of choices (0 <= mChoiceIndex < mNumChoices)

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] mExpansion;                                    // for future use
        }


        //////////////////////////////////////////////////////////////////////////////////////////
        // Identical to WeatherControlInfoV01, except where noted by MM_NEW/MM_NOT_USED comments.
        //////////////////////////////////////////////////////////////////////////////////////////
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2WeatherControlInfo
        {
            // The current conditions are passed in with the API call. The following ET (Elapsed Time) value should typically be far
            // enough in the future that it can be interpolated smoothly, and allow clouds time to roll in before rain starts. In
            // other words you probably shouldn't have mCloudiness and mRaining suddenly change from 0.0 to 1.0 and expect that
            // to happen in a few seconds without looking crazy.
            public double mET;                                           // when you want this weather to take effect

            // mRaining[1][1] is at the origin (2013.12.19 - and currently the only implemented node), while the others
            // are spaced at <trackNodeSize> meters where <trackNodeSize> is the maximum absolute value of a track vertex
            // coordinate (and is passed into the API call).
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 9)]
            public double[] mRaining;                                    // rain (0.0-1.0) at different nodes

            public double mCloudiness;                                   // general cloudiness (0.0=clear to 1.0=dark), will be automatically overridden to help ensure clouds exist over rainy areas
            public double mAmbientTempK;                                 // ambient temperature (Kelvin)
            public double mWindMaxSpeed;                                 // maximum speed of wind (ground speed, but it affects how fast the clouds move, too)

            public bool mApplyCloudinessInstantly;                       // preferably we roll the new clouds in, but you can instantly change them now
            public bool mUnused1;                                        //
            public bool mUnused2;                                        //
            public bool mUnused3;                                        //

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 508)]
            public byte[] mExpansion;                                    // future use (humidity, pressure, air density, etc.)
        }


        ///////////////////////////////////////////
        // Mapped wrapper structures
        ///////////////////////////////////////////

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2MappedBufferVersionBlock
        {
            // If both version variables are equal, buffer is not being written to, or we're extremely unlucky and second check is necessary.
            // If versions don't match, buffer is being written to, or is incomplete (game crash, or missed transition).
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2MappedBufferVersionBlockWithSize
        {
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                      // 0 means unknown (whole buffer should be considered as updated).
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Telemetry
        {
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                      // 0 means unknown (whole buffer should be considered as updated).

            public int mNumVehicles;                                     // current number of vehicles
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_VEHICLES)]
            public GTR2VehicleTelemetry[] mVehicles;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Scoring
        {
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                      // 0 means unknown (whole buffer should be considered as updated).

            public GTR2ScoringInfo mScoringInfo;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_VEHICLES)]
            public GTR2VehicleScoring[] mVehicles;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Rules
        {
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            [JsonIgnore] public int mBytesUpdatedHint;                                // How many bytes of the structure were written during the last update.
                                                                                      // 0 means unknown (whole buffer should be considered as updated).

            public GTR2TrackRules mTrackRules;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_VEHICLES)]
            [JsonIgnore] public GTR2TrackRulesAction[] mActions;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_VEHICLES)]
            public GTR2TrackRulesParticipant[] mParticipants;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2ForceFeedback
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public double mForceValue;                                   // Current FFB value reported via InternalsPlugin::ForceFeedback.
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2GraphicsInfo
        {
            public GTR2Vec3 mCamPos;                                      // camera position

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 3)]
            public GTR2Vec3[] mCamOri;                                    // rows of orientation matrix (use TelemQuat conversions if desired), also converts local

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] mHWND;                                         // app handle

            public double mAmbientRed;
            public double mAmbientGreen;
            public double mAmbientBlue;

            public int mID;                                              // slot ID being viewed (-1 if invalid)

            // Camera types (some of these may only be used for *setting* the camera type in WantsToViewVehicle())
            //    0  = TV cockpit
            //    1  = cockpit
            //    2  = nosecam
            //    3  = swingman
            //    4  = trackside (nearest)
            //    5  = onboard000
            //       :
            //       :
            // 1004  = onboard999
            // 1005+ = (currently unsupported, in the future may be able to set/get specific trackside camera)
            public int mCameraType;                                      // see above comments for possible values

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] mExpansion;                                    // for future use (possibly camera name)
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Graphics
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public GTR2GraphicsInfo mGraphicsInfo;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2PitInfo
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public GTR2PitMenu mPitMneu;
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        struct GTR2Weather
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public double mTrackNodeSize;
            public GTR2WeatherControlInfo mWeatherInfo;
        }


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GTR2TrackedDamage
        {
            public double mMaxImpactMagnitude;                           // Max impact magnitude.  Tracked on every telemetry update, and reset on visit to pits or Session restart.
            public double mAccumulatedImpactMagnitude;                   // Accumulated impact magnitude.  Tracked on every telemetry update, and reset on visit to pits or Session restart.
        };


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GTR2VehScoringCapture
        {
            // VehicleScoringInfoV01 members:
            public int mID;                                              // slot ID (note that it can be re-used in multiplayer after someone leaves)
            public byte mPlace;
            public byte mIsPlayer;
            [JsonIgnore] public sbyte mFinishStatus;                                  // 0=none, 1=finished, 2=dnf, 3=dq
        };


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct GTR2SessionTransitionCapture
        {
            // ScoringInfoV01 members:
            [JsonIgnore] public byte mGamePhase;
            [JsonIgnore] public int mSession;

            // VehicleScoringInfoV01 members:
            public int mNumScoringVehicles;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_VEHICLES)]
            public GTR2VehScoringCapture[] mScoringVehicles;
        };


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2Extended
        {
            [JsonIgnore] public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            [JsonIgnore] public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] mVersion;                                      // API version
            public byte is64bit;                                         // Is 64bit plugin?

            // Physics options (updated on session start):
            public GTR2PhysicsOptions mPhysics;

            // Damage tracking for each vehicle (indexed by mID % GTR2MappedBufferHeader::MAX_MAPPED_IDS):
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_MAPPED_IDS)]
            public GTR2TrackedDamage[] mTrackedDamages;

            // Function call based flags:
            public byte mInRealtimeFC;                                   // in realtime as opposed to at the monitor (reported via last EnterRealtime/ExitRealtime calls).
            [JsonIgnore] public byte mMultimediaThreadStarted;                        // multimedia thread started (reported via ThreadStarted/ThreadStopped calls).
            [JsonIgnore] public byte mSimulationThreadStarted;                        // simulation thread started (reported via ThreadStarted/ThreadStopped calls).

            public byte mSessionStarted;                                 // Set to true on Session Started, set to false on Session Ended.
            [JsonIgnore] public Int64 mTicksSessionStarted;                           // Ticks when session started.
            public Int64 mTicksSessionEnded;                             // Ticks when session ended.
            public GTR2SessionTransitionCapture mSessionTransitionCapture;// Contains partial internals capture at session transition time.

            // Captured non-empty MessageInfoV01::mText message.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] mDisplayedMessageUpdateCapture;

            // Direct Memory access stuff
            public byte mDirectMemoryAccessEnabled;

            public Int64 mTicksStatusMessageUpdated;                     // Ticks when status message was updated;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_STATUS_MSG_LEN)]
            public byte[] mStatusMessage;

            public Int64 mTicksLastHistoryMessageUpdated;                // Ticks when last message history message was updated;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_STATUS_MSG_LEN)]
            public byte[] mLastHistoryMessage;

            public float mCurrentPitSpeedLimit;                          // speed limit m/s.

            public byte mSCRPluginEnabled;                               // Is Stock Car Rules plugin enabled?
            public int mSCRPluginDoubleFileType;                         // Stock Car Rules plugin DoubleFileType value, only meaningful if mSCRPluginEnabled is true.

            public Int64 mTicksLSIPhaseMessageUpdated;                   // Ticks when last LSI phase message was updated.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_RULES_INSTRUCTION_MSG_LEN)]
            public byte[] mLSIPhaseMessage;

            public Int64 mTicksLSIPitStateMessageUpdated;                // Ticks when last LSI pit state message was updated.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_RULES_INSTRUCTION_MSG_LEN)]
            public byte[] mLSIPitStateMessage;

            public Int64 mTicksLSIOrderInstructionMessageUpdated;        // Ticks when last LSI order instruction message was updated.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_RULES_INSTRUCTION_MSG_LEN)]
            public byte[] mLSIOrderInstructionMessage;

            public Int64 mTicksLSIRulesInstructionMessageUpdated;        // Ticks when last FCY rules message was updated.  Currently, only SCR plugin sets that.
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_RULES_INSTRUCTION_MSG_LEN)]
            public byte[] mLSIRulesInstructionMessage;

            [JsonIgnore] public int mUnsubscribedBuffersMask;                         // Currently active UnsbscribedBuffersMask value.  This will be allowed for clients to write to in the future, but not yet.

            public byte mHWControlInputEnabled;                          // HWControl input buffer is enabled.
            public byte mWeatherControlInputEnabled;                     // WeatherControl input buffer is enabled.
            public byte mRulesControlInputEnabled;                       // RulesControl input buffer is enabled.
        }


        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        public struct GTR2HWControl
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public int mLayoutVersion;

            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = GTR2Constants.MAX_HWCONTROL_NAME_LEN)]
            public byte[] mControlName;
            public double mfRetVal;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 4)]
        struct GTR2WeatherControl
        {
            public uint mVersionUpdateBegin;                             // Incremented right before buffer is written to.
            public uint mVersionUpdateEnd;                               // Incremented after buffer write is done.

            public int mLayoutVersion;

            public GTR2WeatherControlInfo mWeatherInfo;
        }

        enum SubscribedBuffer
        {
            Telemetry = 1,
            Scoring = 2,
            Rules = 4,
            MultiRules = 8,
            ForceFeedback = 16,
            Graphics = 32,
            PitInfo = 64,
            Weather = 128,
            All = 255
        };
    }
}