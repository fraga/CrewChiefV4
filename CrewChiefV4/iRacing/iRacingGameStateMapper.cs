﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewChiefV4.GameState;
using CrewChiefV4.Events;
using System.Diagnostics;
using CrewChiefV4.Overlay;
using CrewChiefV4.Audio;

namespace CrewChiefV4.iRacing
{
    class iRacingGameStateMapper : GameStateMapper
    {
        public static String playerName = null;
        public Driver playerCar = null;
        public Driver leaderCar = null;
        public iRacingGameStateMapper()
        {
        }

        public override void versionCheck(Object memoryMappedFileStruct)
        {
            // no version number in iRacing shared data so this is a no-op
        }

        Dictionary<string, DateTime> lastActiveTimeForOpponents = new Dictionary<string, DateTime>();
        DateTime nextOpponentCleanupTime = DateTime.MinValue;
        TimeSpan opponentCleanupInterval = TimeSpan.FromSeconds(3);

        // next track conditions sample due after:
        private DateTime nextConditionsSampleDue = DateTime.MinValue;
        private DateTime lastTimeEngineWasRunning = DateTime.MaxValue;
        private DateTime lastTimeEngineWaterTempWarning = DateTime.MaxValue;
        private DateTime lastTimeEngineOilPressureWarning = DateTime.MaxValue;
        private DateTime lastTimeEngineFuelPressureWarning = DateTime.MaxValue;

        private readonly bool enableFCYPitStateMessages = UserSettings.GetUserSettings().getBoolean("enable_iracing_pit_state_during_fcy");
        private Boolean invalidateCutTrackLaps = UserSettings.GetUserSettings().getBoolean("iracing_invalidate_cut_track_laps");
        private DateTime lastCutTrackTime = DateTime.MaxValue;
        TimeSpan cutTrackValidaitonTime = TimeSpan.FromSeconds(3);


        class PendingRacePositionChange
        {
            public int newPosition;
            public DateTime positionChangeTime;
            public PendingRacePositionChange(int newPosition, DateTime positionChangeTime)
            {
                this.newPosition = newPosition;
                this.positionChangeTime = positionChangeTime;
            }
        }

