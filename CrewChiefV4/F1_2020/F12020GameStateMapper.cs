﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrewChiefV4.GameState;
using CrewChiefV4.Events;
using System.Diagnostics;

/**
 * Maps memory mapped file to a local game-agnostic representation.
 * */
namespace CrewChiefV4.F1_2020
{
    class F12020GameStateMapper : GameStateMapper
    {
        public override void versionCheck(Object memoryMappedFileStruct)
        {
            // no version data in the stream so this is a no-op
        }

        public override GameStateData mapToGameStateData(Object structWrapper, GameStateData previousGameState)
        {
            F12020StructWrapper wrapper = (F12020StructWrapper)structWrapper;
            long ticks = wrapper.ticksWhenRead;

            return new GameStateData(ticks);
        }

        private PitWindow mapToPitWindow(GameStateData currentGameState, uint pitSchedule, uint pitMode)
        {
            return PitWindow.Unavailable;
        }
    }
}
