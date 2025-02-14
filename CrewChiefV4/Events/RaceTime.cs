﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using CrewChiefV4.Audio;
using CrewChiefV4.NumberProcessing;

namespace CrewChiefV4.Events
{
    class RaceTime : AbstractEvent
    {
        private String folder5mins = "race_time/five_minutes_left";
        private String folder5minsLeading = "race_time/five_minutes_left_leading";
        private String folder5minsPodium = "race_time/five_minutes_left_podium";
        private String folder2mins = "race_time/two_minutes_left";
        private String folder0mins = "race_time/zero_minutes_left";

        private String folder10mins = "race_time/ten_minutes_left";
        private String folder15mins = "race_time/fifteen_minutes_left";
        private String folder20mins = "race_time/twenty_minutes_left";
        public static String folderHalfWayHome = "race_time/half_way";
        private String folderLastLap = "race_time/last_lap";
        private String folderLastLapLeading = "race_time/last_lap_leading";
        private String folderLastLapPodium = "race_time/last_lap_top_three";

        public static String folderRemaining = "race_time/remaining";
        private String folderLapsLeft = "race_time/laps_remaining";

        private String folderLessThanOneMinute = "race_time/less_than_one_minute";

        private String folderThisIsTheLastLap = "race_time/this_is_the_last_lap";

        private String folderOneMinuteRemaining = "race_time/one_minute_remaining";

        private String folderOneLapAfterThisOne = "race_time/one_more_lap_after_this_one";

        private Boolean played0mins, played2mins, played5mins, played10mins, played15mins, played20mins, playedHalfWayHome, playedLastLap;

        private float halfTime;

        private Boolean gotHalfTime;

        private int lapsLeft;
        private float timeLeft;

        private int extraLapsAfterTimedSessionComplete;

        private int extraLapsRemaining = 0;

        private Boolean leaderHasFinishedRace;

        private Boolean sessionLengthIsTime;

        // allow condition messages during caution periods
        public override List<SessionPhase> applicableSessionPhases
        {
            get { return new List<SessionPhase> { SessionPhase.Green, SessionPhase.Checkered, SessionPhase.FullCourseYellow, SessionPhase.Formation }; }
        }

        public RaceTime(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }

        public override void clearState()
        {
            played0mins = false; played2mins = false; played5mins = false; played10mins = false; played15mins = false;
            played20mins = false; playedHalfWayHome = false; playedLastLap = false;
            halfTime = 0;
            gotHalfTime = false;
            lapsLeft = -1;
            timeLeft = 0;
            sessionLengthIsTime = false;
            leaderHasFinishedRace = false;
            extraLapsAfterTimedSessionComplete = 0;
            extraLapsRemaining = 0;
        }
        
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            // For now, scope Formation phase to GTR2 only.
            if (CrewChief.gameDefinition.gameEnum != GameEnum.GTR2
                && currentGameState.SessionData.SessionPhase == SessionPhase.Formation)
                return;

