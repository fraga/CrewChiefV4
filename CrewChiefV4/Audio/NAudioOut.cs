using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace CrewChiefV4.Audio
{
    internal abstract class NAudioOut
    {
        internal static NAudioOut CreateOutput()
        {
            if (AudioPlayer.nAudioOutputInterface == AudioPlayer.NAUDIO_OUTPUT_INTERFACE.WAVEOUT)
                return new NAudioOutWaveOut();
            else
                return new NAudioOutWasapi();
        }

        internal abstract NAudio.Wave.PlaybackState PlaybackState { get; }

        internal abstract void SubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler);
        internal abstract void UnsubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler);
        internal abstract void Stop();
        internal abstract void Dispose();
        internal abstract void Play();
        internal abstract void Init(IWaveProvider waveProvider);
        internal abstract void Init(ISampleProvider sampleProvider, bool convertTo16Bit = false);
    }

    internal class NAudioOutWaveOut : NAudioOut
    {
        private NAudio.Wave.WaveOutEvent waveOut = null;

        public NAudioOutWaveOut()
        {
            this.waveOut = new NAudio.Wave.WaveOutEvent();
            this.waveOut.DeviceNumber = AudioPlayer.naudioMessagesPlaybackDeviceId;
        }

        internal override PlaybackState PlaybackState => this.waveOut.PlaybackState;

        internal override void Dispose() => this.waveOut.Dispose();
        internal override void Init(IWaveProvider waveProvider) => waveOut.Init(waveProvider);
        internal override void Init(ISampleProvider sampleProvider, bool convertTo16Bit = false) => this.waveOut.Init(sampleProvider, convertTo16Bit);
        internal override void Play() => this.waveOut.Play();
        internal override void Stop() => this.waveOut.Stop();
        internal override void SubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler) => this.waveOut.PlaybackStopped += eventHandler;
        internal override void UnsubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler) => this.waveOut.PlaybackStopped -= eventHandler;
    }

    internal class NAudioOutWasapi : NAudioOut
    {
        private readonly int wasapiLatency = UserSettings.GetUserSettings().getInt("naudio_wasapi_latency");

        private NAudio.Wave.WasapiOut wasapiOut = null;

        public NAudioOutWasapi()
        {
            this.wasapiOut = new WasapiOut(new MMDeviceEnumerator().GetDevice(AudioPlayer.naudioMessagesPlaybackDeviceGuid), AudioClientShareMode.Shared, true, this.wasapiLatency);
        }

        internal override PlaybackState PlaybackState => this.wasapiOut.PlaybackState;

        internal override void Dispose() => this.wasapiOut.Dispose();
        internal override void Init(IWaveProvider waveProvider) => wasapiOut.Init(waveProvider);
        internal override void Init(ISampleProvider sampleProvider, bool convertTo16Bit = false) => this.wasapiOut.Init(sampleProvider, convertTo16Bit);
        internal override void Play() => this.wasapiOut.Play();
        internal override void Stop() => this.wasapiOut.Stop();
        internal override void SubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler) => this.wasapiOut.PlaybackStopped += eventHandler;
        internal override void UnsubscribePlaybackStopped(EventHandler<StoppedEventArgs> eventHandler) => this.wasapiOut.PlaybackStopped -= eventHandler;
    }
}
