using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.RaceRoom.RaceRoomData;
using System.Threading;
using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using CrewChiefV4.NumberProcessing;
using CrewChiefV4.R3E;

namespace CrewChiefV4.Events
{
    class Opponents : AbstractEvent
    {
        private static String validationDriverAheadKey = "validationDriverAheadKey";
        private static String validationNewLeaderKey = "validationNewLeaderKey";


        public static String folderCarNumber = "opponents/car_number";
        public static String folderLeaderIsPitting = "opponents/the_leader_is_pitting";
        public static String folderCarAheadIsPitting = "opponents/the_car_ahead_is_pitting";
        public static String folderCarBehindIsPitting = "opponents/the_car_behind_is_pitting";

        public static String folderTheLeader = "opponents/the_leader";
        public static String folderIsPitting = "opponents/is_pitting";
        public static String folderAheadIsPitting = "opponents/ahead_is_pitting";
        public static String folderBehindIsPitting = "opponents/behind_is_pitting";

        public static String folderTheLeaderIsNowOn = "opponents/the_leader_is_now_on";
        public static String folderTheCarAheadIsNowOn = "opponents/the_car_ahead_is_now_on";
        public static String folderTheCarBehindIsNowOn = "opponents/the_car_behind_is_now_on";
        public static String folderIsNowOn = "opponents/is_now_on";

        public static String folderLeaderHasJustDoneA = "opponents/the_leader_has_just_done_a";
        public static String folderTheCarAheadHasJustDoneA = "opponents/the_car_ahead_has_just_done_a";
        public static String folderTheCarBehindHasJustDoneA = "opponents/the_car_behind_has_just_done_a";
        public static String folderNewFastestLapFor = "opponents/new_fastest_lap_for";

        public static String folderOneLapBehind = "opponents/one_lap_behind";
        public static String folderOneLapAhead = "opponents/one_lap_ahead";

        public static String folderIsNowLeading = "opponents/is_now_leading";
        public static String folderNextCarIs = "opponents/next_car_is";

        public static String folderCantPronounceName = "opponents/cant_pronounce_name";

        public static String folderWeAre = "opponents/we_are";

        // optional intro for opponent position (not used in English)
        public static String folderOpponentPositionIntro = "position/opponent_position_intro";

        public static String folderHasJustRetired = "opponents/has_just_retired";
        public static String folderHasJustBeenDisqualified = "opponents/has_just_been_disqualified";

        public static String folderLicenseA = "licence/a_licence";
        public static String folderLicenseB = "licence/b_licence";
        public static String folderLicenseC = "licence/c_licence";
        public static String folderLicenseD = "licence/d_licence";
        public static String folderLicenseR = "licence/r_licence";
        public static String folderLicensePro = "licence/pro_licence";

        public static String folderRatingIntro = "opponents/rating_intro";
        public static String folderReputationIntro = "opponents/reputation_intro";

        private int frequencyOfOpponentRaceLapTimes = UserSettings.GetUserSettings().getInt("frequency_of_opponent_race_lap_times");
        private int frequencyOfOpponentPracticeAndQualLapTimes = UserSettings.GetUserSettings().getInt("frequency_of_opponent_practice_and_qual_lap_times");

        private float minImprovementBeforeReadingOpponentRaceTime;
        private float maxOffPaceBeforeReadingOpponentRaceTime;

        private GameStateData currentGameState;

        private DateTime nextLeadChangeMessage = DateTime.MinValue;

        private DateTime nextCarAheadChangeMessage = DateTime.MinValue;

        private string positionIsPlayerKey = "";

        // single set here because we never want to announce a DQ and a retirement for the same guy
        private HashSet<String> announcedRetirementsAndDQs = new HashSet<String>();

        // this prevents us from bouncing between 'next car is...' messages:
        private Dictionary<string, DateTime> onlyAnnounceOpponentAfter = new Dictionary<string, DateTime>();
        private TimeSpan waitBeforeAnnouncingSameOpponentAhead = TimeSpan.FromMinutes(3);
        private String lastNextCarAheadOpponentName = null;

        private String lastLeaderAnnounced = null;

        private int minSecondsBetweenOpponentTyreChangeCalls = 10;
        private int maxSecondsBetweenOpponentTyreChangeCalls = 20;
        private DateTime suppressOpponentTyreChangeUntil = DateTime.MinValue;
        
        public Opponents(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
            maxOffPaceBeforeReadingOpponentRaceTime = (float)frequencyOfOpponentRaceLapTimes / 10f;
            minImprovementBeforeReadingOpponentRaceTime = (1f - maxOffPaceBeforeReadingOpponentRaceTime) / 5f;
        }

        public override List<SessionType> applicableSessionTypes
        {
            get { return new List<SessionType> { SessionType.Practice, SessionType.Qualify, SessionType.Race }; }
        }

