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
            //string trackName = wrapper.dirtData.stageLength > 0 ? "stage_" + wrapper.dirtData.stageLength : "";
            string trackName = "";
            if (wrapper.dirtData.trackNumber > 0)
            {
                trackName = "stage_id_" + wrapper.dirtData.trackNumber;
            }
            else if (wrapper.dirtData.stageLength > 0)
            {
                trackName = "stage_length_" + wrapper.dirtData.stageLength;
            }
            gsd.SessionData.TrackDefinition = new TrackDefinition(trackName, -1, wrapper.dirtData.stageLength, null, null);
            gsd.SessionData.SessionRunningTime = wrapper.dirtData.currentStageTime;
            gsd.SessionData.SessionType = SessionType.Race;
            gsd.SessionData.SessionPhase = gsd.SessionData.SessionRunningTime <= 0 ? SessionPhase.Countdown : SessionPhase.Green;
            gsd.PositionAndMotionData.DistanceRoundTrack = wrapper.dirtData.lapDistance;
            gsd.PositionAndMotionData.CarSpeed = wrapper.dirtData.speed;
            if (previousGameState != null)
            {
                gsd.CoDriverPacenotes = previousGameState.CoDriverPacenotes;
            }
            return gsd;
        }

        private PitWindow mapToPitWindow(GameStateData currentGameState, uint pitSchedule, uint pitMode)
        {
            return PitWindow.Unavailable;
        }
    }
}
