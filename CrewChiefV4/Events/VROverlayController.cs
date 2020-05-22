﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CrewChiefV4.Audio;
using CrewChiefV4.GameState;
using CrewChiefV4.Overlay;

namespace CrewChiefV4.Events
{
    // WIP control class for interpreting SRE command and whatever to manage the overlay
    // Just a collection of ideas at the moment

    class VROverlayController : AbstractEvent
    {
        public static object suspendStateLock = new object();
        public static bool vrOverlayRenderThreadSuspended = false;
        public static bool vrUpdateThreadRunning = false;

        public VROverlayController(AudioPlayer audioPlayer)
        {}

        public override void clearState()
        {}

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
        }

        public override void respond(String voiceMessage)
        {
            if (!VROverlayController.vrUpdateThreadRunning)
                return;

            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.TOGGLE_VR_OVERLAYS))
            {
                if (VROverlayController.vrOverlayRenderThreadSuspended)
                    VROverlayController.resumeVROverlayRenderThread();
                else
                    VROverlayController.suspendVROverlayRenderThread();
            }
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.SHOW_VR_SETTING))
            {
                lock (MainWindow.instanceLock)
                {
                    if (MainWindow.instance != null)
                    {
                        MainWindow.instance.Invoke((MethodInvoker)delegate
                        {
                            MainWindow.instance.vrOverlayForm.ShowDialog();                        
                        });
                    }
                }
            }
            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HIDE_VR_SETTING))
            {
                lock (MainWindow.instanceLock)
                {
                    if (MainWindow.instance != null)
                    {
                        MainWindow.instance.Invoke((MethodInvoker)delegate
                        {
                            MainWindow.instance.vrOverlayForm.Close();                        
                        });
                    }
                }
            }
        }

        public static void suspendVROverlayRenderThread()
        {
            lock (VROverlayController.suspendStateLock)
            {
                VROverlayController.vrOverlayRenderThreadSuspended = true;
            }
        }

        public static void resumeVROverlayRenderThread()
        {
            lock (VROverlayController.suspendStateLock)
            {
                VROverlayController.vrOverlayRenderThreadSuspended = false;
            }
        }
    }
}
