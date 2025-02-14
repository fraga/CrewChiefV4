﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CrewChiefV4.Events;
using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using iRSDKSharp;

namespace CrewChiefV4.iRacing
{
    class iRacingSpotter : Spotter
    {
        private DateTime previousTime = DateTime.UtcNow;

        public iRacingSpotter(AudioPlayer audioPlayer, Boolean initialEnabledState)
        {
            this.audioPlayer = audioPlayer;
            this.enabled = initialEnabledState;
            this.initialEnabledState = initialEnabledState;
            this.internalSpotter = new NoisyCartesianCoordinateSpotter(audioPlayer, initialEnabledState, 0, 0);
        }

        public override void clearState()
        {
            previousTime = DateTime.UtcNow;
            internalSpotter.clearState();
        }

        public override void trigger(Object lastStateObj, Object currentStateObj, GameStateData currentGameState)
        {
            if(enabled && !paused)
            {
                CarLeftRight currentState = (CarLeftRight)currentStateObj;
                internalSpotter.triggerInternal(currentState);
            }
            return;
        }
    }
}