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
        public static Boolean useSystem = UserSettings.GetUserSettings().getBoolean("prefer_system_sre");

        // try to create the preferred SRE impl, fall back to the other type if this isn't available
        public static SREWrapper createNewSREWrapper(Boolean log = false)
        {
            SREWrapper sreWrapper = null;
            if (useSystem)
            {
                sreWrapper = createSystemSREWrapper();
                if (sreWrapper == null)
                {
                    if (log) Console.WriteLine("Unable to create a System SRE, trying with Microsoft SRE");
                    sreWrapper = createMicrosoftSREWrapper();
                    if (sreWrapper != null)
                    {
                        useSystem = false;
                        if (log) Console.WriteLine("Falling back to Microsoft SRE");
                    }
                }
                else
                {
                    if (log) Console.WriteLine("Successfully initialised preferred System SRE");
                }
            }
            else
            {
                sreWrapper = createMicrosoftSREWrapper();
                if (sreWrapper == null)
                {
                    if (log) Console.WriteLine("Unable to create a Microsoft SRE, trying with System SRE");
                    sreWrapper = createSystemSREWrapper();
                    if (sreWrapper != null)
                    {
                        useSystem = true;
                        if (log) Console.WriteLine("Falling back to System SRE");
                    }
                }
                else
                {
                    if (log) Console.WriteLine("Successfully initialised preferred Microsoft SRE");
                }
            }
            return sreWrapper;
        }

        public static GrammarWrapper CreateChatDictationGrammarWrapper()
        {
            System.Speech.Recognition.DictationGrammar dictationGrammar = new System.Speech.Recognition.DictationGrammar();
            dictationGrammar.Name = "default dictation";
            dictationGrammar.Enabled = true;
            return new SystemGrammarWrapper(dictationGrammar);
        }

        public static void LoadChatDictationGrammar(SREWrapper sreWrapper, GrammarWrapper dictationGrammarWrapper, String dictationContextStart, String dictationContextEnd)
        {
            System.Speech.Recognition.DictationGrammar dictationGrammar = (System.Speech.Recognition.DictationGrammar)dictationGrammarWrapper.GetInternalGrammar();
            dictationGrammar.Weight = 0.2f;
            ((System.Speech.Recognition.SpeechRecognitionEngine)sreWrapper.GetInternalSRE()).LoadGrammar(dictationGrammar);
            dictationGrammar.SetDictationContext(dictationContextStart, dictationContextEnd);
        }

        private static SREWrapper createMicrosoftSREWrapper()
        {
            try
            {
                return new MicrosoftSREWrapper();
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static SREWrapper createSystemSREWrapper()
        {
            try
            {
                return new SystemSREWrapper();
            }
            catch (Exception)
            {
                return null;
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

        public static string[] GetCallbackWordsList(object recognitionCallback)
        {
            if (useSystem)
            {
                return ((System.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Words.Select(x => x.Text).ToArray();
            }
            else
            {
                return ((Microsoft.Speech.Recognition.SpeechRecognizedEventArgs)recognitionCallback).Result.Words.Select(x => x.Text).ToArray();
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
