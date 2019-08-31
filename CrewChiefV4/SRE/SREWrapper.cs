using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    interface SREWrapper
    {
        // need to cast these in impl
        void UnloadGrammar(GrammarWrapper grammar);
        void LoadGrammar(GrammarWrapper grammar);
        void AddSpeechRecognizedCallback(Object callback); // internalRealSRE += (CAST to EventHandler<SpeechRecognizedEventArgs>) new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
        void RecognizeAsync();
        void SetInputToAudioStream(RingBufferStream.RingBufferStream stream, Object format);     // cast format to [Microsoft].Speech.AudioFormat.SpeechAudioFormatInfo

        void RecognizeAsyncCancel();
        void SetInputToDefaultAudioDevice();
        void SetInputToNull();

        void SetInitialSilenceTimeout(TimeSpan timeSpan);   // internalRealSRE.InitialSilenceTimeout = timeSpan;

        void RecognizeAsyncStop();
        void UnloadAllGrammars();

        object GetInternalSRE();
    }
}
