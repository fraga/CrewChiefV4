using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // if endSilenceTimeoutAmbiguous is not null we override the default endSilenceTimeoutAmbiguous in the SRE impl
        public static SREWrapper createNewSREWrapper(CultureInfo culture, TimeSpan? endSilenceTimeoutAmbiguous, Boolean log = false)
        {
            SREWrapper sreWrapper = null;
            if (useSystem)
            {
                sreWrapper = createSystemSREWrapper(culture, endSilenceTimeoutAmbiguous);
                if (sreWrapper == null)
                {
                    if (log) Console.WriteLine("Unable to create a System SRE, trying with Microsoft SRE");
                    sreWrapper = createMicrosoftSREWrapper(culture, endSilenceTimeoutAmbiguous);
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
                sreWrapper = createMicrosoftSREWrapper(culture, endSilenceTimeoutAmbiguous);
                if (sreWrapper == null)
                {
                    if (log) Console.WriteLine("Unable to create a Microsoft SRE, trying with System SRE");
                    sreWrapper = createSystemSREWrapper(culture, endSilenceTimeoutAmbiguous);
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

        private static SREWrapper createMicrosoftSREWrapper(CultureInfo culture, TimeSpan? endSilenceTimeoutAmbiguous)
        {
            try
            {
                return new MicrosoftSREWrapper(culture, endSilenceTimeoutAmbiguous);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static SREWrapper createSystemSREWrapper(CultureInfo culture, TimeSpan? endSilenceTimeoutAmbiguous)
        {
            try
            {
                return new SystemSREWrapper(culture, endSilenceTimeoutAmbiguous);
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

        public static CultureInfo GetCultureInfo(String langAndCountryToUse, String langToUse, Boolean log = false)
        {
            try
            {
                if (useSystem)
                {
                    // first check we can get the system installed recognisers
                    ReadOnlyCollection<System.Speech.Recognition.RecognizerInfo> systemRecognisers = null;
                    try
                    {
                        systemRecognisers = System.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unable to get System (Windows) speech recogniser");
                        return null;
                    }
                    if (langAndCountryToUse != null && langAndCountryToUse.Length == 5)
                    {
                        if (log) Console.WriteLine("Attempting to get recogniser for " + langAndCountryToUse);
                        foreach (System.Speech.Recognition.RecognizerInfo ri in systemRecognisers)
                        {
                            if (ri.Culture.Name.Equals(langAndCountryToUse))
                            {
                                return ri.Culture;
                            }
                        }
                    }
                    if (log && langAndCountryToUse != null && langAndCountryToUse.Length == 5)
                    {
                        Console.WriteLine("Failed to get recogniser for " + langAndCountryToUse);
                    }
                    if (log) Console.WriteLine("Attempting to get recogniser for " + langToUse);
                    foreach (System.Speech.Recognition.RecognizerInfo ri in systemRecognisers)
                    {
                        if (ri.Culture.TwoLetterISOLanguageName.Equals(langToUse))
                        {
                            return ri.Culture;
                        }
                    }
                }
                else
                {
                    // first check we can get the microsoft installed recognisers
                    ReadOnlyCollection<Microsoft.Speech.Recognition.RecognizerInfo> microsoftRecognisers = null;
                    try
                    {
                        microsoftRecognisers = Microsoft.Speech.Recognition.SpeechRecognitionEngine.InstalledRecognizers();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Unable to get Microsoft speech recogniser. Is SpeechPlatformRuntime.msi installed?");
                        return null;
                    }
                    if (langAndCountryToUse != null && langAndCountryToUse.Length == 5)
                    {
                        if (log) Console.WriteLine("Attempting to get recogniser for " + langAndCountryToUse + " package name MSSpeech_SR_" + langAndCountryToUse + "_TELE.msi");
                        foreach (Microsoft.Speech.Recognition.RecognizerInfo ri in microsoftRecognisers)
                        {
                            if (ri.Culture.Name.Equals(langAndCountryToUse))
                            {
                                return ri.Culture;
                            }
                        }
                    }
                    if (log && langAndCountryToUse != null && langAndCountryToUse.Length == 5)
                    {
                        Console.WriteLine("Failed to get recogniser for " + langAndCountryToUse);
                    }
                    if (log) Console.WriteLine("Attempting to get recogniser for " + langToUse + " package name MSSpeech_SR_" + langToUse + "-XX_TELE.msi");
                    foreach (Microsoft.Speech.Recognition.RecognizerInfo ri in microsoftRecognisers)
                    {
                        if (ri.Culture.TwoLetterISOLanguageName.Equals(langToUse))
                        {
                            return ri.Culture;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // the engine may return a null InstalledRecognizers List
                Console.WriteLine("Unable to get a SRE CultureInfo object: " + e.Message);
            }
            return null;
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
