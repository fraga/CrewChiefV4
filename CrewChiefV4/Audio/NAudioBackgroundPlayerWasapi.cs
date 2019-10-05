using NAudio.CoreAudioApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrewChiefV4.Audio
{
    class NAudioBackgroundPlayerWasapi : BackgroundPlayer
    {
        private SynchronizationContext mainThreadContext = null;

        private Boolean playing = false;

        // will be re-used and only disposed when we stop the app or switch background sounds
        private NAudio.Wave.WaveFileReader reader = null;
        private NAudio.Wave.WasapiOut waveOut = null;
        AutoResetEvent playWaitHandle = new AutoResetEvent(false);
        private int deviceIdWhenCached = 0;
        private float volumeWhenCached = 0;

        private TimeSpan backgroundLength = TimeSpan.Zero;
        EventHandler<NAudio.Wave.StoppedEventArgs> eventHandler;

        public NAudioBackgroundPlayerWasapi(SynchronizationContext mainThreadContext, String backgroundFilesPath, String defaultBackgroundSound)
        {
            this.mainThreadContext = mainThreadContext;
            this.backgroundFilesPath = backgroundFilesPath;
            this.defaultBackgroundSound = defaultBackgroundSound;
        }

        public override void mute(bool doMute)
        {
            this.muted = doMute;
            if (playing && doMute)
            {
                stop();
            }
        }

        public override void play()
        {
            lock (this)
            {
                float volume = getBackgroundVolume();
                if (playing || muted || volume <= 0)
                {
                    return;
                }
                if (!initialised || volume != this.volumeWhenCached || this.deviceIdWhenCached != AudioPlayer.naudioBackgroundPlaybackDeviceId)
                {
                    initialised = false;
                    initialise(this.defaultBackgroundSound);
                }               
                //waveOut.PlaybackStopped += this.eventHandler;
                playing = true;
                int backgroundOffset = Utilities.random.Next(0, (int)backgroundLength.TotalSeconds - backgroundLeadout);
                this.reader.CurrentTime = TimeSpan.FromSeconds(backgroundOffset);
                this.waveOut.Play();
                //this.playWaitHandle.WaitOne(30);
                //waveOut.PlaybackStopped -= this.playbackStopped;
            }
        }

        public override void stop()
        {
            lock (this)
            {
                if (initialised && this.waveOut != null)
                {                    
                    this.waveOut.Pause();
                    //this.playWaitHandle.Set();
                }
                playing = false;
            }
        }

        private void initReader(String backgroundSoundName)
        {
            if (this.reader != null)
            {
                this.reader.Dispose();
            }
            this.reader = new NAudio.Wave.WaveFileReader(Path.Combine(backgroundFilesPath, backgroundSoundName));
            backgroundLength = reader.TotalTime;
        }

        private void initWaveOut()
        {
            if (this.waveOut != null)
            {
                this.waveOut.Dispose();
            }
            //this.eventHandler = new EventHandler<NAudio.Wave.StoppedEventArgs>(playbackStopped);
            this.volumeWhenCached = getBackgroundVolume();
            this.deviceIdWhenCached = AudioPlayer.naudioBackgroundPlaybackDeviceId;
            this.waveOut = new NAudio.Wave.WasapiOut(new MMDeviceEnumerator().GetDevice(AudioPlayer.naudioBackgroundPlaybackDeviceGuid), AudioClientShareMode.Shared, false, 10);
            //this.waveOut.DeviceNumber = this.deviceIdWhenCached;
            NAudio.Wave.SampleProviders.SampleChannel sampleChannel = new NAudio.Wave.SampleProviders.SampleChannel(reader);                
            sampleChannel.Volume = this.volumeWhenCached;
            this.waveOut.Init(new NAudio.Wave.SampleProviders.SampleToWaveProvider(sampleChannel));
        }

        public override void initialise(String initialBackgroundSound)
        {
            if (!this.initialised)
            {
                lock (this)
                {
                    initReader(initialBackgroundSound);
                    initWaveOut();
                    initialised = true;
                }
            }
        }

        public override void setBackgroundSound(String backgroundSoundName)
        {
            lock (this)
            {
                if (this.waveOut != null)
                {
                    this.waveOut.Stop();
                }
                initReader(backgroundSoundName);
                initWaveOut();
                initialised = true;
            }
        }

        private void playbackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            this.playWaitHandle.Set();
        }

        public override void dispose()
        {
            lock (this)
            {
                try
                {
                    if (reader != null)
                    {
                        reader.Dispose();
                    }
                    if (waveOut != null)
                    {
                        waveOut.Stop();

                        lock (MainWindow.instanceLock)
                        {
                            if (MainWindow.instance != null)
                            {
                                this.mainThreadContext.Post(delegate
                                {
                                    waveOut.Dispose();
                                }, null);
                           }
                        }
                    }
                }
                catch (Exception) { }
                base.dispose();
            }
        }
    }
}