        // allow this event to trigger for FCY, but only the retired and DQ'ed checks:
        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Green, SessionPhase.Countdown, SessionPhase.FullCourseYellow }; }
        }

        public override void clearState()
        {
            currentGameState = null;
            nextLeadChangeMessage = DateTime.MinValue;
            nextCarAheadChangeMessage = DateTime.MinValue;
            announcedRetirementsAndDQs.Clear();
            onlyAnnounceOpponentAfter.Clear();
            lastNextCarAheadOpponentName = null;
            lastLeaderAnnounced = null;
        }

        public override bool isMessageStillValid(string eventSubType, GameStateData currentGameState, Dictionary<String, Object> validationData)
        {
            if (base.isMessageStillValid(eventSubType, currentGameState, validationData))
            {
                if (validationData != null)
                {
                    object validationValue = null;
                    if (validationData.TryGetValue(validationDriverAheadKey, out validationValue))
                    {
                        String expectedOpponentName = (String)validationValue;
                        OpponentData opponentInFront = currentGameState.SessionData.ClassPosition > 1 ?
                            currentGameState.getOpponentAtClassPosition(currentGameState.SessionData.ClassPosition - 1, currentGameState.carClass) : null;
                        String actualOpponentName = opponentInFront == null ? null : opponentInFront.DriverRawName;
                        if (actualOpponentName != expectedOpponentName)
                        {
                            if (actualOpponentName != null && expectedOpponentName != null)
                            {
                                Console.WriteLine("New car in front message for opponent " + expectedOpponentName +
                                    " no longer valid - driver in front is now " + actualOpponentName);
                            }
                            return false;
                        }
                        else if (opponentInFront != null && (opponentInFront.InPits || opponentInFront.isEnteringPits()))
                        {
                            Console.WriteLine("New car in front message for opponent " + expectedOpponentName +
                                " no longer valid - driver is " + (opponentInFront.InPits ? "in pits" : "is entering the pits"));
                        }
                    }
                    else if (validationData.TryGetValue(validationNewLeaderKey, out validationValue))
                    {
                        String expectedLeaderName = (String)validationValue;
                        if (currentGameState.SessionData.ClassPosition == 1)
                        {
                            Console.WriteLine("New leader message for opponent " + expectedLeaderName +
                                    " no longer valid - player is now leader");
                            return false;
                        }
                        OpponentData actualLeader = currentGameState.getOpponentAtClassPosition(1, currentGameState.carClass);
                        String actualLeaderName = actualLeader == null ? null : actualLeader.DriverRawName;
                        if (actualLeaderName != expectedLeaderName)
                        {
                            if (actualLeaderName != null && expectedLeaderName != null)
                            {
                                Console.WriteLine("New leader message for opponent " + expectedLeaderName +
                                    " no longer valid - leader is now " + actualLeaderName);
                            }
                            return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private Object getOpponentIdentifierForTyreChange(OpponentData opponentData, int playerRacePosition)
        {
            // leader
            int positionToCheck;
            if (opponentData.PositionOnApproachToPitEntry > 0)
            {
                positionToCheck = opponentData.PositionOnApproachToPitEntry;
            }
            else
            {
                // fallback if the PositionOnApproachToPitEntry isn't set - shouldn't really happen
                positionToCheck = opponentData.ClassPosition;
            }
            if (positionToCheck == 1)
            {
                return folderTheLeader;
            }
            // 2nd, 3rd, or within 2 positions of the player
            if ((positionToCheck > 1 && positionToCheck <= 3) ||
                (playerRacePosition - 2 <= positionToCheck && playerRacePosition + 2 >= positionToCheck))
            {
                if (opponentData.CanUseName && AudioPlayer.canReadName(opponentData.DriverRawName, false))
                {
                    return opponentData;
                }
                else
                {
                    return Position.folderStub + positionToCheck;
                }
            }
            return null;
        }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (GameStateData.onManualFormationLap)
            {
                return;
            }
            this.currentGameState = currentGameState;
            // skip the lap time checks and stuff under yellow:
            if (currentGameState.SessionData.SessionPhase != SessionPhase.FullCourseYellow)
            {
                if (nextCarAheadChangeMessage == DateTime.MinValue)
                {
                    nextCarAheadChangeMessage = currentGameState.Now.Add(TimeSpan.FromSeconds(30));
                }
                if (nextLeadChangeMessage == DateTime.MinValue)
                {
                    nextLeadChangeMessage = currentGameState.Now.Add(TimeSpan.FromSeconds(30));
                }
                if (currentGameState.SessionData.SessionType != SessionType.Race || frequencyOfOpponentRaceLapTimes > 0)
                {
                    foreach (KeyValuePair<string, OpponentData> entry in currentGameState.OpponentData)
                    {
                        string opponentKey = entry.Key;
                        OpponentData opponentData = entry.Value;
                        if (!CarData.IsCarClassEqual(opponentData.CarClass, currentGameState.carClass))
                        {
                            // not interested in opponents from other classes
                            continue;
                        }

                        // in race sessions, announce tyre type changes once the session is underway
                        if (currentGameState.SessionData.SessionType == SessionType.Race &&
                            currentGameState.SessionData.SessionRunningTime > 30 && opponentData.hasJustChangedToDifferentTyreType && currentGameState.Now > suppressOpponentTyreChangeUntil)
                        {
                            // this may be a race position or an OpponentData object
                            Object opponentIdentifier = getOpponentIdentifierForTyreChange(opponentData, currentGameState.SessionData.ClassPosition);
                            if (opponentIdentifier != null)
                            {
                                suppressOpponentTyreChangeUntil = currentGameState.Now.AddSeconds(Utilities.random.Next(minSecondsBetweenOpponentTyreChangeCalls, maxSecondsBetweenOpponentTyreChangeCalls));
                                audioPlayer.playMessage(new QueuedMessage("opponent_tyre_change_" + opponentIdentifier.ToString(), 20,
                                    messageFragments: MessageContents(opponentIdentifier, folderIsNowOn, TyreMonitor.getFolderForTyreType(opponentData.CurrentTyres)),
                                    abstractEvent: this, priority: 5));
                            }
                        }

                        if (opponentData.IsNewLap && opponentData.LastLapTime > 0 && opponentData.OpponentLapData.Count > 1 &&
                            opponentData.LastLapValid && opponentData.CurrentBestLapTime > 0 && !WatchedOpponents.watchedOpponentKeys.Contains(opponentKey) /* to avoid duplicate messages*/)
                        {
                            float currentFastestLap;
                            if (currentGameState.SessionData.PlayerLapTimeSessionBest == -1)
                            {
                                currentFastestLap = currentGameState.SessionData.OpponentsLapTimeSessionBestOverall;
                            }
                            else if (currentGameState.SessionData.OpponentsLapTimeSessionBestOverall == -1)
                            {
                                currentFastestLap = currentGameState.SessionData.PlayerLapTimeSessionBest;
                            }
                            else
                            {
                                currentFastestLap = Math.Min(currentGameState.SessionData.PlayerLapTimeSessionBest, currentGameState.SessionData.OpponentsLapTimeSessionBestOverall);
                            }

                            // this opponent has just completed a lap - do we need to report it? if it's fast overall and more than
                            // a tenth quicker then his previous best we do...
                            if (((currentGameState.SessionData.SessionType == SessionType.Race && opponentData.CompletedLaps > 2) ||
                                (!PitStops.waitingForMandatoryStopTimer &&
                                 currentGameState.SessionData.SessionType != SessionType.Race && opponentData.CompletedLaps > 1)) && opponentData.LastLapTime <= currentFastestLap &&
                                 (opponentData.CanUseName && AudioPlayer.canReadName(opponentData.DriverRawName, false)))
                            {
                                if ((currentGameState.SessionData.SessionType == SessionType.Race && frequencyOfOpponentRaceLapTimes > 0) ||
                                    (currentGameState.SessionData.SessionType != SessionType.Race && frequencyOfOpponentPracticeAndQualLapTimes > 0))
                                {
                                    audioPlayer.playMessage(new QueuedMessage("new_fastest_lap", 5,
                                        messageFragments: MessageContents(folderNewFastestLapFor, opponentData, 
                                        TimeSpanWrapper.FromSeconds(opponentData.LastLapTime, Precision.AUTO_LAPTIMES)), abstractEvent: this, priority: 3));
                                }
                            }
                            else if ((currentGameState.SessionData.SessionType == SessionType.Race &&
                                    (opponentData.LastLapTime <= opponentData.CurrentBestLapTime &&
                                     opponentData.LastLapTime < opponentData.PreviousBestLapTime - minImprovementBeforeReadingOpponentRaceTime &&
                                     opponentData.LastLapTime < currentFastestLap + maxOffPaceBeforeReadingOpponentRaceTime)) ||
                               ((currentGameState.SessionData.SessionType == SessionType.Practice || currentGameState.SessionData.SessionType == SessionType.Qualify) &&
                                     opponentData.LastLapTime <= opponentData.CurrentBestLapTime))
                            {
                                if (currentGameState.SessionData.ClassPosition > 1 && opponentData.ClassPosition == 1 &&
                                    (currentGameState.SessionData.SessionType == SessionType.Race || frequencyOfOpponentPracticeAndQualLapTimes > 0))
                                {
                                    // he's leading, and has recorded 3 or more laps, and this one's his fastest
                                    Console.WriteLine("Leader fast lap - this lap time = " + opponentData.LastLapTime + " session best = " + currentFastestLap);
                                    audioPlayer.playMessage(new QueuedMessage("leader_good_laptime", 5,
                                         messageFragments: MessageContents(folderLeaderHasJustDoneA, TimeSpanWrapper.FromSeconds(opponentData.LastLapTime, Precision.AUTO_LAPTIMES)),
                                        abstractEvent: this, priority: 3));
                                }
                                else if (currentGameState.SessionData.ClassPosition > 1 && opponentData.ClassPosition == currentGameState.SessionData.ClassPosition - 1 &&
                                    (currentGameState.SessionData.SessionType == SessionType.Race || Utilities.random.Next(10) < frequencyOfOpponentPracticeAndQualLapTimes))
                                {
                                    // he's ahead of us, and has recorded 3 or more laps, and this one's his fastest
                                    Console.WriteLine("Car ahead fast lap - this lap time = " + opponentData.LastLapTime + " session best = " + currentFastestLap);
                                    audioPlayer.playMessage(new QueuedMessage("car_ahead_good_laptime", 5,
                                        messageFragments: MessageContents(folderTheCarAheadHasJustDoneA, TimeSpanWrapper.FromSeconds(opponentData.LastLapTime, Precision.AUTO_LAPTIMES)),
                                        abstractEvent: this, priority: 0));
                                }
                                else if (!currentGameState.isLast() && opponentData.ClassPosition == currentGameState.SessionData.ClassPosition + 1 &&
                                    (currentGameState.SessionData.SessionType == SessionType.Race || Utilities.random.Next(10) < frequencyOfOpponentPracticeAndQualLapTimes))
                                {
                                    // he's behind us, and has recorded 3 or more laps, and this one's his fastest
                                    Console.WriteLine("Car behind fast lap - this lap time = " + opponentData.LastLapTime + " session best = " + currentFastestLap);
                                    audioPlayer.playMessage(new QueuedMessage("car_behind_good_laptime", 5,
                                        messageFragments: MessageContents(folderTheCarBehindHasJustDoneA, TimeSpanWrapper.FromSeconds(opponentData.LastLapTime, Precision.AUTO_LAPTIMES)),
                                        abstractEvent: this, priority: 0));
                                }
                            }
                        }
                    }
                }
            }

            // allow the retired and DQ checks under yellow:
            if (currentGameState.SessionData.SessionType == SessionType.Race &&
                ((currentGameState.SessionData.SessionHasFixedTime && currentGameState.SessionData.SessionTimeRemaining > 0) ||
                 (!currentGameState.SessionData.SessionHasFixedTime && currentGameState.SessionData.SessionLapsRemaining > 0)))
            {
                // don't bother processing retired and DQ'ed drivers and position changes if we're not allowed to use the names:
                if (CrewChief.enableDriverNames)
                {
                    foreach (String retiredDriver in currentGameState.retriedDriverNames)
                    {
                        if (!announcedRetirementsAndDQs.Contains(retiredDriver))
                        {
                            announcedRetirementsAndDQs.Add(retiredDriver);
                            if ((CrewChief.gameDefinition.gameEnum == GameEnum.RF1 || CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT)
                                && currentGameState.SessionData.SessionPhase != SessionPhase.Green
                                && currentGameState.SessionData.SessionPhase != SessionPhase.FullCourseYellow
                                && currentGameState.SessionData.SessionPhase != SessionPhase.Checkered)
                            {
                                // In an offline session of the ISI games it is possible to select more AI drivers than a track can handle.
                                // The ones that don't fit on a track are marked as DNF before session goes Green.  Don't announce those.
                                continue;
                            }
                            if (AudioPlayer.canReadName(retiredDriver, false))
                            {
                                audioPlayer.playMessage(new QueuedMessage("retirement", 10,
                                    messageFragments: MessageContents(DriverNameHelper.getUsableDriverName(retiredDriver), folderHasJustRetired), abstractEvent: this, priority: 0));
                            }
                        }
                    }
                    foreach (String dqDriver in currentGameState.disqualifiedDriverNames)
                    {
                        if (WatchedOpponents.watchedOpponentKeys.Contains(dqDriver))
                        {
                            continue;
                        }
                        if (!announcedRetirementsAndDQs.Contains(dqDriver))
                        {
                            announcedRetirementsAndDQs.Add(dqDriver);
                            if (AudioPlayer.canReadName(dqDriver, false))
                            {
                                audioPlayer.playMessage(new QueuedMessage("retirement", 10,
                                    messageFragments: MessageContents(DriverNameHelper.getUsableDriverName(dqDriver), folderHasJustBeenDisqualified), abstractEvent: this, priority: 0));
                            }
                        }
                    }
                    // skip the position change checks under yellow:
                    if (currentGameState.SessionData.SessionPhase != SessionPhase.FullCourseYellow)
                    {
                        if (!currentGameState.SessionData.IsRacingSameCarInFront)
                        {
                            if (currentGameState.SessionData.ClassPosition > 2 && currentGameState.Now > nextCarAheadChangeMessage && !currentGameState.PitData.InPitlane
                                && currentGameState.SessionData.CompletedLaps > 0)
                            {
                                OpponentData opponentData = currentGameState.getOpponentAtClassPosition(currentGameState.SessionData.ClassPosition - 1, currentGameState.carClass);
                                if (opponentData != null)
                                {
                                    String opponentName = opponentData.DriverRawName;
                                    DateTime announceAfterTime = DateTime.MinValue;
                                    if (!WatchedOpponents.watchedOpponentKeys.Contains(opponentName) &&
                                        !opponentData.isEnteringPits() && !opponentData.InPits && (lastNextCarAheadOpponentName == null || !lastNextCarAheadOpponentName.Equals(opponentName)) &&
                                        opponentData.CanUseName &&
                                        (!onlyAnnounceOpponentAfter.TryGetValue(opponentName, out announceAfterTime) || currentGameState.Now > announceAfterTime))
                                    {
                                        Console.WriteLine("New car ahead: " + opponentName);
                                        int delay = Utilities.random.Next(Position.maxSecondsToWaitBeforeReportingPass + 1, Position.maxSecondsToWaitBeforeReportingPass + 3);

                                        var (main, alt) = getOpponentDetailed(opponentData, includePosition: false);
                                        var intro = MessageFragment.Text(folderNextCarIs);
                                        main.Insert(0, intro);
                                        alt.Insert(0, intro);

                                        audioPlayer.playMessage(new QueuedMessage("new_car_ahead", delay + 2, secondsDelay: delay,
                                            messageFragments: main,
                                            alternateMessageFragments: alt,
                                            abstractEvent: this,
                                            validationData: new Dictionary<string, object> { { validationDriverAheadKey, opponentData.DriverRawName } },
                                            priority: 7));
                                        nextCarAheadChangeMessage = currentGameState.Now.Add(TimeSpan.FromSeconds(30));
                                        onlyAnnounceOpponentAfter[opponentName] = currentGameState.Now.Add(waitBeforeAnnouncingSameOpponentAhead);
                                        lastNextCarAheadOpponentName = opponentName;
                                    }
                                }
                            }
                        }
                        if (currentGameState.SessionData.HasLeadChanged)
                        {
                            OpponentData leader = currentGameState.getOpponentAtClassPosition(1, currentGameState.carClass);
                            if (leader != null)
                            {
                                String name = leader.DriverRawName;
                                if (!WatchedOpponents.watchedOpponentKeys.Contains(name) &&
                                    currentGameState.SessionData.ClassPosition > 1 && previousGameState.SessionData.ClassPosition > 1 &&
                                    !name.Equals(lastLeaderAnnounced) &&
                                    currentGameState.Now > nextLeadChangeMessage && leader.CanUseName && AudioPlayer.canReadName(name, false))
                                {
                                    Console.WriteLine("Lead change, current leader is " + name + " laps completed = " + currentGameState.SessionData.CompletedLaps);
                                    // we use the short version on the basis
                                    // that we don't need to make an assessment
                                    // about who they are, but we still want to
                                    // be able to identify them on track
                                    // (number) and voice (ideally name).
                                    var (main, alt) = getOpponentShort(leader, alwaysIncludeNumber: true);
                                    main.Add(MessageFragment.Text(folderIsNowLeading));
                                    alt.Add(MessageFragment.Text(folderIsNowLeading));

                                    audioPlayer.playMessage(new QueuedMessage("new_leader", 4, secondsDelay:2,
                                        messageFragments: main,
                                        alternateMessageFragments: alt,
                                        abstractEvent: this,
                                        validationData: new Dictionary<string, object> { { validationNewLeaderKey, name } }, priority: 3));
                                    nextLeadChangeMessage = currentGameState.Now.Add(TimeSpan.FromSeconds(60));
                                    lastLeaderAnnounced = name;
                                }
                            }
                        }
                    }
                }

                HashSet<String> announcedPitters = new HashSet<string>();
                if (currentGameState.PitData.LeaderIsPitting &&                                  
                    currentGameState.SessionData.SessionPhase != SessionPhase.Countdown && currentGameState.SessionData.SessionPhase != SessionPhase.Formation &&
                    !WatchedOpponents.watchedOpponentKeys.Contains(currentGameState.PitData.OpponentForLeaderPitting.DriverRawName) &&
                    !Strategy.opponentsWhoWillExitCloseInFront.Contains(currentGameState.PitData.OpponentForLeaderPitting.DriverRawName))
                {
                    audioPlayer.playMessage(new QueuedMessage("leader_is_pitting", 10,
                        messageFragments: MessageContents(folderTheLeader, currentGameState.PitData.OpponentForLeaderPitting,
                        folderIsPitting), 
                        alternateMessageFragments: MessageContents(folderLeaderIsPitting), abstractEvent: this, priority: 3));
                    announcedPitters.Add(currentGameState.PitData.OpponentForLeaderPitting.DriverRawName);
                }

                if (currentGameState.PitData.CarInFrontIsPitting && currentGameState.SessionData.TimeDeltaFront > 3 &&
                    currentGameState.SessionData.SessionPhase != SessionPhase.Countdown && currentGameState.SessionData.SessionPhase != SessionPhase.Formation &&
                    !WatchedOpponents.watchedOpponentKeys.Contains(currentGameState.PitData.OpponentForCarAheadPitting.DriverRawName) &&
                    !Strategy.opponentsWhoWillExitCloseInFront.Contains(currentGameState.PitData.OpponentForCarAheadPitting.DriverRawName) &&
                    !announcedPitters.Contains(currentGameState.PitData.OpponentForCarAheadPitting.DriverRawName))
                {
                    audioPlayer.playMessage(new QueuedMessage("car_in_front_is_pitting", 10,
                        messageFragments: MessageContents(currentGameState.PitData.OpponentForCarAheadPitting,
                        folderAheadIsPitting), 
                        alternateMessageFragments: MessageContents(folderCarAheadIsPitting), abstractEvent: this, priority: 3));
                    announcedPitters.Add(currentGameState.PitData.OpponentForCarAheadPitting.DriverRawName);
                }

                if (currentGameState.PitData.CarBehindIsPitting && currentGameState.SessionData.TimeDeltaBehind > 3 &&
                    currentGameState.SessionData.SessionPhase != SessionPhase.Countdown && currentGameState.SessionData.SessionPhase != SessionPhase.Formation &&
                    !WatchedOpponents.watchedOpponentKeys.Contains(currentGameState.PitData.OpponentForCarBehindPitting.DriverRawName) &&
                    !Strategy.opponentsWhoWillExitCloseBehind.Contains(currentGameState.PitData.OpponentForCarBehindPitting.DriverRawName) &&
                    !announcedPitters.Contains(currentGameState.PitData.OpponentForCarBehindPitting.DriverRawName))
                {
                    audioPlayer.playMessage(new QueuedMessage("car_behind_is_pitting", 10,
                        messageFragments: MessageContents(currentGameState.PitData.OpponentForCarBehindPitting,
                        folderBehindIsPitting),
                        alternateMessageFragments: MessageContents(folderCarBehindIsPitting), abstractEvent: this, priority: 3));
                    announcedPitters.Add(currentGameState.PitData.OpponentForCarBehindPitting.DriverRawName);
                }

                if (Strategy.opponentFrontToWatchForPitting != null && !announcedPitters.Contains(Strategy.opponentFrontToWatchForPitting)
                    && !WatchedOpponents.watchedOpponentKeys.Contains(Strategy.opponentFrontToWatchForPitting))
                {
                    foreach (KeyValuePair<String, OpponentData> entry in currentGameState.OpponentData)
                    {
                        if (entry.Value.DriverRawName == Strategy.opponentFrontToWatchForPitting)
                        {
                            if (entry.Value.InPits)
                            {
                                audioPlayer.playMessage(new QueuedMessage("car_is_pitting", 10,
                                    messageFragments: MessageContents(entry.Value, 
                                    currentGameState.SessionData.ClassPosition > entry.Value.ClassPosition ? folderAheadIsPitting : folderBehindIsPitting),
                                    abstractEvent: this, priority: 3));
                                Strategy.opponentFrontToWatchForPitting = null;
                                break;
                            }
                        }
                    }
                }
                if (Strategy.opponentBehindToWatchForPitting != null && !announcedPitters.Contains(Strategy.opponentBehindToWatchForPitting)
                    && !WatchedOpponents.watchedOpponentKeys.Contains(Strategy.opponentFrontToWatchForPitting))
                {
                    foreach (KeyValuePair<String, OpponentData> entry in currentGameState.OpponentData)
                    {
                        if (entry.Value.DriverRawName == Strategy.opponentBehindToWatchForPitting)
                        {
                            if (entry.Value.InPits)
                            {
                                audioPlayer.playMessage(new QueuedMessage("car_is_pitting", 10,
                                    messageFragments: MessageContents(entry.Value,
                                    currentGameState.SessionData.ClassPosition > entry.Value.ClassPosition ? folderAheadIsPitting : folderBehindIsPitting),
                                    abstractEvent: this, priority: 3));
                                Strategy.opponentBehindToWatchForPitting = null;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private Tuple<string, Boolean> getOpponentKey(String voiceMessage, String expectedNumberSuffix)
        {
            string opponentKey = null;
            Boolean gotByPositionNumber = false;
            if (voiceMessage.Contains(SpeechRecogniser.THE_LEADER))
            {
                if (currentGameState.SessionData.ClassPosition > 1)
                {
                    opponentKey = currentGameState.getOpponentKeyAtClassPosition(1, currentGameState.carClass);
                }
                else if (currentGameState.SessionData.ClassPosition == 1)
                {
                    opponentKey = positionIsPlayerKey;
                }
            }
            if ((voiceMessage.Contains(SpeechRecogniser.THE_CAR_AHEAD) || voiceMessage.Contains(SpeechRecogniser.THE_GUY_AHEAD) ||
                voiceMessage.Contains(SpeechRecogniser.THE_GUY_IN_FRONT) || voiceMessage.Contains(SpeechRecogniser.THE_CAR_IN_FRONT)) && currentGameState.SessionData.ClassPosition > 1)
            {
                opponentKey = currentGameState.getOpponentKeyInFront(currentGameState.carClass);
            }
            else if ((voiceMessage.Contains(SpeechRecogniser.THE_CAR_BEHIND) || voiceMessage.Contains(SpeechRecogniser.THE_GUY_BEHIND)) &&
                            !currentGameState.isLast())
            {
                opponentKey = currentGameState.getOpponentKeyBehind(currentGameState.carClass);
            }
            else if (voiceMessage.Contains(SpeechRecogniser.POSITION_LONG) || voiceMessage.Contains(SpeechRecogniser.POSITION_SHORT))
            {
                int position = 0;
                Boolean found = false;
                foreach (KeyValuePair<String[], int> entry in SpeechRecogniser.racePositionNumberToNumber)
                {
                    foreach (String numberStr in entry.Key)
                    {
                        if (expectedNumberSuffix.Length > 0)
                        {
                            if (voiceMessage.Contains(" " + numberStr + expectedNumberSuffix))
                            {
                                position = entry.Value;
                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            if (voiceMessage.EndsWith(" " + numberStr))
                            {
                                position = entry.Value;
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (position != currentGameState.SessionData.ClassPosition)
                {
                    opponentKey = currentGameState.getOpponentKeyAtClassPosition(position, currentGameState.carClass);
                }
                else
                {
                    opponentKey = positionIsPlayerKey;
                }
                gotByPositionNumber = true;
            }
            else if (voiceMessage.Contains(SpeechRecogniser.CAR_NUMBER))
            {
                String carNumber = "-1";
                Boolean found = false;
                foreach (KeyValuePair<String[], String> entry in SpeechRecogniser.carNumberToNumber)
                {
                    foreach (String numberStr in entry.Key)
                    {
                        if (expectedNumberSuffix.Length > 0)
                        {
                            if (voiceMessage.Contains(" " + numberStr + expectedNumberSuffix))
                            {
                                carNumber = entry.Value;
                                found = true;
                                break;
                            }
                        }
                        else
                        {
                            if (voiceMessage.EndsWith(" " + numberStr))
                            {
                                carNumber = entry.Value;
                                found = true;
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
                if (carNumber != "-1" && carNumber != currentGameState.SessionData.PlayerCarNr)
                {
                    opponentKey = currentGameState.getOpponentKeyForCarNumber(carNumber);
                }
                gotByPositionNumber = false;
            }
            else
            {
                foreach (KeyValuePair<string, OpponentData> entry in currentGameState.OpponentData)
                {
                    String usableDriverNameForSRE = DriverNameHelper.getUsableDriverNameForSRE(entry.Value.DriverRawName);
                    // check for full username match so we're not triggering on substrings within other words
                    if (usableDriverNameForSRE != null
                        && (voiceMessage.Contains(" " + usableDriverNameForSRE + " ") || voiceMessage.EndsWith(" " + usableDriverNameForSRE)))
                    {
                        opponentKey = entry.Key;
                        break;
                    }
                }
            }
            return new Tuple<string, bool>(opponentKey, gotByPositionNumber);
        }

        private float getOpponentLastLap(string opponentKey)
        {
            OpponentData opponentData = null;
            if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponentData))
            {
                return opponentData.LastLapTime;
            }
            return -1;
        }

        private float getOpponentBestLap(string opponentKey)
        {
            OpponentData opponentData = null;
            if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponentData))
            {
                return opponentData.CurrentBestLapTime;
            }
            return -1;
        }

        private Tuple<String, float> getOpponentLicensLevel(string opponentKey)
        {
            OpponentData opponentData = null;
            if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponentData))
            {
                return opponentData.LicensLevel;
            }
            return new Tuple<String, float>("invalid", -1);
        }
        private int getOpponentIRating(string opponentKey)
        {
            OpponentData opponentData = null;
            if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponentData))
            {
                return opponentData.iRating;
            }
            return -1;
        }
        private int getOpponentR3EUserId(string opponentKey)
        {
            OpponentData opponentData = null;
            if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponentData))
            {
                return opponentData.r3eUserId;
            }
            return -1;
        }
        // The philosophy of the detailed message is to keep the message as terse as possible whilst returning as much relevant information
        // about the car and driver as possible. That means not including information that is obvious (e.g. if this is invoked when it is
        // clear that it's for the car ahead then we don't include position). We use the absense of information to imply "similar
        // as you" for car class, license, rating, etc.
        //
        // Requesting to include the car position implies that this is a track position query, rather than a leaderboard query, where
        // the user might not be sure if they are racing the car (different class, lapped).
        //
        // A litmus test is that if some information doesn't help to answer:
        //
        // - how can I refer to this person on the radio?
        // - am I racing them for position?
        // - can I trust this person to race cleanly?
        //
        // or the information is obvious from the context, then we should not include that information.
        //
        // Things we'd like to include in the future:
        //
        //   - club (where US is considered the default for ovals, your own club elsewhere)
        //   - latency (only when it is above a threshold)
        //   - customerid / length of time on the service (e.g. racing less than 6 months)
        //   - when they last pitted (relevant to let us know if we're racing them)
        //   - have we recorded them in a list of wreckless (or clean or clueless) drivers
        //   - incident points in this race
        //
        // Things we should not include, because they are better suited for a "gap to" style query
        //
        //   - lap difference
        //   - faster/slower than us
        private Tuple<List<MessageFragment>, List<MessageFragment>> getOpponentDetailed(OpponentData opponent, bool includePosition)
        {
            List<MessageFragment> fragments = new List<MessageFragment>();

            // if they are a different class, say the class
            if (!CarData.IsCarClassEqual(opponent.CarClass, currentGameState.carClass))
            {
                string clazz;
                MulticlassWarnings.carClassEnumToSound.TryGetValue(opponent.CarClass.carClassEnum, out clazz);
                if (clazz != null)
                {
                    fragments.Add(MessageFragment.Text(clazz));
                }
            }
            else if (includePosition)
            {
                // position in class. There's no point in reporting the position of other classes, although
                // it might make sense to report when it's the leader.
                fragments.Add(MessageFragment.Text(Position.folderStub + opponent.ClassPosition));
            }

            if (CrewChief.gameDefinition.gameEnum == GameEnum.IRACING)
            {
                // safety license (only report if different to us)
                Tuple<string, float> license = opponent.LicensLevel;
                Tuple<string, float> our_license = currentGameState.SessionData.LicenseLevel;
                if (license.Item2 != -1 && license.Item1.ToLower() != our_license.Item1.ToLower())
                {
                    switch (license.Item1.ToLower())
                    {
                        case "a": fragments.Add(MessageFragment.Text(folderLicenseA)); break;
                        case "b": fragments.Add(MessageFragment.Text(folderLicenseB)); break;
                        case "c": fragments.Add(MessageFragment.Text(folderLicenseC)); break;
                        case "d": fragments.Add(MessageFragment.Text(folderLicenseD)); break;
                        case "r": fragments.Add(MessageFragment.Text(folderLicenseR)); break;
                        case "wc": fragments.Add(MessageFragment.Text(folderLicensePro)); break;
                        default: break;
                    }
                }

                // iRating (only report if significantly different to us)
                int irating = opponent.iRating;
                int our_irating = currentGameState.SessionData.iRating;
                if (irating > 0 && Math.Abs(irating - our_irating) >= our_irating / 10)
                {
                    fragments.Add(MessageFragment.Text(folderRatingIntro));
                    Tuple<int, int> parts = Utilities.WholeAndFractionalPart(irating / 1000.0f);
                    // fragments.Add(MessageFragment.Integer(parts.Item1));
                    // fragments.Add(MessageFragment.Text(NumberReader.folderPoint));
                    // fragments.Add(MessageFragment.Integer(parts.Item2));
                    String number = "numbers/" + parts.Item1 + "point" + parts.Item2;
                    fragments.Add(MessageFragment.Text(number));
                    // the "k" sounds a bit forced, there's too much of a gap
                    // fragments.Add(MessageFragment.Text(folderK));
                }
            }
            else if (CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
            {
                R3ERatingData ratingData = R3ERatings.getRatingForUserId(opponent.r3eUserId);
                if (R3ERatings.playerRating != null && ratingData != null)
                {
                    Console.WriteLine("got rating data for opponent:" + ratingData.ToString());

                    float their_rep = ratingData.reputation;
                    float our_rep = R3ERatings.playerRating.reputation;
                    if (Math.Abs(their_rep - our_rep) >= Math.Abs(our_rep / 10.0))
                    {
                        fragments.Add(MessageFragment.Text(folderReputationIntro));
                        Tuple<int, int> rep = Utilities.WholeAndFractionalPart(their_rep);
                        fragments.Add(MessageFragment.Integer(rep.Item1));
                        fragments.Add(MessageFragment.Text(NumberReader.folderPoint));
                        fragments.Add(MessageFragment.Integer(rep.Item2));
                    }

                    float their_rating = ratingData.rating;
                    float our_rating = R3ERatings.playerRating.rating;
                    if (Math.Abs(their_rating - our_rating) >= Math.Abs(our_rating / 10.0))
                    {
                        fragments.Add(MessageFragment.Text(folderRatingIntro));
                        Tuple<int, int> rating = Utilities.WholeAndFractionalPart(their_rating);
                        fragments.Add(MessageFragment.Integer(rating.Item1));
                        fragments.Add(MessageFragment.Text(NumberReader.folderPoint));
                        fragments.Add(MessageFragment.Integer(rating.Item2));
                    }

                    //int their_rank = ratingData.rank;
                    //int our_rank = R3ERatings.playerRating.rank;
                    //if (Math.Abs(their_rank - our_rank) >= Math.Abs(our_rank / 10.0))
                    //{
                    //    // we don't have an intro for rank
                    //    // fragments.Add(MessageFragment.Text(folderRankIntro));
                    //    fragments.Add(MessageFragment.Integer(their_rank, false));
                    //}
                }
            }

            var (main, alt) = getOpponentShort(opponent, !includePosition);
            main.AddRange(fragments);
            alt.AddRange(fragments);

            return Tuple.Create(main, alt);
        }
        // the driver's name and an alt that uses their car number (unless the number is -1 in which case we say we have a stock response)
        private Tuple<List<MessageFragment>, List<MessageFragment>> getOpponentShort(OpponentData opponent, bool alwaysIncludeNumber)
        {
            var main = new List<MessageFragment>();
            var alt = new List<MessageFragment>();

            main.Add(MessageFragment.Opponent(opponent));

            if (opponent.CarNumber.Equals("-1"))
            {
                // AMS2 doesn't have car numbers
                alt.Add(MessageFragment.Text(folderCantPronounceName));
            } else {
                alt.Add(MessageFragment.Text(Opponents.folderCarNumber));
                var num = new CarNumber(opponent.CarNumber).getMessageFragments();
                alt.AddRange(num);

                if (alwaysIncludeNumber) {
                    // this is for cases where it makes sense to know the numbers
                    // that we can distinguish on the car on track.
                    main.AddRange(num);
                }
            }

            return Tuple.Create(main, alt);
        }
        private bool PlayOpponentDetailed(string opponentKey, bool includePosition)
        {
            OpponentData opponent;
            currentGameState.OpponentData.TryGetValue(opponentKey, out opponent);
            return PlayOpponentDetailed(opponent, includePosition);
        }
        private bool PlayOpponentDetailed(OpponentData opponent, bool includePosition)
        {
            if (opponent == null)
            {
                return false;
            }
            Tuple<List<MessageFragment>, List<MessageFragment>> messages = getOpponentDetailed(opponent, includePosition);
            if (messages != null)
            {
                QueuedMessage queuedMessage = new QueuedMessage("opponentDetailed", 0,
                    messageFragments: messages.Item1,
                    alternateMessageFragments: messages.Item2);

                if (queuedMessage.canBePlayed)
                {
                    audioPlayer.playMessageImmediately(queuedMessage);
                    return true;
                }
            }
            return false;
        }

        public override void respond(String voiceMessage)
        {
            Boolean gotData = false;
            if (currentGameState != null)
            {
                if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHAT_TYRES_AM_I_ON))
                {
                    gotData = true;
                    audioPlayer.playMessageImmediately(new QueuedMessage(TyreMonitor.getFolderForTyreType(currentGameState.TyreData.FrontLeftTyreType), 0));
                }
                else if (voiceMessage.StartsWith(SpeechRecogniser.WHAT_TYRE_IS) || voiceMessage.StartsWith(SpeechRecogniser.WHAT_TYRES_IS))
                {
                    // only have data here for r3e and rf2, other games don't expose opponent tyre types
                    if (CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT || CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
                    {
                        string opponentKey = getOpponentKey(voiceMessage, " " + SpeechRecogniser.ON).Item1;
                        if (opponentKey != null)
                        {
                            OpponentData opponentData = currentGameState.OpponentData[opponentKey];
                            if (opponentData != null)
                            {
                                gotData = true;
                                audioPlayer.playMessageImmediately(new QueuedMessage(TyreMonitor.getFolderForTyreType(opponentData.CurrentTyres), 0));
                            }
                        }
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HOW_GOOD_IS))
                {
                    R3ERatingData ratingData = R3ERatings.getRatingForUserId(getOpponentR3EUserId(getOpponentKey(voiceMessage, "").Item1));
                    if (ratingData != null)
                    {
                        gotData = true;
                        Console.WriteLine("got rating data for opponent:" + ratingData.ToString());
                        // if we don't explicitly split the sounds up here they'll be read ints
                        int reputationIntPart = (int)ratingData.reputation;
                        int reputationDecPart = (int)(10 * (ratingData.reputation - (float)reputationIntPart));
                        int ratingIntPart = (int)ratingData.rating;
                        int ratingDecPart = (int)(10 * (ratingData.rating - (float)ratingIntPart));
                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentReputationAndRating", 0,
                            messageFragments: MessageContents(folderReputationIntro, reputationIntPart, NumberReader.folderPoint, reputationDecPart, 
                            folderRatingIntro, ratingIntPart, NumberReader.folderPoint, ratingDecPart)));
                    }
                }
                else if (voiceMessage.StartsWith(SpeechRecogniser.WHATS) &&
                    (voiceMessage.EndsWith(SpeechRecogniser.LAST_LAP) || voiceMessage.EndsWith(SpeechRecogniser.BEST_LAP) || voiceMessage.EndsWith(SpeechRecogniser.LAST_LAP_TIME) || voiceMessage.EndsWith(SpeechRecogniser.BEST_LAP_TIME) ||
                    voiceMessage.EndsWith(SpeechRecogniser.LICENSE_CLASS) ||
                    voiceMessage.EndsWith(SpeechRecogniser.IRATING) ||
                    voiceMessage.EndsWith(SpeechRecogniser.REPUTATION) ||
                    voiceMessage.EndsWith(SpeechRecogniser.RATING) ||
                    voiceMessage.EndsWith(SpeechRecogniser.RANK)))
                {
                    if (voiceMessage.EndsWith(SpeechRecogniser.LAST_LAP) || voiceMessage.EndsWith(SpeechRecogniser.LAST_LAP_TIME))
                    {
                        float lastLap = getOpponentLastLap(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1);
                        if (lastLap != -1)
                        {
                            gotData = true;
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentLastLap", 0, 
                                messageFragments: MessageContents(TimeSpanWrapper.FromSeconds(lastLap, Precision.AUTO_LAPTIMES))));

                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.BEST_LAP) || voiceMessage.EndsWith(SpeechRecogniser.BEST_LAP_TIME))
                    {
                        float bestLap = getOpponentBestLap(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1);
                        if (bestLap != -1)
                        {
                            gotData = true;
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentBestLap", 0, 
                                messageFragments:  MessageContents(TimeSpanWrapper.FromSeconds(bestLap, Precision.AUTO_LAPTIMES))));

                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.LICENSE_CLASS))
                    {
                        Tuple<string, float> licenseLevel = getOpponentLicensLevel(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1);
                        if (licenseLevel.Item2 != -1)
                        {
                            gotData = true;
                            Tuple<int, int> wholeandfractional = Utilities.WholeAndFractionalPart(licenseLevel.Item2, 2);
                            List<MessageFragment> messageFragments = new List<MessageFragment>();

                            if (licenseLevel.Item1.ToLower() == "a")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicenseA));
                            }
                            else if (licenseLevel.Item1.ToLower() == "b")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicenseB));
                            }
                            else if (licenseLevel.Item1.ToLower() == "c")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicenseC));
                            }
                            else if (licenseLevel.Item1.ToLower() == "d")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicenseD));
                            }
                            else if (licenseLevel.Item1.ToLower() == "r")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicenseR));
                            }
                            else if (licenseLevel.Item1.ToLower() == "wc")
                            {
                                messageFragments.Add(MessageFragment.Text(folderLicensePro));
                            }
                            else
                            {
                                gotData = false;
                            }
                            if (gotData)
                            {
                                messageFragments.AddRange(MessageContents(wholeandfractional.Item1, NumberReader.folderPoint, wholeandfractional.Item2));
                                QueuedMessage licenceLevelMessage = new QueuedMessage("License/license", 0, messageFragments: messageFragments);
                                audioPlayer.playDelayedImmediateMessage(licenceLevelMessage);
                            }
                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.IRATING))
                    {
                        int rating = getOpponentIRating(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1);
                        if (rating != -1)
                        {
                            gotData = true;
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentiRating", 0, messageFragments:  MessageContents(rating)));
                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.RATING) && CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
                    {
                        R3ERatingData ratingData = R3ERatings.getRatingForUserId(getOpponentR3EUserId(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1));
                        if (ratingData != null)
                        {
                            gotData = true;
                            Console.WriteLine("got rating data for opponent:" + ratingData.ToString());
                            // if we don't explicitly split the sound up here it'll be read as an int
                            int intPart = (int)ratingData.rating;
                            int decPart = (int)(10 * (ratingData.rating - (float)intPart));
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentRating", 0,
                                messageFragments: MessageContents(intPart, NumberReader.folderPoint, decPart)));
                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.REPUTATION) && CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
                    {
                        R3ERatingData ratingData = R3ERatings.getRatingForUserId(getOpponentR3EUserId(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1));
                        if (ratingData != null)
                        {
                            gotData = true;
                            Console.WriteLine("got rating data for opponent:" + ratingData.ToString());
                            // if we don't explicitly split the sound up here it'll be read as an int
                            int intPart = (int)ratingData.reputation;
                            int decPart = (int)(10 * (ratingData.reputation - (float)intPart));
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentReputation", 0,
                                messageFragments: MessageContents(intPart, NumberReader.folderPoint, decPart)));
                        }
                    }
                    else if (voiceMessage.EndsWith(SpeechRecogniser.RANK) && CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
                    {
                        R3ERatingData ratingData = R3ERatings.getRatingForUserId(getOpponentR3EUserId(getOpponentKey(voiceMessage, SpeechRecogniser.POSSESSIVE + " ").Item1));
                        if (ratingData != null)
                        {
                            gotData = true;
                            Console.WriteLine("got rating data for opponent:" + ratingData.ToString());
                            List<MessageFragment> fragments = new List<MessageFragment>();
                            // ensure hundreds don't get truncated
                            fragments.Add(MessageFragment.Integer(ratingData.rank, false));
                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentRank", 0, messageFragments: fragments));
                        }
                    }
                }
                else if (voiceMessage.StartsWith(SpeechRecogniser.WHERE_IS) || voiceMessage.StartsWith(SpeechRecogniser.WHERES))
                {
                    Tuple<string, Boolean> response = getOpponentKey(voiceMessage, "");
                    string opponentKey = response.Item1;
                    Boolean gotByPositionNumber = response.Item2;
                    OpponentData opponent = null;
                    if (opponentKey != null && currentGameState.OpponentData.TryGetValue(opponentKey, out opponent))
                    {
                        if (opponent.IsActive)
                        {
                            int position = opponent.ClassPosition;
                            Tuple<int, float> deltas = currentGameState.SessionData.DeltaTime.GetSignedDeltaTimeWithLapDifference(opponent.DeltaTime);
                            int lapDifference = deltas.Item1;
                            float timeDelta = deltas.Item2;
                            if (currentGameState.SessionData.SessionType != SessionType.Race || timeDelta == 0 || (lapDifference == 0 && Math.Abs(timeDelta) < 0.05))
                            {
                                // the delta is not usable - say the position if we didn't directly ask by position

                                if (!gotByPositionNumber)
                                {
                                    if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentPosition", 0, 
                                            messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position)));
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentPosition", 0, 
                                            messageFragments: MessageContents(Position.folderStub + position)));
                                    }
                                    gotData = true;
                                }
                            }
                            else
                            {
                                gotData = true;
                                if (lapDifference == 1)
                                {
                                    if (!gotByPositionNumber)
                                    {
                                        if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                                messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position, Pause(200), folderOneLapBehind)));
                                        }
                                        else
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                               messageFragments: MessageContents(Position.folderStub + position, Pause(200), folderOneLapBehind)));
                                        }
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, messageFragments: MessageContents(folderOneLapBehind)));
                                    }
                                }
                                else if (lapDifference > 1)
                                {
                                    if (!gotByPositionNumber)
                                    {
                                        if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                                messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position, Pause(200),
                                                    MessageFragment.Integer(lapDifference, MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)), Position.folderLapsBehind)));
                                        }
                                        else
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, 
                                               messageFragments: MessageContents(Position.folderStub + position, Pause(200), 
                                               MessageFragment.Integer(lapDifference, MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)), Position.folderLapsBehind)));
                                        }
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                            messageFragments: MessageContents(MessageFragment.Integer(lapDifference, MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)),
                                            Position.folderLapsBehind)));
                                    }
                                }
                                else if (lapDifference == -1)
                                {
                                    if (!gotByPositionNumber)
                                    {
                                        if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, 
                                                messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position, Pause(200), folderOneLapAhead)));
                                        }
                                        else
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, 
                                                messageFragments: MessageContents(Position.folderStub + position, Pause(200), folderOneLapAhead)));
                                        }
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, 
                                            messageFragments: MessageContents(folderOneLapAhead)));
                                    }
                                }
                                else if (lapDifference < -1)
                                {
                                    if (!gotByPositionNumber)
                                    {
                                        if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                                messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position, 
                                                    Pause(200), MessageFragment.Integer(Math.Abs(lapDifference), MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)),
                                                    Position.folderLapsAhead)));
                                        }
                                        else
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                                messageFragments: MessageContents(Position.folderStub + position, Pause(200), 
                                                MessageFragment.Integer(Math.Abs(lapDifference), MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)), Position.folderLapsAhead)));
                                        }
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                            messageFragments: MessageContents(MessageFragment.Integer(Math.Abs(lapDifference), MessageFragment.Genders("pt-br", NumberReader.ARTICLE_GENDER.FEMALE)),
                                            Position.folderLapsAhead)));
                                    }
                                }
                                else
                                {
                                    TimeSpanWrapper delta = TimeSpanWrapper.FromSeconds(Math.Abs(timeDelta), Precision.AUTO_GAPS);
                                    String aheadOrBehind = Position.folderAhead;
                                    if (timeDelta >= 0)
                                    {
                                        aheadOrBehind = Position.folderBehind;
                                    }
                                    if (!gotByPositionNumber)
                                    {
                                        if (SoundCache.availableSounds.Contains(folderOpponentPositionIntro))
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, 
                                                messageFragments: MessageContents(folderOpponentPositionIntro, Position.folderStub + position, Pause(200), delta, aheadOrBehind)));
                                        }
                                        else
                                        {
                                            audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0,
                                               messageFragments: MessageContents(Position.folderStub + position, Pause(200), delta, aheadOrBehind)));
                                        }
                                    }
                                    else
                                    {
                                        audioPlayer.playMessageImmediately(new QueuedMessage("opponentTimeDelta", 0, messageFragments: MessageContents(delta, aheadOrBehind)));
                                    }
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Driver " + opponent.DriverRawName + " is no longer active in this session");
                        }
                    }
                }

                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHOS_BEHIND_ON_TRACK))
                {
                    if (PlayOpponentDetailed(currentGameState.getOpponentKeyBehindOnTrack(), includePosition: true))
                    {
                        gotData = true;
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHOS_IN_FRONT_ON_TRACK))
                {
                    if (PlayOpponentDetailed(currentGameState.getOpponentKeyInFrontOnTrack(), includePosition: true))
                    {
                        gotData = true;
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHOS_BEHIND_IN_THE_RACE))
                {
                    if (currentGameState.isLast())
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(Position.folderLast, 0));

                        gotData = true;
                    }
                    else
                    {
                        OpponentData opponent = currentGameState.getOpponentAtClassPosition(currentGameState.SessionData.ClassPosition + 1, currentGameState.carClass);
                        if (PlayOpponentDetailed(opponent, includePosition: false))
                        {
                            gotData = true;
                        }
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHOS_IN_FRONT_IN_THE_RACE))
                {
                    if (currentGameState.SessionData.ClassPosition == 1)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage(Position.folderLeading, 0));

                        gotData = true;
                    }
                    else
                    {
                        OpponentData opponent = currentGameState.getOpponentAtClassPosition(currentGameState.SessionData.ClassPosition - 1, currentGameState.carClass);
                        if (PlayOpponentDetailed(opponent, includePosition: false))
                        {
                            gotData = true;
                        }
                    }
                }
                else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHOS_LEADING) && currentGameState.SessionData.ClassPosition > 1)
                {
                    OpponentData opponent = currentGameState.getOpponentAtClassPosition(1, currentGameState.carClass);
                    if (PlayOpponentDetailed(opponent, includePosition: false))
                    {
                        gotData = true;
                    }
                }
                else if (voiceMessage.StartsWith(SpeechRecogniser.WHOS_IN))
                {
                    string opponentKey = getOpponentKey(voiceMessage, "").Item1;
                    if (opponentKey != null)
                    {
                        if (opponentKey == positionIsPlayerKey)
                        {
                            audioPlayer.playMessageImmediately(new QueuedMessage(folderWeAre, 0));

                            gotData = true;
                        }
                        else
                        {
                            if (PlayOpponentDetailed(opponentKey, includePosition: false))
                            {
                                gotData = true;
                            }
                        }
                    }
                }
            }
            if (!gotData)
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));

            }
        }

        private float getOpponentBestLap(List<float> opponentLapTimes, int lapsToCheck)
        {
            if (opponentLapTimes == null && opponentLapTimes.Count == 0)
            {
                return -1;
            }
            float bestLap = opponentLapTimes[opponentLapTimes.Count - 1];
            int minIndex = opponentLapTimes.Count - lapsToCheck;
            for (int i = opponentLapTimes.Count - 1; i >= minIndex; i--)
            {
                if (opponentLapTimes[i] > 0 && opponentLapTimes[i] < bestLap)
                {
                    bestLap = opponentLapTimes[i];
                }
            }
            return bestLap;
        }
    }
}