        TrackSurfaces prevTrackSurface;
        public override GameStateData mapToGameStateData(Object memoryMappedFileStruct, GameStateData previousGameState)
        {
            if (memoryMappedFileStruct == null)
            {
                return null;
            } 
            CrewChiefV4.iRacing.iRacingSharedMemoryReader.iRacingStructWrapper wrapper = (CrewChiefV4.iRacing.iRacingSharedMemoryReader.iRacingStructWrapper)memoryMappedFileStruct;
            GameStateData currentGameState = new GameStateData(wrapper.ticksWhenRead);
            currentGameState.rawGameData = wrapper;
            Sim shared = wrapper.data;

            if (shared.Telemetry.IsReplayPlaying)
            {
                CrewChief.trackName = shared.SessionData.Track.CodeName;
                CrewChief.carClass = CarData.getCarClassForIRacingId(shared.Driver.Car.CarClassId, shared.Driver.Car.CarId).carClassEnum;
                CrewChief.viewingReplay = true;
                CrewChief.distanceRoundTrack = shared.Driver.Live.CorrectedLapDistance * ((float)shared.SessionData.Track.Length * 1000);
                //return previousGameState;
            }
            updateOvertakingAids(currentGameState, shared.Telemetry.DRS_Status);

            SessionPhase lastSessionPhase = SessionPhase.Unavailable;
            SessionType lastSessionType = SessionType.Unavailable;
            int? previousSessionNumber = -1;
            int previousSessionId = -1;
            float lastSessionRunningTime = 0;
            float previousPitEntranceDistanceRoundTrack = -1.0f;
            if (previousGameState != null)
            {
                lastSessionPhase = previousGameState.SessionData.SessionPhase;
                lastSessionRunningTime = previousGameState.SessionData.SessionRunningTime;
                lastSessionType = previousGameState.SessionData.SessionType;
                currentGameState.readLandmarksForThisLap = previousGameState.readLandmarksForThisLap;
                previousSessionNumber = previousGameState.SessionData.SessionIteration;
                previousSessionId = previousGameState.SessionData.SessionId;
                previousPitEntranceDistanceRoundTrack = previousGameState.SessionData.TrackDefinition.iracingPitEntranceDistanceRoundTrack;
            }
            currentGameState.SessionData.SessionType = mapToSessionType(shared.SessionData.SessionType);
            currentGameState.SessionData.SessionRunningTime = (float)shared.Telemetry.SessionTime;
            currentGameState.SessionData.SessionTimeRemaining = (float)shared.Telemetry.SessionTimeRemain;

            int previousLapsCompleted = previousGameState == null ? 0 : previousGameState.SessionData.CompletedLaps;
            int PlayerCarIdx = shared.Telemetry.PlayerCarIdx;

            if (shared.Driver != null)
            {
                playerCar = shared.Driver;
                playerName = playerCar.Name;
            }
            // only used for auto fuelling check
            currentGameState.ControlData.ControlType = shared.Telemetry.IsReplayPlaying ? ControlType.Replay : ControlType.Player;
            leaderCar = shared.Drivers.Where(d => d.CurrentResults.Position > 0).OrderBy(d => d.CurrentResults.Position).FirstOrDefault();

            if(leaderCar == null)
            {
                leaderCar = playerCar;
            }
            AdditionalDataProvider.validate(playerName);

            currentGameState.SafetyCarData = GetSafetyCarData(previousGameState == null ? null : previousGameState.SafetyCarData, shared.PaceCar, 
                (float)(shared.SessionData.Track.Length * 1000), shared.PaceCarPresent && shared.Drivers.Count <= shared.Telemetry.NumberOfCarsEnabled);                      
           
            currentGameState.SessionData.SessionPhase = mapToSessionPhase(lastSessionPhase, shared.Telemetry.SessionState, currentGameState.SessionData.SessionType, 
                (float)shared.Telemetry.SessionTime, previousLapsCompleted, playerCar.Live.LiveLapsCompleted, (SessionFlags)shared.Telemetry.SessionFlags, 
                shared.SessionData.hasFullCourseCautions,currentGameState.SafetyCarData, previousGameState == null ? null : previousGameState.SafetyCarData, 
                shared.SessionData.Track.FormationLapCount, previousPitEntranceDistanceRoundTrack);
                       
            currentGameState.SessionData.NumCarsOverallAtStartOfSession = shared.PaceCarPresent ? shared.Drivers.Count - 1 : shared.Drivers.Count;
            
            int sessionNumber = shared.Telemetry.SessionNum;

            currentGameState.SessionData.SessionIteration = sessionNumber;
            currentGameState.SessionData.SessionId = shared.SessionData.SessionId;

            if (currentGameState.SessionData.SessionType != SessionType.Unavailable && shared.Telemetry.IsNewSession)
            {
                CarData.clearCachedIRacingClassData();
                currentGameState.SessionData.IsNewSession = true;
                Console.WriteLine("New session, trigger data:");
                Console.WriteLine("SessionType = " + currentGameState.SessionData.SessionType);
                Console.WriteLine("lastSessionPhase = " + lastSessionPhase);
                Console.WriteLine("lastSessionRunningTime = " + lastSessionRunningTime);
                Console.WriteLine("currentSessionPhase = " + currentGameState.SessionData.SessionPhase);
                Console.WriteLine("rawSessionPhase = " + shared.Telemetry.SessionState);
                Console.WriteLine("currentSessionRunningTime = " + currentGameState.SessionData.SessionRunningTime);
                Console.WriteLine("NumCarsAtStartOfSession = " + currentGameState.SessionData.NumCarsOverallAtStartOfSession);
                Console.WriteLine("Category = " + shared.SessionData.Category);
                Console.WriteLine("TrackType = " + shared.SessionData.Track.TrackType);
                Console.WriteLine("TrackPitSpeedLimit = " + shared.SessionData.Track.TrackPitSpeedLimit);
                Console.WriteLine("CourseCautions = " + shared.SessionData.CourseCautions);
                Console.WriteLine("Restarts = " + shared.SessionData.Restarts);
                Console.WriteLine("TrackCodeName " + shared.SessionData.Track.CodeName + " Track Reported Length " + (float)shared.SessionData.Track.Length * 1000);
                if (shared.Telemetry.NumberOfCarsEnabled < shared.Drivers.Count && shared.PaceCarPresent)
                {
                    Console.WriteLine("Advanced Safety/Pace Car calls has been disable for this session, to enable increase the Max Cars in iRacing graphics settings to " + shared.Drivers.Count + ". And restart the app");
                }                
                
                currentGameState.SessionData.DriverRawName = playerName;
                currentGameState.OpponentData.Clear();
                currentGameState.SessionData.PlayerLapData.Clear();
                currentGameState.SessionData.SessionStartTime = currentGameState.Now;
                
                currentGameState.SessionData.TrackDefinition = TrackData.getTrackDefinition(shared.SessionData.Track.CodeName, 0, (float)shared.SessionData.Track.Length * 1000);
                currentGameState.SessionData.TrackDefinition.iracingTrackNorthOffset = shared.SessionData.Track.TrackNorthOffset;
                if (previousGameState != null && previousGameState.SessionData.TrackDefinition != null)
                {
                    if (previousGameState.SessionData.TrackDefinition.name.Equals(currentGameState.SessionData.TrackDefinition.name))
                    {
                        if (previousGameState.hardPartsOnTrackData.hardPartsMapped)
                        {
                            currentGameState.hardPartsOnTrackData.processedHardPartsForBestLap = previousGameState.hardPartsOnTrackData.processedHardPartsForBestLap;
                            currentGameState.hardPartsOnTrackData.isAlreadyBraking = previousGameState.hardPartsOnTrackData.isAlreadyBraking;
                            currentGameState.hardPartsOnTrackData.hardPartStart = previousGameState.hardPartsOnTrackData.hardPartStart;
                            currentGameState.hardPartsOnTrackData.hardPartsMapped = previousGameState.hardPartsOnTrackData.hardPartsMapped;
                        }
                    }
                }
                TrackDataContainer tdc = TrackData.TRACK_LANDMARKS_DATA.getTrackDataForTrackName(shared.SessionData.Track.CodeName, currentGameState.SessionData.TrackDefinition.trackLength);
                currentGameState.SessionData.TrackDefinition.trackLandmarks = tdc.trackLandmarks;
                if (tdc.isDefinedInTracklandmarksData)
                {
                    currentGameState.SessionData.TrackDefinition.iracingPitEntranceDistanceRoundTrack = tdc.iracingPitEntranceDistanceRoundTrack;
                }
                currentGameState.SessionData.TrackDefinition.isOval = shared.SessionData.Track.IsOval;
                currentGameState.SessionData.TrackDefinition.setGapPoints();
                GlobalBehaviourSettings.UpdateFromTrackDefinition(currentGameState.SessionData.TrackDefinition);


                currentGameState.SessionData.LeaderHasFinishedRace = false;
                currentGameState.PitData.IsRefuellingAllowed = true;
                currentGameState.SessionData.SessionTimeRemaining = (float)shared.Telemetry.SessionTimeRemain;

                if (!shared.SessionData.IsLimitedSessionLaps)
                {
                    currentGameState.SessionData.SessionHasFixedTime = true;
                    currentGameState.SessionData.SessionTotalRunTime = (float)shared.SessionData.RaceTime;
                    Console.WriteLine("Treating this as a time limited race");
                }
                else
                {
                    currentGameState.SessionData.SessionHasFixedTime = false;
                    currentGameState.SessionData.SessionNumberOfLaps = Parser.ParseInt(shared.SessionData.RaceLaps);
                    Console.WriteLine("Treating this as a lap limited race");
                }
                

                currentGameState.SessionData.MaxIncidentCount = shared.SessionData.IncidentLimit;
                currentGameState.SessionData.CurrentIncidentCount = shared.Telemetry.PlayerCarMyIncidentCount;
                currentGameState.SessionData.CurrentDriverIncidentCount = shared.Telemetry.PlayerCarDriverIncidentCount;
                currentGameState.SessionData.CurrentTeamIncidentCount = shared.Telemetry.PlayerCarTeamIncidentCount;

                lastActiveTimeForOpponents.Clear();
                nextOpponentCleanupTime = currentGameState.Now + opponentCleanupInterval;

                currentGameState.PitData.InPitlane = shared.Telemetry.OnPitRoad;
                currentGameState.PositionAndMotionData.DistanceRoundTrack = Math.Abs(playerCar.Live.CorrectedLapDistance * currentGameState.SessionData.TrackDefinition.trackLength);

                currentGameState.carClass = CarData.getCarClassForIRacingId(playerCar.Car.CarClassId, playerCar.Car.CarId);
                if (currentGameState.SessionData.TrackDefinition.isOval)
                {
                    currentGameState.carClass.limiterAvailable = false;
                }
                CarData.IRACING_CLASS_ID = playerCar.Car.CarClassId;
                GlobalBehaviourSettings.UpdateFromCarClass(currentGameState.carClass, playerCar.Car.CarIsElectric);

                Console.WriteLine("Player is using car class " + currentGameState.carClass.getClassIdentifier() + " (car ID " + playerCar.Car.CarId + ")" + "Is Electric Vehicle = " + playerCar.Car.CarIsElectric);
                currentGameState.SessionData.PlayerCarNr = playerCar.CarNumber;

                currentGameState.SessionData.DeltaTime = new DeltaTime(currentGameState.SessionData.TrackDefinition.trackLength, 
                    currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.PositionAndMotionData.CarSpeed, currentGameState.Now);
                currentGameState.SessionData.SectorNumber = playerCar.Live.CurrentSector;
                foreach (Driver driver in shared.Drivers)
                {
                    if (driver.IsCurrentDriver || driver.CurrentResults.IsOut || driver.IsPaceCar || driver.IsSpectator)
                    {
                        continue;
                    }
                    else
                    {
                        currentGameState.OpponentData.Add(driver.Id.ToString(), createOpponentData(driver, true,
                            currentGameState.SessionData.TrackDefinition.trackLength));
                    }
                }
                // add a conditions sample when we first start a session so we're not using stale or default data in the pre-lights phase
                currentGameState.Conditions.addSample(currentGameState.Now, 0, 1, shared.Telemetry.AirTemp, shared.Telemetry.TrackTempCrew, 0, shared.Telemetry.WindVel, 0, 0, 0, true, ConditionsMonitor.TrackStatus.UNKNOWN);

                //need to call this after adding opponents else we have nothing to compare against 
                GameStateData.Multiclass = shared.SessionData.NumCarClasses > 1;
                GameStateData.NumberOfClasses = shared.SessionData.NumCarClasses;
                Utilities.TraceEventClass(currentGameState);
            }
            else
            {
                if (lastSessionPhase != currentGameState.SessionData.SessionPhase)
                {
                    Console.WriteLine("New session phase, was " + lastSessionPhase + " now " + currentGameState.SessionData.SessionPhase);
                    if (previousGameState != null && previousGameState.SessionData.TrackDefinition == null)
                    {
                        Console.WriteLine("New session phase without new session initialized previously.");
                    }

                    if (currentGameState.SessionData.SessionPhase == SessionPhase.Green && lastSessionPhase != SessionPhase.Finished && lastSessionPhase != SessionPhase.FullCourseYellow)
                    {
                        currentGameState.SessionData.JustGoneGreen = true;
                        // just gone green, so get the session data

                        if (!shared.SessionData.IsLimitedSessionLaps)
                        {
                            currentGameState.SessionData.SessionHasFixedTime = true;
                            currentGameState.SessionData.SessionTotalRunTime = (float)shared.SessionData.RaceTime;
                            Console.WriteLine("Treating this as a time limited race");
                        }
                        else
                        {
                            currentGameState.SessionData.SessionHasFixedTime = false;
                            currentGameState.SessionData.SessionNumberOfLaps = Parser.ParseInt(shared.SessionData.RaceLaps);
                            Console.WriteLine("Treating this as a lap limited race");
                        }
                        

                        currentGameState.SessionData.TrackDefinition = TrackData.getTrackDefinition(shared.SessionData.Track.CodeName, 0, (float)shared.SessionData.Track.Length * 1000);
                        currentGameState.SessionData.TrackDefinition.iracingTrackNorthOffset = shared.SessionData.Track.TrackNorthOffset;
                        if (previousGameState != null && previousGameState.SessionData.TrackDefinition != null)
                        {
                            if (previousGameState.SessionData.TrackDefinition.name.Equals(currentGameState.SessionData.TrackDefinition.name))
                            {
                                if (previousGameState.hardPartsOnTrackData.hardPartsMapped)
                                {
                                    currentGameState.hardPartsOnTrackData.processedHardPartsForBestLap = previousGameState.hardPartsOnTrackData.processedHardPartsForBestLap;
                                    currentGameState.hardPartsOnTrackData.isAlreadyBraking = previousGameState.hardPartsOnTrackData.isAlreadyBraking;
                                    currentGameState.hardPartsOnTrackData.hardPartStart = previousGameState.hardPartsOnTrackData.hardPartStart;
                                    currentGameState.hardPartsOnTrackData.hardPartsMapped = previousGameState.hardPartsOnTrackData.hardPartsMapped;
                                }
                            }
                        }
                        TrackDataContainer tdc = TrackData.TRACK_LANDMARKS_DATA.getTrackDataForTrackName(shared.SessionData.Track.CodeName, currentGameState.SessionData.TrackDefinition.trackLength);
                        currentGameState.SessionData.TrackDefinition.trackLandmarks = tdc.trackLandmarks;
                        if(tdc.isDefinedInTracklandmarksData)
                        {
                            currentGameState.SessionData.TrackDefinition.iracingPitEntranceDistanceRoundTrack = tdc.iracingPitEntranceDistanceRoundTrack;
                        }
                        currentGameState.SessionData.TrackDefinition.isOval = shared.SessionData.Track.IsOval;
                        currentGameState.SessionData.TrackDefinition.setGapPoints();
                        GlobalBehaviourSettings.UpdateFromTrackDefinition(currentGameState.SessionData.TrackDefinition);

                        currentGameState.carClass = CarData.getCarClassForIRacingId(playerCar.Car.CarClassId, playerCar.Car.CarId);
                        if (currentGameState.SessionData.TrackDefinition.isOval)
                        {
                            currentGameState.carClass.limiterAvailable = false;
                        }
                        GlobalBehaviourSettings.UpdateFromCarClass(currentGameState.carClass, playerCar.Car.CarIsElectric);
                        currentGameState.SessionData.DeltaTime = new DeltaTime(currentGameState.SessionData.TrackDefinition.trackLength,
                            currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.PositionAndMotionData.CarSpeed, currentGameState.Now);
                        Console.WriteLine("Player is using car class " + currentGameState.carClass.getClassIdentifier() + " (car ID " + playerCar.Car.CarId + ")" + "Is Electric Vehicle =" + playerCar.Car.CarIsElectric);
                        currentGameState.SessionData.PlayerCarNr = playerCar.CarNumber;

                        if (previousGameState != null)
                        {
                            currentGameState.PitData.IsRefuellingAllowed = previousGameState.PitData.IsRefuellingAllowed;
                            currentGameState.OpponentData = previousGameState.OpponentData;
                            currentGameState.SessionData.TrackDefinition = previousGameState.SessionData.TrackDefinition;
                            currentGameState.SessionData.DriverRawName = previousGameState.SessionData.DriverRawName;
                            currentGameState.PitData.OnInLap = previousGameState.PitData.OnInLap;
                            currentGameState.PitData.OnOutLap = previousGameState.PitData.OnOutLap;
                        }
                        currentGameState.SessionData.SessionStartTime = currentGameState.Now;


                        lastActiveTimeForOpponents.Clear();
                        nextOpponentCleanupTime = currentGameState.Now + opponentCleanupInterval;
                        lastTimeEngineWasRunning = DateTime.MaxValue;
                        lastTimeEngineWaterTempWarning = DateTime.MaxValue;
                        lastTimeEngineOilPressureWarning = DateTime.MaxValue;
                        lastTimeEngineFuelPressureWarning = DateTime.MaxValue;
                        //currentGameState.SessionData.CompletedLaps = shared.Driver.CurrentResults.LapsComplete;

                        Console.WriteLine("Just gone green, session details...");

                        Console.WriteLine("SessionType " + currentGameState.SessionData.SessionType);
                        Console.WriteLine("SessionPhase " + currentGameState.SessionData.SessionPhase);
                        Console.WriteLine("HasMandatoryPitStop " + currentGameState.PitData.HasMandatoryPitStop);
                        Console.WriteLine("NumCarsAtStartOfSession " + currentGameState.SessionData.NumCarsOverallAtStartOfSession);
                        Console.WriteLine("SessionNumberOfLaps " + currentGameState.SessionData.SessionNumberOfLaps);
                        Console.WriteLine("SessionRunTime " + currentGameState.SessionData.SessionTotalRunTime);
                        Console.WriteLine("SessionStartTime " + currentGameState.SessionData.SessionStartTime);
                        String trackName = currentGameState.SessionData.TrackDefinition == null ? "unknown" : currentGameState.SessionData.TrackDefinition.name;
                        Console.WriteLine("TrackName " + trackName + " Track Reported Length " + currentGameState.SessionData.TrackDefinition.trackLength);

                    }
                }
                if (!currentGameState.SessionData.JustGoneGreen && previousGameState != null)
                {
                    //Console.WriteLine("regular update, session type = " + currentGameState.SessionData.SessionType + " phase = " + currentGameState.SessionData.SessionPhase);

                    currentGameState.SessionData.SessionStartTime = previousGameState.SessionData.SessionStartTime;
                    currentGameState.SessionData.SessionTotalRunTime = previousGameState.SessionData.SessionTotalRunTime;
                    currentGameState.SessionData.SessionNumberOfLaps = previousGameState.SessionData.SessionNumberOfLaps;
                    currentGameState.SessionData.SessionHasFixedTime = previousGameState.SessionData.SessionHasFixedTime;
                    currentGameState.SessionData.ExtraLapsAfterTimedSessionComplete = previousGameState.SessionData.ExtraLapsAfterTimedSessionComplete;
                    currentGameState.SessionData.NumCarsOverallAtStartOfSession = previousGameState.SessionData.NumCarsOverallAtStartOfSession;
                    currentGameState.SessionData.NumCarsInPlayerClassAtStartOfSession = previousGameState.SessionData.NumCarsInPlayerClassAtStartOfSession;
                    currentGameState.SessionData.EventIndex = previousGameState.SessionData.EventIndex;
                    currentGameState.SessionData.SessionIteration = previousGameState.SessionData.SessionIteration;
                    currentGameState.SessionData.PositionAtStartOfCurrentLap = previousGameState.SessionData.PositionAtStartOfCurrentLap;
                    currentGameState.SessionData.SessionStartClassPosition = previousGameState.SessionData.SessionStartClassPosition;
                    currentGameState.SessionData.ClassPositionAtStartOfCurrentLap = previousGameState.SessionData.ClassPositionAtStartOfCurrentLap;

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
                    currentGameState.PitData.NumPitStops = previousGameState.PitData.NumPitStops;
                    currentGameState.PitData.PitBoxPositionEstimate = previousGameState.PitData.PitBoxPositionEstimate;
                    currentGameState.PitData.IsTeamRacing = previousGameState.PitData.IsTeamRacing;

                    currentGameState.SessionData.TrackDefinition = previousGameState.SessionData.TrackDefinition;
                    currentGameState.SessionData.formattedPlayerLapTimes = previousGameState.SessionData.formattedPlayerLapTimes;
                    currentGameState.SessionData.PlayerLapTimeSessionBest = previousGameState.SessionData.PlayerLapTimeSessionBest;
                    currentGameState.SessionData.PlayerLapTimeSessionBestPrevious = previousGameState.SessionData.PlayerLapTimeSessionBestPrevious;
                    currentGameState.SessionData.OpponentsLapTimeSessionBestOverall = previousGameState.SessionData.OpponentsLapTimeSessionBestOverall;
                    currentGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass = previousGameState.SessionData.OpponentsLapTimeSessionBestPlayerClass;
                    currentGameState.carClass = previousGameState.carClass;
                    currentGameState.SessionData.PlayerClassSessionBestLapTimeByTyre = previousGameState.SessionData.PlayerClassSessionBestLapTimeByTyre;
                    currentGameState.SessionData.PlayerBestLapTimeByTyre = previousGameState.SessionData.PlayerBestLapTimeByTyre;
                    currentGameState.SessionData.DriverRawName = previousGameState.SessionData.DriverRawName;
                    currentGameState.SessionData.SessionTimesAtEndOfSectors = previousGameState.SessionData.SessionTimesAtEndOfSectors;
                    currentGameState.SessionData.LapTimePreviousEstimateForInvalidLap = previousGameState.SessionData.LapTimePreviousEstimateForInvalidLap;
                    currentGameState.SessionData.OverallSessionBestLapTime = previousGameState.SessionData.OverallSessionBestLapTime;
                    currentGameState.SessionData.PlayerClassSessionBestLapTime = previousGameState.SessionData.PlayerClassSessionBestLapTime;
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
                    currentGameState.SessionData.PlayerLapData = previousGameState.SessionData.PlayerLapData;
                    currentGameState.SessionData.trackLandmarksTiming = previousGameState.SessionData.trackLandmarksTiming;
                    currentGameState.SessionData.CompletedLaps = previousGameState.SessionData.CompletedLaps;
                    currentGameState.SessionData.LapCount = previousGameState.SessionData.LapCount;
                    currentGameState.SessionData.PlayerCarNr = previousGameState.SessionData.PlayerCarNr;

                    currentGameState.OpponentData = previousGameState.OpponentData;
                    currentGameState.SessionData.SectorNumber = previousGameState.SessionData.SectorNumber;
                    currentGameState.SessionData.DeltaTime = previousGameState.SessionData.DeltaTime;
                    currentGameState.SessionData.MaxIncidentCount = previousGameState.SessionData.MaxIncidentCount;
                    currentGameState.SessionData.CurrentIncidentCount = previousGameState.SessionData.CurrentIncidentCount;
                    currentGameState.SessionData.CurrentDriverIncidentCount = previousGameState.SessionData.CurrentDriverIncidentCount;
                    currentGameState.SessionData.CurrentTeamIncidentCount = previousGameState.SessionData.CurrentTeamIncidentCount;
                    currentGameState.SessionData.HasLimitedIncidents = previousGameState.SessionData.HasLimitedIncidents;
                    currentGameState.SessionData.StrengthOfField = previousGameState.SessionData.StrengthOfField;

                    currentGameState.Conditions.samples = previousGameState.Conditions.samples;
                    currentGameState.Conditions.CurrentConditions = previousGameState.Conditions.CurrentConditions;
                    currentGameState.PenaltiesData.CutTrackWarnings = previousGameState.PenaltiesData.CutTrackWarnings;
                    currentGameState.retriedDriverNames = previousGameState.retriedDriverNames;
                    currentGameState.disqualifiedDriverNames = previousGameState.disqualifiedDriverNames;
                    currentGameState.hardPartsOnTrackData = previousGameState.hardPartsOnTrackData;

                    currentGameState.TimingData = previousGameState.TimingData;
                    //currentGameState.FrozenOrderData = previousGameState.FrozenOrderData;
                    currentGameState.SessionData.JustGoneGreenTime = previousGameState.SessionData.JustGoneGreenTime;
                    currentGameState.FlagData.useImprovisedIncidentCalling = previousGameState.FlagData.useImprovisedIncidentCalling;
                    //currentGameState.FlagData = previousGameState.FlagData;
                    currentGameState.FlagData.fcyPhase = previousGameState.FlagData.fcyPhase;
                }
            }


            currentGameState.ControlData.ThrottlePedal = shared.Telemetry.Throttle;
            currentGameState.ControlData.ClutchPedal = shared.Telemetry.Clutch;
            currentGameState.ControlData.BrakePedal = shared.Telemetry.Brake;
            currentGameState.ControlData.HandBrake = shared.Telemetry.HandBrake;
            currentGameState.ControlData.SteeringWheelAngle = shared.Telemetry.SteeringWheelAngle;
            currentGameState.TransmissionData.Gear = shared.Telemetry.Gear;


            currentGameState.SessionData.LicenseLevel = playerCar.licensLevel;
            currentGameState.SessionData.iRating = playerCar.IRating;

            currentGameState.EngineData.EngineOilTemp = shared.Telemetry.OilTemp;
            currentGameState.EngineData.EngineWaterTemp = shared.Telemetry.WaterTemp;

            currentGameState.EngineData.MinutesIntoSessionBeforeMonitoring = 0;

            bool additionalEngineCheckFlags = shared.Telemetry.IsOnTrack && !shared.Telemetry.OnPitRoad && shared.Telemetry.Voltage > 0f;

            if (!shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.EngineStalled))
            {
                lastTimeEngineWasRunning = currentGameState.Now;
            }
            if (previousGameState != null && !previousGameState.EngineData.EngineStalledWarning &&
                currentGameState.SessionData.SessionRunningTime > 60 && shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.EngineStalled) &&
                lastTimeEngineWasRunning < currentGameState.Now.Subtract(TimeSpan.FromSeconds(2)) && additionalEngineCheckFlags)
            {
                currentGameState.EngineData.EngineStalledWarning = true;
                lastTimeEngineWasRunning = DateTime.MaxValue;

            }