            if (CrewChief.gameDefinition.gameEnum == GameEnum.IRACING
                && currentGameState.SessionData.SessionType == SessionType.Race
                && previousGameState != null
                && currentGameState.SessionData.SessionHasFixedTime
                && previousGameState.SessionData.SessionTimeRemaining == -1
                && currentGameState.SessionData.SessionTimeRemaining > 0)
            {
                played0mins = false; played2mins = false; played5mins = false; played10mins = false; played15mins = false;
                played20mins = false; playedHalfWayHome = false; playedLastLap = false; gotHalfTime = false;
            }
            // store this in a local var so it's available for voice command responses
            extraLapsAfterTimedSessionComplete = currentGameState.SessionData.ExtraLapsAfterTimedSessionComplete;
            leaderHasFinishedRace = currentGameState.SessionData.LeaderHasFinishedRace;
            timeLeft = currentGameState.SessionData.SessionTimeRemaining;
            if (!currentGameState.SessionData.SessionHasFixedTime)
            {
                lapsLeft = currentGameState.SessionData.SessionLapsRemaining;
                sessionLengthIsTime = false;
            }
            else
            {
                sessionLengthIsTime = true;
            }
            if (sessionLengthIsTime)
            {
                if (timeLeft > 0)
                {
                    extraLapsRemaining = extraLapsAfterTimedSessionComplete;
                }
                if (extraLapsAfterTimedSessionComplete > 0 && gotHalfTime && timeLeft <= 0 && currentGameState.SessionData.IsNewLap)
                {
                    extraLapsRemaining--;
                }
                if (!gotHalfTime
                    && (CrewChief.gameDefinition.gameEnum != GameEnum.GTR2 || currentGameState.inCar))  // No timed session length in GTR2 until we are in the realtime.
                {
                    Console.WriteLine("Session time remaining = " + timeLeft + "  (" + TimeSpan.FromSeconds(timeLeft).ToString(@"hh\:mm\:ss\:fff") + ")");
                    halfTime = timeLeft / 2;
                    gotHalfTime = true;
                    if (currentGameState.FuelData.FuelUseActive)
                    {
                        // don't allow the half way message to play if fuel use is active - there's already one in there
                        playedHalfWayHome = true;
                    }
                }
                PearlsOfWisdom.PearlType pearlType = PearlsOfWisdom.PearlType.NONE;
                if (currentGameState.SessionData.SessionType == SessionType.Race 
                    && currentGameState.SessionData.CompletedLaps >= LapTimes.lapsBeforeAnnouncingGaps[currentGameState.SessionData.TrackDefinition.trackLengthClass])
                {
                    pearlType = PearlsOfWisdom.PearlType.NEUTRAL;
                    if (currentGameState.SessionData.ClassPosition < 4)
                    {
                        pearlType = PearlsOfWisdom.PearlType.GOOD;
                    }
                    else if (currentGameState.SessionData.ClassPosition > currentGameState.SessionData.SessionStartClassPosition + 5 &&
                        !currentGameState.PitData.OnOutLap && !currentGameState.PitData.InPitlane &&
                        currentGameState.SessionData.LapTimePrevious > currentGameState.TimingData.getPlayerBestLapTime() &&
                        // yuk... AC SessionStartPosition is suspect so don't allow "you're shit" messages based on it.
                        CrewChief.gameDefinition.gameEnum != GameEnum.ASSETTO_32BIT && CrewChief.gameDefinition.gameEnum != GameEnum.ASSETTO_64BIT)
                    {
                        // don't play bad-pearl if we're on an out lap or are pitting
                        pearlType = PearlsOfWisdom.PearlType.BAD;
                    }
                }

                if (currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.IsNewLap &&
                    currentGameState.SessionData.SessionRunningTime > 60 && !playedLastLap)
                {
                    Boolean timeWillBeZeroAtEndOfLeadersLap = false;
                    if (currentGameState.SessionData.OverallPosition == 1)
                    {
                        float playerBest = currentGameState.TimingData.getPlayerBestLapTime();
                        timeWillBeZeroAtEndOfLeadersLap = timeLeft > 0 && playerBest > 0 &&
                            timeLeft < playerBest - 5;
                    }
                    else
                    {
                        OpponentData leader = currentGameState.getOpponentAtClassPosition(1, currentGameState.carClass);
                        timeWillBeZeroAtEndOfLeadersLap = leader != null && leader.isProbablyLastLap;
                    }
                    if ((extraLapsAfterTimedSessionComplete > 0 && extraLapsRemaining == 0 && timeLeft <= 0) ||
                        (extraLapsAfterTimedSessionComplete == 0 && timeWillBeZeroAtEndOfLeadersLap)) {
                        playedLastLap = true;
                        played2mins = true;
                        played5mins = true;
                        played10mins = true;
                        played15mins = true;
                        played20mins = true;
                        playedHalfWayHome = true;
                        // rF2 and iR implement SessionData.IsLastLap so last lap logic is handled in LapCounter.
                        if (CrewChief.gameDefinition.gameEnum != GameEnum.RF2_64BIT && CrewChief.gameDefinition.gameEnum != GameEnum.IRACING)
                        {
                            if (currentGameState.SessionData.ClassPosition == 1)
                            {
                                // don't add a pearl here - the audio clip already contains encouragement
                                audioPlayer.playMessage(new QueuedMessage(folderLastLapLeading, 10, abstractEvent: this, priority: 5), pearlType, 0);
                            }
                            else if (currentGameState.SessionData.ClassPosition < 4)
                            {
                                // don't add a pearl here - the audio clip already contains encouragement
                                audioPlayer.playMessage(new QueuedMessage(folderLastLapPodium, 10, abstractEvent: this, priority: 5), pearlType, 0);
                            }
                            else
                            {
                                audioPlayer.playMessage(new QueuedMessage(folderLastLap, 10, abstractEvent: this, priority: 5));
                            }
                        }
                    }
                }
                if (currentGameState.SessionData.SessionRunningTime > 60 && timeLeft / 60 < 3 && timeLeft / 60 > 2.9)
                {
                    // disable pearls for the last part of the race
                    audioPlayer.disablePearlsOfWisdom = true;
                }
                // Console.WriteLine("Session time left = " + timeLeft + " SessionRunningTime = " + currentGameState.SessionData.SessionRunningTime);
                if (currentGameState.SessionData.ExtraLapsAfterTimedSessionComplete == 0 && 
                    currentGameState.SessionData.SessionRunningTime > 0 && !played0mins && timeLeft <= 0.2)
                {
                    played0mins = true;
                    played2mins = true;
                    played5mins = true;
                    played10mins = true;
                    played15mins = true;
                    played20mins = true;
                    playedHalfWayHome = true;
                    audioPlayer.suspendPearlsOfWisdom();
                    // PCars hack - don't play this if it's an unlimited session - no lap limit and no time limit
                    if (!currentGameState.SessionData.SessionHasFixedTime && currentGameState.SessionData.SessionNumberOfLaps <= 0)
                    {
                        Console.WriteLine("Skipping session end messages for unlimited session");
                    }
                    else if (currentGameState.SessionData.SessionType != SessionType.Race
                        && !(CrewChief.gameDefinition.gameEnum != GameEnum.RACE_ROOM &&
                             (currentGameState.SessionData.SessionType == SessionType.Qualify
                             || currentGameState.SessionData.SessionType == SessionType.PrivateQualify)))
                    {
                        // don't play the chequered flag message in race sessions or in R3E qual sessions (where the session end trigger takes care if things)
                        audioPlayer.playMessage(new QueuedMessage("session_complete", 5,
                            messageFragments: MessageContents(folder0mins, Position.folderStub + currentGameState.SessionData.ClassPosition), abstractEvent: this, priority: 10));
                    }
                } 
                if (currentGameState.SessionData.SessionRunningTime > 60 && !played2mins && timeLeft / 60 < 2 && timeLeft / 60 > 1.9)
                {
                    played2mins = true;
                    played5mins = true;
                    played10mins = true;
                    played15mins = true;
                    played20mins = true;
                    playedHalfWayHome = true;
                    audioPlayer.suspendPearlsOfWisdom();
                    audioPlayer.playMessage(new QueuedMessage(folder2mins, 15, abstractEvent: this, priority: 10));
                }
                if (currentGameState.SessionData.SessionRunningTime > 120 && !played5mins && timeLeft / 60 < 5 && timeLeft / 60 > 4.9)
                {
                    played5mins = true;
                    played10mins = true;
                    played15mins = true;
                    played20mins = true;
                    playedHalfWayHome = true;
                    if (currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.ClassPosition == 1)
                    {
                        // don't add a pearl here - the audio clip already contains encouragement
                        audioPlayer.playMessage(new QueuedMessage(folder5minsLeading, 20, abstractEvent: this, priority: 5), pearlType, 0);
                    }
                    else if (currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.ClassPosition < 4)
                    {
                        // don't add a pearl here - the audio clip already contains encouragement
                        audioPlayer.playMessage(new QueuedMessage(folder5minsPodium, 20, abstractEvent: this, priority: 5), pearlType, 0);
                    }
                    else
                    {
                        audioPlayer.playMessage(new QueuedMessage(folder5mins, 20, abstractEvent: this, priority: 5), pearlType, 0.7);
                    }
                }
                if (currentGameState.SessionData.SessionRunningTime > 120 && !played10mins && timeLeft / 60 < 10 && timeLeft / 60 > 9.9)
                {
                    played10mins = true;
                    played15mins = true;
                    played20mins = true;
                    audioPlayer.playMessage(new QueuedMessage(folder10mins, 20, abstractEvent: this, priority: 3), pearlType, 0.7);
                }
                if (currentGameState.SessionData.SessionRunningTime > 120 && !played15mins && timeLeft / 60 < 15 && timeLeft / 60 > 14.9)
                {
                    played15mins = true;
                    played20mins = true;
                    audioPlayer.playMessage(new QueuedMessage(folder15mins, 20, abstractEvent: this, priority: 3), pearlType, 0.7);
                }
                if (currentGameState.SessionData.SessionRunningTime > 120 && !played20mins && timeLeft / 60 < 20 && timeLeft / 60 > 19.9)
                {
                    played20mins = true;
                    audioPlayer.playMessage(new QueuedMessage(folder20mins, 20, abstractEvent: this, priority: 3), pearlType, 0.7);
                }
                else if (currentGameState.SessionData.SessionType == SessionType.Race &&
                    currentGameState.SessionData.SessionRunningTime > 120 && !playedHalfWayHome && timeLeft > 0 && timeLeft < halfTime)
                {
                    // this one sounds weird in practice and qual sessions, so skip it
                    playedHalfWayHome = true;
                    audioPlayer.playMessage(new QueuedMessage(folderHalfWayHome, 20, abstractEvent: this, priority: 3), pearlType, 0.7);
                }
            }
        }

