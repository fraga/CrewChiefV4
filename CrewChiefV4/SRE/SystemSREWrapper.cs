using System;
using System.Speech.AudioFormat;
using System.Speech.Recognition;

namespace CrewChiefV4.SRE
{
    class SystemSREWrapper : SREWrapper
    {
        private SpeechRecognitionEngine internalSRE;

        public SystemSREWrapper()
        {
            this.internalSRE = new SpeechRecognitionEngine();
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

        public void SetInputToAudioStream(RingBufferStream.RingBufferStream stream, object format)
        {
            internalSRE.SetInputToAudioStream(stream, (SpeechAudioFormatInfo)format);
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
    }
}
