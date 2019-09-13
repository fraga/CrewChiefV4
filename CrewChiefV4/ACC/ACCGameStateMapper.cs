﻿using CrewChiefV4.ACC.accData;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;

/**
 * Maps memory mapped file to a local game-agnostic representation.
 */

namespace CrewChiefV4.ACC
{
    public class ACCGameStateMapper : GameStateMapper
    {
        public static Boolean versionChecked = false;
        public static int numberOfSectorsOnTrack = 3;

        private class AcTyres
        {
            public List<CornerData.EnumWithThresholds> tyreWearThresholdsForAC = new List<CornerData.EnumWithThresholds>();
            public List<CornerData.EnumWithThresholds> tyreTempThresholdsForAC = new List<CornerData.EnumWithThresholds>();
            public float tyreWearMinimumValue;

            public AcTyres(List<CornerData.EnumWithThresholds> tyreWearThresholds, List<CornerData.EnumWithThresholds> tyreTempThresholds, float tyreWearMinimum)
            {
                tyreWearThresholdsForAC = tyreWearThresholds;
                tyreTempThresholdsForAC = tyreTempThresholds;
                tyreWearMinimumValue = tyreWearMinimum;
            }
        }

        // lapCount and splinePosition disagree when we're near to the start line. Who knows why.
        private const float startOfShitSplinePoint = 0.993f;
        private const float endOfShitSplinePoint = 0.007f;

        List<CornerData.EnumWithThresholds> tyreTempThresholds = new List<CornerData.EnumWithThresholds>();
        private static Dictionary<string, AcTyres> acTyres = new Dictionary<string, AcTyres>();

        // these are set when we start a new session, from the car name / class
        private TyreType defaultTyreTypeForPlayersCar = TyreType.Unknown_Race;

        private float[] loggedSectorStart = new float[] { -1f, -1f };

        private List<CornerData.EnumWithThresholds> brakeTempThresholdsForPlayersCar = CarData.getBrakeTempThresholds(CarData.getCarClassFromEnum(CarData.CarClassEnum.GT3));

        private int lapCountAtSector1End = -1;

        // next track conditions sample due after:
        private DateTime nextConditionsSampleDue = DateTime.MinValue;

        private List<CornerData.EnumWithThresholds> suspensionDamageThresholds = new List<CornerData.EnumWithThresholds>();

        private float trivialSuspensionDamageThreshold = 0.01f;
        private float minorSuspensionDamageThreshold = 0.05f;
        private float severeSuspensionDamageThreshold = 0.15f;
        private float destroyedSuspensionDamageThreshold = 0.60f;

        private float trivialEngineDamageThreshold = 900.0f;
        private float minorEngineDamageThreshold = 600.0f;
        private float severeEngineDamageThreshold = 350.0f;
        private float destroyedEngineDamageThreshold = 25.0f;

        private float trivialAeroDamageThreshold = 40.0f;
        private float minorAeroDamageThreshold = 100.0f;
        private float severeAeroDamageThreshold = 200.0f;
        private float destroyedAeroDamageThreshold = 400.0f;
        private HashSet<int> msgHash = new HashSet<int>();

        // ABS can trigger below 1.1 in the Ferrari 488
        private float wheelSlipThreshold = 1.3f;

        private AC_SESSION_TYPE sessionTypeOnPreviousTick = AC_SESSION_TYPE.AC_UNKNOWN;
        private DateTime ignoreUnknownSessionTypeUntil = DateTime.MinValue;
        private Boolean waitingForUnknownSessionTypeToSettle = false;

        private Dictionary<string, int> opponentDisconnectionCounter = new Dictionary<string, int>();

        #region WaYToManyTyres
        public ACCGameStateMapper()
        {
            acTyres.Clear();
            suspensionDamageThresholds.Clear();

            CornerData.EnumWithThresholds suspensionDamageNone = new CornerData.EnumWithThresholds(DamageLevel.NONE, -10000, trivialSuspensionDamageThreshold);
            CornerData.EnumWithThresholds suspensionDamageTrivial = new CornerData.EnumWithThresholds(DamageLevel.TRIVIAL, trivialSuspensionDamageThreshold, minorSuspensionDamageThreshold);
            CornerData.EnumWithThresholds suspensionDamageMinor = new CornerData.EnumWithThresholds(DamageLevel.MINOR, trivialSuspensionDamageThreshold, severeSuspensionDamageThreshold);
            CornerData.EnumWithThresholds suspensionDamageMajor = new CornerData.EnumWithThresholds(DamageLevel.MAJOR, severeSuspensionDamageThreshold, destroyedSuspensionDamageThreshold);
            CornerData.EnumWithThresholds suspensionDamageDestroyed = new CornerData.EnumWithThresholds(DamageLevel.DESTROYED, destroyedSuspensionDamageThreshold, 10000);
            suspensionDamageThresholds.Add(suspensionDamageNone);
            suspensionDamageThresholds.Add(suspensionDamageTrivial);
            suspensionDamageThresholds.Add(suspensionDamageMinor);
            suspensionDamageThresholds.Add(suspensionDamageMajor);
            suspensionDamageThresholds.Add(suspensionDamageDestroyed);

            //GTE Classes
            List<CornerData.EnumWithThresholds> tyreWearThresholdsSlickHard = new List<CornerData.EnumWithThresholds>();
            tyreWearThresholdsSlickHard.Add(new CornerData.EnumWithThresholds(TyreCondition.NEW, -10000f, 0.000f));
            tyreWearThresholdsSlickHard.Add(new CornerData.EnumWithThresholds(TyreCondition.SCRUBBED, 0.000f, 14.96601f));
            tyreWearThresholdsSlickHard.Add(new CornerData.EnumWithThresholds(TyreCondition.MINOR_WEAR, 14.96601f, 22.68041f));
            tyreWearThresholdsSlickHard.Add(new CornerData.EnumWithThresholds(TyreCondition.MAJOR_WEAR, 22.68041f, 30.55553f));
            tyreWearThresholdsSlickHard.Add(new CornerData.EnumWithThresholds(TyreCondition.WORN_OUT, 30.55553f, 1000f));

            List<CornerData.EnumWithThresholds> tyreTempsThresholdsHardSlick = new List<CornerData.EnumWithThresholds>();
            tyreTempsThresholdsHardSlick.Add(new CornerData.EnumWithThresholds(TyreTemp.COLD, -10000f, 75f));
            tyreTempsThresholdsHardSlick.Add(new CornerData.EnumWithThresholds(TyreTemp.WARM, 75f, 100f));
            tyreTempsThresholdsHardSlick.Add(new CornerData.EnumWithThresholds(TyreTemp.HOT, 100f, 180f));
            tyreTempsThresholdsHardSlick.Add(new CornerData.EnumWithThresholds(TyreTemp.COOKING, 180f, 10000f));
            acTyres.Add("dry_compound", new AcTyres(tyreWearThresholdsSlickHard, tyreTempsThresholdsHardSlick, 88f));
        }
        #endregion

        public override void versionCheck(Object memoryMappedFileStruct)
        {
            //AssettoCorsaShared shared = ((ACCSharedMemoryReader.ACCStructWrapper)memoryMappedFileStruct).data;
            //String currentVersion = shared.acsStatic.smVersion;
            //String currentPluginVersion = getNameFromBytes(shared.acsChief.pluginVersion);
            //if (currentVersion.Length != 0 && currentPluginVersion.Length != 0 && versionChecked == false)
            //{
            //    System.Diagnostics.Debug.WriteLine("Shared Memory Version: " + shared.acsStatic.smVersion);
            //    if (!currentVersion.Equals(expectedVersion, StringComparison.Ordinal))
            //    {
            //        throw new GameDataReadException("Expected shared data version " + expectedVersion + " but got version " + currentVersion);
            //    }
            //    System.Diagnostics.Debug.WriteLine("Plugin Version: " + currentPluginVersion);
            //    if (!currentPluginVersion.Equals(expectedPluginVersion, StringComparison.Ordinal))
            //    {

            //        throw new GameDataReadException("Expected python plugin version " + expectedPluginVersion + " but got version " + currentPluginVersion);
            //    }
            versionChecked = true;
            //}
        }

        public static OpponentData getOpponentForName(GameStateData gameState, String nameToFind)
        {
            if (gameState.OpponentData == null || gameState.OpponentData.Count == 0 || nameToFind == null || nameToFind.Length == 0)
            {
                return null;
            }

            OpponentData od = null;
            if (gameState.OpponentData.TryGetValue(nameToFind, out od))
            {
                return od;
            }
            return null;
        }

