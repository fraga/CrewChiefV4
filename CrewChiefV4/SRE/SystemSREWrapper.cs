using System;
using System.Collections.Generic;
using System.Globalization;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace CrewChiefV4.SRE
{
    class SystemSREWrapper : SREWrapper
    {
        private SpeechRecognitionEngine internalSRE;

        private int maxAudioLevel = 0;

        private List<string> lastProblems = new List<string>();

        private bool debugRecognitionAttempt = false;

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
            this.internalSRE.AudioLevelUpdated += new EventHandler<AudioLevelUpdatedEventArgs>(recognizer_AudioLevelUpdated); ;
            this.internalSRE.AudioSignalProblemOccurred += new EventHandler<AudioSignalProblemOccurredEventArgs>(recognizer_AudioSignalProblemOccurred);
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
            this.maxAudioLevel = 0;
            this.lastProblems.Clear();
            this.debugRecognitionAttempt = false;
            internalSRE.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void RecognizeAsyncCancel()
        {
            internalSRE.RecognizeAsyncCancel();
            if (this.debugRecognitionAttempt)
            {
                Console.WriteLine("Max audio level for recogniser operation = " + this.maxAudioLevel);
                Console.WriteLine(string.Join(", ", this.lastProblems));
            }
        }

        public void RecognizeAsyncStop()
        {
            internalSRE.RecognizeAsyncStop();
            if (this.debugRecognitionAttempt)
            {
                Console.WriteLine("Max audio level for recogniser operation = " + this.maxAudioLevel);
                Console.WriteLine(string.Join(", ", this.lastProblems));
            }
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

        void recognizer_AudioLevelUpdated(object sender, AudioLevelUpdatedEventArgs e)
        {
            if (this.maxAudioLevel < e.AudioLevel)
            {
                this.maxAudioLevel = e.AudioLevel;
            }
        }

        // Gather information when the AudioSignalProblemOccurred event is raised.  
        private void recognizer_AudioSignalProblemOccurred(object sender, AudioSignalProblemOccurredEventArgs e)
        {
            this.lastProblems.Add(string.Format("Audio signal problem information: {0}", e.AudioSignalProblem));
            this.debugRecognitionAttempt = true;
        }
    }
}
