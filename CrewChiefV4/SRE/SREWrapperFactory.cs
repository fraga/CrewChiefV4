using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewChiefV4.SRE
{
    class SREWrapperFactory
    {
        // TODO: make this an option:
        public static Boolean useSystem = true;

        public static SREWrapper createNewSREWrapper()
        {
            if (useSystem)
            {
                return new SystemSREWrapper();
            }
            else
            {
                return new MicrosoftSREWrapper();
            }
        }

        public static GrammarWrapper createNewGrammarWrapper(GrammarBuilderWrapper grammarBuilderWrapper)
        {
            if (useSystem)
            {
                return new SystemGrammarWrapper(grammarBuilderWrapper);
            }
            else
            {
                return new MicrosoftGrammarWrapper(grammarBuilderWrapper);
            }
        }

        public static GrammarBuilderWrapper createNewGrammarBuilderWrapper()
        {
            if (useSystem)
            {
                return new SystemGrammarBuilderWrapper();
            }
            else
            {
                return new MicrosoftGrammarBuilderWrapper();
            }
        }

        public static GrammarBuilderWrapper createNewGrammarBuilderWrapper(ChoicesWrapper choicesWrapper)
        {
            if (useSystem)
            {
                return new SystemGrammarBuilderWrapper(choicesWrapper);
            }
            else
            {
                return new MicrosoftGrammarBuilderWrapper(choicesWrapper);
            }
        }

        public static ChoicesWrapper createNewChoicesWrapper()
        {
            if (useSystem)
            {
                return new SystemChoicesWrapper();
            }
            else
            {
                return new MicrosoftChoicesWrapper();
            }
        }

        public static ChoicesWrapper createNewChoicesWrapper(string[] choices)
        {
            if (useSystem)
            {
                return new SystemChoicesWrapper(choices);
            }
            else
            {
                return new MicrosoftChoicesWrapper(choices);
            }
        }

        public static CultureInfo GetCultureInfo(String langToUse)
        {
            if (useSystem)
            {
                foreach (System.Speech.Recognition.RecognizerInfo ri in System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers())
                {
                    if (ri.Culture.TwoLetterISOLanguageName.Equals(langToUse))
                    {
                        return ri.Culture;
                    }
                }
                return null;
            }
            else
            {
                foreach (Microsoft.Speech.Recognition.RecognizerInfo ri in Microsoft.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers())
                {
                    if (ri.Culture.TwoLetterISOLanguageName.Equals(langToUse))
                    {
                        return ri.Culture;
                    }
                }
                return null;
            }
        }

        public static float GetCallbackConfidence(object recognitionCallback)
        {
            if (useSystem)
            {
                return ((System.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Confidence;
            }
            else
            {
                return ((Microsoft.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Confidence;
            }
        }

        public static string GetCallbackText(object recognitionCallback)
        {
            if (useSystem)
            {
                return ((System.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Text;
            }
            else
            {
                return ((Microsoft.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Text;
            }
        }

        public static object GetCallbackGrammar(object recognitionCallback)
        {
            if (useSystem)
            {
                return ((System.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Grammar;
            }
            else
            {
                return ((Microsoft.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Grammar;
            }
        }
    }
}