        public override GameStateData mapToGameStateData(Object memoryMappedFileStruct, GameStateData previousGameState)
        {
            ACCSharedMemoryReader.ACCStructWrapper wrapper = (ACCSharedMemoryReader.ACCStructWrapper)memoryMappedFileStruct;
            GameStateData currentGameState = new GameStateData(wrapper.ticksWhenRead);
            ACCShared shared = wrapper.data;

            if (shared == null)
            {
                return null;
            }

            // If this is empty then it is because we haven't loaded the players car yet
            if (shared.accChief.vehicle.Length == 0)
            {
                // no participant data
                return null;
            }
            accVehicleInfo playerVehicle = shared.accChief.vehicle[0];

            if (String.IsNullOrEmpty(playerVehicle.driverName))
                return null;

            AC_STATUS status = shared.accGraphic.status;
            if (status == AC_STATUS.AC_REPLAY)
            {
                CrewChief.trackName = shared.accStatic.track + ":" + shared.accStatic.trackConfiguration;
                CrewChief.carClass = CarData.getCarClassForClassName(shared.accStatic.carModel).carClassEnum;
                CrewChief.viewingReplay = true;
                CrewChief.distanceRoundTrack = (shared.accChief.vehicle?.Length ?? 0) == 0 ? 0 : spLineLengthToDistanceRoundTrack(shared.accChief.trackLength, playerVehicle.spLineLength);
            }

            if (status == AC_STATUS.AC_REPLAY || status == AC_STATUS.AC_OFF || shared.accChief.vehicle.Length <= 0)
            {
                return previousGameState;
            }

            Boolean isOnline = shared.accChief.serverName.Length > 0;
            Boolean isSinglePlayerPracticeSession = shared.accChief.vehicle.Length == 1 && !isOnline && shared.accGraphic.session == AC_SESSION_TYPE.AC_PRACTICE;
            float distanceRoundTrack = spLineLengthToDistanceRoundTrack(shared.accChief.trackLength, playerVehicle.spLineLength);

            currentGameState.SessionData.TrackDefinition = new TrackDefinition(shared.accStatic.track + ":" + shared.accStatic.trackConfiguration, shared.accChief.trackLength);

            Validator.validate(playerVehicle.driverName);
            AC_SESSION_TYPE sessionTypeAsSentByGame = shared.accGraphic.session;

            SessionPhase lastSessionPhase = SessionPhase.Unavailable;
            SessionType lastSessionType = SessionType.Unavailable;
            float lastSessionRunningTime = 0;
            int lastSessionLapsCompleted = 0;
            TrackDefinition lastSessionTrack = null;
            Boolean lastSessionHasFixedTime = false;
            int lastSessionNumberOfLaps = 0;
            float lastSessionTotalRunTime = 0;
            float lastSessionTimeRemaining = 0;

            if (previousGameState != null)
            {
                lastSessionPhase = previousGameState.SessionData.SessionPhase;
                lastSessionType = previousGameState.SessionData.SessionType;
                lastSessionRunningTime = previousGameState.SessionData.SessionRunningTime;
                lastSessionHasFixedTime = previousGameState.SessionData.SessionHasFixedTime;
                lastSessionTrack = previousGameState.SessionData.TrackDefinition;
                lastSessionLapsCompleted = previousGameState.SessionData.CompletedLaps;
                lastSessionNumberOfLaps = previousGameState.SessionData.SessionNumberOfLaps;
                lastSessionTotalRunTime = previousGameState.SessionData.SessionTotalRunTime;
                lastSessionTimeRemaining = previousGameState.SessionData.SessionTimeRemaining;
                currentGameState.carClass = previousGameState.carClass;

                currentGameState.SessionData.PlayerLapTimeSessionBest = previousGameState.SessionData.PlayerLapTimeSessionBest;
                currentGameState.SessionData.PlayerLapTimeSessionBestPrevious = previousGameState.SessionData.PlayerLapTimeSessionBestPrevious;
                currentGameState.SessionData.OpponentsLapTimeSessionBestOverall = previousGameState.SessionData.OpponentsLapTimeSessionBestOverall;
                currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass = previousGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass;
                currentGameState.SessionData.OverallSessionBestLapTime = previousGameState.SessionData.OverallSessionBestLapTime;
                currentGameState.SessionData.PlayerClassSessionBestLapTime = previousGameState.SessionData.PlayerClassSessionBestLapTime;
                currentGameState.SessionData.CurrentLapIsValid = previousGameState.SessionData.CurrentLapIsValid;
                currentGameState.SessionData.PreviousLapWasValid = previousGameState.SessionData.PreviousLapWasValid;
                currentGameState.readLandmarksForThisLap = previousGameState.readLandmarksForThisLap;
            }

            if (currentGameState.carClass.carClassEnum == CarData.CarClassEnum.UNKNOWN_RACE)
            {
                CarData.CarClass newClass = CarData.getCarClassForClassName(shared.accStatic.carModel);
                CarData.CLASS_ID = shared.accStatic.carModel;
                if (!CarData.IsCarClassEqual(newClass, currentGameState.carClass, true))
                {
                    currentGameState.carClass = newClass;
                    GlobalBehaviourSettings.UpdateFromCarClass(currentGameState.carClass);
                    System.Diagnostics.Debug.WriteLine("Player is using car class " + currentGameState.carClass.getClassIdentifier());
                    brakeTempThresholdsForPlayersCar = CarData.getBrakeTempThresholds(currentGameState.carClass);
                    // no tyre data in the block so get the default tyre types for this car
                    defaultTyreTypeForPlayersCar = CarData.getDefaultTyreType(currentGameState.carClass);
                }
            }

            // this is the corrected AC_SESSION_TYPE, ignoring occasional shit data. I wonder what else is set to "shit" in shared memory every few seconds?
            AC_SESSION_TYPE sessionType;
            if (sessionTypeAsSentByGame != AC_SESSION_TYPE.AC_UNKNOWN)
            {
                waitingForUnknownSessionTypeToSettle = false;
                sessionTypeOnPreviousTick = sessionTypeAsSentByGame;
                sessionType = sessionTypeAsSentByGame;
            }
            else
            {
                if (!waitingForUnknownSessionTypeToSettle)
                {
                    // transitioned to UNKNOWN session type. ACC sends this semi-randomly in shared memory during a running session, no idea why
                    waitingForUnknownSessionTypeToSettle = true;
                    ignoreUnknownSessionTypeUntil = currentGameState.Now.AddSeconds(1);
                    sessionType = sessionTypeOnPreviousTick;
                }
                else if (currentGameState.Now > ignoreUnknownSessionTypeUntil)
                {
                    // transition to UNKNOWN has settled
                    waitingForUnknownSessionTypeToSettle = false;
                    sessionTypeOnPreviousTick = sessionTypeAsSentByGame;
                    sessionType = sessionTypeAsSentByGame;
                }
                else
                {
                    // transition to UNKNOWN has not settled
                    sessionType = sessionTypeOnPreviousTick;
                }
            }

            currentGameState.SessionData.SessionType = mapToSessionState(sessionType);

            Boolean leaderHasFinished = previousGameState != null && previousGameState.SessionData.LeaderHasFinishedRace;
            currentGameState.SessionData.LeaderHasFinishedRace = leaderHasFinished;

            int numberOfLapsInSession = (int)shared.accGraphic.numberOfLaps;

            float gameSessionTimeLeft = 0.0f;
            if (!Double.IsInfinity(shared.accGraphic.sessionTimeLeft))
            {
                gameSessionTimeLeft = shared.accGraphic.sessionTimeLeft / 1000f;
            }

            float sessionTimeRemaining = -1;
            //if (sessionType != AC_SESSION_TYPE.AC_PRACTICE && (numberOfLapsInSession == 0 || shared.accStatic.isTimedRace == 1))
            if (numberOfLapsInSession == 0 || shared.accStatic.isTimedRace == 1)
            {
                currentGameState.SessionData.SessionHasFixedTime = true;
                sessionTimeRemaining = gameSessionTimeLeft;
            }

            Boolean isCountDown = false;
            TimeSpan countDown = TimeSpan.FromSeconds(gameSessionTimeLeft);

            if (sessionType == AC_SESSION_TYPE.AC_RACE || sessionType == AC_SESSION_TYPE.AC_DRIFT || sessionType == AC_SESSION_TYPE.AC_DRAG)
            {
                //Make sure to check for both numberOfLapsInSession and isTimedRace as latter sometimes tells lies!
                if (shared.accStatic.isTimedRace == 1 || numberOfLapsInSession == 0)
                {
                    isCountDown = playerVehicle.currentLapTimeMS <= 0 && playerVehicle.lapCount <= 0;
                }
                else
                {
                    isCountDown = countDown.TotalMilliseconds >= 0.25;
                }
            }

            AC_FLAG_TYPE currentFlag = shared.accGraphic.flag;

            currentGameState.SessionData.IsDisqualified = currentFlag == AC_FLAG_TYPE.AC_BLACK_FLAG;
            bool isInPits = shared.accGraphic.isInPit == 1;
            int lapsCompleted = shared.accGraphic.completedLaps;
            ACCGameStateMapper.numberOfSectorsOnTrack = shared.accStatic.sectorCount;

            Boolean raceFinished = lapsCompleted == numberOfLapsInSession || (previousGameState != null && previousGameState.SessionData.LeaderHasFinishedRace && previousGameState.SessionData.IsNewLap);
            
            currentGameState.SessionData.SessionPhase = shared.accChief.SessionPhase;
            if (raceFinished && shared.accChief.SessionPhase == SessionPhase.Checkered)
            {
                currentGameState.SessionData.SessionPhase = SessionPhase.Finished;
            }

            // use leaderboard position in non-race sessions, and for the first 10 seconds of a race
            Boolean useLeaderboardPosition = sessionType == AC_SESSION_TYPE.AC_PRACTICE
                || sessionType == AC_SESSION_TYPE.AC_QUALIFY
                || raceFinished
                || (previousGameState != null && previousGameState.SessionData.SessionRunningTime < 10 && shared.accGraphic.session == AC_SESSION_TYPE.AC_RACE);

            if (useLeaderboardPosition)
            {
                currentGameState.SessionData.OverallPosition = playerVehicle.carLeaderboardPosition;
            }
            else
            {
                // sanity check for realtime position data - the lap start point and the spline zero point aren't in the same place. Here we assume they're quite close
                // and if we're quite close to 0 on the spline, re-use the previous position data
                if (previousGameState != null && (playerVehicle.spLineLength > ACCGameStateMapper.startOfShitSplinePoint || playerVehicle.spLineLength < ACCGameStateMapper.endOfShitSplinePoint))
                {
                    currentGameState.SessionData.OverallPosition = previousGameState.SessionData.OverallPosition;
                }
                else
                {
                    currentGameState.SessionData.OverallPosition = playerVehicle.carRealTimeLeaderboardPosition;
                }
            }

            currentGameState.SessionData.TrackDefinition = TrackData.getTrackDefinition(shared.accStatic.track + ":" + shared.accStatic.trackConfiguration, shared.accChief.trackLength, shared.accStatic.sectorCount);

            Boolean sessionOfSameTypeRestarted = ((currentGameState.SessionData.SessionType == SessionType.Race && lastSessionType == SessionType.Race) ||
                (currentGameState.SessionData.SessionType == SessionType.Practice && lastSessionType == SessionType.Practice) ||
                (currentGameState.SessionData.SessionType == SessionType.Qualify && lastSessionType == SessionType.Qualify)) &&
                ((lastSessionPhase == SessionPhase.Green || lastSessionPhase == SessionPhase.FullCourseYellow) || lastSessionPhase == SessionPhase.Finished) &&
                currentGameState.SessionData.SessionPhase == SessionPhase.Countdown &&
                (currentGameState.SessionData.SessionType == SessionType.Race ||
                    currentGameState.SessionData.SessionHasFixedTime && sessionTimeRemaining > lastSessionTimeRemaining + 1);

            if (sessionOfSameTypeRestarted ||
                (currentGameState.SessionData.SessionType != SessionType.Unavailable &&
                 currentGameState.SessionData.SessionPhase != SessionPhase.Finished &&
                    (lastSessionType != currentGameState.SessionData.SessionType ||
                        lastSessionTrack == null || lastSessionTrack.name != currentGameState.SessionData.TrackDefinition.name ||
                            (currentGameState.SessionData.SessionHasFixedTime && sessionTimeRemaining > lastSessionTimeRemaining + 1))))
            {
                opponentDisconnectionCounter.Clear();
                System.Diagnostics.Debug.WriteLine("New session, trigger...");
                if (sessionOfSameTypeRestarted)
                {
                    System.Diagnostics.Debug.WriteLine("Session of same type (" + lastSessionType + ") restarted (green / finished -> countdown)");
                }
                if (lastSessionType != currentGameState.SessionData.SessionType)
                {
                    System.Diagnostics.Debug.WriteLine("lastSessionType = " + lastSessionType + " currentGameState.SessionData.SessionType = " + currentGameState.SessionData.SessionType);
                }
                else if (lastSessionTrack != currentGameState.SessionData.TrackDefinition)
                {
                    String lastTrackName = lastSessionTrack == null ? "unknown" : lastSessionTrack.name;
                    String currentTrackName = currentGameState.SessionData.TrackDefinition == null ? "unknown" : currentGameState.SessionData.TrackDefinition.name;
                    System.Diagnostics.Debug.WriteLine("lastSessionTrack = " + lastTrackName + " currentGameState.SessionData.Track = " + currentTrackName);
                    if (currentGameState.SessionData.TrackDefinition.unknownTrack)
                    {
                        System.Diagnostics.Debug.WriteLine("Track is unknown, setting virtual sectors");
                        currentGameState.SessionData.TrackDefinition.setSectorPointsForUnknownTracks();
                    }

                }
                else if (currentGameState.SessionData.SessionHasFixedTime && sessionTimeRemaining > lastSessionTimeRemaining + 1)
                {
                    System.Diagnostics.Debug.WriteLine("sessionTimeRemaining = " + sessionTimeRemaining.ToString("0.000") + " lastSessionTimeRemaining = " + lastSessionTimeRemaining.ToString("0.000"));
                }
                lapCountAtSector1End = -1;
                currentGameState.SessionData.IsNewSession = true;
                currentGameState.SessionData.SessionNumberOfLaps = numberOfLapsInSession;
                currentGameState.SessionData.LeaderHasFinishedRace = false;
                currentGameState.SessionData.SessionStartTime = currentGameState.Now;
                if (currentGameState.SessionData.SessionHasFixedTime)
                {
                    currentGameState.SessionData.SessionTotalRunTime = sessionTimeRemaining;
                    currentGameState.SessionData.SessionTimeRemaining = sessionTimeRemaining;
                    currentGameState.SessionData.HasExtraLap = shared.accStatic.hasExtraLap == 1;
                    if (currentGameState.SessionData.SessionTotalRunTime == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("Setting session run time to 0");
                    }
                    System.Diagnostics.Debug.WriteLine("Time in this new session = " + sessionTimeRemaining.ToString("0.000"));
                }
                currentGameState.SessionData.DriverRawName = playerVehicle.driverName;
                currentGameState.PitData.IsRefuellingAllowed = true;

                //add carclasses for assetto corsa.
                currentGameState.carClass = CarData.getCarClassForClassName(shared.accStatic.carModel);
                GlobalBehaviourSettings.UpdateFromCarClass(currentGameState.carClass);
                CarData.CLASS_ID = shared.accStatic.carModel;

                if (acTyres.Count > 0 && !acTyres.ContainsKey(shared.accGraphic.tyreCompound))
                {
                    System.Diagnostics.Debug.WriteLine("Tyre information is disabled. Player is using unknown Tyre Type " + shared.accGraphic.tyreCompound);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Player is using Tyre Type " + shared.accGraphic.tyreCompound);
                }
                currentGameState.TyreData.TyreTypeName = shared.accGraphic.tyreCompound;
                tyreTempThresholds = getTyreTempThresholds(currentGameState.carClass, currentGameState.TyreData.TyreTypeName);
                brakeTempThresholdsForPlayersCar = CarData.getBrakeTempThresholds(currentGameState.carClass);
                // no tyre data in the block so get the default tyre types for this car
                defaultTyreTypeForPlayersCar = CarData.getDefaultTyreType(currentGameState.carClass);

                // 0th element is always the player - this is set in the ACCSharedMemoryReader code
                for (int i = 1; i < shared.accChief.vehicle.Length; i++)
                {
                    accVehicleInfo participantStruct = shared.accChief.vehicle[i];
                    if (participantStruct.isConnected == 1)
                    {
                        String participantName = participantStruct.driverName.ToLower();
                        if (i != 0 && participantName != null && participantName.Length > 0)
                        {
                            CarData.CarClass opponentCarClass = CarData.getCarClassForClassName(participantStruct.carModel);
                            addOpponentForName(participantName, createOpponentData(participantStruct, false, opponentCarClass, shared.accChief.trackLength, false), currentGameState);
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("Player is using car class " + currentGameState.carClass.getClassIdentifier());
                Utilities.TraceEventClass(currentGameState);

                currentGameState.SessionData.PlayerLapTimeSessionBest = -1;
                currentGameState.SessionData.OpponentsLapTimeSessionBestOverall = -1;
                currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass = -1;
                currentGameState.SessionData.OverallSessionBestLapTime = -1;
                currentGameState.SessionData.PlayerClassSessionBestLapTime = -1;
                TrackDataContainer tdc = TrackData.TRACK_LANDMARKS_DATA.getTrackDataForTrackName(currentGameState.SessionData.TrackDefinition.name, shared.accChief.trackLength);
                currentGameState.SessionData.TrackDefinition.trackLandmarks = tdc.trackLandmarks;
                currentGameState.SessionData.TrackDefinition.isOval = tdc.isOval;
                currentGameState.SessionData.TrackDefinition.setGapPoints();
                GlobalBehaviourSettings.UpdateFromTrackDefinition(currentGameState.SessionData.TrackDefinition);
                if (previousGameState != null && previousGameState.SessionData.TrackDefinition != null)
                {
                    if (previousGameState.SessionData.TrackDefinition.name.Equals(currentGameState.SessionData.TrackDefinition.name))
                    {
                        if (previousGameState.hardPartsOnTrackData.hardPartsMapped)
                        {
                            currentGameState.hardPartsOnTrackData = previousGameState.hardPartsOnTrackData;
                        }
                    }
                }
                currentGameState.SessionData.DeltaTime = new DeltaTime(currentGameState.SessionData.TrackDefinition.trackLength, distanceRoundTrack, currentGameState.Now);
            }
            else
            {
                if (lastSessionPhase != currentGameState.SessionData.SessionPhase)
                {
                    if (currentGameState.SessionData.SessionPhase == SessionPhase.Green || currentGameState.SessionData.SessionPhase == SessionPhase.FullCourseYellow)
                    {
                        // just gone green, so get the session data.
                        if (currentGameState.SessionData.SessionType == SessionType.Race)
                        {
                            currentGameState.SessionData.JustGoneGreen = true;
                            if (currentGameState.SessionData.SessionHasFixedTime)
                            {
                                currentGameState.SessionData.SessionTotalRunTime = sessionTimeRemaining;
                                currentGameState.SessionData.SessionTimeRemaining = sessionTimeRemaining;
                                if (currentGameState.SessionData.SessionTotalRunTime == 0)
                                {
                                    System.Diagnostics.Debug.WriteLine("Setting session run time to 0");
                                }
                            }
                            currentGameState.SessionData.SessionStartTime = currentGameState.Now;
                            currentGameState.SessionData.SessionNumberOfLaps = numberOfLapsInSession;
                        }
                        lapCountAtSector1End = -1;
                        currentGameState.SessionData.LeaderHasFinishedRace = false;
                        currentGameState.SessionData.NumCarsOverallAtStartOfSession = shared.accChief.vehicle.Length;
                        currentGameState.SessionData.TrackDefinition = TrackData.getTrackDefinition(shared.accStatic.track + ":" + shared.accStatic.trackConfiguration, shared.accChief.trackLength, shared.accStatic.sectorCount);
                        if (currentGameState.SessionData.TrackDefinition.unknownTrack)
                        {
                            currentGameState.SessionData.TrackDefinition.setSectorPointsForUnknownTracks();
                        }

                        if (currentGameState.SessionData.TrackDefinition.sectorsOnTrack < shared.accStatic.sectorCount)
                        {
                            Console.WriteLine("Track definition has " + shared.accStatic.sectorCount +
                                " sectors - these will be combined into " + currentGameState.SessionData.TrackDefinition.sectorsOnTrack + " equal sectors");
                        }

                        TrackDataContainer tdc = TrackData.TRACK_LANDMARKS_DATA.getTrackDataForTrackName(currentGameState.SessionData.TrackDefinition.name, shared.accChief.trackLength);
                        currentGameState.SessionData.TrackDefinition.trackLandmarks = tdc.trackLandmarks;
                        currentGameState.SessionData.TrackDefinition.isOval = tdc.isOval;
                        currentGameState.SessionData.TrackDefinition.setGapPoints();
                        GlobalBehaviourSettings.UpdateFromTrackDefinition(currentGameState.SessionData.TrackDefinition);
                        if (previousGameState != null && previousGameState.SessionData.TrackDefinition != null)
                        {
                            if (previousGameState.SessionData.TrackDefinition.name.Equals(currentGameState.SessionData.TrackDefinition.name))
                            {
                                if (previousGameState.hardPartsOnTrackData.hardPartsMapped)
                                {
                                    currentGameState.hardPartsOnTrackData = previousGameState.hardPartsOnTrackData;
                                }
                            }
                        }
                        currentGameState.SessionData.DeltaTime = new DeltaTime(currentGameState.SessionData.TrackDefinition.trackLength, distanceRoundTrack, currentGameState.Now);

                        currentGameState.carClass = CarData.getCarClassForClassName(shared.accStatic.carModel);
                        CarData.CLASS_ID = shared.accStatic.carModel;
                        GlobalBehaviourSettings.UpdateFromCarClass(currentGameState.carClass);
                        System.Diagnostics.Debug.WriteLine("Player is using car class " + currentGameState.carClass.getClassIdentifier());
                        brakeTempThresholdsForPlayersCar = CarData.getBrakeTempThresholds(currentGameState.carClass);
                        // no tyre data in the block so get the default tyre types for this car
                        defaultTyreTypeForPlayersCar = CarData.getDefaultTyreType(currentGameState.carClass);

                        currentGameState.TyreData.TyreTypeName = shared.accGraphic.tyreCompound;
                        tyreTempThresholds = getTyreTempThresholds(currentGameState.carClass, currentGameState.TyreData.TyreTypeName);

                        currentGameState.PitData.HasMandatoryPitStop = shared.accStatic.PitWindowStart < shared.accStatic.PitWindowEnd && (shared.accStatic.PitWindowStart > 0 || shared.accStatic.PitWindowEnd > 0);
                        if (currentGameState.SessionData.SessionHasFixedTime)
                        {
                            currentGameState.PitData.PitWindowStart = shared.accStatic.PitWindowStart / 60000f;
                            currentGameState.PitData.PitWindowEnd = shared.accStatic.PitWindowEnd / 60000f;
                        }
                        else
                        {
                            currentGameState.PitData.PitWindowStart = shared.accStatic.PitWindowStart - 1;
                            currentGameState.PitData.PitWindowEnd = shared.accStatic.PitWindowEnd - 1;
                        }

                        if (previousGameState != null)
                        {
                            currentGameState.OpponentData = previousGameState.OpponentData;
                            currentGameState.PitData.IsRefuellingAllowed = previousGameState.PitData.IsRefuellingAllowed;
                            if (currentGameState.SessionData.SessionType != SessionType.Race)
                            {
                                currentGameState.SessionData.SessionStartTime = previousGameState.SessionData.SessionStartTime;
                                currentGameState.SessionData.SessionTotalRunTime = previousGameState.SessionData.SessionTotalRunTime;
                                currentGameState.SessionData.SessionTimeRemaining = previousGameState.SessionData.SessionTimeRemaining;
                                currentGameState.SessionData.SessionNumberOfLaps = previousGameState.SessionData.SessionNumberOfLaps;
                            }
                        }

                        System.Diagnostics.Debug.WriteLine("Just gone green, session details...");
                        System.Diagnostics.Debug.WriteLine("SessionType " + currentGameState.SessionData.SessionType);
                        System.Diagnostics.Debug.WriteLine("SessionPhase " + currentGameState.SessionData.SessionPhase);
                        if (previousGameState != null)
                        {
                            System.Diagnostics.Debug.WriteLine("previous SessionPhase " + previousGameState.SessionData.SessionPhase);
                        }
                        System.Diagnostics.Debug.WriteLine("EventIndex " + currentGameState.SessionData.EventIndex);
                        System.Diagnostics.Debug.WriteLine("SessionIteration " + currentGameState.SessionData.SessionIteration);
                        System.Diagnostics.Debug.WriteLine("HasMandatoryPitStop " + currentGameState.PitData.HasMandatoryPitStop);
                        System.Diagnostics.Debug.WriteLine("PitWindowStart " + currentGameState.PitData.PitWindowStart);
                        System.Diagnostics.Debug.WriteLine("PitWindowEnd " + currentGameState.PitData.PitWindowEnd);
                        System.Diagnostics.Debug.WriteLine("NumCarsAtStartOfSession " + currentGameState.SessionData.NumCarsOverallAtStartOfSession);
                        System.Diagnostics.Debug.WriteLine("SessionNumberOfLaps " + currentGameState.SessionData.SessionNumberOfLaps);
                        System.Diagnostics.Debug.WriteLine("SessionRunTime " + currentGameState.SessionData.SessionTotalRunTime.ToString("0.000"));
                        System.Diagnostics.Debug.WriteLine("SessionStartTime " + currentGameState.SessionData.SessionStartTime.ToString("0.000"));
                        String trackName = currentGameState.SessionData.TrackDefinition == null ? "unknown" : currentGameState.SessionData.TrackDefinition.name;
                        System.Diagnostics.Debug.WriteLine("TrackName " + trackName);
                    }
                }
                if (!currentGameState.SessionData.JustGoneGreen && previousGameState != null)
                {
                    currentGameState.SessionData.SessionStartTime = previousGameState.SessionData.SessionStartTime;
                    currentGameState.SessionData.SessionTotalRunTime = previousGameState.SessionData.SessionTotalRunTime;
                    currentGameState.SessionData.SessionNumberOfLaps = previousGameState.SessionData.SessionNumberOfLaps;
                    currentGameState.SessionData.HasExtraLap = previousGameState.SessionData.HasExtraLap;
                    currentGameState.SessionData.NumCarsOverallAtStartOfSession = previousGameState.SessionData.NumCarsOverallAtStartOfSession;
                    currentGameState.SessionData.TrackDefinition = previousGameState.SessionData.TrackDefinition;
                    currentGameState.SessionData.EventIndex = previousGameState.SessionData.EventIndex;
                    currentGameState.SessionData.SessionIteration = previousGameState.SessionData.SessionIteration;
                    currentGameState.SessionData.PositionAtStartOfCurrentLap = previousGameState.SessionData.PositionAtStartOfCurrentLap;
                    currentGameState.SessionData.SessionStartClassPosition = previousGameState.SessionData.SessionStartClassPosition;
                    currentGameState.SessionData.ClassPositionAtStartOfCurrentLap = previousGameState.SessionData.ClassPositionAtStartOfCurrentLap;
                    currentGameState.SessionData.CompletedLaps = previousGameState.SessionData.CompletedLaps;

                    currentGameState.OpponentData = previousGameState.OpponentData;
                    currentGameState.PitData.PitWindowStart = previousGameState.PitData.PitWindowStart;
                    currentGameState.PitData.PitWindowEnd = previousGameState.PitData.PitWindowEnd;
                    currentGameState.PitData.HasMandatoryPitStop = previousGameState.PitData.HasMandatoryPitStop;
                    currentGameState.PitData.HasMandatoryTyreChange = previousGameState.PitData.HasMandatoryTyreChange;
                    currentGameState.PitData.MandatoryTyreChangeRequiredTyreType = previousGameState.PitData.MandatoryTyreChangeRequiredTyreType;
                    currentGameState.PitData.IsRefuellingAllowed = previousGameState.PitData.IsRefuellingAllowed;
                    currentGameState.PitData.MaxPermittedDistanceOnCurrentTyre = previousGameState.PitData.MaxPermittedDistanceOnCurrentTyre;
                    currentGameState.PitData.MinPermittedDistanceOnCurrentTyre = previousGameState.PitData.MinPermittedDistanceOnCurrentTyre;
                    currentGameState.PitData.OnInLap = previousGameState.PitData.OnInLap;
                    currentGameState.PitData.OnOutLap = previousGameState.PitData.OnOutLap;
                    // the other properties of PitData are updated each tick, and shouldn't be copied over here. Nasty...
                    currentGameState.SessionData.SessionTimesAtEndOfSectors = previousGameState.SessionData.SessionTimesAtEndOfSectors;
                    currentGameState.PenaltiesData.CutTrackWarnings = previousGameState.PenaltiesData.CutTrackWarnings;
                    currentGameState.PenaltiesData.NumPenalties = previousGameState.PenaltiesData.NumPenalties;
                    currentGameState.SessionData.formattedPlayerLapTimes = previousGameState.SessionData.formattedPlayerLapTimes;
                    currentGameState.SessionData.GameTimeAtLastPositionFrontChange = previousGameState.SessionData.GameTimeAtLastPositionFrontChange;
                    currentGameState.SessionData.GameTimeAtLastPositionBehindChange = previousGameState.SessionData.GameTimeAtLastPositionBehindChange;
                    currentGameState.SessionData.LastSector1Time = previousGameState.SessionData.LastSector1Time;
                    currentGameState.SessionData.LastSector2Time = previousGameState.SessionData.LastSector2Time;
                    currentGameState.SessionData.LastSector3Time = previousGameState.SessionData.LastSector3Time;
                    currentGameState.SessionData.PlayerBestSector1Time = previousGameState.SessionData.PlayerBestSector1Time;
                    currentGameState.SessionData.PlayerBestSector2Time = previousGameState.SessionData.PlayerBestSector2Time;
                    currentGameState.SessionData.PlayerBestSector3Time = previousGameState.SessionData.PlayerBestSector3Time;
                    currentGameState.SessionData.PlayerBestLapSector1Time = previousGameState.SessionData.PlayerBestLapSector1Time;
                    currentGameState.SessionData.PlayerBestLapSector2Time = previousGameState.SessionData.PlayerBestLapSector2Time;
                    currentGameState.SessionData.PlayerBestLapSector3Time = previousGameState.SessionData.PlayerBestLapSector3Time;
                    currentGameState.SessionData.LapTimePrevious = previousGameState.SessionData.LapTimePrevious;
                    currentGameState.Conditions.samples = previousGameState.Conditions.samples;
                    currentGameState.SessionData.trackLandmarksTiming = previousGameState.SessionData.trackLandmarksTiming;
                    currentGameState.TyreData.TyreTypeName = previousGameState.TyreData.TyreTypeName;

                    currentGameState.SessionData.DeltaTime = previousGameState.SessionData.DeltaTime;
                    currentGameState.hardPartsOnTrackData = previousGameState.hardPartsOnTrackData;

                    currentGameState.SessionData.PlayerLapData = previousGameState.SessionData.PlayerLapData;

                    currentGameState.TimingData = previousGameState.TimingData;

                    currentGameState.SessionData.JustGoneGreenTime = previousGameState.SessionData.JustGoneGreenTime;
                }

                //------------------- Variable session data ---------------------------

                if (currentGameState.SessionData.SessionHasFixedTime)
                {
                    // when the race timer reaches zero, this will make SessionRunningTime 'stick' at the SessionTotalRunTime
                    if (sessionTimeRemaining == 0)
                    {
                        currentGameState.SessionData.SessionRunningTime = (float)(currentGameState.Now - currentGameState.SessionData.SessionStartTime).TotalSeconds;
                    }
                    else
                    {
                        currentGameState.SessionData.SessionRunningTime = currentGameState.SessionData.SessionTotalRunTime - sessionTimeRemaining;
                    }
                    currentGameState.SessionData.SessionTimeRemaining = sessionTimeRemaining;
                }
                else
                {
                    currentGameState.SessionData.SessionRunningTime = (float)(currentGameState.Now - currentGameState.SessionData.SessionStartTime).TotalSeconds;
                }

                currentGameState.SessionData.SectorNumber = shared.accGraphic.currentSectorIndex + 1;

                if (currentGameState.SessionData.OverallPosition == 1)
                {
                    currentGameState.SessionData.LeaderSectorNumber = currentGameState.SessionData.SectorNumber;
                }
                currentGameState.SessionData.IsNewSector = previousGameState == null || currentGameState.SessionData.SectorNumber != previousGameState.SessionData.SectorNumber;

                //if(currentGameState.SessionData.IsNewSector)
                //    if (shared.accChief.vehicle != null && shared.accChief.vehicle.Length > 0)
                //        System.Diagnostics.Debug.WriteLine("Sector: " + currentGameState.SessionData.SectorNumber + " Track: " + shared.accChief.trackLength + " Pos: " + (shared.accChief.trackLength * shared.accChief.vehicle[0].spLineLength));

                if (previousGameState != null && currentGameState.SessionData.IsNewSector && previousGameState.SessionData.SectorNumber == 1)
                {
                    lapCountAtSector1End = shared.accGraphic.completedLaps;
                    // belt & braces, just in case we never had 'new lap data' so never updated the lap count on crossing the line
                    currentGameState.SessionData.CompletedLaps = lapCountAtSector1End;
                }
                currentGameState.SessionData.LapTimeCurrent = mapToFloatTime(shared.accGraphic.iCurrentTime);

                if (previousGameState != null && previousGameState.SessionData.CurrentLapIsValid /*&& shared.acsStatic.penaltiesEnabled == 1*/)
                {
                    currentGameState.SessionData.CurrentLapIsValid = shared.accPhysics.numberOfTyresOut < 3;
                }
                bool hasCrossedSFLine = currentGameState.SessionData.IsNewSector && currentGameState.SessionData.SectorNumber == 1;
                float lastLapTime = mapToFloatTime(shared.accGraphic.iLastTime);
                currentGameState.SessionData.IsNewLap = currentGameState.HasNewLapData(previousGameState, lastLapTime, hasCrossedSFLine)
                    || ((lastSessionPhase == SessionPhase.Countdown)
                    && (currentGameState.SessionData.SessionPhase == SessionPhase.Green || currentGameState.SessionData.SessionPhase == SessionPhase.FullCourseYellow));

                if (currentGameState.SessionData.IsNewLap)
                {
                    currentGameState.readLandmarksForThisLap = false;
                    // correct IsNewSector so it's in sync with IsNewLap
                    currentGameState.SessionData.IsNewSector = true;
                    // if we have new lap data, update the lap count using the laps completed at sector1 end + 1, or the game provided data (whichever is bigger)
                    currentGameState.SessionData.CompletedLaps = Math.Max(lapCountAtSector1End + 1, shared.accGraphic.completedLaps);

                    currentGameState.SessionData.playerCompleteLapWithProvidedLapTime(currentGameState.SessionData.OverallPosition,
                        currentGameState.SessionData.SessionRunningTime,
                        lastLapTime, currentGameState.SessionData.CurrentLapIsValid,
                        currentGameState.PitData.InPitlane,
                        shared.accChief.rainLevel > 0.05,
                        shared.accPhysics.roadTemp,
                        shared.accPhysics.airTemp,
                        currentGameState.SessionData.SessionHasFixedTime,
                        currentGameState.SessionData.SessionTimeRemaining,
                        ACCGameStateMapper.numberOfSectorsOnTrack,
                        currentGameState.TimingData);
                    currentGameState.SessionData.playerStartNewLap(currentGameState.SessionData.CompletedLaps + 1,
                        currentGameState.SessionData.OverallPosition, currentGameState.PitData.InPitlane, currentGameState.SessionData.SessionRunningTime);
                }
                else if (previousGameState != null && currentGameState.SessionData.SectorNumber == 1 && currentGameState.SessionData.IsNewSector && previousGameState.SessionData.SectorNumber != 0)
                {
                    // don't allow IsNewSector to be true if IsNewLap is not - roll back to the previous sector number and correct the flag
                    currentGameState.SessionData.SectorNumber = previousGameState.SessionData.SectorNumber;
                    currentGameState.SessionData.IsNewSector = false;
                }

                //Sector
                if (previousGameState != null && currentGameState.SessionData.IsNewSector && !currentGameState.SessionData.IsNewLap &&
                    previousGameState.SessionData.SectorNumber != 0 && currentGameState.SessionData.SessionRunningTime > 10)
                {
                    currentGameState.SessionData.playerAddCumulativeSectorData(
                        previousGameState.SessionData.SectorNumber,
                        currentGameState.SessionData.OverallPosition,
                        currentGameState.SessionData.LapTimeCurrent,
                        currentGameState.SessionData.SessionRunningTime,
                        currentGameState.SessionData.CurrentLapIsValid,
                        shared.accChief.rainLevel > 0.05,
                        shared.accPhysics.roadTemp,
                        shared.accPhysics.airTemp);
                }

                currentGameState.SessionData.Flag = mapToFlagEnum(currentFlag, false);
                /*if (currentGameState.SessionData.Flag == FlagEnum.YELLOW && previousGameState != null && previousGameState.SessionData.Flag != FlagEnum.YELLOW)
                {
                    currentGameState.SessionData.YellowFlagStartTime = currentGameState.Now;
                }*/
                currentGameState.SessionData.NumCarsOverall = shared.accChief.vehicle.Length;

                /*previousGameState != null && previousGameState.SessionData.IsNewLap == false &&
                    (shared.acsGraphic.completedLaps == previousGameState.SessionData.CompletedLaps + 1 || ((lastSessionPhase == SessionPhase.Countdown)
                    && (currentGameState.SessionData.SessionPhase == SessionPhase.Green || currentGameState.SessionData.SessionPhase == SessionPhase.FullCourseYellow)));
                */
                if (previousGameState != null)
                {
                    String stoppedInLandmark = currentGameState.SessionData.trackLandmarksTiming.updateLandmarkTiming(currentGameState.SessionData.TrackDefinition,
                        currentGameState.SessionData.SessionRunningTime, previousGameState.PositionAndMotionData.DistanceRoundTrack, distanceRoundTrack, playerVehicle.speedMS);
                    currentGameState.SessionData.stoppedInLandmark = shared.accGraphic.isInPitLane == 1 ? null : stoppedInLandmark;
                    if (currentGameState.SessionData.IsNewLap)
                    {
                        currentGameState.SessionData.trackLandmarksTiming.cancelWaitingForLandmarkEnd();
                    }
                }

                currentGameState.SessionData.DeltaTime.SetNextDeltaPoint(distanceRoundTrack, currentGameState.SessionData.CompletedLaps, playerVehicle.speedMS, currentGameState.Now);


                if (currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass == -1 ||
                    currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass > mapToFloatTime(playerVehicle.bestLapMS))
                {
                    currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass = mapToFloatTime(playerVehicle.bestLapMS);
                }

                HashSet<string> driversWhoMayHaveDisconnected = new HashSet<string>();
                if (previousGameState != null)
                {
                    // start with the entire opponent set on the previous tick:
                    driversWhoMayHaveDisconnected.UnionWith(previousGameState.OpponentData.Keys);
                }

                for (int i = 1; i < shared.accChief.vehicle.Length; i++)
                {
                    accVehicleInfo participantStruct = shared.accChief.vehicle[i];

                    String participantName = participantStruct.driverName.ToLower();
                    OpponentData currentOpponentData = getOpponentForName(currentGameState, participantName);

                    if (participantName != null && participantName.Length > 0)
                    {
                        // there's a driver in the game data so remove him from the set who may have left the game
                        driversWhoMayHaveDisconnected.Remove(participantName);
                        opponentDisconnectionCounter[participantName] = 0;

                        if (currentOpponentData != null)
                        {
                            currentOpponentData.IsReallyDisconnectedCounter = 0;
                            if (previousGameState != null)
                            {
                                int previousOpponentSectorNumber = 1;
                                int previousOpponentCompletedLaps = 0;
                                int previousOpponentPosition = 0;
                                int currentOpponentSector = 0;
                                Boolean previousOpponentIsEnteringPits = false;
                                Boolean previousOpponentIsExitingPits = false;
                                /* previous tick data for hasNewLapData check*/
                                Boolean previousOpponentDataWaitingForNewLapData = false;
                                DateTime previousOpponentNewLapDataTimerExpiry = DateTime.MaxValue;
                                float previousOpponentLastLapTime = -1;
                                Boolean previousOpponentLastLapValid = false;

                                float[] previousOpponentWorldPosition = new float[] { 0, 0, 0 };
                                float previousOpponentSpeed = 0;
                                float previousDistanceRoundTrack = 0;
                                int currentOpponentRacePosition = 0;
                                OpponentData previousOpponentData = getOpponentForName(previousGameState, participantName);
                                int previousCompletedLapsWhenHasNewLapDataWasLastTrue = -2;
                                float previousOpponentGameTimeWhenLastCrossedStartFinishLine = -1;
                                // store some previous opponent data that we'll need later
                                if (previousOpponentData != null)
                                {
                                    previousOpponentSectorNumber = previousOpponentData.CurrentSectorNumber;
                                    previousOpponentCompletedLaps = previousOpponentData.CompletedLaps;
                                    previousOpponentPosition = previousOpponentData.OverallPosition;
                                    previousOpponentIsEnteringPits = previousOpponentData.isEnteringPits();
                                    previousOpponentIsExitingPits = previousOpponentData.isExitingPits();
                                    previousOpponentWorldPosition = previousOpponentData.WorldPosition;
                                    previousOpponentSpeed = previousOpponentData.Speed;
                                    previousDistanceRoundTrack = previousOpponentData.DistanceRoundTrack;

                                    previousOpponentDataWaitingForNewLapData = previousOpponentData.WaitingForNewLapData;
                                    previousOpponentNewLapDataTimerExpiry = previousOpponentData.NewLapDataTimerExpiry;
                                    previousCompletedLapsWhenHasNewLapDataWasLastTrue = previousOpponentData.CompletedLapsWhenHasNewLapDataWasLastTrue;
                                    previousOpponentGameTimeWhenLastCrossedStartFinishLine = previousOpponentData.GameTimeWhenLastCrossedStartFinishLine;

                                    previousOpponentLastLapTime = previousOpponentData.LastLapTime;
                                    previousOpponentLastLapValid = previousOpponentData.LastLapValid;
                                    currentOpponentData.ClassPositionAtPreviousTick = previousOpponentData.ClassPosition;
                                    currentOpponentData.OverallPositionAtPreviousTick = previousOpponentData.OverallPosition;
                                }
                                float currentOpponentLapDistance = spLineLengthToDistanceRoundTrack(shared.accChief.trackLength, participantStruct.spLineLength);
                                currentOpponentSector = getCurrentSector(currentGameState.SessionData.TrackDefinition, currentOpponentLapDistance);

                                currentOpponentData.DeltaTime.SetNextDeltaPoint(currentOpponentLapDistance, participantStruct.lapCount,
                                    participantStruct.speedMS, currentGameState.Now);

                                int currentOpponentLapsCompleted = participantStruct.lapCount;

                                Boolean finishedAllottedRaceLaps = false;
                                Boolean finishedAllottedRaceTime = false;
                                if (currentGameState.SessionData.SessionType == SessionType.Race)
                                {
                                    if (!currentGameState.SessionData.SessionHasFixedTime)
                                    {
                                        // Using same approach here as in R3E
                                        finishedAllottedRaceLaps = currentGameState.SessionData.SessionNumberOfLaps > 0 && currentGameState.SessionData.SessionNumberOfLaps == currentOpponentLapsCompleted;
                                    }
                                    else
                                    {
                                        if (currentGameState.SessionData.HasExtraLap)
                                        {
                                            if (currentGameState.SessionData.SessionTotalRunTime > 0 && currentGameState.SessionData.SessionTimeRemaining <= 0 &&
                                                previousOpponentCompletedLaps < currentOpponentLapsCompleted)
                                            {
                                                if (!currentOpponentData.HasStartedExtraLap)
                                                {
                                                    currentOpponentData.HasStartedExtraLap = true;
                                                }
                                                else
                                                {
                                                    finishedAllottedRaceTime = true;
                                                }
                                            }
                                        }
                                        else if (currentGameState.SessionData.SessionTotalRunTime > 0 && currentGameState.SessionData.SessionTimeRemaining <= 0 &&
                                            previousOpponentCompletedLaps < currentOpponentLapsCompleted)
                                        {
                                            finishedAllottedRaceTime = true;
                                        }
                                    }
                                }

                                if (useLeaderboardPosition || finishedAllottedRaceLaps || finishedAllottedRaceTime)
                                {
                                    currentOpponentRacePosition = participantStruct.carLeaderboardPosition;
                                }
                                else
                                {
                                    // realtime position in race sessions tends to be more accurate than leaderboard position so we always use it.
                                    // However, if this car is close to the track spline zero point then we can't trust the calculation
                                    if (previousOpponentPosition > 0 && (participantStruct.spLineLength > ACCGameStateMapper.startOfShitSplinePoint || participantStruct.spLineLength < ACCGameStateMapper.endOfShitSplinePoint))
                                    {
                                        currentOpponentRacePosition = previousOpponentPosition;
                                    }
                                    else
                                    {
                                        currentOpponentRacePosition = participantStruct.carRealTimeLeaderboardPosition;
                                    }
                                }


                                if (currentOpponentRacePosition == 1 && (finishedAllottedRaceTime || finishedAllottedRaceLaps))
                                {
                                    currentGameState.SessionData.LeaderHasFinishedRace = true;
                                }

                                Boolean isEnteringPits = participantStruct.isCarInPitline == 1 && currentOpponentSector == ACCGameStateMapper.numberOfSectorsOnTrack;
                                Boolean isLeavingPits = participantStruct.isCarInPitline == 1 && currentOpponentSector == 1;

                                float secondsSinceLastUpdate = (float)new TimeSpan(currentGameState.Ticks - previousGameState.Ticks).TotalSeconds;

                                upateOpponentData(currentOpponentData,
                                    previousOpponentData,
                                    currentOpponentRacePosition,
                                    currentOpponentLapsCompleted,
                                    currentOpponentSector,
                                    mapToFloatTime(participantStruct.currentLapTimeMS),
                                    mapToFloatTime(participantStruct.lastLapTimeMS),
                                    participantStruct.isCarInPitline == 1,
                                    participantStruct.currentLapInvalid == 0,
                                    currentGameState.SessionData.SessionRunningTime,
                                    secondsSinceLastUpdate,
                                    new float[] { participantStruct.worldPosition.x, participantStruct.worldPosition.z },
                                    participantStruct.speedMS,
                                    currentOpponentLapDistance,
                                    currentGameState.SessionData.SessionHasFixedTime,
                                    currentGameState.SessionData.SessionTimeRemaining,
                                    shared.accPhysics.airTemp,
                                    shared.accPhysics.roadTemp,
                                    currentGameState.SessionData.SessionType == SessionType.Race,
                                    currentGameState.SessionData.TrackDefinition.distanceForNearPitEntryChecks,
                                    previousOpponentCompletedLaps,
                                    previousOpponentDataWaitingForNewLapData,
                                    previousOpponentNewLapDataTimerExpiry,
                                    previousOpponentLastLapTime,
                                    previousOpponentLastLapValid,
                                    previousCompletedLapsWhenHasNewLapDataWasLastTrue,
                                    previousOpponentGameTimeWhenLastCrossedStartFinishLine,
                                    currentGameState.TimingData,
                                    currentGameState.carClass,
                                    participantStruct.raceNumber);

                                if (previousOpponentData != null)
                                {
                                    currentOpponentData.trackLandmarksTiming = previousOpponentData.trackLandmarksTiming;
                                    String stoppedInLandmark = currentOpponentData.trackLandmarksTiming.updateLandmarkTiming(
                                        currentGameState.SessionData.TrackDefinition, currentGameState.SessionData.SessionRunningTime,
                                        previousDistanceRoundTrack, currentOpponentData.DistanceRoundTrack, currentOpponentData.Speed);
                                    currentOpponentData.stoppedInLandmark = participantStruct.isCarInPitline == 1 ? null : stoppedInLandmark;
                                }
                                if (currentGameState.SessionData.JustGoneGreen)
                                {
                                    currentOpponentData.trackLandmarksTiming = new TrackLandmarksTiming();
                                }
                                if (currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass == -1 ||
                                        currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass > mapToFloatTime(participantStruct.bestLapMS))
                                {
                                    currentGameState.SessionData.SessionFastestLapTimeFromGamePlayerClass = mapToFloatTime(participantStruct.bestLapMS);
                                }
                                if (currentOpponentData.IsNewLap)
                                {
                                    currentOpponentData.trackLandmarksTiming.cancelWaitingForLandmarkEnd();
                                    if (currentOpponentData.CurrentBestLapTime > 0)
                                    {
                                        if (currentGameState.SessionData.OpponentsLapTimeSessionBestOverall == -1 ||
                                            currentOpponentData.CurrentBestLapTime < currentGameState.SessionData.OpponentsLapTimeSessionBestOverall)
                                        {
                                            currentGameState.SessionData.OpponentsLapTimeSessionBestOverall = currentOpponentData.CurrentBestLapTime;
                                            if (currentGameState.SessionData.OverallSessionBestLapTime == -1 ||
                                                currentGameState.SessionData.OverallSessionBestLapTime > currentOpponentData.CurrentBestLapTime)
                                            {
                                                currentGameState.SessionData.OverallSessionBestLapTime = currentOpponentData.CurrentBestLapTime;
                                            }
                                        }
                                        if (CarData.IsCarClassEqual(currentOpponentData.CarClass, currentGameState.carClass, true))
                                        {
                                            if (currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass == -1 ||
                                                currentOpponentData.CurrentBestLapTime < currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass)
                                            {
                                                currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass = currentOpponentData.CurrentBestLapTime;
                                                if (currentGameState.SessionData.PlayerClassSessionBestLapTime == -1 ||
                                                    currentGameState.SessionData.PlayerClassSessionBestLapTime > currentOpponentData.CurrentBestLapTime)
                                                {
                                                    currentGameState.SessionData.PlayerClassSessionBestLapTime = currentOpponentData.CurrentBestLapTime;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (participantName != null && participantName.Length > 0)
                        {
                            addOpponentForName(participantName, createOpponentData(participantStruct,
                                true,
                                CarData.getCarClassForClassName(participantStruct.carModel),
                                shared.accChief.trackLength,
                                currentGameState.SessionData.SessionType == SessionType.Race),
                                currentGameState);
                        }                        
                    }
                }

                // now process the disconnected drivers
                foreach (String name in driversWhoMayHaveDisconnected)
                {
                    int disconnectionCount;
                    if (opponentDisconnectionCounter.TryGetValue(name, out disconnectionCount))
                    {
                        if (disconnectionCount > 4)
                        {
                            // this guy has been missing for 5 or more ticks, so we assume he's gone and remove him
                            currentGameState.OpponentData.Remove(name);
                            opponentDisconnectionCounter.Remove(name);
                        }
                        else
                        {
                            opponentDisconnectionCounter[name] = disconnectionCount + 1;
                        }
                    }
                    else
                    {
                        opponentDisconnectionCounter.Add(name, 1);
                    }
                }

                currentGameState.sortClassPositions();
                currentGameState.setPracOrQualiDeltas();
            }

            // engine/transmission data
            currentGameState.EngineData.EngineRpm = shared.accPhysics.rpms;
            currentGameState.EngineData.MaxEngineRpm = shared.accStatic.maxRpm;
            currentGameState.EngineData.MinutesIntoSessionBeforeMonitoring = 2;

            currentGameState.FuelData.FuelCapacity = shared.accStatic.maxFuel;
            currentGameState.FuelData.FuelLeft = shared.accPhysics.fuel;
            currentGameState.FuelData.FuelUseActive = shared.accStatic.aidFuelRate > 0;

            currentGameState.TransmissionData.Gear = shared.accPhysics.gear - 1;

            currentGameState.ControlData.BrakePedal = shared.accPhysics.brake;
            currentGameState.ControlData.ThrottlePedal = shared.accPhysics.gas;
            currentGameState.ControlData.ClutchPedal = shared.accPhysics.clutch;

            // penalty data
            currentGameState.PenaltiesData.HasDriveThrough = shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_Cutting
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_IgnoredDriverStint
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_PitSpeeding;
            currentGameState.PenaltiesData.HasSlowDown = false;
            currentGameState.PenaltiesData.HasStopAndGo = shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_10_Cutting
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_10_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_20_Cutting
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_20_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_30_Cutting
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_30_PitSpeeding;
            currentGameState.PenaltiesData.HasTimeDeduction = shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_PostRaceTime;
            if (previousGameState != null)
            {
                if ((!previousGameState.PenaltiesData.HasDriveThrough && currentGameState.PenaltiesData.HasDriveThrough)
                    || (!previousGameState.PenaltiesData.HasDriveThrough && currentGameState.PenaltiesData.HasDriveThrough)
                    || (!previousGameState.PenaltiesData.HasDriveThrough && currentGameState.PenaltiesData.HasDriveThrough))
                {
                    currentGameState.PenaltiesData.NumPenalties++;
                }
            }
            if (shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_Disqualified_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_10_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_20_PitSpeeding
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_30_PitSpeeding)
            {
                currentGameState.PenaltiesData.PenaltyCause = PenatiesData.DetailedPenaltyCause.SPEEDING_IN_PITLANE;
            }
            else if (shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_Disqualified_Cutting
                            || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_Cutting
                            || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_10_Cutting
                            || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_20_Cutting
                            || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_StopAndGo_30_Cutting)
            {
                currentGameState.PenaltiesData.PenaltyCause = PenatiesData.DetailedPenaltyCause.CUT_TRACK;
            }
            else if (shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_Disqualified_ExceededDriverStintLimit)
            {
                currentGameState.PenaltiesData.PenaltyCause = PenatiesData.DetailedPenaltyCause.EXCEEDED_TOTAL_DRIVER_STINT_LIMIT;
            }
            else if (shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_Disqualified_IgnoredDriverStint
                || shared.accGraphic.penalty == AC_PENALTY_TYPE.ACC_DriveThrough_IgnoredDriverStint)
            {
                currentGameState.PenaltiesData.PenaltyCause = PenatiesData.DetailedPenaltyCause.EXCEEDED_SINGLE_DRIVER_STINT_LIMIT;
            }
            if (currentGameState.PenaltiesData.HasDriveThrough)
            {
                currentGameState.PenaltiesData.PenaltyType = PenatiesData.DetailedPenaltyType.DRIVE_THROUGH;
            }
            else if (currentGameState.PenaltiesData.HasStopAndGo)
            {
                currentGameState.PenaltiesData.PenaltyType = PenatiesData.DetailedPenaltyType.STOP_AND_GO;
            }

            // motion data
            currentGameState.PositionAndMotionData.CarSpeed = playerVehicle.speedMS;
            currentGameState.PositionAndMotionData.DistanceRoundTrack = distanceRoundTrack;

            currentGameState.SessionData.PlayerCarNr = playerVehicle.raceNumber.ToString();

            //------------------------ Pit stop data -----------------------
            currentGameState.PitData.InPitlane = shared.accGraphic.isInPitLane == 1;

            if (currentGameState.PitData.InPitlane)
            {
                if (previousGameState != null && !previousGameState.PitData.InPitlane)
                {
                    if (currentGameState.SessionData.SessionRunningTime > 30 && currentGameState.SessionData.SessionType == SessionType.Race)
                    {
                        currentGameState.PitData.NumPitStops++;
                    }
                    currentGameState.PitData.OnInLap = true;
                    currentGameState.PitData.OnOutLap = false;
                }
                else if (currentGameState.SessionData.IsNewLap)
                {
                    currentGameState.PitData.OnInLap = false;
                    currentGameState.PitData.OnOutLap = true;
                }
            }
            else if (previousGameState != null && previousGameState.PitData.InPitlane)
            {
                currentGameState.PitData.OnInLap = false;
                currentGameState.PitData.OnOutLap = true;
                currentGameState.PitData.IsAtPitExit = true;
            }
            else if (currentGameState.SessionData.IsNewLap)
            {
                // starting a new lap while not in the pitlane so clear the in / out lap flags
                currentGameState.PitData.OnInLap = false;
                currentGameState.PitData.OnOutLap = false;
            }

            if (previousGameState != null && currentGameState.PitData.OnOutLap && previousGameState.PitData.InPitlane && !currentGameState.PitData.InPitlane)
            {
                currentGameState.PitData.IsAtPitExit = true;
            }

            if (currentGameState.PitData.HasMandatoryPitStop)
            {
                float lapsOrMinutes;
                if (currentGameState.SessionData.SessionHasFixedTime)
                {
                    lapsOrMinutes = (float) Math.Floor(currentGameState.SessionData.SessionRunningTime / 60f);
                }
                else
                {
                    lapsOrMinutes = playerVehicle.lapCount;
                }
                currentGameState.PitData.PitWindow = mapToPitWindow(lapsOrMinutes, currentGameState.PitData.InPitlane,
                    currentGameState.PitData.PitWindowStart, currentGameState.PitData.PitWindowEnd, shared.accGraphic.MandatoryPitDone == 1);
            }
            else
            {
                currentGameState.PitData.PitWindow = PitWindow.Unavailable;
            }

            currentGameState.PitData.IsMakingMandatoryPitStop = (currentGameState.PitData.PitWindow == PitWindow.StopInProgress);
            if (previousGameState != null)
            {
                currentGameState.PitData.MandatoryPitStopCompleted = previousGameState.PitData.MandatoryPitStopCompleted || shared.accGraphic.MandatoryPitDone == 1;
            }

            currentGameState.PitData.DriverStintSecondsRemaining = shared.accGraphic.driverStintTimeLeft * 1000;
            currentGameState.PitData.DriverStintTotalSecondsRemaining = shared.accGraphic.driverStintTotalTimeLeft * 1000;

            //damage data
            if (shared.accChief.isInternalMemoryModuleLoaded == 1)
            {
                currentGameState.CarDamageData.DamageEnabled = true;

                currentGameState.CarDamageData.OverallEngineDamage = DamageLevel.UNKNOWN; // mapToEngineDamageLevel(playerVehicle.engineLifeLeft);

                currentGameState.CarDamageData.OverallAeroDamage = mapToAeroDamageLevel(shared.accPhysics.carDamage[0] +
                    shared.accPhysics.carDamage[1] +
                    shared.accPhysics.carDamage[2] +
                    shared.accPhysics.carDamage[3]);

                playerVehicle.tyreInflation[0] = 1;
                playerVehicle.tyreInflation[1] = 1;
                playerVehicle.tyreInflation[2] = 1;
                playerVehicle.tyreInflation[3] = 1;
            }
            else
            {
                currentGameState.CarDamageData.DamageEnabled = false;
                playerVehicle.tyreInflation[0] = 1;
                playerVehicle.tyreInflation[1] = 1;
                playerVehicle.tyreInflation[2] = 1;
                playerVehicle.tyreInflation[3] = 1;
            }

            //tyre data
            currentGameState.TyreData.HasMatchedTyreTypes = true;
            currentGameState.TyreData.TyreWearActive = shared.accStatic.aidTireRate > 0;

            currentGameState.TyreData.FrontLeftPressure = playerVehicle.tyreInflation[0] == 1.0f ? shared.accPhysics.wheelsPressure[0] * 6.894f : 0.0f;
            currentGameState.TyreData.FrontRightPressure = playerVehicle.tyreInflation[1] == 1.0f ? shared.accPhysics.wheelsPressure[1] * 6.894f : 0.0f;
            currentGameState.TyreData.RearLeftPressure = playerVehicle.tyreInflation[2] == 1.0f ? shared.accPhysics.wheelsPressure[2] * 6.894f : 0.0f;
            currentGameState.TyreData.RearRightPressure = playerVehicle.tyreInflation[3] == 1.0f ? shared.accPhysics.wheelsPressure[3] * 6.894f : 0.0f;

            currentGameState.TyreData.BrakeTempStatus = CornerData.getCornerData(brakeTempThresholdsForPlayersCar, shared.accPhysics.brakeTemp[0], shared.accPhysics.brakeTemp[1], shared.accPhysics.brakeTemp[2], shared.accPhysics.brakeTemp[3]);
            currentGameState.TyreData.LeftFrontBrakeTemp = shared.accPhysics.brakeTemp[0];
            currentGameState.TyreData.RightFrontBrakeTemp = shared.accPhysics.brakeTemp[1];
            currentGameState.TyreData.LeftRearBrakeTemp = shared.accPhysics.brakeTemp[2];
            currentGameState.TyreData.RightRearBrakeTemp = shared.accPhysics.brakeTemp[3];

            String currentTyreCompound = shared.accGraphic.tyreCompound;

            // Only middle tire temperature is available
            if (previousGameState != null && !previousGameState.TyreData.TyreTypeName.Equals(currentTyreCompound))
            {
                tyreTempThresholds = getTyreTempThresholds(currentGameState.carClass, currentTyreCompound);
                currentGameState.TyreData.TyreTypeName = currentTyreCompound;
            }
            //Front Left
            currentGameState.TyreData.FrontLeft_CenterTemp = shared.accPhysics.tyreTempM[0];
            currentGameState.TyreData.FrontLeft_LeftTemp = shared.accPhysics.tyreTempO[0];
            currentGameState.TyreData.FrontLeft_RightTemp = shared.accPhysics.tyreTempI[0];
            currentGameState.TyreData.FrontLeftTyreType = shared.accGraphic.rainTyres == 1 ? TyreType.Wet : defaultTyreTypeForPlayersCar;
            if (currentGameState.SessionData.IsNewLap || currentGameState.TyreData.PeakFrontLeftTemperatureForLap == 0)
            {
                currentGameState.TyreData.PeakFrontLeftTemperatureForLap = currentGameState.TyreData.FrontLeft_CenterTemp;
            }
            else if (previousGameState == null || currentGameState.TyreData.FrontLeft_CenterTemp > previousGameState.TyreData.PeakFrontLeftTemperatureForLap)
            {
                currentGameState.TyreData.PeakFrontLeftTemperatureForLap = currentGameState.TyreData.FrontLeft_CenterTemp;
            }
            //Front Right
            currentGameState.TyreData.FrontRight_CenterTemp = shared.accPhysics.tyreTempM[1];
            currentGameState.TyreData.FrontRight_LeftTemp = shared.accPhysics.tyreTempI[1];
            currentGameState.TyreData.FrontRight_RightTemp = shared.accPhysics.tyreTempO[1];
            currentGameState.TyreData.FrontRightTyreType = shared.accGraphic.rainTyres == 1 ? TyreType.Wet : defaultTyreTypeForPlayersCar;
            if (currentGameState.SessionData.IsNewLap || currentGameState.TyreData.PeakFrontRightTemperatureForLap == 0)
            {
                currentGameState.TyreData.PeakFrontRightTemperatureForLap = currentGameState.TyreData.FrontRight_CenterTemp;
            }
            else if (previousGameState == null || currentGameState.TyreData.FrontRight_CenterTemp > previousGameState.TyreData.PeakFrontRightTemperatureForLap)
            {
                currentGameState.TyreData.PeakFrontRightTemperatureForLap = currentGameState.TyreData.FrontRight_CenterTemp;
            }
            //Rear Left
            currentGameState.TyreData.RearLeft_CenterTemp = shared.accPhysics.tyreTempM[2];
            currentGameState.TyreData.RearLeft_LeftTemp = shared.accPhysics.tyreTempO[2];
            currentGameState.TyreData.RearLeft_RightTemp = shared.accPhysics.tyreTempI[2];
            currentGameState.TyreData.RearLeftTyreType = shared.accGraphic.rainTyres == 1 ? TyreType.Wet : defaultTyreTypeForPlayersCar;
            if (currentGameState.SessionData.IsNewLap || currentGameState.TyreData.PeakRearLeftTemperatureForLap == 0)
            {
                currentGameState.TyreData.PeakRearLeftTemperatureForLap = currentGameState.TyreData.RearLeft_CenterTemp;
            }
            else if (previousGameState == null || currentGameState.TyreData.RearLeft_CenterTemp > previousGameState.TyreData.PeakRearLeftTemperatureForLap)
            {
                currentGameState.TyreData.PeakRearLeftTemperatureForLap = currentGameState.TyreData.RearLeft_CenterTemp;
            }
            //Rear Right
            currentGameState.TyreData.RearRight_CenterTemp = shared.accPhysics.tyreTempM[3];
            currentGameState.TyreData.RearRight_LeftTemp = shared.accPhysics.tyreTempI[3];
            currentGameState.TyreData.RearRight_RightTemp = shared.accPhysics.tyreTempO[3];
            currentGameState.TyreData.RearRightTyreType = shared.accGraphic.rainTyres == 1 ? TyreType.Wet : defaultTyreTypeForPlayersCar;
            if (currentGameState.SessionData.IsNewLap || currentGameState.TyreData.PeakRearRightTemperatureForLap == 0)
            {
                currentGameState.TyreData.PeakRearRightTemperatureForLap = currentGameState.TyreData.RearRight_CenterTemp;
            }
            else if (previousGameState == null || currentGameState.TyreData.RearRight_CenterTemp > previousGameState.TyreData.PeakRearRightTemperatureForLap)
            {
                currentGameState.TyreData.PeakRearRightTemperatureForLap = currentGameState.TyreData.RearRight_CenterTemp;
            }

            currentGameState.TyreData.TyreTempStatus = CornerData.getCornerData(tyreTempThresholds, currentGameState.TyreData.PeakFrontLeftTemperatureForLap,
                    currentGameState.TyreData.PeakFrontRightTemperatureForLap, currentGameState.TyreData.PeakRearLeftTemperatureForLap,
                    currentGameState.TyreData.PeakRearRightTemperatureForLap);


            // TODO: tyre wear is always 0.0 in the shared memory data
            /*Boolean currentTyreValid = currentTyreCompound != null && currentTyreCompound.Length > 0 &&
                acTyres.Count > 0 && acTyres.ContainsKey(currentTyreCompound);

            if (currentTyreValid)
            {
                float currentTyreWearMinimumValue = acTyres[currentTyreCompound].tyreWearMinimumValue;
                currentGameState.TyreData.FrontLeftPercentWear = getTyreWearPercentage(shared.accPhysics.tyreWear[0], currentTyreWearMinimumValue);
                currentGameState.TyreData.FrontRightPercentWear = getTyreWearPercentage(shared.accPhysics.tyreWear[1], currentTyreWearMinimumValue);
                currentGameState.TyreData.RearLeftPercentWear = getTyreWearPercentage(shared.accPhysics.tyreWear[2], currentTyreWearMinimumValue);
                currentGameState.TyreData.RearRightPercentWear = getTyreWearPercentage(shared.accPhysics.tyreWear[3], currentTyreWearMinimumValue);
                if (!currentGameState.PitData.OnOutLap)
                {
                    currentGameState.TyreData.TyreConditionStatus = CornerData.getCornerData(acTyres[currentTyreCompound].tyreWearThresholdsForAC,
                        currentGameState.TyreData.FrontLeftPercentWear, currentGameState.TyreData.FrontRightPercentWear,
                        currentGameState.TyreData.RearLeftPercentWear, currentGameState.TyreData.RearRightPercentWear);
                }
                else
                {
                    currentGameState.TyreData.TyreConditionStatus = CornerData.getCornerData(acTyres[currentTyreCompound].tyreWearThresholdsForAC, -1f, -1f, -1f, -1f);
                }
            }

            var msg = $"Wear: {shared.accPhysics.tyreWear[0].ToString("N2")} : {shared.accPhysics.tyreWear[1].ToString("N2")} : {shared.accPhysics.tyreWear[2].ToString("N2")} : { shared.accPhysics.tyreWear[3].ToString("N2")}";
            if (msgHash.Add(msg.GetHashCode()))
                System.Diagnostics.Debug.WriteLine(msg);

            msg = $"Flag: {shared.accGraphic.flag} : {shared.accGraphic.penaltyTime}";
            if (msgHash.Add(msg.GetHashCode()))
                System.Diagnostics.Debug.WriteLine(msg);
            */

            currentGameState.PenaltiesData.IsOffRacingSurface = shared.accPhysics.numberOfTyresOut > 2;
            if (!currentGameState.PitData.OnOutLap && previousGameState != null && !previousGameState.PenaltiesData.IsOffRacingSurface && currentGameState.PenaltiesData.IsOffRacingSurface &&
                !(shared.accGraphic.session == AC_SESSION_TYPE.AC_RACE && isCountDown))
            {
                currentGameState.PenaltiesData.CutTrackWarnings = previousGameState.PenaltiesData.CutTrackWarnings + 1;
            }

            if (playerVehicle.speedMS > 7 && currentGameState.carClass != null)
            {
                float minRotatingSpeed = (float)Math.PI * playerVehicle.speedMS / currentGameState.carClass.maxTyreCircumference;
                currentGameState.TyreData.LeftFrontIsLocked = Math.Abs(shared.accPhysics.wheelAngularSpeed[0]) < minRotatingSpeed;
                currentGameState.TyreData.RightFrontIsLocked = Math.Abs(shared.accPhysics.wheelAngularSpeed[1]) < minRotatingSpeed;
                currentGameState.TyreData.LeftRearIsLocked = Math.Abs(shared.accPhysics.wheelAngularSpeed[2]) < minRotatingSpeed;
                currentGameState.TyreData.RightRearIsLocked = Math.Abs(shared.accPhysics.wheelAngularSpeed[3]) < minRotatingSpeed;
                
                // '3' here is a magic number - we want to allow the wheel to spin significantly faster than the ideal we we're not always grumbling about it
                float maxRotatingSpeed = 3 * (float)Math.PI * playerVehicle.speedMS / currentGameState.carClass.minTyreCircumference;
                // all the cars are RWD (so far), so don't bother checking front wheelspin
                //currentGameState.TyreData.LeftFrontIsSpinning = Math.Abs(shared.accPhysics.wheelAngularSpeed[0]) > maxRotatingSpeed;
                //currentGameState.TyreData.RightFrontIsSpinning = Math.Abs(shared.accPhysics.wheelAngularSpeed[1]) > maxRotatingSpeed;
                currentGameState.TyreData.LeftRearIsSpinning = Math.Abs(shared.accPhysics.wheelAngularSpeed[2]) > maxRotatingSpeed;
                currentGameState.TyreData.RightRearIsSpinning = Math.Abs(shared.accPhysics.wheelAngularSpeed[3]) > maxRotatingSpeed;
            }

            //conditions
            if (currentGameState.Now > nextConditionsSampleDue)
            {
                nextConditionsSampleDue = currentGameState.Now.Add(ConditionsMonitor.ConditionsSampleFrequency);
                currentGameState.Conditions.addSample(currentGameState.Now, currentGameState.SessionData.CompletedLaps, currentGameState.SessionData.SectorNumber,
                    shared.accPhysics.airTemp, shared.accPhysics.roadTemp, shared.accChief.rainLevel, 0, 0, 0, 0, currentGameState.SessionData.IsNewLap);
            }

            if (currentGameState.SessionData.TrackDefinition != null)
            {
                CrewChief.trackName = currentGameState.SessionData.TrackDefinition.name;
            }
            if (currentGameState.carClass != null)
            {
                CrewChief.carClass = currentGameState.carClass.carClassEnum;
            }
            CrewChief.distanceRoundTrack = currentGameState.PositionAndMotionData.DistanceRoundTrack;
            CrewChief.viewingReplay = false;

            currentGameState.PositionAndMotionData.Orientation.Pitch = shared.accPhysics.pitch;
            currentGameState.PositionAndMotionData.Orientation.Roll = shared.accPhysics.roll;
            currentGameState.PositionAndMotionData.Orientation.Yaw = shared.accPhysics.heading;

            if (currentGameState.SessionData.IsNewLap)
            {
                if (currentGameState.hardPartsOnTrackData.updateHardPartsForNewLap(currentGameState.SessionData.LapTimePrevious))
                {
                    currentGameState.SessionData.TrackDefinition.adjustGapPoints(currentGameState.hardPartsOnTrackData.processedHardPartsForBestLap);
                }
            }
            else if (!currentGameState.PitData.OnOutLap && !currentGameState.SessionData.TrackDefinition.isOval &&
                !(currentGameState.SessionData.SessionType == SessionType.Race &&
                   (currentGameState.SessionData.CompletedLaps < 1 || (GameStateData.useManualFormationLap && currentGameState.SessionData.CompletedLaps < 2))))
            {
                currentGameState.hardPartsOnTrackData.mapHardPartsOnTrack(currentGameState.ControlData.BrakePedal, currentGameState.ControlData.ThrottlePedal,
                    currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.SessionData.CurrentLapIsValid, currentGameState.SessionData.TrackDefinition.trackLength);
            }

            // don't enable improvised incident calling until we can work out how to cull the disconnected drivers from the opponents set
            // before we disable this lets see if it works better with proper purging of disconnected players (note that it defaults to true)
            // currentGameState.FlagData.useImprovisedIncidentCalling = false;
            
            return currentGameState;
        }

        private List<CornerData.EnumWithThresholds> getTyreTempThresholds(CarData.CarClass carClass, string currentTyreCompound)
        {
            List<CornerData.EnumWithThresholds> tyreTempThresholds = new List<CornerData.EnumWithThresholds>();
            CarData.TyreTypeData tyreTypeData = null;
            AcTyres acTyre = null;
            if (carClass.acTyreTypeData.TryGetValue(currentTyreCompound, out tyreTypeData))
            {
                tyreTempThresholds.Add(new CornerData.EnumWithThresholds(TyreTemp.COLD, -10000f, tyreTypeData.maxColdTyreTemp));
                tyreTempThresholds.Add(new CornerData.EnumWithThresholds(TyreTemp.WARM, tyreTypeData.maxColdTyreTemp, tyreTypeData.maxWarmTyreTemp));
                tyreTempThresholds.Add(new CornerData.EnumWithThresholds(TyreTemp.HOT, tyreTypeData.maxWarmTyreTemp, tyreTypeData.maxHotTyreTemp));
                tyreTempThresholds.Add(new CornerData.EnumWithThresholds(TyreTemp.COOKING, tyreTypeData.maxHotTyreTemp, 10000f));
                System.Diagnostics.Debug.WriteLine("Using user defined temperature thresholds for TyreType: " + currentTyreCompound);
            }
            else if (acTyres.TryGetValue(currentTyreCompound, out acTyre))
            {
                tyreTempThresholds = acTyre.tyreTempThresholdsForAC;
                System.Diagnostics.Debug.WriteLine("Using buildin defined temperature thresholds for TyreType: " + currentTyreCompound);
            }
            else
            {
                tyreTempThresholds = CarData.getTyreTempThresholds(carClass);
                System.Diagnostics.Debug.WriteLine("Using temperature thresholds for TyreType: " + carClass.defaultTyreType.ToString() +
                    " maxColdTyreTemp: " + tyreTempThresholds[0].upperThreshold + " maxWarmTyreTemp: " + tyreTempThresholds[1].upperThreshold +
                    " maxHotTyreTemp: " + tyreTempThresholds[2].upperThreshold);
            }
            return tyreTempThresholds;
        }

        private void upateOpponentData(OpponentData opponentData, OpponentData previousOpponentData, int realtimeRacePosition, int completedLaps, int sector,
            float completedLapTime, float lastLapTime, Boolean isInPits, Boolean lapIsValid, float sessionRunningTime, float secondsSinceLastUpdate,
            float[] currentWorldPosition, float speed, float distanceRoundTrack, Boolean sessionLengthIsTime, float sessionTimeRemaining,
            float airTemperature, float trackTempreture, Boolean isRace, float nearPitEntryPointDistance,
            /* previous tick data for hasNewLapData check*/
            int previousOpponentDataLapsCompleted, Boolean previousOpponentDataWaitingForNewLapData,
            DateTime previousOpponentNewLapDataTimerExpiry, float previousOpponentLastLapTime, Boolean previousOpponentLastLapValid,
            int previousCompletedLapsWhenHasNewLapDataWasLastTrue, float previousOpponentGameTimeWhenLastCrossedStartFinishLine,
            TimingData timingData, CarData.CarClass playerCarClass, int raceNumber)
        {
            if (opponentData.CurrentSectorNumber == 0)
            {
                opponentData.CurrentSectorNumber = sector;
            }
            float previousDistanceRoundTrack = opponentData.DistanceRoundTrack;
            opponentData.DistanceRoundTrack = distanceRoundTrack;
            opponentData.CarNumber = raceNumber.ToString();
            Boolean validSpeed = true;
            if (speed > 500)
            {
                // faster than 500m/s (1000+mph) suggests the player has quit to the pit. Might need to reassess this as the data are quite noisy
                validSpeed = false;
                opponentData.Speed = 0;
            }
            opponentData.Speed = speed;
            if (opponentData.OverallPosition != realtimeRacePosition)
            {
                opponentData.SessionTimeAtLastPositionChange = sessionRunningTime;
            }
            opponentData.OverallPosition = realtimeRacePosition;
            if (previousDistanceRoundTrack < nearPitEntryPointDistance && opponentData.DistanceRoundTrack > nearPitEntryPointDistance)
            {
                opponentData.PositionOnApproachToPitEntry = opponentData.OverallPosition;
            }
            opponentData.WorldPosition = currentWorldPosition;
            opponentData.IsNewLap = false;
            opponentData.JustEnteredPits = !opponentData.InPits && isInPits;
            if (opponentData.JustEnteredPits)
            {
                opponentData.NumPitStops++;
            }
            opponentData.InPits = isInPits;

            bool hasCrossedSFline = opponentData.CurrentSectorNumber == ACCGameStateMapper.numberOfSectorsOnTrack && sector == 1;

            bool hasNewLapData = opponentData.HasNewLapData(lastLapTime, hasCrossedSFline, completedLaps, isRace, sessionRunningTime, previousOpponentDataWaitingForNewLapData,
                 previousOpponentNewLapDataTimerExpiry, previousOpponentLastLapTime, previousOpponentLastLapValid, previousCompletedLapsWhenHasNewLapDataWasLastTrue, previousOpponentGameTimeWhenLastCrossedStartFinishLine);

            if (opponentData.CurrentSectorNumber == ACCGameStateMapper.numberOfSectorsOnTrack && sector == ACCGameStateMapper.numberOfSectorsOnTrack && (!lapIsValid || !validSpeed))
            {
                // special case for s3 - need to invalidate lap immediately
                opponentData.InvalidateCurrentLap();
            }
            if (opponentData.CurrentSectorNumber != sector || hasNewLapData)
            {
                if (hasNewLapData)
                {
                    int correctedLapCount = Math.Max(completedLaps, opponentData.lapCountAtSector1End + 1);
                    // if we have new lap data, we must be in sector 1
                    opponentData.CurrentSectorNumber = 1;
                    if (opponentData.OpponentLapData.Count > 0)
                    {
                        // special case here: if there's only 1 lap in the list, and it's marked as an in-lap, and we don't have a laptime, remove it.
                        // This is because we might have created a new LapData entry to hold a partially completed in-lap if we join mid-session, but
                        // this also results in each opponent having a spurious 'empty' LapData element.
                        if (opponentData.OpponentLapData.Count == 1 && opponentData.OpponentLapData[0].InLap && lastLapTime == 0)
                        {
                            opponentData.OpponentLapData.Clear();
                        }
                        else
                        {
                            opponentData.CompleteLapWithProvidedLapTime(realtimeRacePosition, sessionRunningTime, lastLapTime, isInPits,
                                false, trackTempreture, airTemperature, sessionLengthIsTime, sessionTimeRemaining, ACCGameStateMapper.numberOfSectorsOnTrack,
                                timingData, CarData.IsCarClassEqual(opponentData.CarClass, playerCarClass, true));
                        }
                    }

                    opponentData.StartNewLap(correctedLapCount + 1, realtimeRacePosition, isInPits, sessionRunningTime, false, trackTempreture, airTemperature);
                    opponentData.IsNewLap = true;
                    opponentData.CompletedLaps = correctedLapCount;
                    // recheck the car class here?
                }
                else if (((opponentData.CurrentSectorNumber == 1 && sector == 2) || (opponentData.CurrentSectorNumber == 2 && sector == 3)))
                {
                    opponentData.AddCumulativeSectorData(opponentData.CurrentSectorNumber, realtimeRacePosition, completedLapTime, sessionRunningTime,
                        lapIsValid && validSpeed, false, trackTempreture, airTemperature);

                    // if we've just finished sector 1, capture the laps completed (and ensure the CompleteLaps count is up to date)
                    if (opponentData.CurrentSectorNumber == 1)
                    {
                        opponentData.lapCountAtSector1End = completedLaps;
                        opponentData.CompletedLaps = completedLaps;
                    }

                    // only update the sector number if it's one of the above cases. This prevents us from moving the opponent sector number to 1 before
                    // he has new lap data
                    opponentData.CurrentSectorNumber = sector;
                }
            }
            if (sector == ACCGameStateMapper.numberOfSectorsOnTrack && isInPits)
            {
                opponentData.setInLap();
            }
        }

        private OpponentData createOpponentData(accVehicleInfo participantStruct, Boolean loadDriverName, CarData.CarClass carClass, float trackLength, bool raceSessionIsUnderway)
        {
            OpponentData opponentData = new OpponentData();
            String participantName = participantStruct.driverName.ToLower();
            opponentData.DriverRawName = participantName;
            opponentData.DriverNameSet = true;
            // note that in AC, drivers may be added to the session during the race - we don't want to load these driver names
            if (participantName != null && participantName.Length > 0 && loadDriverName && CrewChief.enableDriverNames && !raceSessionIsUnderway)
            {
                if (speechRecogniser != null) speechRecogniser.addNewOpponentName(opponentData.DriverRawName, "-1");
            }

            // when we first create an opponent use the game-provided leadboard position. Subsequent updates will use the realtime position
            opponentData.OverallPosition = participantStruct.carLeaderboardPosition;
            opponentData.CompletedLaps = participantStruct.lapCount;
            opponentData.CurrentSectorNumber = 0;
            opponentData.WorldPosition = new float[] { participantStruct.worldPosition.x, participantStruct.worldPosition.z };
            opponentData.DistanceRoundTrack = spLineLengthToDistanceRoundTrack(trackLength, participantStruct.spLineLength);
            opponentData.DeltaTime = new DeltaTime(trackLength, opponentData.DistanceRoundTrack, DateTime.UtcNow);
            opponentData.CarClass = carClass;
            opponentData.IsActive = true;
            opponentData.CarNumber = participantStruct.raceNumber.ToString();
            String nameToLog = opponentData.DriverRawName == null ? "unknown" : opponentData.DriverRawName;
            System.Diagnostics.Debug.WriteLine("New driver " + nameToLog + " is using car class " + opponentData.CarClass.getClassIdentifier());
            return opponentData;
        }

        public static void addOpponentForName(String name, OpponentData opponentData, GameStateData gameState)
        {
            if (name == null || name.Length == 0)
            {
                return;
            }
            if (gameState.OpponentData == null)
            {
                gameState.OpponentData = new Dictionary<string, OpponentData>();
            }
            gameState.OpponentData.Remove(name);
            gameState.OpponentData.Add(name, opponentData);
        }

        public float mapToFloatTime(int time)
        {
            TimeSpan ts = TimeSpan.FromTicks(time);
            return (float)ts.TotalMilliseconds * 10;
        }

        private FlagEnum mapToFlagEnum(AC_FLAG_TYPE flag, Boolean disableYellowFlag)
        {
            if (flag == AC_FLAG_TYPE.AC_CHECKERED_FLAG)
            {
                return FlagEnum.CHEQUERED;
            }
            else if (flag == AC_FLAG_TYPE.AC_BLACK_FLAG)
            {
                return FlagEnum.BLACK;
            }
            else if (flag == AC_FLAG_TYPE.AC_YELLOW_FLAG)
            {
                if (disableYellowFlag)
                {
                    return FlagEnum.UNKNOWN;
                }
                return FlagEnum.YELLOW;
            }
            else if (flag == AC_FLAG_TYPE.AC_WHITE_FLAG)
            {
                return FlagEnum.WHITE;
            }
            else if (flag == AC_FLAG_TYPE.AC_BLUE_FLAG)
            {
                return FlagEnum.BLUE;
            }
            else if (flag == AC_FLAG_TYPE.AC_NO_FLAG)
            {
                return FlagEnum.GREEN;
            }
            return FlagEnum.UNKNOWN;
        }

        private float mapToPercentage(float level, float minimumIn, float maximumIn, float minimumOut, float maximumOut)
        {
            return (level - minimumIn) * (maximumOut - minimumOut) / (maximumIn - minimumIn) + minimumOut;
        }

        private float getTyreWearPercentage(float wearLevel, float minimumLevel)
        {
            if (wearLevel == -1)
            {
                return -1;
            }
            return Math.Min(100, mapToPercentage((minimumLevel / wearLevel) * 100, minimumLevel, 100, 0, 100));
        }

        public SessionType mapToSessionType(Object memoryMappedFileStruct)
        {
            return SessionType.Unavailable;
        }

        private SessionType mapToSessionState(AC_SESSION_TYPE sessionState)
        {
            if (sessionState == AC_SESSION_TYPE.AC_RACE || sessionState == AC_SESSION_TYPE.AC_DRIFT || sessionState == AC_SESSION_TYPE.AC_DRAG)
            {
                return SessionType.Race;
            }
            else if (sessionState == AC_SESSION_TYPE.AC_PRACTICE)
            {
                return SessionType.Practice;
            }
            else if (sessionState == AC_SESSION_TYPE.AC_QUALIFY)
            {
                return SessionType.Qualify;
            }
            else if (sessionState == AC_SESSION_TYPE.AC_TIME_ATTACK || sessionState == AC_SESSION_TYPE.AC_HOTLAP)
            {
                return SessionType.HotLap;
            }
            else
            {
                return SessionType.Unavailable;
            }

        }

        private TyreType mapToTyreType(int r3eTyreType, CarData.CarClassEnum carClass)
        {
            return TyreType.Unknown_Race;
        }

        private ControlType mapToControlType(int controlType)
        {
            return ControlType.Player;
        }

        private DamageLevel mapToEngineDamageLevel(float engineDamage)
        {
            if (engineDamage >= 1000.0)
            {
                return DamageLevel.NONE;
            }
            else if (engineDamage <= destroyedEngineDamageThreshold)
            {
                return DamageLevel.DESTROYED;
            }
            else if (engineDamage <= severeEngineDamageThreshold)
            {
                return DamageLevel.MAJOR;
            }
            else if (engineDamage <= minorEngineDamageThreshold)
            {
                return DamageLevel.MINOR;
            }
            else if (engineDamage <= trivialEngineDamageThreshold)
            {
                return DamageLevel.TRIVIAL;
            }
            return DamageLevel.NONE;
        }

        private PitWindow mapToPitWindow(float lapsOrMinutes, Boolean isInPits, float pitWindowStart, float pitWindowEnd, Boolean mandatoryPitDone)
        {
            if (lapsOrMinutes < pitWindowStart && lapsOrMinutes > pitWindowEnd)
            {
                return PitWindow.Closed;
            }
            if (mandatoryPitDone)
            {
                return PitWindow.Completed;
            }
            else if (lapsOrMinutes >= pitWindowStart && lapsOrMinutes <= pitWindowEnd)
            {
                return PitWindow.Open;
            }
            else if (isInPits && lapsOrMinutes >= pitWindowStart && lapsOrMinutes <= pitWindowEnd)
            {
                return PitWindow.StopInProgress;
            }
            else
            {
                return PitWindow.Unavailable;
            }
        }

        private DamageLevel mapToAeroDamageLevel(float aeroDamage)
        {

            if (aeroDamage >= destroyedAeroDamageThreshold)
            {
                return DamageLevel.DESTROYED;
            }
            else if (aeroDamage >= severeAeroDamageThreshold)
            {
                return DamageLevel.MAJOR;
            }
            else if (aeroDamage >= minorAeroDamageThreshold)
            {
                return DamageLevel.MINOR;
            }
            else if (aeroDamage >= trivialAeroDamageThreshold)
            {
                return DamageLevel.TRIVIAL;
            }
            else
            {
                return DamageLevel.NONE;
            }

        }
        public Boolean isBehindWithinDistance(float trackLength, float minDistance, float maxDistance, float playerTrackDistance, float opponentTrackDistance)
        {
            float difference = playerTrackDistance - opponentTrackDistance;
            if (difference > 0)
            {
                return difference < maxDistance && difference > minDistance;
            }
            else
            {
                difference = (playerTrackDistance + trackLength) - opponentTrackDistance;
                return difference < maxDistance && difference > minDistance;
            }
        }

        private int getCurrentSector(TrackDefinition trackDef, float distanceRoundtrack)
        {

            int ret = 3;
            if (distanceRoundtrack >= 0 && distanceRoundtrack < trackDef.sectorPoints[0])
            {
                ret = 1;
            }
            if (distanceRoundtrack >= trackDef.sectorPoints[0] && (trackDef.sectorPoints[1] == 0 || distanceRoundtrack < trackDef.sectorPoints[1]))
            {
                ret = 2;
            }
            return ret;
        }
        void logSectorsForUnknownTracks(TrackDefinition trackDef, float distanceRoundTrack, int currentSector)
        {
            if (loggedSectorStart[0] == -1 && currentSector == 2)
            {
                loggedSectorStart[0] = distanceRoundTrack;
            }
            if (loggedSectorStart[1] == -1 && currentSector == 3)
            {
                loggedSectorStart[1] = distanceRoundTrack;
            }
            if (trackDef.sectorsOnTrack == 2 && loggedSectorStart[0] != -1)
            {
                System.Diagnostics.Debug.WriteLine("new TrackDefinition(\"" + trackDef.name + "\", " + trackDef.trackLength + ", " + trackDef.sectorsOnTrack + ", new float[] {" + loggedSectorStart[0] + "f, " + 0 + "f})");
            }
            else if (trackDef.sectorsOnTrack == 3 && loggedSectorStart[0] != -1 && loggedSectorStart[1] != -1)
            {
                System.Diagnostics.Debug.WriteLine("new TrackDefinition(\"" + trackDef.name + "\", " + trackDef.trackLength + "f, " + trackDef.sectorsOnTrack + ", new float[] {" + loggedSectorStart[0] + "f, " + loggedSectorStart[1] + "f})");
            }
        }

        private float spLineLengthToDistanceRoundTrack(float trackLength, float spLine)
        {
            if (spLine < 0.0f)
            {
                spLine -= 1f;
            }
            return spLine * trackLength;
        }
    }
}
