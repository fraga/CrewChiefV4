﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using System.Diagnostics;

namespace CrewChiefV4.Events
{
    class Penalties : AbstractEvent
    {
        // time (in seconds) to delay messages about penalty laps to go - 
        // we need this because the play might cross the start line while serving 
        // a penalty, so we should wait before telling them how many laps they have to serve it
        private int pitstopDelay = 20;

        private String folderNewPenaltyStopGo = "penalties/new_penalty_stopgo";

        private String folderNewPenaltyDriveThrough = "penalties/new_penalty_drivethrough";

        private String folderNewPenaltySlowDown = "penalties/new_penalty_slowdown";

        private String folderThreeLapsToServe = "penalties/penalty_three_laps_left";

        private String folderTwoLapsToServe = "penalties/penalty_two_laps_left";

        private String folderOneLapToServeStopGo = "penalties/penalty_one_lap_left_stopgo";

        private String folderOneLapToServeDriveThrough = "penalties/penalty_one_lap_left_drivethrough";

        public static String folderDisqualified = "penalties/penalty_disqualified";

        private String folderPitNowStopGo = "penalties/pit_now_stop_go";

        private String folderPitNowDriveThrough = "penalties/pit_now_drive_through";

        private String folderTimePenalty = "penalties/time_penalty";

        public static String folderCutTrackInRace = "penalties/cut_track_in_race";

        private String folderPossibleTrackLimitsViolation = "penalties/possible_track_limits_warning";

        public static String folderLapDeleted = "penalties/lap_deleted";

        public static String folderCutTrackPracticeOrQual = "penalties/cut_track_in_prac_or_qual";

        public static String folderCutTrackPracticeOrQualNextLapInvalid = "penalties/cut_track_in_prac_or_qual_next_invalid";

        private String folderPenaltyNotServed = "penalties/penalty_not_served";

        // for voice requests
        private String folderYouStillHavePenalty = "penalties/you_still_have_a_penalty";

        private String folderYouStillHaveToServeDriveThrough = "penalties/still_have_to_serve_drive_through";

        private String folderYouStillHaveToServeStopGo = "penalties/still_have_to_serve_stop_go";

        private String folderYouHavePenalty = "penalties/you_have_a_penalty";

        private String folderPenaltyServed = "penalties/penalty_served";

        // Detailed penalty messages
        private String folderYouDontHaveAPenalty = "penalties/you_dont_have_a_penalty";

        private String folderStopGoSpeedingInPitlane = "penalties/stop_go_penalty_speeding_in_pit_lane";

        private String folderStopGoCutTrack = "penalties/stop_go_penalty_cutting_track";

        private String folderStopGoFalseStart = "penalties/stop_go_penalty_false_start";

        private String folderStopGoExitingPitsUnderRed = "penalties/stop_go_exitting_pits_on_red";

        private String folderStopGoRollingPassBeforeGreen = "penalties/stop_go_penalty_overtaking_on_formation_lap";

        private String folderStopGoFCYPassBeforeGreenEU = "penalties/stop_go_penalty_overtaking_under_safety_car";

        private String folderStopGoFCYPassBeforeGreenUS = "penalties/stop_go_penalty_overtaking_under_pace_car";

        private String folderDisqualifiedDrivingWithoutHeadlights = "penalties/disqualified_driving_without_headlights";

        private String folderDisqualifiedExceededAllowedLapCount = "penalties/disqualified_exceeded_allowed_lap_count";

        private String folderDriveThroughSpeedingInPitlane = "penalties/drive_through_speeding_in_pit_lane";

        private String folderDriveThroughCutTrack = "penalties/drive_through_cutting_track";

        // TODO_SOUND:
        private String folderDriveThroughIgnoredBlueFlag = "penalties/drive_through_ignored_blue";

        private String folderDriveThroughFalseStart = "penalties/drive_through_false_start";

        private String folderWarningDrivingTooSlow = "penalties/warning_driving_too_slow";

        private String folderWarningWrongWay = "penalties/warning_wrong_way";

        private String folderWarningHeadlightsRequired = "penalties/warning_headlights_required";

        private String folderWarningEnterPitsToAvoidExceedingLaps = "penalties/warning_enter_pits_to_avoid_exceeding_laps";

