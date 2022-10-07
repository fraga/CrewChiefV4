using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewChiefV4.NumberProcessing;
using CrewChiefV4.Overlay;
using CrewChiefV4.R3E;

namespace CrewChiefV4.Events
{
    class CommonActions : AbstractEvent
    {
        private Boolean keepQuietEnabled = false;
        private Boolean useVerboseResponses = UserSettings.GetUserSettings().getBoolean("use_verbose_responses");
        GameStateData currentGameState = null;
        public CommonActions(AudioPlayer audioPlayer)
        {
            this.audioPlayer = audioPlayer;
        }
        public override void clearState()
        {
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            this.currentGameState = currentGameState;
        }

        public override void respond(String voiceMessage)
        {
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.RADIO_CHECK, false))
            {
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderRadioCheckResponse, 0));
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.KEEP_QUIET, false))
            {
                enableKeepQuietMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.PLAY_CORNER_NAMES, false))
            {
                playCornerNamesForCurrentLap();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DONT_TELL_ME_THE_GAPS, false))
            {
                disableDeltasMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.TELL_ME_THE_GAPS, false))
            {
                enableDeltasMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.ENABLE_YELLOW_FLAG_MESSAGES, false))
            {
                enableYellowFlagMessages();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DISABLE_YELLOW_FLAG_MESSAGES, false))
            {
                disableYellowFlagMessages();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.ENABLE_CUT_TRACK_WARNINGS, false))
            {
                enableCutTrackWarnings();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DISABLE_CUT_TRACK_WARNINGS, false))
            {
                disableCutTrackWarnings();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.ENABLE_MANUAL_FORMATION_LAP, false))
            {
                enableManualFormationLapMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DISABLE_MANUAL_FORMATION_LAP, false))
            {
                disableManualFormationLapMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHATS_THE_TIME, false))
            {
                reportCurrentTime();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.TALK_TO_ME_ANYWHERE, false))
            {
                disableDelayMessagesInHardParts();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DONT_TALK_IN_THE_CORNERS, false))
            {
                enableDelayMessagesInHardParts();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.KEEP_ME_INFORMED, false))
            {
                disableKeepQuietMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.STOP_COMPLAINING, false))
            {
                Console.WriteLine("Disabling complaining messages for this session");
                GlobalBehaviourSettings.maxComplaintsPerSession = 0;
                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHATS_MY_RANK, false))
            {
                R3ERatingData playerRatings = R3ERatings.playerRating;
                if (playerRatings != null)
                {
                    List<MessageFragment> fragments = new List<MessageFragment>();
                    // ensure hundreds don't get truncated
                    fragments.Add(MessageFragment.Integer(playerRatings.rank, false));
                    audioPlayer.playMessageImmediately(new QueuedMessage("playerRank", 0, messageFragments: fragments));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHATS_MY_REPUTATION, false))
            {
                R3ERatingData playerRatings = R3ERatings.playerRating;
                if (playerRatings != null)
                {
                    // if we don't explicitly split the sound up here it'll be read as an int
                    int intPart = (int)playerRatings.reputation;
                    int decPart = (int)(10 * (playerRatings.reputation - (float)intPart));
                    audioPlayer.playMessageImmediately(new QueuedMessage("playerReputation", 0,
                        messageFragments: MessageContents(intPart, NumberReader.folderPoint, decPart)));
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.WHATS_MY_RATING, false))
            {
                R3ERatingData playerRatings = R3ERatings.playerRating;
                if (playerRatings != null)
                {
                    // if we don't explicitly split the sound up here it'll be read as an int
                    int intPart = (int)playerRatings.rating;
                    int decPart = (int)(10 * (playerRatings.rating - (float)intPart));
                    audioPlayer.playMessageImmediately(new QueuedMessage("playerRating", 0,
                        messageFragments: MessageContents(intPart, NumberReader.folderPoint, decPart)));
                }
            }

            // multiple events for status reporting:
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.DAMAGE_REPORT, false) ||
                SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.GET_DAMAGE_REPORT }))
            {
                Console.WriteLine("Getting damage report");
                getDamageReport();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.CAR_STATUS, false) ||
                SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.GET_CAR_STATUS }))
            {
                Console.WriteLine("Getting car status");
                getCarStatus();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.STATUS, false) ||
                SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.GET_STATUS }))
            {
                Console.WriteLine("Getting full status");
                getStatus();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.SESSION_STATUS, false) ||
                SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.GET_SESSION_STATUS }))
            {
                Console.WriteLine("Getting session status");
                getSessionStatus();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.START_PACE_NOTES_PLAYBACK, false))
            {
                if (!DriverTrainingService.isPlayingPaceNotes)
                {
                    togglePaceNotesPlayback();
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.STOP_PACE_NOTES_PLAYBACK, false))
            {
                if (DriverTrainingService.isPlayingPaceNotes)
                {
                    togglePaceNotesPlayback();
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.SHOW_SUBTITLES, false))
            {
                SubtitleOverlay.shown = true;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HIDE_SUBTITLES, false))
            {
                SubtitleOverlay.shown = false;
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_RACE_UPDATES_FUNCTION }))
            {
                Console.WriteLine("Toggling keep quiet mode");
                toggleKeepQuietMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_READ_OPPONENT_DELTAS }))
            {
                Console.WriteLine("Toggling read opponent deltas mode");
                toggleReadOpponentDeltasMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_MANUAL_FORMATION_LAP }))
            {
                Console.WriteLine("Toggling manual formation lap mode");
                toggleManualFormationLapMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.READ_CORNER_NAMES_FOR_LAP }))
            {
                Console.WriteLine("Enabling corner name reading for current lap");
                playCornerNamesForCurrentLap();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.REPEAT_LAST_MESSAGE_BUTTON }))
            {
                Console.WriteLine("Repeating last message");
                audioPlayer.repeatLastMessage();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_YELLOW_FLAG_MESSAGES }))
            {
                Console.WriteLine("Toggling yellow flag messages to: " + (CrewChief.yellowFlagMessagesEnabled ? "disabled" : "enabled"));
                toggleEnableYellowFlagsMode();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_BLOCK_MESSAGES_IN_HARD_PARTS }))
            {
                Console.WriteLine("Toggling delay-messages-in-hard-parts");
                toggleDelayMessagesInHardParts();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.GET_FUEL_STATUS }))
            {
                Console.WriteLine("Getting fuel/battery status");
                reportFuelBatteryStatus();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_PACE_NOTES_RECORDING }))
            {
                Console.WriteLine("Start / stop pace notes recording");
                togglePaceNotesRecording();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_PACE_NOTES_PLAYBACK }))
            {
                Console.WriteLine("Start / stop pace notes playback");
                togglePaceNotesPlayback();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_TRACK_LANDMARKS_RECORDING }))
            {
                Console.WriteLine("Start / stop track landmark recording");
                toggleTrackLandmarkRecording();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.TOGGLE_ENABLE_CUT_TRACK_WARNINGS }))
            {
                Console.WriteLine("Enable / disable cut track warnings");
                toggleEnableCutTrackWarnings();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.ADD_TRACK_LANDMARK }))
            {
                //dont confirm press here we do that in addLandmark
                toggleAddTrackLandmark();
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.PIT_PREDICTION }))
            {
                Console.WriteLine("pit prediction");
                if (currentGameState != null)
                {
                    Strategy strategy = (Strategy)CrewChief.getEvent("Strategy");
                    if (currentGameState.SessionData.SessionType == SessionType.Race)
                    {
                        strategy.respondRace();
                    }
                    else if (currentGameState.SessionData.SessionType == SessionType.Practice)
                    {
                        strategy.respondPracticeStop();
                    }
                }
            }
            else if (SpeechRecogniser.ResultContains(voiceMessage, new string[] { ControllerConfiguration.PRINT_TRACK_DATA }))
            {
                if (currentGameState != null && currentGameState.SessionData != null &&
                    currentGameState.SessionData.TrackDefinition != null)
                {
                    string posInfo = "";
                    var worldPos = currentGameState.PositionAndMotionData.WorldPosition;
                    if (worldPos != null && worldPos.Length > 2)
                    {
                        posInfo = string.Format(", position x:{0:0.000} y:{1:0.000} z:{2:0.000}", worldPos[0], worldPos[1], worldPos[2]);
                    }
                    if (CrewChief.gameDefinition.gameEnum == GameEnum.RACE_ROOM)
                    {
                        Console.WriteLine("RaceroomLayoutId: " + currentGameState.SessionData.TrackDefinition.id + ", distanceRoundLap:" +
                            currentGameState.PositionAndMotionData.DistanceRoundTrack.ToString("0.000") + ", player's car ID: " + currentGameState.carClass.getClassIdentifier() + posInfo);
                    }
                    else
                    {
                        Console.WriteLine("TrackName: " + currentGameState.SessionData.TrackDefinition.name + ", distanceRoundLap:" +
                            currentGameState.PositionAndMotionData.DistanceRoundTrack.ToString("0.000") + ", player's car ID: " + currentGameState.carClass.getClassIdentifier() + posInfo);
                    }
                }
                else
                {
                    Console.WriteLine("No track data available");
                }
            }
            //Console.WriteLine(voiceMessage);
        }

        public void enableKeepQuietMode()
        {
            keepQuietEnabled = true;
            audioPlayer.enableKeepQuietMode();
        }
        public void disableKeepQuietMode()
        {
            keepQuietEnabled = false;
            // also disable the global speak-only-when-spoken-to setting
            GlobalBehaviourSettings.speakOnlyWhenSpokenTo = false;
            audioPlayer.disableKeepQuietMode();
        }
        public void toggleKeepQuietMode()
        {
            if (keepQuietEnabled)
            {
                disableKeepQuietMode();
            }
            else
            {
                enableKeepQuietMode();
            }
        }

        public void toggleEnableCutTrackWarnings()
        {
            if (GlobalBehaviourSettings.cutTrackWarningsEnabled)
            {
                disableCutTrackWarnings();
            }
            else
            {
                enableCutTrackWarnings();
            }
        }

        public void enableCutTrackWarnings()
        {
            GlobalBehaviourSettings.cutTrackWarningsEnabled = true;
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderCutWarningsEnabled, 0));
        }

        public void disableCutTrackWarnings()
        {
            GlobalBehaviourSettings.cutTrackWarningsEnabled = false;
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderCutWarningsDisabled, 0));
        }

        public void toggleDelayMessagesInHardParts()
        {
            if (AudioPlayer.delayMessagesInHardParts)
            {
                disableDelayMessagesInHardParts();
            }
            else
            {
                enableDelayMessagesInHardParts();
            }
        }

        public void enableDelayMessagesInHardParts()
        {
            if (!AudioPlayer.delayMessagesInHardParts)
            {
                AudioPlayer.delayMessagesInHardParts = true;
            }
            // switch the gap points to use the adjusted ones
            if (currentGameState != null && currentGameState.SessionData.TrackDefinition != null && currentGameState.hardPartsOnTrackData.hardPartsMapped)
            {
                currentGameState.SessionData.TrackDefinition.adjustGapPoints(currentGameState.hardPartsOnTrackData.processedHardPartsForBestLap);
            }
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowledgeEnableDelayInHardParts, 0));
        }

        public void disableDelayMessagesInHardParts()
        {
            if (AudioPlayer.delayMessagesInHardParts)
            {
                AudioPlayer.delayMessagesInHardParts = false;
            }
            // switch the gap points back to use the regular ones
            if (currentGameState != null && currentGameState.SessionData.TrackDefinition != null && currentGameState.hardPartsOnTrackData.hardPartsMapped)
            {
                currentGameState.SessionData.TrackDefinition.setGapPoints();
            }
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowledgeDisableDelayInHardParts, 0));
        }

        public void toggleReadOpponentDeltasMode()
        {
            if (CrewChief.readOpponentDeltasForEveryLap)
            {
                disableDeltasMode();
            }
            else
            {
                enableDeltasMode();
            }
        }

        public void enableDeltasMode()
        {
            if (!CrewChief.readOpponentDeltasForEveryLap)
            {
                CrewChief.readOpponentDeltasForEveryLap = true;
            }
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDeltasEnabled, 0));
        }

        public void disableDeltasMode()
        {
            if (CrewChief.readOpponentDeltasForEveryLap)
            {
                CrewChief.readOpponentDeltasForEveryLap = false;
            }
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderDeltasDisabled, 0));
        }

        public void toggleEnableYellowFlagsMode()
        {
            if (CrewChief.yellowFlagMessagesEnabled)
            {
                disableYellowFlagMessages();
            }
            else
            {
                enableYellowFlagMessages();
            }
        }

        public void enableYellowFlagMessages()
        {
            CrewChief.yellowFlagMessagesEnabled = true;
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderYellowEnabled, 0));
        }

        public void disableYellowFlagMessages()
        {
            CrewChief.yellowFlagMessagesEnabled = false;
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderYellowDisabled, 0));
        }

        public void toggleManualFormationLapMode()
        {
            if (GameStateData.useManualFormationLap)
            {
                disableManualFormationLapMode();
            }
            else
            {
                enableManualFormationLapMode();
            }
        }

        public void enableManualFormationLapMode()
        {
            // Prevent accidential trigger during the race.  Luckily, there's a handy hack available :)
            if (currentGameState != null && currentGameState.SessionData.SessionType == SessionType.Race && currentGameState.SessionData.CompletedLaps >= 1)
            {
                Console.WriteLine("Rejecting manual formation lap request due to race already in progress");
                return;
            }
            if (!GameStateData.useManualFormationLap)
            {
                GameStateData.useManualFormationLap = true;
                GameStateData.onManualFormationLap = true;
            }
            Console.WriteLine("Manual formation lap mode is ACTIVE");
            audioPlayer.playMessageImmediately(new QueuedMessage(LapCounter.folderManualFormationLapModeEnabled, 0));
        }

        public void disableManualFormationLapMode()
        {
            if (GameStateData.useManualFormationLap)
            {
                GameStateData.useManualFormationLap = false;
                GameStateData.onManualFormationLap = false;
            }
            Console.WriteLine("Manual formation lap mode is DISABLED");
            audioPlayer.playMessageImmediately(new QueuedMessage(LapCounter.folderManualFormationLapModeDisabled, 0));
        }

        private void reportFuelBatteryStatus()
        {
            if (GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.BATTERY))
            {
                ((Battery)CrewChief.getEvent("Battery")).reportBatteryStatus(true);
                if (useVerboseResponses)
                {
                    ((Battery)CrewChief.getEvent("Battery")).reportExtendedBatteryStatus(true, false);
                }
            }
            else
            {
                ((Fuel)CrewChief.getEvent("Fuel")).reportFuelStatus(true, (CrewChief.currentGameState != null && CrewChief.currentGameState.SessionData.SessionType == SessionType.Race));
            }
        }
        public void playCornerNamesForCurrentLap()
        {
            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
            if (currentGameState == null && CrewChief.currentGameState != null)
                CrewChief.currentGameState.readLandmarksForThisLap = true;
            else if (currentGameState != null)
                currentGameState.readLandmarksForThisLap = true;
        }

        public void togglePaceNotesPlayback()
        {
            if (DriverTrainingService.isPlayingPaceNotes)
            {
                DriverTrainingService.stopPlayingPaceNotes();
                if (SoundCache.availableSounds.Contains(DriverTrainingService.folderEndedPlayback))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(DriverTrainingService.folderEndedPlayback, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                }
            }
            else
            {
                if (CrewChief.currentGameState != null && CrewChief.currentGameState.SessionData.TrackDefinition != null)
                {
                    if (!DriverTrainingService.isPlayingPaceNotes)
                    {
                        if (DriverTrainingService.loadPaceNotes(CrewChief.gameDefinition.gameEnum,
                                CrewChief.currentGameState.SessionData.TrackDefinition.name, CrewChief.currentGameState.carClass.carClassEnum, audioPlayer))
                        {
                            if (SoundCache.availableSounds.Contains(DriverTrainingService.folderStartedPlayback))
                            {
                                audioPlayer.playMessageImmediately(new QueuedMessage(DriverTrainingService.folderStartedPlayback, 0));
                            }
                            else
                            {
                                audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                            }
                        }
                        else
                        {
                            audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderNoData, 0));
                            Console.WriteLine("Attempted to start pace notes, but none are available for this circuit");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No track or car has been loaded - start an on-track session before loading a pace notes");
                }
            }
        }

        public void togglePaceNotesRecording()
        {
            if (DriverTrainingService.isRecordingPaceNotes)
            {
                DriverTrainingService.completeRecordingPaceNotes();
                if (SoundCache.availableSounds.Contains(DriverTrainingService.folderEndedRecording))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(DriverTrainingService.folderEndedRecording, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                }
            }
            else
            {
                if (CrewChief.trackName == null || CrewChief.trackName.Equals(""))
                {
                    Console.WriteLine("No track has been loaded - start an on-track session before recording pace notes");
                    return;
                }
                if (CrewChief.carClass == CarData.CarClassEnum.UNKNOWN_RACE || CrewChief.carClass == CarData.CarClassEnum.USER_CREATED)
                {
                    Console.WriteLine("No car class has been set - this pace notes session will not be class specific");
                }
                
                if (SoundCache.availableSounds.Contains(DriverTrainingService.folderStartedRecording))
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(DriverTrainingService.folderStartedRecording, 0));
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
                }
                DriverTrainingService.startRecordingPaceNotes(CrewChief.gameDefinition.gameEnum,
                    CrewChief.trackName, CrewChief.carClass);
            }
        }
        public void toggleTrackLandmarkRecording()
        {
            if (TrackLandMarksRecorder.isRecordingTrackLandmarks)
            {
                TrackLandMarksRecorder.completeRecordingTrackLandmarks();
            }
            else
            {
                if (CrewChief.trackName == null || CrewChief.trackName.Equals(""))
                {
                    Console.WriteLine("No track has been loaded - start an on-track session before recording landmarks");
                    return;
                }
                else
                {
                    TrackLandMarksRecorder.startRecordingTrackLandmarks(CrewChief.gameDefinition.gameEnum,
                    CrewChief.trackName, CrewChief.raceroomTrackId);
                }

            }
        }
        public void toggleAddTrackLandmark()
        {
            if (TrackLandMarksRecorder.isRecordingTrackLandmarks)
            {
                TrackLandMarksRecorder.addLandmark(CrewChief.distanceRoundTrack);
            }
        }
        // nasty... these triggers come from the speech recogniser or from button presses, and invoke speech
        // recognition 'respond' methods in the events
        public void getStatus()
        {
            CrewChief.getEvent("Penalties").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("RaceTime").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("Position").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("PitStops").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("DamageReporting").respond(SpeechRecogniser.STATUS[0]);
            if (GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.BATTERY))
            {
                CrewChief.getEvent("Battery").respond(SpeechRecogniser.CAR_STATUS[0]);
            }
            else
            {
                CrewChief.getEvent("Fuel").respond(SpeechRecogniser.CAR_STATUS[0]);
            }
            CrewChief.getEvent("TyreMonitor").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("EngineMonitor").respond(SpeechRecogniser.STATUS[0]);
            CrewChief.getEvent("Timings").respond(SpeechRecogniser.STATUS[0]);
        }

        public void getSessionStatus()
        {
            CrewChief.getEvent("Penalties").respond(SpeechRecogniser.SESSION_STATUS[0]);
            CrewChief.getEvent("RaceTime").respond(SpeechRecogniser.SESSION_STATUS[0]);
            CrewChief.getEvent("Position").respond(SpeechRecogniser.SESSION_STATUS[0]);
            CrewChief.getEvent("PitStops").respond(SpeechRecogniser.SESSION_STATUS[0]);
            CrewChief.getEvent("Timings").respond(SpeechRecogniser.SESSION_STATUS[0]);
        }

        public void getCarStatus()
        {
            CrewChief.getEvent("DamageReporting").respond(SpeechRecogniser.CAR_STATUS[0]);
            if (GlobalBehaviourSettings.enabledMessageTypes.Contains(MessageTypes.BATTERY))
            {
                CrewChief.getEvent("Battery").respond(SpeechRecogniser.CAR_STATUS[0]);
            }
            else
            {
                CrewChief.getEvent("Fuel").respond(SpeechRecogniser.CAR_STATUS[0]);
            }
            CrewChief.getEvent("TyreMonitor").respond(SpeechRecogniser.CAR_STATUS[0]);
            CrewChief.getEvent("EngineMonitor").respond(SpeechRecogniser.CAR_STATUS[0]);
        }

        public void getDamageReport()
        {
            CrewChief.getEvent("DamageReporting").respond(SpeechRecogniser.DAMAGE_REPORT[0]);
        }

        public void reportCurrentTime()
        {
            DateTime now = DateTime.Now;
            int hour = now.Hour;
            int minute = now.Minute;
            Boolean isPastMidDay = false;
            if (hour >= 12)
            {
                isPastMidDay = true;
            }
            if ("it".Equals(AudioPlayer.soundPackLanguage, StringComparison.InvariantCultureIgnoreCase))
            {
                audioPlayer.playMessageImmediately(new QueuedMessage("current_time", 0,
                    messageFragments: AbstractEvent.MessageContents(hour, NumberReaderIt2.folderAnd, now.Minute)));
            }
            else
            {
                if (hour == 0)
                {
                    isPastMidDay = false;
                    hour = 24;
                }
                if (hour > 12)
                {
                    hour = hour - 12;
                }
                if (minute < 10)
                {
                    if (minute == 0)
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage("current_time", 0,
                           messageFragments: AbstractEvent.MessageContents(hour, isPastMidDay ? AlarmClock.folderPM : AlarmClock.folderAM)));
                    }
                    else
                    {
                        audioPlayer.playMessageImmediately(new QueuedMessage("current_time", 0,
                            messageFragments: AbstractEvent.MessageContents(hour, NumberReader.folderOh, now.Minute, isPastMidDay ? AlarmClock.folderPM : AlarmClock.folderAM)));
                    }
                }
                else
                {
                    audioPlayer.playMessageImmediately(new QueuedMessage("current_time", 0,
                        messageFragments: AbstractEvent.MessageContents(hour, now.Minute, isPastMidDay ? AlarmClock.folderPM : AlarmClock.folderAM)));
                }
            }
        }

    }
}
