using System;
using CrewChiefV4.GameState;

/**
 * Maps memory mapped file to a local game-agnostic representation.
 * */
namespace CrewChiefV4.Dirt
{
    class DirtGameStateMapper : GameStateMapper
    {
        public override void versionCheck(Object memoryMappedFileStruct)
        {
            // no version data in the stream so this is a no-op
        }

        public override GameStateData mapToGameStateData(Object structWrapper, GameStateData previousGameState)
        {
            DirtStructWrapper wrapper = (DirtStructWrapper)structWrapper;
            long ticks = wrapper.ticksWhenRead;
            GameStateData gsd = new GameStateData(ticks);
            bool createNewTrackDefinition = false;
            if (previousGameState != null)
            {
                // if we appear to be starting a new session, we'll want to generate our track name again:
                if ((wrapper.dirtData.stageLength > 0 && previousGameState.SessionData.TrackDefinition != null && previousGameState.SessionData.TrackDefinition.trackLength != wrapper.dirtData.stageLength)
                    || (previousGameState.PositionAndMotionData.DistanceRoundTrack == 0 && wrapper.dirtData.lapDistance != 0)
                    || (previousGameState.PositionAndMotionData.DistanceRoundTrack <= 0 && wrapper.dirtData.lapDistance > 0))
                {
                    createNewTrackDefinition = true;
                }
                else
                {
                    // otherwise keep the last generated track name and notes
                    gsd.CoDriverPacenotes = previousGameState.CoDriverPacenotes;
                    gsd.SessionData.TrackDefinition = previousGameState.SessionData.TrackDefinition;
                }
            }
            if (createNewTrackDefinition || gsd.SessionData.TrackDefinition == null)
            {
                // track name includes the start point x and z co-ordinates (rounded to the nearest hundred metres in case
                // the start position varies a little). This allows tracks with the same distance but different start points (e.g.
                // reversed stages) to be differentiated
                int worldXAtFirstDistanceUpdate = roundToNearest100(wrapper.dirtData.worldX);
                int worldZAtFirstDistanceUpdate = roundToNearest100(wrapper.dirtData.worldZ);
                float stageLengthAtFirstDistanceUpdate = wrapper.dirtData.stageLength;
                string trackName = "stage_length_" + stageLengthAtFirstDistanceUpdate + "^x" + worldXAtFirstDistanceUpdate + "z" + worldZAtFirstDistanceUpdate;
                gsd.SessionData.TrackDefinition = new TrackDefinition(trackName, -1, wrapper.dirtData.stageLength, null, null);
            }
            gsd.SessionData.SessionRunningTime = wrapper.dirtData.currentStageTime;
            gsd.SessionData.SessionType = SessionType.Race;
            gsd.SessionData.SessionPhase = gsd.SessionData.SessionRunningTime <= 0 ? SessionPhase.Countdown : SessionPhase.Green;
            gsd.PositionAndMotionData.DistanceRoundTrack = wrapper.dirtData.lapDistance;
            gsd.PositionAndMotionData.CarSpeed = wrapper.dirtData.speed;

            return gsd;
        }

        private PitWindow mapToPitWindow(GameStateData currentGameState, uint pitSchedule, uint pitMode)
        {
            return PitWindow.Unavailable;
        }

        private int roundToNearest100(float f)
        {
            return 100 * ((int) Math.Round(f / 100f));
        }
    }
}