        private String folderWarningOneLapToServeDriveThrough = "penalties/one_lap_to_serve_drive_through";

        private String folderWarningOneLapToServeStopAndGo = "penalties/one_lap_to_serve_stop_go";

        // TODO_SOUND:
        private String folderWarningBlueFlagMoveOrBePenalized = "penalties/blue_move_now_or_be_penalized";

        private Boolean hasHadAPenalty;

        private int penaltyLap;

        private int lapsCompleted;

        private Boolean playedPitNow;

        private Boolean hasOutstandingPenalty = false;
        private PenatiesData.DetailedPenaltyType outstandingPenaltyType = PenatiesData.DetailedPenaltyType.NONE;
        private PenatiesData.DetailedPenaltyCause outstandingPenaltyCause = PenatiesData.DetailedPenaltyCause.NONE;

        private Boolean playedTimePenaltyMessage;

        private int cutTrackWarningsCount;

        private TimeSpan cutTrackWarningFrequency = TimeSpan.FromSeconds(30);

        private Boolean playedTrackCutWarningInPracticeOrQualOnThisLap = false;

        private DateTime lastCutTrackWarningTime;
    
        private Boolean playedNotServedPenalty;

        private Boolean warnedOfPossibleTrackLimitsViolationOnThisLap = false;

        private Boolean waitingToNotifyOfSlowdown = false;

        private DateTime timeToNotifyOfSlowdown = DateTime.MinValue;

        private Boolean playedSlowdownNotificationOnThisLap = false;

