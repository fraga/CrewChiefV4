using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace CrewChiefV4.NumberProcessing
{
    public static class SpokenNumberParser
    {
        /// <summary>
        /// Parse a string for a number
        /// </summary>
        /// <param name="text">e.g. "box change tyre pressures twenty one point four five psi"</param>
        /// <returns>the number, or -1 if not able to parse one</returns>
        public static float Parse(string text)
        {
            float result;
            string stringToParse = "";
            bool lastWasDecade = false; // last word was ten twenty etc.
            string[] words = text.Split(' ');
            foreach (string word in words)
            {
                if (SpeechRecogniser.POINT[0] == word)
                {
                    if (lastWasDecade)
                    {
                        stringToParse += 0.ToString();
                        lastWasDecade = false;
                    }
                    stringToParse += ".";
                }

                int num = ExtractInt(word);
                if (num >= 0)
                {
                    if (num > 9)
                    { // e.g. "twenty one point two" or "point twenty five"
                        if (lastWasDecade)
                        {
                            stringToParse += 0.ToString();
                        }
                        num /= 10;
                        lastWasDecade = true;
                    }
                    else
                    {
                        lastWasDecade = false;
                    }
                    stringToParse += num.ToString();
                }
                else if (lastWasDecade)
                {
                    stringToParse += 0.ToString();
                    lastWasDecade = false;
                }

            }

            if (!float.TryParse(stringToParse, out result))
            {
                result = -1;
            }
            return result;
        }
#if StateMachineAlternative // thought it might catch some edge cases
                            // may be clearer
        enum states {
            TEXT,
            DIGIT,
            DECADE
        }
        public static float ParseSM(string text)
        {
            float result;
            string stringToParse = "";
            states state = states.TEXT;
            string[] words = text.Split(' ');
            foreach (string word in words)
            {
                int num = ExtractInt(word);
                switch (state)
                {
                    case states.TEXT:
                        if (SpeechRecogniser.POINT[0] == word)
                        {
                            stringToParse = "0.";
                        }
                        else if (num >= 0)
                        {
                            if (num > 9)
                            { // e.g. "twenty one point two" or "point twenty five"
                                num /= 10;
                                state = states.DECADE;
                            }
                            else
                            {
                                state = states.DIGIT;
                            }

                            stringToParse += num.ToString();
                        }

                        break;
                    case states.DIGIT:
                        if (SpeechRecogniser.POINT[0] == word)
                        {
                            stringToParse += ".";
                        }
                        else if (num >= 0)
                        {
                            if (num > 9)
                            { // e.g. "twenty one point two" or "point twenty five"
                                num /= 10;
                                state = states.DECADE;
                            }
                            else
                            {
                                state = states.DIGIT;
                            }

                            stringToParse += num.ToString();
                        }
                        else
                        {
                            state = states.TEXT;
                        }

                        break;
                    case states.DECADE:
                        if (SpeechRecogniser.POINT[0] == word)
                        {
                            stringToParse += "0.";
                            state = states.DIGIT;
                        }
                        else if (num >= 0)
                        {
                            if (num > 9)
                            { // e.g. "twenty twenty two"
                                stringToParse += "0";
                                num /= 10;
                                state = states.DECADE;
                            }
                            else
                            {
                                state = states.DIGIT;
                            }

                            stringToParse += num.ToString();
                        }
                        else
                        {
                            stringToParse += "0";
                            state = states.TEXT;
                        }

                        break;
                }
            }
            
            if (!float.TryParse(stringToParse, out result))
            {
                result = -1;
            }
            return result;
        }
#endif
        private static int ExtractInt(String word)
        {
            foreach (KeyValuePair<String[], int> entry in SpeechRecogniser.numbers0_99)
            {
                foreach (String numberStr in entry.Key)
                {
                    if (word == numberStr)
                    {
                        return entry.Value;
                    }
                }
            }
            return -1;
        }
    }
}
