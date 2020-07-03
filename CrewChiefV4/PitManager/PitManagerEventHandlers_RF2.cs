﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CrewChiefV4.Audio;
using CrewChiefV4.Events;
using CrewChiefV4.GameState;
using rF2SharedMemoryAPI;

namespace CrewChiefV4.PitManager
{
    class PitManagerEventHandlers_RF2 : AbstractEvent
    {
        static private PitMenuController Pmc = new PitMenuController();
        static private bool xxx = Pmc.Connect();    // tbd
        public override void clearState()
        {
        }
        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
            if (CrewChief.gameDefinition.gameEnum != GameEnum.RF2_64BIT)
            {
                return;
            }
        }
        public PitManagerEventHandlers_RF2(AudioPlayer audioPlayer)
        {
        }

            /// <summary>
            /// The event handlers for rF2
            /// </summary>
            static public bool actionHandler_example()
        {
            return true;

        }
        static public bool actionHandler_example2()
        {
            return true;
        }
        static public bool actionHandler_changeAllTyres()
        {
            bool response = false;
            foreach (string tyre in new[] { "RR TIRE:", "RL TIRE:", "FR TIRE:", "FL TIRE:" })
            {
                if (Pmc.SetCategory(tyre))
                {
                    response = Pmc.SetTyreType("Medium");
                }
            }

            return response;
        }
        /// <summary>
        /// The response handlers for rF2
        /// </summary>
        static public bool responseHandler_example()
        {
            CrewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderAcknowlegeOK, 0));
            return true;
        }
        static public bool responseHandler_fuelToEnd()
        {
            CrewChief.audioPlayer.playMessageImmediately(new QueuedMessage(AudioPlayer.folderFuelToEnd, 0));
            return true;
        }
    }
}
