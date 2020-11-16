using System;
using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace CrewChiefV4.SRE
{
    class SystemSREWrapper : SREWrapper
    {
        private SpeechRecognitionEngine internalSRE;

        public SystemSREWrapper(CultureInfo culture, TimeSpan? endSilenceTimeoutAmbiguous)
        {
            // if the culture is null we won't be able to use the SRE - this is checked later in the initialisation code.
            // We still want to check if the engine is available so we know which warning to display to the user
            if (culture == null)
            {
                this.internalSRE = new SpeechRecognitionEngine();
            }
            else
            {
                this.internalSRE = new SpeechRecognitionEngine(culture);
            }
            if (endSilenceTimeoutAmbiguous != null)
            {
                this.internalSRE.EndSilenceTimeoutAmbiguous = endSilenceTimeoutAmbiguous.Value;
            }
        }

        public void AddSpeechRecognizedCallback(object callback)
        {
            internalSRE.SpeechRecognized += (EventHandler<SpeechRecognizedEventArgs>)callback;
        }

        public void LoadGrammar(GrammarWrapper grammarWrapper)
        {
            internalSRE.LoadGrammar((Grammar)grammarWrapper.GetInternalGrammar());
        }

        public void RecognizeAsync()
        {
            internalSRE.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void RecognizeAsyncCancel()
        {
            internalSRE.RecognizeAsyncCancel();
        }

        public void RecognizeAsyncStop()
        {
            internalSRE.RecognizeAsyncStop();
        }

        public void SetInitialSilenceTimeout(TimeSpan timeSpan)
        {
            internalSRE.InitialSilenceTimeout = timeSpan;
        }

        public void SetInputToAudioStream(RingBufferStream.RingBufferStream stream, int rate, int depth, int channelCount)
        {
            SpeechAudioFormatInfo safi = new SpeechAudioFormatInfo(rate,
                            depth == 16 ? AudioBitsPerSample.Sixteen : AudioBitsPerSample.Eight,
                            channelCount == 2 ? AudioChannel.Stereo : AudioChannel.Mono);
            internalSRE.SetInputToAudioStream(stream, safi);
        }

        public void SetInputToDefaultAudioDevice()
        {
            internalSRE.SetInputToDefaultAudioDevice();
        }

        public void SetInputToNull()
        {
            internalSRE.SetInputToNull();
        }

        public void UnloadAllGrammars()
        {
            internalSRE.UnloadAllGrammars();
        }

        public void UnloadGrammar(GrammarWrapper grammarWrapper)
        {
            internalSRE.UnloadGrammar((Grammar)grammarWrapper.GetInternalGrammar());
        }

        public object GetInternalSRE()
        {
            return internalSRE;
        }
    }
}