        public static Boolean playerMustPitThisLap = false;

        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Green, SessionPhase.Countdown, SessionPhase.Garage /*Apparently rF2 issues penalties in garage too :)*/, SessionPhase.FullCourseYellow /*Some rF2 warnings come up under FCY*/, SessionPhase.Gridwalk /*Announce some rF2 warnings during gridwalk*/}; }
        }

        public Penalties(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

        public override void clearState()
        {
            clearPenaltyState();
            lastCutTrackWarningTime = DateTime.MinValue;
            cutTrackWarningsCount = 0;
            hasHadAPenalty = false;
            warnedOfPossibleTrackLimitsViolationOnThisLap = false;
            playedTrackCutWarningInPracticeOrQualOnThisLap = false;
            waitingToNotifyOfSlowdown = false;
            timeToNotifyOfSlowdown = DateTime.MinValue;
            playedSlowdownNotificationOnThisLap = false;
            playerMustPitThisLap = false;
        }

        private void clearPenaltyState()
        {
            penaltyLap = -1;
            lapsCompleted = -1;
            hasOutstandingPenalty = false;
            outstandingPenaltyType = PenatiesData.DetailedPenaltyType.NONE;
            outstandingPenaltyCause = PenatiesData.DetailedPenaltyCause.NONE;
            // edge case here: if a penalty is given and immediately served (slow down penalty), then
            // the player gets another within the next 20 seconds, the 'you have 3 laps to come in to serve'
            // message would be in the queue and would be made valid again, so would play. So we explicity 
            // remove this message from the queue
            audioPlayer.removeQueuedMessage(folderThreeLapsToServe);
            playedPitNow = false;
            playedTimePenaltyMessage = false;
            playedNotServedPenalty = false;
            waitingToNotifyOfSlowdown = false;
            timeToNotifyOfSlowdown = DateTime.MinValue;
        }

        public override bool isMessageStillValid(String eventSubType, GameStateData currentGameState, Dictionary<String, Object> validationData)
        {
            if (base.isMessageStillValid(eventSubType, currentGameState, validationData))
            {
                if (eventSubType == folderPossibleTrackLimitsViolation)
                {
                    return true;
                }
                // When a new penalty is given we queue a 'three laps left to serve' delayed message.
                // If, the moment message is about to play, the player has started a new lap, this message is no longer valid so shouldn't be played
                if (eventSubType == folderThreeLapsToServe)
                {
                    Console.WriteLine("Checking penalty validity, pen lap = " + penaltyLap + ", completed =" + lapsCompleted);
                    return hasOutstandingPenalty && lapsCompleted == penaltyLap && currentGameState.SessionData.SessionPhase != SessionPhase.Finished;
                }
                else if (eventSubType == folderCutTrackInRace)
                {
                    return !hasOutstandingPenalty && currentGameState.SessionData.SessionPhase != SessionPhase.Finished && !currentGameState.PitData.InPitlane;
                }
                else if (eventSubType == folderCutTrackPracticeOrQual || eventSubType == folderCutTrackPracticeOrQualNextLapInvalid || eventSubType == folderLapDeleted)
                {
                    return currentGameState.SessionData.SessionPhase != SessionPhase.Finished && !currentGameState.PitData.InPitlane;
                }
                else if (eventSubType == folderNewPenaltySlowDown)
                {
                    return currentGameState.PenaltiesData.HasSlowDown;
                }
                else if (eventSubType == folderPenaltyServed &&
                    (CrewChief.gameDefinition.gameEnum == GameEnum.RF1 || CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT))
                {
                    // Don't validate "Penalty served" in rF1/rF2, hasOutstandingPenalty is false by the time we get here.
                    return true;
                }
                else
                {
                    return hasOutstandingPenalty && currentGameState.SessionData.SessionPhase != SessionPhase.Finished;
                }
            }
            else
            {
                return false;
            }
        }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            // Play warning messages:
            if (currentGameState.PenaltiesData.Warning != PenatiesData.WarningMessage.NONE)
            {
                string warningMsg = null;
                switch (currentGameState.PenaltiesData.Warning)
                {
                    case PenatiesData.WarningMessage.WRONG_WAY:
                        warningMsg = folderWarningWrongWay;
                        break;
                    case PenatiesData.WarningMessage.DRIVING_TOO_SLOW:
                        warningMsg = folderWarningDrivingTooSlow;
                        break;
                    case PenatiesData.WarningMessage.HEADLIGHTS_REQUIRED:
                        warningMsg = folderWarningHeadlightsRequired;
                        break;
                    case PenatiesData.WarningMessage.ENTER_PITS_TO_AVOID_EXCEEDING_LAPS:
                        if (!currentGameState.PitData.HasRequestedPitStop)
                        {
                            warningMsg = folderWarningEnterPitsToAvoidExceedingLaps;
                            playerMustPitThisLap = true;
                        }
                        break;
                    case PenatiesData.WarningMessage.DISQUALIFIED_DRIVING_WITHOUT_HEADLIGHTS:
                        warningMsg = folderDisqualifiedDrivingWithoutHeadlights;
                        break;
                    case PenatiesData.WarningMessage.DISQUALIFIED_EXCEEDING_ALLOWED_LAP_COUNT:
                        warningMsg = folderDisqualifiedExceededAllowedLapCount;
                        break;
                    case PenatiesData.WarningMessage.ONE_LAP_TO_SERVE_DRIVE_THROUGH:
                        warningMsg = folderWarningOneLapToServeDriveThrough;
                        playerMustPitThisLap = true;
                        break;
                    case PenatiesData.WarningMessage.ONE_LAP_TO_SERVE_STOP_AND_GO:
                        warningMsg = folderWarningOneLapToServeStopAndGo;
                        playerMustPitThisLap = true;
                        break;
                    case PenatiesData.WarningMessage.BLUE_MOVE_OR_BE_PENALIZED:
                        warningMsg = folderWarningBlueFlagMoveOrBePenalized;
                        break;
                    default:
                        Debug.Assert(false, "Unhandled warning");
                        Console.WriteLine("Penalties: unhandled warning: " + currentGameState.PenaltiesData.Warning);
                        break;
                }

                if (!String.IsNullOrWhiteSpace(warningMsg))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(warningMsg, 0, priority: 15));
                }
            }

            if (currentGameState.SessionData.SessionPhase == SessionPhase.FullCourseYellow)
            {
                // For now, only allow warning messages under FCY.
                return;
            }

            if (currentGameState.SessionData.IsNewLap)
            {
                warnedOfPossibleTrackLimitsViolationOnThisLap = false;
                playedTrackCutWarningInPracticeOrQualOnThisLap = false;
                playedSlowdownNotificationOnThisLap = false;
                playerMustPitThisLap = false;
            }
            if (currentGameState.SessionData.SessionType == SessionType.Race && previousGameState != null &&
                (currentGameState.PenaltiesData.HasDriveThrough || currentGameState.PenaltiesData.HasStopAndGo || currentGameState.PenaltiesData.HasTimeDeduction))
            {
                if (currentGameState.PenaltiesData.HasDriveThrough && !previousGameState.PenaltiesData.HasDriveThrough)
                {
                    lapsCompleted = currentGameState.SessionData.CompletedLaps;
                    // this is a new penalty
                    audioPlayer.playMessage(new QueuedMessage(folderNewPenaltyDriveThrough, 0, abstractEvent: this, priority: 10));
                    // queue a '3 laps to serve penalty' message - this might not get played
                    audioPlayer.playMessage(new QueuedMessage(folderThreeLapsToServe, 0, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    // we don't already have a penalty
                    if (penaltyLap == -1 || !hasOutstandingPenalty)
                    {
                        penaltyLap = currentGameState.SessionData.CompletedLaps;
                    }
                    hasOutstandingPenalty = true;
                    hasHadAPenalty = true;
                }
                else if (currentGameState.PenaltiesData.HasStopAndGo && !previousGameState.PenaltiesData.HasStopAndGo)
                {
                    lapsCompleted = currentGameState.SessionData.CompletedLaps;
                    // this is a new penalty
                    audioPlayer.playMessage(new QueuedMessage(folderNewPenaltyStopGo, 0, abstractEvent: this, priority: 10));
                    // queue a '3 laps to serve penalty' message - this might not get played
                    audioPlayer.playMessage(new QueuedMessage(folderThreeLapsToServe, 0, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    // we don't already have a penalty
                    if (penaltyLap == -1 || !hasOutstandingPenalty)
                    {
                        penaltyLap = currentGameState.SessionData.CompletedLaps;
                    }
                    hasOutstandingPenalty = true;
                    hasHadAPenalty = true;
                }
                else if (currentGameState.PitData.InPitlane && currentGameState.PitData.OnOutLap && !playedNotServedPenalty &&
                    (currentGameState.PenaltiesData.HasStopAndGo || currentGameState.PenaltiesData.HasDriveThrough))
                {
                    // we've exited the pits but there's still an outstanding penalty
                    audioPlayer.playMessage(new QueuedMessage(folderPenaltyNotServed, 0, secondsDelay: 3, abstractEvent: this, priority: 10));
                    playedNotServedPenalty = true;
                }
                else if (currentGameState.SessionData.IsNewLap && (currentGameState.PenaltiesData.HasStopAndGo || currentGameState.PenaltiesData.HasDriveThrough))
                {
                    lapsCompleted = currentGameState.SessionData.CompletedLaps;
                    if (lapsCompleted - penaltyLap == 3 && !currentGameState.PitData.InPitlane)
                    {
                        // run out of laps, and not in the pitlane
                        if (!audioPlayer.playRant("disqualified_rant", MessageContents(folderDisqualified)))
                        {
                            audioPlayer.playMessage(new QueuedMessage(folderDisqualified, 0, secondsDelay: 5, abstractEvent: this, priority: 10));
                        }
                    }
                    else if (lapsCompleted - penaltyLap == 2 && currentGameState.PenaltiesData.HasDriveThrough)
                    {
                        audioPlayer.playMessage(new QueuedMessage(folderOneLapToServeDriveThrough, 0, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    }
                    else if (lapsCompleted - penaltyLap == 2 && currentGameState.PenaltiesData.HasStopAndGo)
                    {
                        audioPlayer.playMessage(new QueuedMessage(folderOneLapToServeStopGo, 0, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    }
                    else if (lapsCompleted - penaltyLap == 1)
                    {
                        audioPlayer.playMessage(new QueuedMessage(folderTwoLapsToServe, 0, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    }
                }
                else if (!playedPitNow && currentGameState.SessionData.SectorNumber == 3 && currentGameState.PenaltiesData.HasStopAndGo && lapsCompleted - penaltyLap == 2)
                {
                    playedPitNow = true;
                    audioPlayer.playMessage(new QueuedMessage(folderPitNowStopGo, 14, secondsDelay: 6, abstractEvent: this, priority: 10));
                }
                else if (!playedPitNow && currentGameState.SessionData.SectorNumber == 3 && currentGameState.PenaltiesData.HasDriveThrough && lapsCompleted - penaltyLap == 2)
                {
                    playedPitNow = true;
                    audioPlayer.playMessage(new QueuedMessage(folderPitNowDriveThrough, 14, secondsDelay: 6, abstractEvent: this, priority: 10));
                }
                else if (!playedTimePenaltyMessage && currentGameState.PenaltiesData.HasTimeDeduction)
                {
                    playedTimePenaltyMessage = true;
                    audioPlayer.playMessage(new QueuedMessage(folderTimePenalty, 0, abstractEvent: this, priority: 10));
                }
            }
            else if (currentGameState.PositionAndMotionData.CarSpeed > 1 && GlobalBehaviourSettings.cutTrackWarningsEnabled &&
                !currentGameState.PitData.OnOutLap &&
                currentGameState.PenaltiesData.CutTrackWarnings > cutTrackWarningsCount &&
                currentGameState.PenaltiesData.NumPenalties == previousGameState.PenaltiesData.NumPenalties)  // Make sure we've no new penalty for this cut.
            {
                cutTrackWarningsCount = currentGameState.PenaltiesData.CutTrackWarnings;
                if (currentGameState.ControlData.ControlType != ControlType.AI &&
                    lastCutTrackWarningTime.Add(cutTrackWarningFrequency) < currentGameState.Now)
                {
                    lastCutTrackWarningTime = currentGameState.Now;
                    // don't warn on the first lap of the session
                    if (currentGameState.SessionData.CompletedLaps > 0)
                    {
                        if (currentGameState.SessionData.SessionType == SessionType.Race)
                        {
                            audioPlayer.playMessage(new QueuedMessage(folderCutTrackInRace, 5, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 3));
                        }
                        else if (!playedTrackCutWarningInPracticeOrQualOnThisLap)
                        {
                            // cut track in prac / qual is the same as lap deleted. Rather than dick about with the sound files, just allow either here
                            if (CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM
                                && currentGameState.SessionData.TrackDefinition.raceroomRollingStartLapDistance != -1.0f
                                && currentGameState.PositionAndMotionData.DistanceRoundTrack > currentGameState.SessionData.TrackDefinition.raceroomRollingStartLapDistance)
                            {
                                audioPlayer.playMessage(new QueuedMessage(Utilities.random.NextDouble() < 0.3 ? folderLapDeleted : folderCutTrackPracticeOrQualNextLapInvalid, 5, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 10));
                            }
                            else
                            {
                                audioPlayer.playMessage(new QueuedMessage(Utilities.random.NextDouble() < 0.3 ? folderLapDeleted : folderCutTrackPracticeOrQual, 5, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 10));
                            }
                            playedTrackCutWarningInPracticeOrQualOnThisLap = true;
                        }
                    }
                    clearPenaltyState();
                }
            }
            else if (currentGameState.PositionAndMotionData.CarSpeed > 1 && GlobalBehaviourSettings.cutTrackWarningsEnabled && currentGameState.SessionData.SessionType != SessionType.Race &&
              !currentGameState.SessionData.CurrentLapIsValid && previousGameState != null && previousGameState.SessionData.CurrentLapIsValid &&
                CrewChief.gameDefinition.gameEnum != GameEnum.IRACING && !currentGameState.PitData.OnOutLap)
            {
                // JB: don't think we need this block - the previous block should always trigger in preference to this, but we'll leave it here just in case
                cutTrackWarningsCount = currentGameState.PenaltiesData.CutTrackWarnings;
                // don't warn about cut track if the AI is driving
                if (currentGameState.ControlData.ControlType != ControlType.AI &&
                    lastCutTrackWarningTime.Add(cutTrackWarningFrequency) < currentGameState.Now)
                {
                    lastCutTrackWarningTime = currentGameState.Now;
                    // cut track in prac / qual is the same as lap deleted. Rather than dick about with the sound files, just allow either here
                    if (CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM
                        && currentGameState.SessionData.TrackDefinition.raceroomRollingStartLapDistance != -1.0f
                        && currentGameState.PositionAndMotionData.DistanceRoundTrack > currentGameState.SessionData.TrackDefinition.raceroomRollingStartLapDistance)
                    {
                        audioPlayer.playMessage(new QueuedMessage(Utilities.random.NextDouble() < 0.3 ? folderLapDeleted : folderCutTrackPracticeOrQualNextLapInvalid, 5, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 10));
                    }
                    else
                    {
                        audioPlayer.playMessage(new QueuedMessage(Utilities.random.NextDouble() < 0.3 ? folderLapDeleted : folderCutTrackPracticeOrQual, 5, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 10));
                    }
                    clearPenaltyState();
                }
            }
            else if ((currentGameState.SessionData.SessionType == SessionType.Race || currentGameState.SessionData.SessionType == SessionType.Qualify
                        || currentGameState.SessionData.SessionType == SessionType.Practice || currentGameState.SessionData.SessionType == SessionType.LonePractice)
                    && previousGameState != null && currentGameState.PenaltiesData.NumPenalties > 0
                    && (CrewChief.gameDefinition.gameEnum == GameEnum.RF1 || CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT ||
                (CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM && currentGameState.PenaltiesData.PenaltyType != PenatiesData.DetailedPenaltyType.NONE)))
            {
                if (currentGameState.PenaltiesData.NumPenalties > previousGameState.PenaltiesData.NumPenalties)
                {
                    lapsCompleted = currentGameState.SessionData.CompletedLaps;
                    // this is a new penalty
                    int delay1 = Utilities.random.Next(1, 5);
                    int delay2 = Utilities.random.Next(7, 12);
                    outstandingPenaltyType = currentGameState.PenaltiesData.PenaltyType;
                    outstandingPenaltyCause = currentGameState.PenaltiesData.PenaltyCause;

                    var message = getPenaltyMessge(outstandingPenaltyType, outstandingPenaltyCause);
                    audioPlayer.playMessage(new QueuedMessage(message, delay1 + 6, secondsDelay: delay1, abstractEvent: this, priority: 15));

                    // queue a '3 laps to serve penalty' message - this might not get played if player crosses s/f line before
                    audioPlayer.playMessage(new QueuedMessage(folderThreeLapsToServe, delay2 + 6, secondsDelay: delay2, abstractEvent: this, priority: 12));

                    // we don't already have a penalty
                    if (penaltyLap == -1 || !hasOutstandingPenalty)
                    {
                        penaltyLap = currentGameState.SessionData.CompletedLaps;
                    }
                    
                    hasOutstandingPenalty = true;
                    hasHadAPenalty = true;
                }
                else if (currentGameState.PitData.InPitlane && currentGameState.PitData.OnOutLap && !playedNotServedPenalty &&
                    currentGameState.PenaltiesData.NumPenalties > 0)
                {
                    // we've exited the pits but there's still an outstanding penalty
                    audioPlayer.playMessage(new QueuedMessage(folderPenaltyNotServed, 0, secondsDelay: 3, abstractEvent: this, priority: 10));
                    playedNotServedPenalty = true;
                }
                else if (currentGameState.SessionData.IsNewLap && currentGameState.PenaltiesData.NumPenalties > 0)
                {
                    lapsCompleted = currentGameState.SessionData.CompletedLaps;
                    if (lapsCompleted - penaltyLap >= 2 && !currentGameState.PitData.InPitlane)
                    {
                        // run out of laps, an not in the pitlane
                        if (Utilities.random.NextDouble() < 0.2)
                        {
                            // For variety, sometimes just play basic reminder.
                            audioPlayer.playMessage(new QueuedMessage(folderYouStillHavePenalty, 0, secondsDelay: 5, abstractEvent: this, priority: 10));
                        }
                        else
                        {
                            var message = getOutstandingPenaltyMessage();
                            if (!String.IsNullOrWhiteSpace(message))
                            {
                                audioPlayer.playMessage(new QueuedMessage(message, 0, secondsDelay: 5, abstractEvent: this, priority: 10));
                            }
                        }
                    }
                    else if (lapsCompleted - penaltyLap == 1)
                    {
                        audioPlayer.playMessage(new QueuedMessage(folderTwoLapsToServe, pitstopDelay + 6, secondsDelay: pitstopDelay, abstractEvent: this, priority: 10));
                    }
                }
            }
            else if (currentGameState.PenaltiesData.PossibleTrackLimitsViolation && GlobalBehaviourSettings.cutTrackWarningsEnabled && !warnedOfPossibleTrackLimitsViolationOnThisLap)
            {
                warnedOfPossibleTrackLimitsViolationOnThisLap = true;
                audioPlayer.playMessage(new QueuedMessage(folderPossibleTrackLimitsViolation, 4, secondsDelay: Utilities.random.Next(2, 4), abstractEvent: this, priority: 0));
            }
            else if (currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.PenaltiesData.HasSlowDown && !playedSlowdownNotificationOnThisLap)
            {
                if (!waitingToNotifyOfSlowdown)
                {
                    waitingToNotifyOfSlowdown = true;
                    timeToNotifyOfSlowdown = currentGameState.Now.AddSeconds(4);
                }
                else if (currentGameState.Now > timeToNotifyOfSlowdown)
                {
                    playedSlowdownNotificationOnThisLap = true;
                    audioPlayer.playMessage(new QueuedMessage(folderNewPenaltySlowDown, 0, abstractEvent: this));
                    waitingToNotifyOfSlowdown = false;
                    timeToNotifyOfSlowdown = DateTime.MinValue;
                }
            }
            else
            {
                clearPenaltyState();
            }
            if ((currentGameState.SessionData.SessionType == SessionType.Race ||
                currentGameState.SessionData.SessionType == SessionType.Qualify ||
                currentGameState.SessionData.SessionType == SessionType.Practice ||
                currentGameState.SessionData.SessionType == SessionType.LonePractice) && previousGameState != null &&
                ((previousGameState.PenaltiesData.HasStopAndGo && !currentGameState.PenaltiesData.HasStopAndGo) ||
                (previousGameState.PenaltiesData.HasDriveThrough && !currentGameState.PenaltiesData.HasDriveThrough) ||
                // can't read penalty type in Automobilista (and presumably in rF2).
                (previousGameState.PenaltiesData.NumPenalties > currentGameState.PenaltiesData.NumPenalties &&
                (CrewChief.gameDefinition.gameEnum == GameEnum.RF1 || CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT))))
            {
                audioPlayer.playMessage(new QueuedMessage(folderPenaltyServed, 0, abstractEvent: this, priority: 10));
            }
        }

        public override void respond(string voiceMessage)
        {
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.SESSION_STATUS) ||
                     SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.STATUS))
            {
                if (hasOutstandingPenalty)
                {
                    var penaltyMessage = getPenaltyMessge(outstandingPenaltyType, outstandingPenaltyCause);
                    if (lapsCompleted - penaltyLap == 2)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage("youHaveAPenaltyBoxThisLap", 0,
                            messageFragments: MessageContents(penaltyMessage, PitStops.folderMandatoryPitStopsPitThisLap)));
                    }
                    else
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(penaltyMessage, 0));
                    }
                }
            }
            else
            {
                if (!hasHadAPenalty)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderYouDontHaveAPenalty, 0));
                    return;
                }
                if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DO_I_HAVE_A_PENALTY))
                {
                    if (hasOutstandingPenalty)
                    {
                        var penaltyMessage = getPenaltyMessge(outstandingPenaltyType, outstandingPenaltyCause);
                        if (lapsCompleted - penaltyLap == 2)
                        {
                            audioPlayer.playMessageImmediately(new QueuedMessage("youHaveAPenaltyBoxThisLap", 0,
                            messageFragments: MessageContents(penaltyMessage, PitStops.folderMandatoryPitStopsPitThisLap)));
                        }
                        else
                        {
                            audioPlayer.playMessageImmediately(new QueuedMessage(penaltyMessage, 0));
                        }
                    }
                    else
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderYouDontHaveAPenalty, 0));
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HAVE_I_SERVED_MY_PENALTY))
                {
                    if (hasOutstandingPenalty)
                    {
                        List<MessageFragment> messages = new List<MessageFragment>();
                        messages.Add(MessageFragment.Text(AudioPlayer.folderNo));
                        messages.Add(MessageFragment.Text(getOutstandingPenaltyMessage()));
                        if (lapsCompleted - penaltyLap == 2)
                        {
                            messages.Add(MessageFragment.Text(PitStops.folderMandatoryPitStopsPitThisLap));
                        }
                        audioPlayer.playMessageImmediately(new QueuedMessage("noYouStillHaveAPenalty", 0, messageFragments: messages));
                    }
                    else
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage("yesYouServedYourPenalty", 0,
                            messageFragments: MessageContents(AudioPlayer.folderYes, folderPenaltyServed)));
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DO_I_STILL_HAVE_A_PENALTY))
                {
                    if (hasOutstandingPenalty)
                    {
                        List<MessageFragment> messages = new List<MessageFragment>();
                        messages.Add(MessageFragment.Text(AudioPlayer.folderYes));
                        messages.Add(MessageFragment.Text(getOutstandingPenaltyMessage()));
                        if (lapsCompleted - penaltyLap == 2)
                        {
                            messages.Add(MessageFragment.Text(PitStops.folderMandatoryPitStopsPitThisLap));
                        }
                        audioPlayer.playMessageImmediately(new QueuedMessage("yesYouStillHaveAPenalty", 0, messageFragments: messages));
                    }
                    else
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage("noYouServedYourPenalty", 0,
                            messageFragments: MessageContents(AudioPlayer.folderNo, folderPenaltyServed)));
                    }
                }
            }
        }

        private String getPenaltyMessge(PenatiesData.DetailedPenaltyType penaltyType, PenatiesData.DetailedPenaltyCause penaltyCause)
        {
            if (penaltyType == PenatiesData.DetailedPenaltyType.STOP_AND_GO)
            {
                switch (penaltyCause)
                {
                    case PenatiesData.DetailedPenaltyCause.NONE:
                        Debug.Assert(false, "Penalty without cause.");
                        break;
                    case PenatiesData.DetailedPenaltyCause.SPEEDING_IN_PITLANE:
                        return folderStopGoSpeedingInPitlane;
                    case PenatiesData.DetailedPenaltyCause.FALSE_START:
                        return folderStopGoFalseStart;
                    case PenatiesData.DetailedPenaltyCause.CUT_TRACK:
                        return folderStopGoCutTrack;
                    case PenatiesData.DetailedPenaltyCause.EXITING_PITS_UNDER_RED:
                        return folderStopGoExitingPitsUnderRed;
                    case PenatiesData.DetailedPenaltyCause.ILLEGAL_PASS_ROLLING_BEFORE_GREEN:
                        return folderStopGoRollingPassBeforeGreen;
                    case PenatiesData.DetailedPenaltyCause.ILLEGAL_PASS_FCY_BEFORE_GREEN:
                        return GlobalBehaviourSettings.useAmericanTerms ? folderStopGoFCYPassBeforeGreenUS : folderStopGoFCYPassBeforeGreenEU;
                    default:
                        Debug.Assert(false, "Unhandled penalty cause");
                        Console.WriteLine("Penalties: unhandled stop/go penalty cause: " + penaltyCause);
                        break;
                }
            }
            else if (penaltyType == PenatiesData.DetailedPenaltyType.DRIVE_THROUGH)
            {
                switch (penaltyCause)
                {
                    case PenatiesData.DetailedPenaltyCause.NONE:
                        Debug.Assert(false, "Penalty without cause.");
                        break;
                    case PenatiesData.DetailedPenaltyCause.SPEEDING_IN_PITLANE:
                        return folderDriveThroughSpeedingInPitlane;
                    case PenatiesData.DetailedPenaltyCause.FALSE_START:
                        return folderDriveThroughFalseStart;
                    case PenatiesData.DetailedPenaltyCause.CUT_TRACK:
                        return folderDriveThroughCutTrack;
                    case PenatiesData.DetailedPenaltyCause.IGNORED_BLUE_FLAG:
                        return folderDriveThroughIgnoredBlueFlag;
                    default:
                        Debug.Assert(false, "Unhandled penalty cause");
                        Console.WriteLine("Penalties: unhandled stop/go penalty cause: " + penaltyCause);
                        break;
                }
            }

            // If no detailed message available, play basic message.
            return folderYouHavePenalty;
        }

        private String getOutstandingPenaltyMessage()
        {
            if (hasOutstandingPenalty)
            {
                switch (outstandingPenaltyType)
                {
                    case PenatiesData.DetailedPenaltyType.NONE:
                        return folderYouStillHavePenalty;
                    case PenatiesData.DetailedPenaltyType.STOP_AND_GO:
                        return folderYouStillHaveToServeStopGo;
                    case PenatiesData.DetailedPenaltyType.DRIVE_THROUGH:
                        return folderYouStillHaveToServeDriveThrough;
                    default:
                        Debug.Assert(false, "Unhandled penalty cause");
                        break;
                }
            }

            return String.Empty;
        }
    }
}