        public override void respond(string voiceMessage)
        {
            if (sessionLengthIsTime)
            {
                if (CrewChief.gameDefinition.gameEnum == GameEnum.RF2_64BIT
                    && CrewChief.currentGameState != null
                    && CrewChief.currentGameState.SessionData.SessionType == SessionType.Race
                    && (CrewChief.currentGameState.FrozenOrderData.Phase == FrozenOrderPhase.FastRolling
                        || CrewChief.currentGameState.FrozenOrderData.Phase == FrozenOrderPhase.Rolling
                        || CrewChief.currentGameState.SessionData.SessionPhase == SessionPhase.Formation
                        || CrewChief.currentGameState.SessionData.SessionPhase == SessionPhase.Garage
                        || CrewChief.currentGameState.SessionData.SessionPhase == SessionPhase.Gridwalk))
                {
                    // rF2 data is shit for above cases.
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                    return;
                }

                if (leaderHasFinishedRace)
                {
                    Console.WriteLine("Playing last lap message, timeleft = " + timeLeft);
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderThisIsTheLastLap, 0));
                }
                if (timeLeft >= 120)
                {
                    int minutesLeft = (int)Math.Round(timeLeft / 60f);
                    audioPlayer.playMessageImmediately(new QueuedMessage("RaceTime/time_remaining", 0,
                        messageFragments: MessageContents(TimeSpanWrapper.FromMinutes(minutesLeft, Precision.MINUTES), folderRemaining)));
                }
                else if (timeLeft >= 60)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderOneMinuteRemaining, 0));
                }
                else if (timeLeft <= 0)
                {
                    if (extraLapsAfterTimedSessionComplete > 0 && extraLapsRemaining == 1)
                    {
                        Console.WriteLine("Playing extra lap one more lap message, timeleft = " + timeLeft);
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderOneLapAfterThisOne, 0));
                    }
                    else if (extraLapsAfterTimedSessionComplete > 0 && extraLapsRemaining > 1)
                    {
                        Console.WriteLine("Playing extra lap message, timeleft = " + timeLeft);
                        audioPlayer.playMessageImmediately(new QueuedMessage("RaceTime/laps_remaining", 0,
                            messageFragments: MessageContents(extraLapsRemaining, folderLapsLeft)));
                    }
                    else 
                    {
                        Console.WriteLine("Playing last lap message, timeleft = " + timeLeft);
                        audioPlayer.playMessageImmediately(new QueuedMessage(folderThisIsTheLastLap, 0));
                    }
                }
                else if (timeLeft < 60)
                {
                    Console.WriteLine("Playing less than a minute message, timeleft = " + timeLeft);
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderLessThanOneMinute, 0));
                }
            }
            else
            {
                if (lapsLeft > 2)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("RaceTime/laps_remaining", 0,
                        messageFragments: MessageContents(lapsLeft, folderLapsLeft)));
                }
                else if (lapsLeft == 2)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderOneLapAfterThisOne, 0));
                }
                else if (lapsLeft == 1)
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(folderThisIsTheLastLap, 0));
                }
            }     
        }
    }
}
