using System;
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
    // Control class for interpreting SRE commands and whatever to manage the overlay

    class VROverlayController : AbstractEvent
    {
        public static object suspendStateLock = new object();
        public static bool vrOverlayRenderThreadSuspended = false;
        public static bool vrUpdateThreadRunning = false;

        public VROverlayController(AudioPlayer audioPlayer)
        { }

        public override void clearState()
        { }

        override protected void triggerInternal(GameStateData previousGameState, GameStateData currentGameState)
        {
        }

        public override void respond(String voiceMessage)
        {
            if (!vrUpdateThreadRunning)
                return;

            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.TOGGLE_VR_OVERLAYS))
            {
                if (vrOverlayRenderThreadSuspended)
                    resumeVROverlayRenderThread();
                else
                    suspendVROverlayRenderThread();
            }

            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.SHOW_VR_SETTING))
            {
                lock (MainWindow.instanceLock)
                {
                    if (MainWindow.instance?.vrOverlayForm != null)
                    {
                        MainWindow.instance.Invoke(() => MainWindow.instance.vrOverlayForm.ShowDialog(MainWindow.instance));
                    }
                    else
                    {
                        Log.Error("vrOverlayForm not available");
                    }
                }
            }

            if (SpeechRecogniser.ResultContains(voiceMessage, SpeechRecogniser.HIDE_VR_SETTING))
            {
                lock (MainWindow.instanceLock)
                {
                    if (MainWindow.instance?.vrOverlayForm != null)
                    {
                        MainWindow.instance.Invoke(() => MainWindow.instance.vrOverlayForm.Close());
                    }
                }
            }
        }

        public static void suspendVROverlayRenderThread()
        {
            lock (suspendStateLock)
            {
                vrOverlayRenderThreadSuspended = true;
            }
        }

        public static void resumeVROverlayRenderThread()
        {
            lock (suspendStateLock)
            {
                vrOverlayRenderThreadSuspended = false;
            }
        }
    }
}