            if (!shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.WaterTemperatureWarning))
            {
                lastTimeEngineWaterTempWarning = currentGameState.Now;
            }

            if (previousGameState != null && !previousGameState.EngineData.EngineWaterTempWarning && !shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.EngineStalled) &&
                currentGameState.SessionData.SessionRunningTime > 60 && shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.WaterTemperatureWarning) &&
                lastTimeEngineWaterTempWarning < currentGameState.Now.Subtract(TimeSpan.FromSeconds(2)) && additionalEngineCheckFlags)
            {
                currentGameState.EngineData.EngineWaterTempWarning = true;
                lastTimeEngineWaterTempWarning = DateTime.MaxValue;
            }

            if (!shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.OilPressureWarning))
            {
                lastTimeEngineOilPressureWarning = currentGameState.Now;
            }
            if (previousGameState != null && !previousGameState.EngineData.EngineWaterTempWarning && !shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.EngineStalled) &&
                currentGameState.SessionData.SessionRunningTime > 60 && shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.OilPressureWarning) &&
                lastTimeEngineOilPressureWarning < currentGameState.Now.Subtract(TimeSpan.FromSeconds(2)) && additionalEngineCheckFlags)
            {
                currentGameState.EngineData.EngineWaterTempWarning = true;
                lastTimeEngineOilPressureWarning = DateTime.MaxValue;
            }
            if (!shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.FuelPressureWarning))
            {
                lastTimeEngineFuelPressureWarning = currentGameState.Now;
            }
            if (previousGameState != null && !previousGameState.EngineData.EngineWaterTempWarning && !shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.EngineStalled) &&
                currentGameState.SessionData.SessionRunningTime > 60 && shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.FuelPressureWarning) &&
                lastTimeEngineFuelPressureWarning < currentGameState.Now.Subtract(TimeSpan.FromSeconds(2)) && additionalEngineCheckFlags)
            {
                currentGameState.EngineData.EngineWaterTempWarning = true;
                lastTimeEngineFuelPressureWarning = DateTime.MaxValue;
            }

            currentGameState.SessionData.CompletedLaps = playerCar.Live.LiveLapsCompleted;
            currentGameState.SessionData.LapCount = currentGameState.SessionData.CompletedLaps + 1;
            currentGameState.SessionData.LapTimeCurrent = shared.Telemetry.LapCurrentLapTime;
            currentGameState.FlagData.isFullCourseYellow = currentGameState.SessionData.SessionPhase == SessionPhase.FullCourseYellow;
            
            currentGameState.PitData.InPitlane = (shared.Telemetry.CarIdxOnPitRoad[PlayerCarIdx] || playerCar.Live.TrackSurface == TrackSurfaces.InPitStall) && currentGameState.SessionData.SessionPhase != SessionPhase.Formation;

            if (currentGameState.SessionData.TrackDefinition != null && currentGameState.SessionData.TrackDefinition.isOval)
            {
                currentGameState.StockCarRulesData.stockCarRulesEnabled = true;
            }

            if (previousGameState != null && !currentGameState.FlagData.isFullCourseYellow && !previousGameState.FlagData.isFullCourseYellow)
            {
                // turn on improvised incident calling when not under fcy.               
                currentGameState.FlagData.useImprovisedIncidentCalling = true;
            }

            if (previousGameState != null && !currentGameState.FlagData.isFullCourseYellow && previousGameState.FlagData.isFullCourseYellow 
                && currentGameState.SessionData.SessionPhase != SessionPhase.Checkered)
            {                
                currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.RACING;
                currentGameState.FlagData.lapCountWhenLastWentGreen = currentGameState.SessionData.CompletedLaps;
            }
            else if (previousGameState != null && currentGameState.FlagData.isFullCourseYellow)
            {
                // turn off improvised incident calling doing fcy.
                currentGameState.FlagData.useImprovisedIncidentCalling = false;
                
                SessionFlags flags =  (SessionFlags)shared.Telemetry.SessionFlags;
                currentGameState.FlagData.currentLapIsFCY = true;
                if (!currentGameState.SafetyCarData.fcySafetyCarCallsEnabled || !enableFCYPitStateMessages)
                {
                    if (previousGameState.FlagData.fcyPhase == FullCourseYellowPhase.RACING)
                    {
                        currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.PENDING;
                    }
                    if (flags.HasFlag(SessionFlags.OneLapToGreen))
                    {
                        currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.LAST_LAP_CURRENT;
                    }
                }
                else
                {
                    if (!currentGameState.SafetyCarData.isOnTrack)
                    {
                        currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.PENDING;
                    }
                    else
                    {
                        if (!shared.Telemetry.PitsOpen)
                        {
                            currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.PITS_CLOSED;
                        }
                        if (shared.Telemetry.PitsOpen)
                        {
                            currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.PITS_OPEN;
                        }
                    }
                    if (flags.HasFlag(SessionFlags.OneLapToGreen))
                    {
                        currentGameState.FlagData.fcyPhase = FullCourseYellowPhase.LAST_LAP_CURRENT;
                    }
                }
            }

            currentGameState.SessionData.NumCarsOverall = shared.PaceCarPresent ? shared.Drivers.Count - 1 : shared.Drivers.Count;
            //use qual position in race session position until we green and first lap has been started. 
            //Here i'm a bit blindfolded, but this might be a fix for wrong multiclass start position message.
            Boolean useQualifyingPosition = false;
            if ((currentGameState.SessionData.SessionPhase == SessionPhase.Formation || currentGameState.SessionData.SessionPhase == SessionPhase.Gridwalk ||
                currentGameState.SessionData.SessionPhase == SessionPhase.Countdown || playerCar.Live.Lap < 1) && currentGameState.SessionData.SessionType == SessionType.Race)
            {
                currentGameState.SessionData.OverallPosition = playerCar.CurrentResults.QualifyingPosition;
                currentGameState.SessionData.ClassPosition = playerCar.CurrentResults.ClassQualifyingPosition;
                useQualifyingPosition = true;
            }
            else
            {
                currentGameState.SessionData.OverallPosition = currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionPhase != SessionPhase.Finished && previousGameState != null
                    ? getRacePosition(currentGameState.SessionData.DriverRawName, previousGameState.SessionData.OverallPosition, playerCar.Live.Position, currentGameState.Now)
                    : playerCar.Live.Position;
                currentGameState.SessionData.ClassPosition = currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionPhase != SessionPhase.Finished && previousGameState != null
                    ? getRacePosition(currentGameState.SessionData.DriverRawName, previousGameState.SessionData.ClassPosition, playerCar.Live.ClassPosition, currentGameState.Now, true)
                    : playerCar.Live.ClassPosition;
            }
            if (currentGameState.FlagData.isFullCourseYellow)
            {
                currentGameState.SessionData.OverallPosition = playerCar.CurrentResults.Position;
            }
            if (previousGameState != null
                && previousGameState.SessionData.SessionPhase != SessionPhase.Finished
                && currentGameState.SessionData.SessionPhase == SessionPhase.Finished
                && currentGameState.SessionData.SessionType == SessionType.Race
                && previousGameState.SessionData.OverallPosition != playerCar.Live.Position)
            {
                // Note: resolved position at crossing the finish line will be incorrect if any of the cars ahead are disconnected.
                // Scoring position will be incorrect for ~2.5secs after crossing s/f, if it changed during the last lap.
                // Lastly, as long as we detect finish within the PositionChangeLag, finishing position will be correct (should be)
                // because disconnecters only affect race pos after s/f (due to lap dist falling behind player's).
                Console.WriteLine("Finished position ambigous:  prev overall: {0}  curr overall (delayed): {1}  results pos: {2}  curr resolved: {3}",
                    previousGameState.SessionData.OverallPosition,
                    currentGameState.SessionData.OverallPosition,
                    playerCar.CurrentResults.Position,
                    playerCar.Live.Position);
            }

            if (currentGameState.SessionData.SessionType != SessionType.Race)
            {
                currentGameState.SessionData.ClassPosition = playerCar.Live.ClassPosition;
            }

            if (currentGameState.SessionData.OverallSessionBestLapTime == -1 ||
                currentGameState.SessionData.OverallSessionBestLapTime > shared.Telemetry.LapBestLapTime)
            {
                currentGameState.SessionData.OverallSessionBestLapTime = shared.Telemetry.LapBestLapTime;
            }

            if (currentGameState.SessionData.OverallPosition == 1)
            {
                currentGameState.SessionData.LeaderSectorNumber = currentGameState.SessionData.SectorNumber;
            }

            int currentSector = playerCar.Live.CurrentSector;

            if (playerCar.Car.DriverPitTrkPct != -1.0f && currentGameState.SessionData.TrackDefinition != null)
            {
                currentGameState.PitData.PitBoxPositionEstimate = currentGameState.SessionData.TrackDefinition.trackLength * playerCar.Car.DriverPitTrkPct;
                if ((previousGameState != null && currentGameState.PitData.PitBoxPositionEstimate != previousGameState.PitData.PitBoxPositionEstimate)
                    || previousGameState == null)
                {
                    Console.WriteLine("Pit box position = " + currentGameState.PitData.PitBoxPositionEstimate.ToString("0.000"));
                }
            }

            if (previousGameState != null)
            {
                if (previousGameState.SessionData.SectorNumber == 2 && currentSector == 3)
                {
                    currentGameState.SessionData.HasCompletedSector2ThisLap = true;
                }
                else if (previousGameState.SessionData.SectorNumber == 1 && currentSector == 2 || currentGameState.PitData.JumpedToPits)
                {
                    currentGameState.SessionData.HasCompletedSector2ThisLap = false;
                }
            }
            currentGameState.SessionData.TrackSurface = (int)playerCar.Live.TrackSurface;
            if (previousGameState != null && (playerCar.Live.TrackSurface == TrackSurfaces.AproachingPits ||
                (TrackSurfaces)previousGameState.SessionData.TrackSurface == TrackSurfaces.OnTrack || 
                (TrackSurfaces)previousGameState.SessionData.TrackSurface == TrackSurfaces.OffTrack))
            {
                
                currentGameState.PitData.IsApproachingPitlane = (playerCar.Live.TrackSurface == TrackSurfaces.AproachingPits && 
                    ((TrackSurfaces)previousGameState.SessionData.TrackSurface == TrackSurfaces.OnTrack || (TrackSurfaces)previousGameState.SessionData.TrackSurface == TrackSurfaces.OffTrack)) || 
                    (playerCar.Live.TrackSurface == TrackSurfaces.AproachingPits && !currentGameState.PitData.InPitlane && previousGameState.PitData.IsApproachingPitlane);            
            }
            if (previousGameState != null)
            {
                if (previousGameState.PitData.TowedToPits == PitData.TowDetection.TowOnTrack && playerCar.Live.TrackSurface == TrackSurfaces.NotInWorld)
                {
                    currentGameState.PitData.TowedToPits = PitData.TowDetection.TowNotInWorld;
                }
                else if (previousGameState.PitData.TowedToPits == PitData.TowDetection.TowNotInWorld && playerCar.Live.TrackSurface == TrackSurfaces.InPitStall)
                {
                    currentGameState.PitData.TowedToPits = PitData.TowDetection.TowInPitStall;
                }
                else
                {
                    currentGameState.PitData.TowedToPits = PitData.TowDetection.TowOnTrack;
                }               
            }
            currentGameState.PitData.JumpedToPits = previousGameState != null && currentGameState.PitData.TowedToPits == PitData.TowDetection.TowInPitStall && previousGameState.PitData.TowedToPits == PitData.TowDetection.TowNotInWorld;
      
            currentGameState.PitData.IsInGarage = shared.Telemetry.IsInGarage;

            currentGameState.PitData.IsTeamRacing = shared.SessionData.IsTeamRacing;


            currentGameState.SessionData.IsNewLap = playerCar.Live.IsNewLap && !(currentGameState.PitData.JumpedToPits || playerCar.Live.TrackSurface == TrackSurfaces.NotInWorld);
            currentGameState.SessionData.IsNewSector = (currentGameState.SessionData.SectorNumber != currentSector && currentSector != 1 || currentGameState.SessionData.IsNewLap) && !(currentGameState.PitData.JumpedToPits || playerCar.Live.TrackSurface == TrackSurfaces.NotInWorld);

            if (previousGameState != null)
            {
                currentGameState.SessionData.CurrentLapIsValid = (previousGameState.SessionData.CurrentLapIsValid && !currentGameState.PitData.JumpedToPits) || currentGameState.SessionData.IsNewLap;
            }

            currentGameState.SessionData.SectorNumber = currentSector;

            if (currentGameState.SessionData.IsNewSector || currentGameState.SessionData.IsNewLap)
            {
                Boolean lapValid = previousGameState != null && playerCar.Live.PreviousLapWasValid && previousGameState.SessionData.CurrentLapIsValid && !currentGameState.PitData.JumpedToPits;
                if (currentSector == 1 && currentGameState.SessionData.IsNewLap)
                {
                    currentGameState.SessionData.playerCompleteLapWithProvidedLapTime(currentGameState.SessionData.OverallPosition, currentGameState.SessionData.SessionRunningTime,
                        playerCar.Live.LapTimePrevious, lapValid, currentGameState.PitData.InPitlane, false, shared.Telemetry.TrackTempCrew, shared.Telemetry.AirTemp,
                        currentGameState.SessionData.SessionHasFixedTime, currentGameState.SessionData.SessionTimeRemaining, 3, currentGameState.TimingData, null, null);

                    if (currentGameState.carClass.carClassEnum == CarData.CarClassEnum.UNKNOWN_RACE)
                    {
                        currentGameState.carClass = CarData.getCarClassForIRacingId(playerCar.Car.CarClassId, playerCar.Car.CarId);
                    }
                }
                else if ((currentSector == 2 || currentSector == 3) && playerCar.Live.Lap > 0 && !currentGameState.PitData.JumpedToPits)
                {
                    currentGameState.SessionData.playerAddCumulativeSectorData(currentSector - 1, currentGameState.SessionData.OverallPosition, shared.Telemetry.LapCurrentLapTime,
                        currentGameState.SessionData.SessionRunningTime, currentGameState.SessionData.CurrentLapIsValid && !currentGameState.PitData.JumpedToPits, false, shared.Telemetry.TrackTempCrew, shared.Telemetry.AirTemp);
                }
            }

            if (currentGameState.SessionData.IsNewLap)
            {
                currentGameState.readLandmarksForThisLap = false;
            }

            if (currentGameState.SessionData.IsNewLap)
            {
                currentGameState.SessionData.playerStartNewLap(currentGameState.SessionData.CompletedLaps + 1,
                    currentGameState.SessionData.OverallPosition, currentGameState.PitData.InPitlane, playerCar.Live.GameTimeWhenLastCrossedSFLine);
            }

            currentGameState.PositionAndMotionData.DistanceRoundTrack = currentGameState.SessionData.TrackDefinition != null ? Math.Abs(currentGameState.SessionData.TrackDefinition.trackLength * playerCar.Live.CorrectedLapDistance) : -1.0f;
            currentGameState.PositionAndMotionData.CarSpeed = (float)shared.Telemetry.Speed;

            currentGameState.PositionAndMotionData.Orientation.Pitch = shared.Telemetry.Pitch;
            currentGameState.PositionAndMotionData.Orientation.Roll = shared.Telemetry.Roll;
            currentGameState.PositionAndMotionData.Orientation.Yaw = shared.Telemetry.Yaw;

            currentGameState.PositionAndMotionData.AccelerationVector.VertAccel = shared.Telemetry.VertAccel;
            currentGameState.PositionAndMotionData.AccelerationVector.LatAccel = shared.Telemetry.LatAccel;
            currentGameState.PositionAndMotionData.AccelerationVector.LongAccel = shared.Telemetry.LongAccel;

            //Console.WriteLine("LatAccel = " + shared.Telemetry.LatAccel.ToString());
            //Console.WriteLine("VertAccel = " + shared.Telemetry.VertAccel.ToString());
            //Console.WriteLine("LongAccel = " + shared.Telemetry.LongAccel.ToString());
       


            if (currentGameState.PitData.InPitlane)
            {
                if (currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionRunningTime > 10 &&
                    previousGameState != null && !previousGameState.PitData.InPitlane)
                {
                    currentGameState.PitData.NumPitStops++;
                }
                if (previousGameState != null && previousGameState.PitData.IsApproachingPitlane)
                {
                    currentGameState.PitData.OnInLap = true;
                    currentGameState.PitData.OnOutLap = false;
                }
            }
            if ((previousGameState != null && previousGameState.PitData.InPitlane && !currentGameState.PitData.InPitlane) ||
                currentGameState.PitData.JumpedToPits || (currentGameState.PitData.InPitlane && currentGameState.SessionData.IsNewLap))
            {
                currentGameState.PitData.OnInLap = false;
                currentGameState.PitData.OnOutLap = true;
            }
            if (currentGameState.SessionData.IsNewLap && playerCar.Live.TrackSurface != TrackSurfaces.AproachingPits)
            {
                // starting a new lap while not in the pitlane so clear the in / out lap flags
                currentGameState.PitData.OnInLap = false;
                currentGameState.PitData.OnOutLap = false;
            }

            if (previousGameState != null && currentGameState.PitData.OnOutLap && previousGameState.PitData.InPitlane && !currentGameState.PitData.InPitlane)
            {
                currentGameState.PitData.IsAtPitExit = true;
            }
            //Console.WriteLine("Voltage: " + shared.Telemetry.Voltage);
            SessionFlags flag = (SessionFlags)shared.Telemetry.SessionFlags;
            if (flag.HasFlag(SessionFlags.Black) && !flag.HasFlag(SessionFlags.Furled))
            {
                currentGameState.PenaltiesData.HasPitStop = true;
            }
            if (flag.HasFlag(SessionFlags.Furled) && currentGameState.SessionData.SessionType != SessionType.Qualify)
            {
                currentGameState.PenaltiesData.HasSlowDown = true;
            }
            if (flag.HasFlag(SessionFlags.Repair))
            {
                currentGameState.PenaltiesData.HasMeatballFlag = true;
            }
            if (flag.HasFlag(SessionFlags.YellowWaving))
            {
                currentGameState.SessionData.Flag = FlagEnum.YELLOW;
            }
            else if (previousGameState != null && !previousGameState.SessionData.Flag.HasFlag(FlagEnum.BLUE) && flag.HasFlag(SessionFlags.Blue) && !flag.HasFlag(SessionFlags.Green))
            {
                currentGameState.SessionData.Flag = FlagEnum.BLUE;
            }
            /*else if (flag.HasFlag(SessionFlags.CautionWaving))
            {
                currentGameState.SessionData.Flag = FlagEnum.DOUBLE_YELLOW;
            }*/
            if (flag.HasFlag(SessionFlags.White))
            {
                if (GlobalBehaviourSettings.useAmericanTerms)
                {
                    currentGameState.SessionData.Flag = FlagEnum.WHITE;
                }
                currentGameState.SessionData.IsLastLap = true;
            }
            currentGameState.PitData.PitSpeedLimit = shared.SessionData.Track.TrackPitSpeedLimit;
            float speed = (float)playerCar.Live.Speed;
            bool hasValidSpeed = speed > 0 && speed < 110;  // we get some wild speed data occasionally (>400m/s)
            currentGameState.SessionData.DeltaTime.SetNextDeltaPoint(currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.SessionData.CompletedLaps,
                speed, currentGameState.Now, 
                hasValidSpeed && !currentGameState.PitData.InPitlane && !currentGameState.PitData.IsApproachingPitlane && !currentGameState.PitData.OnOutLap && playerCar.Live.TrackSurface != TrackSurfaces.NotInWorld);

            if (previousGameState != null)
            {
                String stoppedInLandmark = currentGameState.SessionData.trackLandmarksTiming.updateLandmarkTiming(currentGameState.SessionData.TrackDefinition,
                    currentGameState.SessionData.SessionRunningTime, previousGameState.PositionAndMotionData.DistanceRoundTrack,
                    currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.PositionAndMotionData.CarSpeed, currentGameState.carClass);
                currentGameState.SessionData.stoppedInLandmark = currentGameState.PitData.InPitlane ? null : stoppedInLandmark;
            }
            if (playerCar.Live.HasCrossedSFLine)
            {
                currentGameState.SessionData.trackLandmarksTiming.cancelWaitingForLandmarkEnd();
            }

            GameStateData.Multiclass = shared.SessionData.NumCarClasses > 1;
            GameStateData.NumberOfClasses = shared.SessionData.NumCarClasses;

            List<double> combinedStrengthOfField = new List<double>();
            foreach (Driver driver in shared.Drivers)
            {
                String opponentDataKey = driver.Id.ToString();

                if (driver.IsPaceCar || driver.IsSpectator)
                {
                    continue;
                }
                combinedStrengthOfField.Add(driver.IRating);

                if (driver.IsCurrentDriver || currentGameState.disqualifiedDriverNames.Contains(driver.Name) || currentGameState.retriedDriverNames.Contains(driver.Name))
                {
                    continue;
                }

                if (driver.CurrentResults.OutReasonId == ReasonOutId.IDS_DISQUALIFIED)
                {
                    // remove this driver from the set immediately
                    if (!currentGameState.disqualifiedDriverNames.Contains(driver.Name))
                    {
                        Console.WriteLine("Opponent " + driver.Name + " has been disqualified");
                        currentGameState.disqualifiedDriverNames.Add(driver.Name);
                    }
                    currentGameState.OpponentData.Remove(opponentDataKey);
                    continue;
                }

                if (driver.CurrentResults.IsOut)
                {
                    // remove this driver from the set immediately
                    if (!currentGameState.retriedDriverNames.Contains(driver.Name))
                    {
                        Console.WriteLine("Opponent " + driver.Name + " has retired");
                        currentGameState.retriedDriverNames.Add(driver.Name);
                    }
                    currentGameState.OpponentData.Remove(opponentDataKey);
                    continue;
                }

                String driverName = driver.Name;
                lastActiveTimeForOpponents[opponentDataKey] = currentGameState.Now;
                Boolean createNewDriver = true;
                OpponentData currentOpponentData = null;
                if (currentGameState.OpponentData.TryGetValue(opponentDataKey, out currentOpponentData))
                {
                    // if we're team racing or the car number matches, update this driver.
                    if (shared.SessionData.IsTeamRacing || (driver.CarNumber.Equals(currentOpponentData.CarNumber)))
                    {
                        createNewDriver = false;
                        if (previousGameState != null)
                        {
                            OpponentData previousOpponentData = null;
                            int previousOpponentSectorNumber = 0;
                            int previousOpponentCompletedLaps = 0;
                            int previousOpponentOverallPosition = 0;
                            Boolean previousOpponentIsEnteringPits = false;
                            float previousOpponentSpeed = 0;
                            float previousDistanceRoundTrack = 0;
                            bool previousIsInPits = false;
                            bool hasCrossedSFLine = false;
                            bool previousIsApporchingPits = false;
                            Boolean jumpedToPits = false;
                            TrackSurfaces previousTrackSurface = TrackSurfaces.NotInWorld;
                            float previousOpponentGameTimeWhenLastCrossedStartFinishLine = -1;
                            OpponentData opponentPrevious = null;
                            if (previousGameState.OpponentData.TryGetValue(opponentDataKey, out opponentPrevious))
                            {
                                previousOpponentData = opponentPrevious;
                                previousOpponentSectorNumber = previousOpponentData.CurrentSectorNumber;
                                previousOpponentCompletedLaps = previousOpponentData.CompletedLaps;
                                previousOpponentOverallPosition = previousOpponentData.OverallPosition;
                                previousOpponentIsEnteringPits = previousOpponentData.isEnteringPits();
                                previousOpponentSpeed = previousOpponentData.Speed;
                                previousDistanceRoundTrack = previousOpponentData.DistanceRoundTrack;
                                previousIsInPits = previousOpponentData.InPits;
                                previousOpponentGameTimeWhenLastCrossedStartFinishLine = previousOpponentData.GameTimeWhenLastCrossedStartFinishLine;
                                previousTrackSurface = (TrackSurfaces)previousOpponentData.trackSurface;
                                previousIsApporchingPits = previousOpponentData.isApporchingPits;
                                currentOpponentData.ClassPositionAtPreviousTick = previousOpponentData.ClassPosition;
                                currentOpponentData.OverallPositionAtPreviousTick = previousOpponentData.OverallPosition;
                            }
                            bool isInWorld = driver.Live.TrackSurface != TrackSurfaces.NotInWorld;
                            hasCrossedSFLine = driver.Live.HasCrossedSFLine;
                            int currentOpponentSector = isInWorld ? driver.Live.CurrentSector : 0;                            
                            currentOpponentData.IsActive = true;
                            currentOpponentData.trackSurface = (int)driver.Live.TrackSurface;
                            bool previousOpponentLapValid = driver.Live.PreviousLapWasValid;
                            currentOpponentData.isApporchingPits = false;
                            if (driver.Live.TrackSurface == TrackSurfaces.AproachingPits || previousTrackSurface == TrackSurfaces.OnTrack)
                            {
                                if((driver.Live.TrackSurface == TrackSurfaces.AproachingPits && previousTrackSurface == TrackSurfaces.OnTrack) || //allow only aproach to pit if previous state was on track
                                    (driver.Live.TrackSurface == TrackSurfaces.AproachingPits && !shared.Telemetry.CarIdxOnPitRoad[driver.Id] && previousIsApporchingPits))
                                {
                                    currentOpponentData.isApporchingPits = true;
                                }                                
                            }
                            //currentOpponentData.isApporchingPits = driver.Live.TrackSurface == TrackSurfaces.AproachingPits && !shared.Telemetry.CarIdxOnPitRoad[driver.Id] && !previousIsInPits;
                            // TODO:
                            // reset to to pitstall
                            bool currentOpponentLapValid = true;
                            if ((previousOpponentSectorNumber == 1 || previousOpponentSectorNumber == 2) && !previousIsInPits && shared.Telemetry.CarIdxOnPitRoad[driver.Id])
                            {
                                currentOpponentLapValid = false;
                            }
                                                        
                            int currentOpponentOverallPosition = currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionPhase != SessionPhase.Finished && previousOpponentOverallPosition > 0 ?
                                getRacePosition(opponentDataKey, previousOpponentOverallPosition, driver.Live.Position, currentGameState.Now)
                                : driver.Live.Position;
                            if(currentGameState.FlagData.isFullCourseYellow)
                            {
                                currentOpponentOverallPosition = driver.CurrentResults.Position;
                            }
                            if(useQualifyingPosition)
                            {
                                currentOpponentOverallPosition = driver.CurrentResults.QualifyingPosition;
                                currentOpponentData.ClassPosition = driver.CurrentResults.ClassQualifyingPosition;
                            }
                            int currentOpponentLapsCompleted = driver.Live.LiveLapsCompleted;

                            if (currentOpponentSector == 0)
                            {
                                currentOpponentSector = previousOpponentSectorNumber;
                            }
                            float currentOpponentLapDistance = isInWorld && currentGameState.SessionData.TrackDefinition != null ? currentGameState.SessionData.TrackDefinition.trackLength * driver.Live.CorrectedLapDistance : 0;
                            float currentOpponentSpeed = isInWorld ? (float)driver.Live.Speed : 0;
                            //Console.WriteLine("lapdistance:" + currentOpponentLapDistance);
                            bool opponentHasValidSpeed = currentOpponentSpeed > 7 && currentOpponentSpeed < 110;    // we get some wild speed data occasionally (>400m/s)
                            currentOpponentData.DeltaTime.SetNextDeltaPoint(currentOpponentLapDistance, currentOpponentLapsCompleted, currentOpponentSpeed, currentGameState.Now,
                                opponentHasValidSpeed && driver.Live.TrackSurface != TrackSurfaces.AproachingPits && !shared.Telemetry.CarIdxOnPitRoad[driver.Id] && driver.Live.TrackSurface != TrackSurfaces.NotInWorld);

                            Boolean finishedAllottedRaceLaps = currentGameState.SessionData.SessionNumberOfLaps > 0 && currentGameState.SessionData.SessionNumberOfLaps == currentOpponentLapsCompleted;
                            Boolean finishedAllottedRaceTime = false;

                            if (currentGameState.SessionData.SessionTotalRunTime > 0 && currentGameState.SessionData.SessionTimeRemaining <= 0)
                            {
                                if (previousOpponentCompletedLaps < currentOpponentLapsCompleted)
                                {
                                    // timed session, we've started a new lap after the time has reached zero. Where there's no extra lap this means we've finished. If there's 1 or more
                                    // extras he's finished when he's started more than the extra laps number
                                    currentOpponentData.LapsStartedAfterRaceTimeEnd++;
                                }
                                finishedAllottedRaceTime = currentOpponentData.LapsStartedAfterRaceTimeEnd > currentGameState.SessionData.ExtraLapsAfterTimedSessionComplete;
                            }

                            if (currentOpponentOverallPosition == 1 && (finishedAllottedRaceTime || finishedAllottedRaceLaps))
                            {
                                currentGameState.SessionData.LeaderHasFinishedRace = true;
                            }

                            currentOpponentData.LicensLevel = driver.licensLevel;
                            currentOpponentData.iRating = driver.IRating;

                            updateOpponentData(currentOpponentData, driverName, driver.CustId, currentOpponentOverallPosition, currentOpponentLapsCompleted,
                                        currentOpponentSector, (float)driver.Live.LapTimePrevious, driver.Live.Gear,driver.Live.Rpm,driver.Live.SteeringAngle,
                                        shared.Telemetry.CarIdxOnPitRoad[driver.Id] || driver.Live.TrackSurface == TrackSurfaces.InPitStall, previousIsApporchingPits,
                                        previousOpponentLapValid, currentOpponentLapValid, currentGameState.SessionData.SessionRunningTime, currentOpponentLapDistance,
                                        currentGameState.SessionData.SessionHasFixedTime, currentGameState.SessionData.SessionTimeRemaining,
                                        currentGameState.SessionData.SessionType == SessionType.Race, shared.Telemetry.TrackTempCrew,
                                        shared.Telemetry.AirTemp, currentOpponentSpeed, previousOpponentSpeed, driver.Live.GameTimeWhenLastCrossedSFLine, driver.Live.IsNewLap,
                                        driver.Car.CarClassId, driver.Car.CarId, currentGameState.TimingData, currentGameState.carClass, shared.SessionData.IsTeamRacing);

                            //allow gaps in qual and prac, delta here is not on track delta but diff on fastest time 
                            if (currentGameState.SessionData.SessionType != SessionType.Race)
                            {
                                currentOpponentData.ClassPosition = driver.Live.ClassPosition;
                                if (currentOpponentData.ClassPosition == currentGameState.SessionData.ClassPosition + 1)
                                {
                                    currentGameState.SessionData.TimeDeltaBehind = Math.Abs(currentOpponentData.CurrentBestLapTime - currentGameState.SessionData.PlayerLapTimeSessionBest);
                                }
                                if (currentOpponentData.ClassPosition == currentGameState.SessionData.ClassPosition - 1)
                                {
                                    currentGameState.SessionData.TimeDeltaFront = Math.Abs(currentGameState.SessionData.PlayerLapTimeSessionBest - currentOpponentData.CurrentBestLapTime);
                                }
                            }

                            if (currentGameState.SessionData.SessionType == SessionType.Race && !useQualifyingPosition)
                            {
                                currentOpponentData.ClassPosition = driver.Live.ClassPosition;
                            }

                            if (previousOpponentData != null)
                            {
                                currentOpponentData.trackLandmarksTiming = previousOpponentData.trackLandmarksTiming;
                                String stoppedInLandmark = currentOpponentData.trackLandmarksTiming.updateLandmarkTiming(
                                    currentGameState.SessionData.TrackDefinition, currentGameState.SessionData.SessionRunningTime,
                                    previousDistanceRoundTrack, currentOpponentData.DistanceRoundTrack, currentOpponentData.Speed, currentOpponentData.CarClass);
                                currentOpponentData.stoppedInLandmark = shared.Telemetry.CarIdxOnPitRoad[driver.Id] || !isInWorld || finishedAllottedRaceTime || finishedAllottedRaceLaps || driver.Live.TrackSurface == TrackSurfaces.AproachingPits ? null : stoppedInLandmark;
                            }
                            if (currentGameState.SessionData.JustGoneGreen)
                            {
                                currentOpponentData.trackLandmarksTiming = new TrackLandmarksTiming();
                            }
                            if (hasCrossedSFLine)
                            {
                                currentOpponentData.trackLandmarksTiming.cancelWaitingForLandmarkEnd();
                            }
                            if (currentOpponentData.CurrentBestLapTime > 1)
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
                                if (CarData.IsCarClassEqual(currentOpponentData.CarClass, currentGameState.carClass))
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
                    else
                    {
                        Console.WriteLine("Removing driver " + currentOpponentData.DriverRawName + " (custId " + currentOpponentData.CostId +
                            ") and replacing with " + driverName + " (custId " + driver.CustId +")");
                        currentGameState.OpponentData.Remove(opponentDataKey);
                    }
                }
                if (createNewDriver && currentGameState.SessionData.TrackDefinition != null)
                {
                    if (!driver.CurrentResults.IsOut || !driver.IsPaceCar || !driver.IsSpectator)
                    {
                        currentGameState.OpponentData.Add(opponentDataKey, createOpponentData(driver,
                            false, currentGameState.SessionData.TrackDefinition.trackLength));
                    }
                }


            }
            if (currentGameState.Now > nextOpponentCleanupTime)
            {
                nextOpponentCleanupTime = currentGameState.Now + opponentCleanupInterval;
                DateTime oldestAllowedUpdate = currentGameState.Now - opponentCleanupInterval;
                foreach (KeyValuePair<string, OpponentData> entry in currentGameState.OpponentData)
                {
                    DateTime lastTimeForOpponent = DateTime.MinValue;
                    if (!lastActiveTimeForOpponents.TryGetValue(entry.Key, out lastTimeForOpponent) || (lastTimeForOpponent < oldestAllowedUpdate && entry.Value.IsActive))
                    {
                        entry.Value.IsActive = false;
                        entry.Value.InPits = true;
                        entry.Value.Speed = 0;
                        entry.Value.stoppedInLandmark = null;
                        entry.Value.trackLandmarksTiming.cancelWaitingForLandmarkEnd();
                        entry.Value.DeltaTime.SetNextDeltaPoint(0, entry.Value.CompletedLaps, 0, currentGameState.Now, !entry.Value.InPits);
                        Console.WriteLine("Opponent " + entry.Value.DriverRawName + "(index " + entry.Key + ") has been inactive for " + opponentCleanupInterval + ", sending him back to pits");
                    }
                }
            }

            //Sof calculations
            if (combinedStrengthOfField.Count > 0)
            {
                double baseSof = 1350 / Math.Log(2);
                double sofExpSum = 0;
                foreach (double ir in combinedStrengthOfField)
                {
                    sofExpSum += Math.Exp(-ir / baseSof);
                }
                currentGameState.SessionData.StrengthOfField = (int)Math.Round(Math.Floor(baseSof * Math.Log(combinedStrengthOfField.Count / sofExpSum)));
            }

            if (currentGameState.SessionData.IsNewLap && currentGameState.SessionData.PreviousLapWasValid &&
                currentGameState.SessionData.LapTimePrevious > 1)
            {
                if (currentGameState.SessionData.PlayerLapTimeSessionBest == -1 ||
                     currentGameState.SessionData.LapTimePrevious < currentGameState.SessionData.PlayerLapTimeSessionBest)
                {
                    currentGameState.SessionData.PlayerLapTimeSessionBest = currentGameState.SessionData.LapTimePrevious;

                    if (currentGameState.SessionData.OverallSessionBestLapTime == -1 ||
                        currentGameState.SessionData.LapTimePrevious < currentGameState.SessionData.OverallSessionBestLapTime)
                    {
                        currentGameState.SessionData.OverallSessionBestLapTime = currentGameState.SessionData.LapTimePrevious;
                    }

                    if (currentGameState.SessionData.PlayerClassSessionBestLapTime == -1 ||
                        currentGameState.SessionData.LapTimePrevious < currentGameState.SessionData.PlayerClassSessionBestLapTime)
                    {
                        currentGameState.SessionData.PlayerClassSessionBestLapTime = currentGameState.SessionData.LapTimePrevious;
                    }
                }
            }


            if(playerCar.Car.CarIsElectric)
            {
                currentGameState.BatteryData.BatteryUseActive = true;
                currentGameState.BatteryData.BatteryCapacity = playerCar.Car.DriverCarFuelMaxKWh * playerCar.Car.DriverCarMaxFuelPct;
                currentGameState.BatteryData.BatteryPercentageLeft =  shared.Telemetry.FuelLevel;
                currentGameState.PitData.IsElectricVehicleSwapAllowed = true;
            }
            else
            {
                currentGameState.FuelData.FuelUseActive = true;
                currentGameState.FuelData.FuelPressure = shared.Telemetry.FuelPress;
                currentGameState.FuelData.FuelLeft = shared.Telemetry.FuelLevel;
                currentGameState.FuelData.FuelCapacity = playerCar.Car.DriverCarFuelMaxLtr * playerCar.Car.DriverCarMaxFuelPct;
            }


            if (currentGameState.carClass.limiterAvailable)
            {
                currentGameState.PitData.limiterStatus = shared.Telemetry.EngineWarnings.HasFlag(EngineWarnings.PitSpeedLimiter) == true ? PitData.LimiterStatus.ACTIVE : PitData.LimiterStatus.INACTIVE;
            }

            //conditions
            if (currentGameState.Now > nextConditionsSampleDue)
            {
                nextConditionsSampleDue = currentGameState.Now.Add(ConditionsMonitor.ConditionsSampleFrequency);
                currentGameState.Conditions.addSample(currentGameState.Now, currentGameState.SessionData.CompletedLaps, currentGameState.SessionData.SectorNumber,
                    shared.Telemetry.AirTemp, shared.Telemetry.TrackTempCrew, 0, shared.Telemetry.WindVel, 0, 0, 0, currentGameState.SessionData.IsNewLap, ConditionsMonitor.TrackStatus.UNKNOWN);
            }

            currentGameState.PenaltiesData.IsOffRacingSurface = shared.Telemetry.PlayerTrackSurface == TrackSurfaces.OffTrack;

            if (invalidateCutTrackLaps && !currentGameState.PitData.OnOutLap && previousGameState != null &&
                !(currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.SessionPhase == SessionPhase.Countdown) &&
                previousGameState.SessionData.CurrentIncidentCount + 1 == shared.Telemetry.PlayerCarMyIncidentCount /*&& 
                currentGameState.SessionData.TrackSurface == (int)TrackSurfaces.OffTrack*/)
            {
                currentGameState.PenaltiesData.CutTrackWarnings = previousGameState.PenaltiesData.CutTrackWarnings + 1;
                currentGameState.SessionData.CurrentLapIsValid = false;
            }

            currentGameState.SessionData.CurrentIncidentCount = shared.Telemetry.PlayerCarMyIncidentCount;
            currentGameState.SessionData.CurrentDriverIncidentCount = shared.Telemetry.PlayerCarDriverIncidentCount;
            currentGameState.SessionData.CurrentTeamIncidentCount = shared.Telemetry.PlayerCarTeamIncidentCount;
            currentGameState.SessionData.HasLimitedIncidents = shared.SessionData.IsLimitedIncidents;
            currentGameState.SessionData.MaxIncidentCount = shared.SessionData.IncidentLimit;
            if(currentGameState.PenaltiesData.HasSlowDown)
            {
                currentGameState.PenaltiesData.NumOutstandingPenalties++;
            }
            if (currentGameState.PenaltiesData.HasPitStop)
            {
                currentGameState.PenaltiesData.NumOutstandingPenalties++;
            }
            currentGameState.TyreData.FrontLeftPressure = shared.Telemetry.LFcoldPressure;
            currentGameState.TyreData.FrontRightPressure = shared.Telemetry.RFcoldPressure;
            currentGameState.TyreData.RearLeftPressure = shared.Telemetry.LRcoldPressure;
            currentGameState.TyreData.RearRightPressure = shared.Telemetry.RRcoldPressure;
            
            if (previousGameState != null)
            {
                currentGameState.FrozenOrderData = GetFrozenOrderData(currentGameState, previousGameState, flag, shared.SessionData.StartingGridPoleOnLeft);
            }
            else
            {
                currentGameState.FrozenOrderData = new FrozenOrderData();
            }

            if (previousGameState != null
                && currentGameState.SessionData.SessionType == SessionType.Race
                && currentGameState.SessionData.SessionPhase == SessionPhase.Finished
                && previousGameState.SessionData.SessionPhase != SessionPhase.Finished)
            {
                var driverInfo = new List<Tuple<int, string, int, double, double, Driver.FinishState>>();
                foreach (var driver in shared.Drivers)
                {
                    if (driver.IsSpectator || driver.IsPaceCar || driver.CurrentResults.IsOut)
                    {
                        continue;
                    }

                    var delayedPosition = -1;
                    OpponentData od = null;
                    if (currentGameState.OpponentData.TryGetValue(driver.Id.ToString(), out od))
                    {
                        delayedPosition = od.OverallPosition;
                    }
                    else if (driver.Id == playerCar.Id)  // Player.
                    {
                        delayedPosition = currentGameState.SessionData.OverallPosition;
                    }
                    else  // Retired
                    {
                        delayedPosition = driver.Live.Position;
                    }

                    driverInfo.Add(new Tuple<int, string, int, double, double, Driver.FinishState>(
                        delayedPosition,
                        driver.Name,
                        driver.Live.LiveLapsCompleted,
                        driver.Live.TotalLapDistance,
                        driver.Live.TotalLapDistanceCorrected,
                        driver.FinishStatus));
                }

                Console.WriteLine("Estimated standings:");
                foreach (var driver in driverInfo.OrderBy(d => d.Item1))
                {
                    Console.WriteLine("P:{0}  Name:{1}  LLC:{2}  TLD:{3}  TLDC:{4}  FS:{5}",
                        driver.Item1,
                        driver.Item2,
                        driver.Item3,
                        driver.Item4,
                        driver.Item5,
                        driver.Item6);
                }
            }
            if (currentGameState.SessionData.TrackDefinition != null)
            {
                CrewChief.trackName = currentGameState.SessionData.TrackDefinition.name;
            }
            if (currentGameState.carClass != null)
            {
                CrewChief.carClass = currentGameState.carClass.carClassEnum;
            }
            currentGameState.carName = shared.Driver.Car.CarName;
            CrewChief.distanceRoundTrack = currentGameState.PositionAndMotionData.DistanceRoundTrack;
            CrewChief.viewingReplay = false;
            if (currentGameState.SessionData.IsNewLap)
            {
                if (currentGameState.hardPartsOnTrackData.updateHardPartsForNewLap(currentGameState.SessionData.LapTimePrevious))
                {
                    currentGameState.SessionData.TrackDefinition.adjustGapPoints(currentGameState.hardPartsOnTrackData.processedHardPartsForBestLap);
                }
            }
            else if (!currentGameState.SessionData.TrackDefinition.isOval &&
                !(currentGameState.SessionData.SessionType == SessionType.Race && !currentGameState.PitData.OnOutLap &&
                   (currentGameState.SessionData.CompletedLaps < 1 || (GameStateData.useManualFormationLap && currentGameState.SessionData.CompletedLaps < 2))))// if(!currentGameState.PitData.OnOutLap*/)
            {
                currentGameState.hardPartsOnTrackData.mapHardPartsOnTrack(currentGameState.ControlData.BrakePedal, currentGameState.ControlData.ThrottlePedal,
                    currentGameState.PositionAndMotionData.DistanceRoundTrack, currentGameState.SessionData.CurrentLapIsValid && !currentGameState.PitData.InPitlane,
                    currentGameState.SessionData.TrackDefinition.trackLength);
            }
                                           
            return currentGameState;
        }

        private void updateOvertakingAids(GameStateData currentGameState, DrsStatus drsStatus)
        {
            currentGameState.OvertakingAids.DrsDetected = drsStatus == DrsStatus.DrsDetected;
            currentGameState.OvertakingAids.DrsAvailable = drsStatus == DrsStatus.DrsAvailable;
            currentGameState.OvertakingAids.DrsEngaged = drsStatus == DrsStatus.DrsEnabled;
        }

        private void updateOpponentData(OpponentData opponentData, String driverName, int CostId, int racePosition, int completedLaps,
            int sector, float completedLapTime, int gear, float rpm,float steeringAngle, Boolean isInPits, bool previousIsApporchingPits,
            Boolean previousLapWasValid, Boolean currentLapValid, float sessionRunningTime,
            float distanceRoundTrack, Boolean sessionLengthIsTime, float sessionTimeRemaining,
            Boolean isRace, float airTemperature, float trackTempreture, float speed, float previousSpeed,
            float GameTimeWhenLastCrossedStartFinishLine,bool isNewLap, int carClassId, int carId, TimingData timingData, CarData.CarClass playerCarClass, Boolean IsTeamRacing)
        {
            if (!opponentData.DriverRawName.Equals(driverName) && IsTeamRacing)
            {
                Console.WriteLine("Driver " + opponentData.DriverRawName + " has been swapped for " + driverName);
                opponentData.DriverRawName = driverName;
                opponentData.CostId = CostId;
                if (speechRecogniser != null) speechRecogniser.addNewOpponentName(driverName, opponentData.CarNumber);
                SoundCache.loadDriverNameSound(DriverNameHelper.getUsableDriverName(opponentData.DriverRawName));
            }
            Boolean validSpeed = true;
            if (speed > 500)
            {
                // faster than 500m/s (1000+mph) suggests the player has quit to the pit. Might need to reassess this as the data are quite noisy
                validSpeed = false;
                opponentData.Speed = 0;
                //Console.WriteLine(opponentData.DriverRawName + " invalidating lap based of car speed = " + speed + "m/s");
            }
            else if(speed < -20f)
            {
                opponentData.Speed = previousSpeed;
            }
            else
            {
                opponentData.Speed= speed;
            }
            if (opponentData.OverallPosition != racePosition)
            {
                opponentData.SessionTimeAtLastPositionChange = sessionRunningTime;
            }
            opponentData.IsNewLap = false;
            opponentData.OverallPosition = racePosition;
            opponentData.DistanceRoundTrack = distanceRoundTrack;
            opponentData.Gear = gear;
            opponentData.RPM = rpm;
            opponentData.SteeringWheelAngle = steeringAngle;
            //Check that previous state was IsApporchingPits, this includes the zone befor the pitlane(striped lines on track)
            opponentData.JustEnteredPits = previousIsApporchingPits && isInPits;

            if (sessionRunningTime > 10 && isRace && !opponentData.InPits && isInPits)
            {
                opponentData.NumPitStops++;
            }
            opponentData.InPits = isInPits;
            if (isNewLap)
            {
                if (opponentData.OpponentLapData.Count > 0)
                {
                    opponentData.CompleteLapThatMightHaveMissingSectorTimes(racePosition, sessionRunningTime, completedLapTime, 
                        completedLapTime > 1 && validSpeed, false, trackTempreture, airTemperature, sessionLengthIsTime, sessionTimeRemaining, 3,
                        timingData, CarData.IsCarClassEqual(opponentData.CarClass, playerCarClass));
                    //Console.WriteLine(opponentData.ToString() + " time: " + TimeSpan.FromSeconds(completedLapTime).ToString(@"mm\:ss\.fff") + " lap valid: " + (completedLapTime > 1 && validSpeed) + " Updated From Live Data " + isInWorld); 
                }                
                opponentData.StartNewLap(completedLaps + 1, racePosition, isInPits, GameTimeWhenLastCrossedStartFinishLine, false, trackTempreture, airTemperature);
                opponentData.IsNewLap = true;
            }
            if (opponentData.CurrentSectorNumber != sector)
            {
                if (opponentData.CurrentSectorNumber == 1 && sector == 2 || opponentData.CurrentSectorNumber == 2 && sector == 3)
                {               
                    if (opponentData.CurrentSectorNumber == 1 && opponentData.CarClass.carClassEnum == CarData.CarClassEnum.UNKNOWN_RACE)
                    {
                        // re-evaluate the car class
                        opponentData.CarClass = CarData.getCarClassForIRacingId(carClassId, carId);
                    }
                    opponentData.AddCumulativeSectorData(opponentData.CurrentSectorNumber, racePosition, -1, sessionRunningTime, currentLapValid && validSpeed, false, trackTempreture, airTemperature);
                }
                opponentData.CurrentSectorNumber = sector;
            }
            opponentData.CompletedLaps = completedLaps;
            if (opponentData.JustEnteredPits)
            {
                opponentData.setInLap();
            }
        }
         
        private static Dictionary<String, SessionType> sessionTypeMap = new Dictionary<String, SessionType>()
        {
            {"Offline Testing", SessionType.LonePractice},
            {"Practice", SessionType.Practice},
            {"Lone Practice", SessionType.LonePractice},
            {"Warmup", SessionType.Practice},
            {"Open Qualify", SessionType.Qualify},
            {"Lone Qualify", SessionType.Qualify},
            {"Race", SessionType.Race}
        };

        public SessionType mapToSessionType(Object memoryMappedFileStruct)
        {
            String sessionString = (String)memoryMappedFileStruct;
            SessionType st = SessionType.Unavailable;
            if (sessionTypeMap.TryGetValue(sessionString, out st))
            {
                return st;
            }
            else
            {
                Console.WriteLine("Unrecognized SessionType: " + sessionString);
            }
            return SessionType.Unavailable;
        }
        SessionFlags previousSessionFlags;
        SessionStates previousSessionState;
        private SessionPhase mapToSessionPhase(SessionPhase lastSessionPhase, SessionStates sessionState, SessionType currentSessionType, float thisSessionRunningTime,
            int previousLapsCompleted, int laps, SessionFlags sessionFlags, bool IsFullCourseCautions, SafetyCarData currentSafetyCarData, SafetyCarData previousSafetyCarData, 
            int formationLapCount, float pitEntranceDistanceRoundTrack)
        {
           /* if (previousSessionFlags != sessionFlags)
            {
                Console.WriteLine("Previous sessionFlags: " + previousSessionFlags);
                Console.WriteLine("Current sessionFlags: " + sessionFlags);
                previousSessionFlags = sessionFlags; 
            }
            if (previousSessionState != sessionState)
            {
                Console.WriteLine("Previous sessionState: " + previousSessionState);
                Console.WriteLine("Current sessionState: " + sessionState);
                previousSessionState = sessionState;
            }*/
            if (currentSessionType == SessionType.Practice)
            {
                if (sessionState == SessionStates.CoolDown)
                {
                    return SessionPhase.Finished;
                }
                else if (sessionState.HasFlag(SessionStates.Checkered))
                {
                    return SessionPhase.Checkered;
                }
                else if (lastSessionPhase == SessionPhase.Unavailable)
                {
                    return SessionPhase.Countdown;
                }

                return SessionPhase.Green;
            }
            else if (currentSessionType == SessionType.LonePractice)
            {
                if (sessionState == SessionStates.CoolDown)
                {
                    return SessionPhase.Finished;
                }
                else if (sessionState.HasFlag(SessionStates.Checkered))
                {
                    return SessionPhase.Checkered;
                }
                else if (lastSessionPhase == SessionPhase.Unavailable)
                {
                    return SessionPhase.Countdown;
                }

                return SessionPhase.Green;
            }
            else if (currentSessionType == SessionType.Qualify)
            {
                if (sessionState == SessionStates.GetInCar)
                {
                    return SessionPhase.Unavailable;
                }
                if (sessionState == SessionStates.CoolDown)
                {
                    return SessionPhase.Finished;
                }
                else if (sessionState == SessionStates.Checkered)
                {
                    return SessionPhase.Checkered;
                }
                else if (!sessionFlags.HasFlag(SessionFlags.Green) && sessionFlags.HasFlag(SessionFlags.OneLapToGreen) && lastSessionPhase != SessionPhase.Green)
                {
                    return SessionPhase.Countdown;
                }
                else if (sessionFlags.HasFlag(SessionFlags.Green) || 
                    sessionState.HasFlag(SessionStates.Racing) /* covers mid-session join in hosted racing */)
                {
                    return SessionPhase.Green;
                }

                return lastSessionPhase;
            }
            else if (currentSessionType.HasFlag(SessionType.Race))
            {
                if (sessionState.HasFlag(SessionStates.Checkered) || sessionState.HasFlag(SessionStates.CoolDown))
                {
                    if (lastSessionPhase == SessionPhase.Green || lastSessionPhase == SessionPhase.FullCourseYellow
                         || lastSessionPhase == SessionPhase.Checkered)
                    {
                        if (playerCar.FinishStatus == Driver.FinishState.Finished)
                        {
                            Console.WriteLine("Finished - completed " + laps + " laps (was " + previousLapsCompleted + "), session running time = " +
                                thisSessionRunningTime);
                            return SessionPhase.Finished;
                        }
                        else if (sessionFlags.HasFlag(SessionFlags.Checkered))
                        {
                            if (lastSessionPhase != SessionPhase.Checkered)
                            {
                                Console.WriteLine("Checkered flag - completed " + laps + " laps (was " + previousLapsCompleted + "), session running time = " +
                                    thisSessionRunningTime);
                            }
                            return SessionPhase.Checkered;
                        }
                    }
                }
                else if (sessionState == SessionStates.GetInCar)
                {
                    return SessionPhase.Unavailable;
                }
                else if(sessionState == SessionStates.Warmup && !sessionFlags.HasFlag(SessionFlags.StartSet) && !sessionFlags.HasFlag(SessionFlags.StartGo)) 
                {
                    return SessionPhase.Gridwalk;
                }
                else if (sessionState.HasFlag(SessionStates.ParadeLaps) && !sessionFlags.HasFlag(SessionFlags.StartGo))                
                {
                    if ((lastSessionPhase == SessionPhase.Formation || lastSessionPhase == SessionPhase.Countdown) && 
                        (/*(currentSafetyCarData.Lap == formationLapCount && currentSafetyCarData.CurrentSector == 3 && pitEntranceDistanceRoundTrack == -1.0f) ||*/
                        (currentSafetyCarData.Lap == formationLapCount && currentSafetyCarData.DistanceRoundTrack >= pitEntranceDistanceRoundTrack && pitEntranceDistanceRoundTrack != -1.0f) ||
                         (previousSafetyCarData != null && (TrackSurfaces)currentSafetyCarData.TrackSurface == TrackSurfaces.AproachingPits && (TrackSurfaces)previousSafetyCarData.TrackSurface == TrackSurfaces.OnTrack)))
                    {
                        return SessionPhase.Countdown;                       
                    }
                    else if(lastSessionPhase != SessionPhase.Countdown)
                    {
                        return SessionPhase.Formation;
                    }                    
                }
                else if ((sessionFlags.HasFlag(SessionFlags.StartSet) && !sessionFlags.HasFlag(SessionFlags.StartGo)))
                {
                    return SessionPhase.Countdown;
                }
                else if ( IsFullCourseCautions && ((lastSessionPhase == SessionPhase.Green && sessionFlags.HasFlag(SessionFlags.CautionWaving)) || 
                    (lastSessionPhase == SessionPhase.FullCourseYellow && sessionFlags.HasFlag(SessionFlags.CautionWaving))))
                {
                    return SessionPhase.FullCourseYellow;
                }
                else if (sessionFlags.HasFlag(SessionFlags.Green) || 
                    sessionFlags.HasFlag(SessionFlags.StartGo) || SessionPhase.Unavailable == lastSessionPhase)
                {
                    return SessionPhase.Green;
                }
            }
            return lastSessionPhase;
        }

        private OpponentData createOpponentData(Driver driver, Boolean loadDriverName, float trackLength)
        {
            String driverName = driver.Name;
            if (loadDriverName && CrewChief.enableDriverNames)
            {
                if (speechRecogniser != null) speechRecogniser.addNewOpponentName(driverName, driver.CarNumber);
                SoundCache.loadDriverNameSound(DriverNameHelper.getUsableDriverName(driverName));
            }
            OpponentData opponentData = new OpponentData();
            opponentData.IsActive = true;
            opponentData.DriverRawName = driverName;
            opponentData.CostId = driver.CustId;
            opponentData.OverallPosition = driver.Live.PositionRaw;
            opponentData.CompletedLaps = driver.Live.LiveLapsCompleted;
            opponentData.DistanceRoundTrack = driver.Live.CorrectedLapDistance * trackLength;
            opponentData.DeltaTime = new DeltaTime(trackLength, opponentData.DistanceRoundTrack, (float)driver.Live.Speed, DateTime.UtcNow);
            opponentData.CarClass = CarData.getCarClassForIRacingId(driver.Car.CarClassId, driver.Car.CarId);
            opponentData.CurrentSectorNumber = driver.Live.CurrentSector;
            opponentData.CarNumber = driver.CarNumber;
            Console.WriteLine("New driver " + driverName + " is using car class " +
                opponentData.CarClass.getClassIdentifier() + " (car ID " + driver.Car.CarId + ")");

            return opponentData;
        }
        private FrozenOrderData GetFrozenOrderData(GameStateData currentGamestate, GameStateData previousGamestate, SessionFlags sessioFlags, Boolean StartingGridPoleOnLeft)
        {
            FrozenOrderData frozenOrderData = new FrozenOrderData();
            frozenOrderData.Phase = FrozenOrderPhase.None;
            if (!(currentGamestate.SessionData.SessionPhase == SessionPhase.Formation || currentGamestate.SessionData.SessionPhase == SessionPhase.FullCourseYellow))
            {
                return frozenOrderData;
            }
            bool useStockCarRules = currentGamestate.SessionData.TrackDefinition.isOval;
            var scrLastLapDoubleFile = (useStockCarRules && currentGamestate.SessionData.SessionPhase == SessionPhase.FullCourseYellow && sessioFlags.HasFlag(SessionFlags.OneLapToGreen))
                || currentGamestate.SessionData.SessionPhase == SessionPhase.Formation;

            bool isEvenPosition = IsEvenNumber(currentGamestate.SessionData.OverallPosition);
            if (!scrLastLapDoubleFile)
            {
                frozenOrderData.AssignedColumn = FrozenOrderColumn.None;
            }
            else if (StartingGridPoleOnLeft)
            {
                if (!isEvenPosition)
                {
                    frozenOrderData.AssignedColumn = FrozenOrderColumn.Left;
                }
                else
                {
                    frozenOrderData.AssignedColumn = FrozenOrderColumn.Right;
                }
            }
            else
            {
                if (isEvenPosition)
                {
                    frozenOrderData.AssignedColumn = FrozenOrderColumn.Left;
                }
                else
                {
                    frozenOrderData.AssignedColumn = FrozenOrderColumn.Right;
                }
            }
            if (currentGamestate.SessionData.SessionPhase == SessionPhase.Formation)
            {
                frozenOrderData.AssignedPosition = currentGamestate.SessionData.OverallPosition;
                frozenOrderData.AssignedGridPosition = frozenOrderData.AssignedPosition <= 2 ? 1 : (isEvenPosition ? currentGamestate.SessionData.OverallPosition / 2 : (currentGamestate.SessionData.OverallPosition + 1) / 2);
                frozenOrderData.Phase = FrozenOrderPhase.Rolling;                    
                OpponentData opponentInfront = null;
                if (currentGamestate.SessionData.OverallPosition > 2)
                {
                    opponentInfront = currentGamestate.getOpponentAtOverallPosition(currentGamestate.SessionData.OverallPosition - 2);
                    if (opponentInfront != null)
                    {
                        frozenOrderData.CarNumberToFollowRaw = opponentInfront.CarNumber;
                        frozenOrderData.DriverToFollowRaw = opponentInfront.DriverRawName;
                    }

                }
                //this is real basic and still needs checking if it is infact correct driver infront 
                frozenOrderData.Action = FrozenOrderAction.Follow;
                if (opponentInfront != null)
                {
                    float playerDistanceRoundTrack = currentGamestate.PositionAndMotionData.DistanceRoundTrack;
                    float driverinfrontDistanceRoundTrack = opponentInfront.DistanceRoundTrack;;
                    if (Math.Abs((playerDistanceRoundTrack) - (driverinfrontDistanceRoundTrack)) > 50)
                    {
                        frozenOrderData.Action = FrozenOrderAction.CatchUp;
                    }
                }
                return frozenOrderData;
            }
            if (currentGamestate.SessionData.SessionPhase == SessionPhase.FullCourseYellow)
            {
                frozenOrderData.AssignedPosition = currentGamestate.SessionData.OverallPosition;
                
                frozenOrderData.AssignedGridPosition = frozenOrderData.AssignedPosition <= 2 ? 1 : (isEvenPosition ? currentGamestate.SessionData.OverallPosition / 2 : (currentGamestate.SessionData.OverallPosition + 1) / 2);
                
                int positionTocheckAgainst  = scrLastLapDoubleFile ? 2 : 1;

                if (currentGamestate.SafetyCarData.isOnTrack)
                {
                    frozenOrderData.Phase = FrozenOrderPhase.FullCourseYellow;
                    frozenOrderData.Action = FrozenOrderAction.Follow;
                    OpponentData opponentInfront = null;
                    if (currentGamestate.SessionData.OverallPosition > positionTocheckAgainst)
                    {
                        opponentInfront = currentGamestate.getOpponentAtOverallPosition(currentGamestate.SessionData.OverallPosition - positionTocheckAgainst);
                        if (opponentInfront != null)
                        {
                            frozenOrderData.CarNumberToFollowRaw = opponentInfront.CarNumber;
                            frozenOrderData.DriverToFollowRaw = opponentInfront.DriverRawName;
                        }
                    }
                    //this is real basic and still needs checking if it is infact correct driver infront 
                    /*if (opponentInfront != null)
                    {
                        float playerDistanceRoundTrack = currentGamestate.PositionAndMotionData.DistanceRoundTrack;
                        float driverinfrontDistanceRoundTrack = opponentInfront.DistanceRoundTrack;
                        if (Math.Abs((playerDistanceRoundTrack) - (driverinfrontDistanceRoundTrack)) > 50)
                        {
                            frozenOrderData.Action = FrozenOrderAction.CatchUp;
                        }
                    }*/
                }

                return frozenOrderData;
            }
            return frozenOrderData;
        }
        private SafetyCarData GetSafetyCarData(SafetyCarData previousSafetyCarData, Driver safetyCar, float trackLength, bool safetyCarPresent)
        {
            SafetyCarData currentSafetyCarData = new SafetyCarData();
            if (!safetyCarPresent)
            {
                return currentSafetyCarData;
            }
            currentSafetyCarData.fcySafetyCarCallsEnabled = true;
            if (previousSafetyCarData != null)
            {
                currentSafetyCarData.Lap = previousSafetyCarData.Lap;
                currentSafetyCarData.PitEntranceDistanceRoundTrack = previousSafetyCarData.PitEntranceDistanceRoundTrack;
                if (safetyCar.Live.TrackSurface == TrackSurfaces.NotInWorld)
                {
                    currentSafetyCarData.TrackSurface = previousSafetyCarData.TrackSurface;
                    currentSafetyCarData.CurrentSector = previousSafetyCarData.CurrentSector;
                    currentSafetyCarData.DistanceRoundTrack = previousSafetyCarData.DistanceRoundTrack;
                }
                else
                {
                    currentSafetyCarData.TrackSurface = (int)safetyCar.Live.TrackSurface;
                    currentSafetyCarData.CurrentSector = safetyCar.Live.CurrentSector;
                    currentSafetyCarData.DistanceRoundTrack = safetyCar.Live.LapDistance * trackLength;
                }
                if (currentSafetyCarData.PitEntranceDistanceRoundTrack == -1.0f && (TrackSurfaces)previousSafetyCarData.TrackSurface == TrackSurfaces.OnTrack
                    && (TrackSurfaces)currentSafetyCarData.TrackSurface == TrackSurfaces.AproachingPits)
                {
                    currentSafetyCarData.PitEntranceDistanceRoundTrack = safetyCar.Live.LapDistance * trackLength;
                    //Console.WriteLine("currentSafetyCarData.PitEntranceDistanceRoundTrack = " + currentSafetyCarData.PitEntranceDistanceRoundTrack);
                }
            }
            else
            {
                currentSafetyCarData.TrackSurface = (int)safetyCar.Live.TrackSurface;
                currentSafetyCarData.CurrentSector = safetyCar.Live.CurrentSector;
                currentSafetyCarData.DistanceRoundTrack = safetyCar.Live.LapDistance * trackLength;
            }
            currentSafetyCarData.isOnTrack = (TrackSurfaces)currentSafetyCarData.TrackSurface == TrackSurfaces.OnTrack;
            /*if (prevTrackSurface != (TrackSurfaces)currentSafetyCarData.TrackSurface)
            {
                Console.WriteLine("shared.PaceCar.Live.TrackSurface = " + (TrackSurfaces)currentSafetyCarData.TrackSurface);
                prevTrackSurface = (TrackSurfaces)currentSafetyCarData.TrackSurface;
            }*/
            if (currentSafetyCarData.isOnTrack)
            {
                if (previousSafetyCarData != null && previousSafetyCarData.CurrentSector == 3 && currentSafetyCarData.CurrentSector == 1)
                {
                    currentSafetyCarData.Lap++;
                    //Console.WriteLine("currentGameState.SafetyCarData.Lap = " + currentSafetyCarData.Lap);
                }
            }
            else
            {
                currentSafetyCarData.Lap = 0;
            }
            return currentSafetyCarData;
        }
        private bool IsEvenNumber(int number)
        {
            return (number % 2 == 0);
        }
    }
}
