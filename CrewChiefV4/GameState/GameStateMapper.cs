﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrewChiefV4.GameState
{
    public abstract class GameStateMapper
    {
        protected SpeechRecogniser speechRecogniser;

        /** May return null if the game state raw data is considered invalid */
        public abstract GameStateData mapToGameStateData(Object memoryMappedFileStruct, GameStateData previousGameState);

        public abstract void versionCheck(Object memoryMappedFileStruct);

        public abstract SessionType mapToSessionType(Object memoryMappedFileStruct);

        public virtual void setSpeechRecogniser(SpeechRecogniser speechRecogniser)
        {
            this.speechRecogniser = speechRecogniser;
        }

        // This method populates data derived from the mapped data, that's common to all games.
        // This is specific to race sessions. At the time of writing there's nothing needed for qual and
        // practice, but these may be added later
        public virtual void populateDerivedRaceSessionData(GameStateData currentGameState)
        {
            Boolean singleClass = GameStateData.NumberOfClasses == 1 || GameStateData.forceSingleClass(currentGameState);
            // always set the session start class position and lap start class position:
            if (currentGameState.SessionData.JustGoneGreen || currentGameState.SessionData.IsNewSession)
            {
                currentGameState.SessionData.SessionStartClassPosition = currentGameState.SessionData.ClassPosition;
                if (singleClass)
                {
                    currentGameState.SessionData.NumCarsInPlayerClassAtStartOfSession = currentGameState.SessionData.NumCarsOverallAtStartOfSession;
                }
            }
            if (currentGameState.SessionData.IsNewLap)
            {
                currentGameState.SessionData.ClassPositionAtStartOfCurrentLap = currentGameState.SessionData.ClassPosition;
            }

            float PitApproachPosition = currentGameState.SessionData.TrackDefinition != null ? currentGameState.SessionData.TrackDefinition.distanceForNearPitEntryChecks : -1;
            
            // sort out all the class position stuff
            int numCarsInPlayerClass = 1;
            foreach (OpponentData opponent in currentGameState.OpponentData.Values)
            {
                if (singleClass || CarData.IsCarClassEqual(opponent.CarClass, currentGameState.carClass))
                {
                    // don't care about other classes
                    numCarsInPlayerClass++;
                    if (PitApproachPosition != -1
                        && opponent.DistanceRoundTrack < PitApproachPosition + 20
                        && opponent.DistanceRoundTrack > PitApproachPosition - 20)
                    {
                        opponent.PositionOnApproachToPitEntry = opponent.ClassPosition;
                    }
                    if (opponent.ClassPosition == 1)
                    {
                        if (opponent.ClassPositionAtPreviousTick != 1)
                        {
                            currentGameState.SessionData.HasLeadChanged = true;
                        }
                        currentGameState.SessionData.LeaderSectorNumber = opponent.CurrentSectorNumber;
                        if (opponent.JustEnteredPits)
                        {
                            currentGameState.PitData.LeaderIsPitting = true;
                            currentGameState.PitData.OpponentForLeaderPitting = opponent;
                        }
                        else
                        {
                            currentGameState.PitData.LeaderIsPitting = false;
                            currentGameState.PitData.OpponentForLeaderPitting = null;
                        }
                    }
                    else if (opponent.ClassPosition == currentGameState.SessionData.ClassPosition - 1)
                    {
                        currentGameState.SessionData.TimeDeltaFront = opponent.DeltaTime.GetAbsoluteTimeDeltaAllowingForLapDifferences(currentGameState.SessionData.DeltaTime);
                        if (opponent.JustEnteredPits)
                        {
                            currentGameState.PitData.CarInFrontIsPitting = true;
                            currentGameState.PitData.OpponentForCarAheadPitting = opponent;
                        }
                        else
                        {
                            currentGameState.PitData.CarInFrontIsPitting = false;
                            currentGameState.PitData.OpponentForCarAheadPitting = null;
                        }
                    }
                    else if (opponent.ClassPosition == currentGameState.SessionData.ClassPosition + 1)
                    {
                        currentGameState.SessionData.TimeDeltaBehind = opponent.DeltaTime.GetAbsoluteTimeDeltaAllowingForLapDifferences(currentGameState.SessionData.DeltaTime);
                        if (opponent.JustEnteredPits)
                        {
                            currentGameState.PitData.CarBehindIsPitting = true;
                            currentGameState.PitData.OpponentForCarBehindPitting = opponent;
                        }
                        else
                        {
                            currentGameState.PitData.CarBehindIsPitting = false;
                            currentGameState.PitData.OpponentForCarBehindPitting = null;
                        }
                    }
                }
            }
            if (currentGameState.SessionData.JustGoneGreen || currentGameState.SessionData.IsNewSession)
            {
                currentGameState.SessionData.NumCarsInPlayerClassAtStartOfSession = numCarsInPlayerClass;
            }
            currentGameState.SessionData.NumCarsInPlayerClass = numCarsInPlayerClass;
            
            // now derive some stuff that we always need to derive, using the correct class positions:
            String previousBehindKey = currentGameState.getOpponentKeyBehind(currentGameState.carClass, true);
            String currentBehindKey = currentGameState.getOpponentKeyBehind(currentGameState.carClass);
            String previousAheadKey = currentGameState.getOpponentKeyInFront(currentGameState.carClass, true);
            String currentAheadKey = currentGameState.getOpponentKeyInFront(currentGameState.carClass);
            if (currentGameState.SessionData.ClassPosition > 2)
            {
                // if we're first or second we don't care about lead changes
                String previousLeaderKey = currentGameState.getOpponentKeyAtClassPosition(1, currentGameState.carClass, true);
                String currentLeaderKey = currentGameState.getOpponentKeyAtClassPosition(1, currentGameState.carClass);                
            }
            // don't really need this any more:
            /*
            if (CrewChief.Debugging 
                && ((currentBehindKey == null && currentGameState.SessionData.ClassPosition < currentGameState.SessionData.NumCarsInPlayerClass)
                      || (currentAheadKey == null && currentGameState.SessionData.ClassPosition > 1)))
            {
                Console.WriteLine("Non-contiguous class positions");
                List<OpponentData> opponentsInClass = new List<OpponentData>();
                foreach (OpponentData opponent in currentGameState.OpponentData.Values)
                {
                    if (CarData.IsCarClassEqual(opponent.CarClass, currentGameState.carClass))
                    {
                        opponentsInClass.Add(opponent);
                    }
                    Console.WriteLine(String.Join("\n", opponentsInClass.OrderBy(o => o.ClassPosition)));
                }
            }*/

            currentGameState.SessionData.IsRacingSameCarBehind = currentBehindKey == previousBehindKey;
            currentGameState.SessionData.IsRacingSameCarInFront = currentAheadKey == previousAheadKey;
            if (!currentGameState.SessionData.IsRacingSameCarInFront)
            {
                currentGameState.SessionData.GameTimeAtLastPositionFrontChange = currentGameState.SessionData.SessionRunningTime;
            }
            if (!currentGameState.SessionData.IsRacingSameCarBehind)
            {
                currentGameState.SessionData.GameTimeAtLastPositionBehindChange = currentGameState.SessionData.SessionRunningTime;
            }
        }
    }
}
