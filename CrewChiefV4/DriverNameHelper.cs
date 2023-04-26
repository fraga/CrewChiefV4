using FuzzySharp;
using Phonix;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using CrewChiefV4.Audio;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("UnitTest")]
/**
 * Utility class to ease some of the pain of managing driver names.
 */
namespace CrewChiefV4
{
    /// <summary>
    /// Mapping the game's driver names to sound files.
    /// </summary>
    class DriverNameHelper
    {
        public static HashSet<String> unvocalizedNames = new HashSet<string>();

        private static readonly String[] middleBits = { "de la", "de le", "van der", "van de", "van", "de", "da", "le", "la", "von", "di",
            "eg", "du", "el", "del", "saint", "st", "mac", "mc" }; // mac and mc may have been split off during CamelCase processing

        // need a special case here for "junior" and "senior"
        // - if it's "Something Junior" then allow it, it it's "Something Somthingelse Junior" then remove it
        //
        // 2 part suffix list - remove these even if the name only has one other part
        private static readonly String[] ignored2PartSuffixes = { "jr", "vr", "uk", "us", "fr", "de", "sw", "es", "dk", "div", "proam", "am", "pro" };
        // 3 part suffix list - remove these if the name 2 or more other parts
        private static readonly String[] ignored3PartSuffixes = { "junior", "senior", "division", "group" };

        // provide a hint to the phonics matcher - only allow names whose first letters match these pairs
        private static readonly (string, string)[] closeFirstLetters = {
            ( "C", "K" ), ( "J", "G" ), ( "W", "V" ), ( "Z", "S" ),
            ( "Sh", "Ch" ), ( "Ch", "Sch" ), ( "Th", "T" ), ( "Ts", "S" ),
            ( "X", "Z" ), ( "Dj", "J" ) };

        private static readonly (char, char)[] numberLetterSubstitutions = {
            ( '0', 'o' ), ( '1', 'l' ), ( '3', 'b' ), ( '5', 's' ) };
        // brackets could have the same treatment
        // all the above could be in a JSON file...

        internal static Dictionary<String, String> lowerCaseRawNameToUsableName = new Dictionary<String, String>();

        private static Dictionary<String, String> usableNamesForSession = new Dictionary<String, String>();

        private static HashSet<string> suppressFuzzyMatchesOnTheseNames = new HashSet<string>();

        private static string generatedDriverNamesPath;

        private static readonly bool useFuzzyDrivernameMatching = UserSettings.GetUserSettings().getBoolean("use_fuzzy_driver_name_matching");

        /// <summary>
        /// Specific opponent names used by the game can be mapped to specific 
        /// sound files as well as the straight name match.
        /// There is more than one file containing mappings
        /// </summary>
        /// <param name="soundsFolderName">Where the lists are stored</param>
        /// Loads the list of driver name : sound file mappings into lowerCaseRawNameToUsableName{}
        public static void ReadDriverNameMappings(String soundsFolderName)
        {
            lowerCaseRawNameToUsableName.Clear();
            readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\additional_names.txt");
            readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\names.txt");
            if (useFuzzyDrivernameMatching)
            {
                // Generating fuzzy match driver names is costly so they're stored once
                // they're generated
                readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\generated_names.txt");
                generatedDriverNamesPath = Path.Combine(soundsFolderName, @"driver_names\generated_names.txt");
            }
        }

        internal static void readRawNamesToUsableNamesFile(String soundsFolderName, String filename)
        {
            Console.WriteLine("Reading driver name mappings");
            int counter = 0;
            string line;
            try
            {
                StreamReader file = new StreamReader(Path.Combine(soundsFolderName, filename));
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length > 1 && line.Trim().EndsWith(":"))
                    {
                        suppressFuzzyMatchesOnTheseNames.Add(line.Trim(':').ToLower());
                    }
                    else
                    {
                        int separatorIndex = line.LastIndexOf(":");
                        if (separatorIndex > 0 && line.Length > separatorIndex + 1)
                        {
                            String lowerCaseRawName = line.Substring(0, separatorIndex).ToLower();
                            String usableName = line.Substring(separatorIndex + 1).Trim().ToLower();
                            if (usableName != null && usableName.Length > 0)
                            {
                                // add new or replace the existing mapping - last one wins
                                lowerCaseRawNameToUsableName[lowerCaseRawName] = usableName;
                            }
                        }
                    }
                    counter++;
                }
                file.Close();
                Console.WriteLine("Read " + counter + " driver name mappings");
            }
            catch (IOException)
            {}
        }
        
        internal static String validateAndCleanUpName(String name)
        {
            try
            {
                name = replaceObviousChars(name);
                name = cleanBrackets(name);
                if (name.Count() < 2)
                {
                    return null;
                }
                name = undoNumberSubstitutions(name);
                name = trimNumbersOffEnd(name);
                if (name.Count() < 2)
                {
                    return null;
                }
                name = trimNumbersOffStart(name);
                if (name.Count() < 2)
                {
                    return null;
                }
                Boolean allCharsValid = true;
                String charsFromName = "";
                for (int i = 0; i < name.Count(); i++)
                {
                    char ch = name[i];
                    if (Char.IsLetter(ch) || ch == ' ' || ch == '\'')
                    {
                        charsFromName = charsFromName + ch;
                    }
                    else
                    {
                        allCharsValid = false;
                    }
                }
                string validCharsOnly = null;
                // at this point we may have a name like BobSmith or garyMcShit.
                // Before we lower case it, see if there's a split hiding in the case (PascalCase or camelCase)
                if (allCharsValid && name.Trim().Count() > 1)
                {
                    validCharsOnly = SpacesFromCamel(name.Trim()).ToLower();
                }
                else if (charsFromName.Trim().Count() > 1)
                {
                    validCharsOnly = SpacesFromCamel(charsFromName.Trim()).ToLower();
                }
                return validCharsOnly;
            }
            catch (Exception)
            {
                
            }
            return null;
        }
        private static String replaceObviousChars(String name)
        {
            name = name.Replace('_', ' ');
            // be a bit careful with hypens - if it's before the first space, just remove it as
            // it's a separated firstname
            if (name.IndexOf(' ') > 0 && name.IndexOf('-') > 0 && name.IndexOf('-') < name.IndexOf(' '))
            {
                name = name.Replace("-", "");
            }
            name = name.Replace('-', ' ');
            name = name.Replace('.', ' ');
            name = name.Replace("$", "s");
            // trim the string and replace any multiple whitespace chars with a single space
            return Regex.Replace(name.Trim(), @"\s+", " ");
        }
        private static string cleanBrackets(string name)
        {
            name = Regex.Replace(name, @"\[.*\]", string.Empty);
            name = Regex.Replace(name, @"\<.*\>", string.Empty);
            name = Regex.Replace(name, @"\(.*\)", string.Empty);
            name = Regex.Replace(name, @"\{.*\}", string.Empty);
            return name.Trim();
        }
        private static String undoNumberSubstitutions(String name)
        {
            // handle letter -> number substitutions
            String nameWithLetterSubstitutions = "";
            for (int i = 0; i < name.Count(); i++)
            {
                char ch = name[i];
                Boolean changedNumberForLetter = false;
                // see if this is a letter -> number subtitution - can only handle one of these
                if (i > 0 && i < name.Count() - 1)
                {
                    if (Char.IsNumber(ch) && Char.IsLetter(name[i - 1]) && Char.IsLetter(name[i + 1]))
                    {
                        foreach (var nls in numberLetterSubstitutions)
                        {
                            if (ch == nls.Item1)
                            {
                                changedNumberForLetter = true;
                                nameWithLetterSubstitutions = nameWithLetterSubstitutions + nls.Item2;
                                break;
                            }
                        }
                    }
                }
                if (!changedNumberForLetter)
                {
                    nameWithLetterSubstitutions = nameWithLetterSubstitutions + ch;
                }
            }
            return nameWithLetterSubstitutions;
        }

        private static String trimNumbersOffEnd(String name)
        {
            // trim numbers off the end
            while (name.Count() > 2 && char.IsNumber(name[name.Count() - 1]))
            {
                name = name.Substring(0, name.Count() - 1);
            }
            return name;
        }

        private static String trimNumbersOffStart(String name)
        {
            int index = 0;
            while (name.Count() > 2 && index < name.Count() - 1 && char.IsNumber(name[index]))
            {
                name = name.Substring(index + 1);
            }
            return name;
        }

        public static String getUsableDriverName(String rawDriverName)
        {
            string matchedDriverName;
            if (usableNamesForSession.ContainsKey(rawDriverName))
            {   // We found a match previously
                matchedDriverName = usableNamesForSession[rawDriverName];
            }
            else
            {
                matchedDriverName = tryToMatchDriverName(rawDriverName);
                if (matchedDriverName != null)
                {
                    usableNamesForSession.Add(rawDriverName, matchedDriverName);
                }
            }
            return matchedDriverName;
        }
        private static string tryToMatchDriverName(string rawDriverName)
        {
            // Multiple returns from a method are usually a no-no but
            // returns are used here to reduce confusing indenting
            String matchedDriverName = null;
            if (lowerCaseRawNameToUsableName.TryGetValue(rawDriverName.ToLower(), out matchedDriverName))
            {   // Clause 1: A straight match
                Console.WriteLine("Using mapped drivername " + matchedDriverName + " for raw driver name " + rawDriverName);
                return matchedDriverName;
            }

            string usableDriverName = validateAndCleanUpName(rawDriverName);
            if (usableDriverName == null)
            {   // Clause 2: an error
                Log.Error("Unable to create a usable driver name for " + rawDriverName);
                return null;
            }

            if (lowerCaseRawNameToUsableName.TryGetValue(usableDriverName.ToLower(), out matchedDriverName))
            {   // Clause 3: Using mapped driver name for cleaned up driver name
                Console.WriteLine("Using mapped driver name " + matchedDriverName + " for cleaned up raw driver name " + rawDriverName);
                return matchedDriverName;
            }

            // Nothing mapped, see if there is a last name
            // (if not use the whole name)
            String anyFirstNamesRemoved = getUnambiguousLastName(usableDriverName);
            if (anyFirstNamesRemoved != null && anyFirstNamesRemoved.Count() > 1)
            {
                if (SoundCache.availableDriverNames.Contains(anyFirstNamesRemoved))
                {
                    // Clause 4: We have a sound file for the driver last name
                    Console.WriteLine("Using driver last name " + anyFirstNamesRemoved + " for driver raw name " + rawDriverName);
                    return matchedDriverName;
                }
                if (lowerCaseRawNameToUsableName.TryGetValue(anyFirstNamesRemoved.ToLower(), out matchedDriverName))
                {   // Clause 5: Using mapped driver name for cleaned up driver (last) name
                    Console.WriteLine("Using mapped driver name " + matchedDriverName + " for cleaned up driver (last) name " + anyFirstNamesRemoved);
                    return matchedDriverName;
                }
                var fuzzyDriverLastName = MatchForOpponentName(anyFirstNamesRemoved);
                if (fuzzyDriverLastName.matched)
                {
                    matchedDriverName = fuzzyDriverLastName.driverNameMatches[0].ToLower();
                    // Clause 6: Using fuzzy matched driver name for cleaned up driver (last) name
                    Utilities.AddLinesToFile(generatedDriverNamesPath, new List<string> { $"{anyFirstNamesRemoved}:{matchedDriverName}" });
                    Console.WriteLine($"Adding fuzzy mapping for name {anyFirstNamesRemoved}:{matchedDriverName}");
                    // add the newly-mapped name to the list
                    lowerCaseRawNameToUsableName[anyFirstNamesRemoved] = matchedDriverName;
                    return matchedDriverName;
                }
                // Clause 6: Using unmapped driver last name for raw driver name
                Console.WriteLine("Using unvocalised driver (last) name " + anyFirstNamesRemoved + " for raw driver name " + rawDriverName);
                return anyFirstNamesRemoved;
            }
            Console.WriteLine("Using unvocalised drivername " + usableDriverName + " for raw driver name " + rawDriverName);
            return usableDriverName;
        }
        // For unit testing
        internal static int GetSize_usableNamesForSession()
        {
            return usableNamesForSession.Count;
        }

        public struct FuzzyDriverNameResult
        {
            public List<string> driverNameMatches;
            public int matchLevel;
            public bool matched;
            public int fuzzyConfidence;
        }
        private static string[] getAvailableNamesWithCloseFirstLetters(string driverName, string[] availableDriverNames)
        {
            driverName = char.ToUpper(driverName[0]) + driverName.Substring(1);
            List<string> names = new List<string>();
            int minNameLength = driverName.Length / 2;
            int maxNameLength = driverName.Length * 2;
            foreach (string name in availableDriverNames)
            {
                if (name.Length > minNameLength && name.Length < maxNameLength &&
                        (name[0] == driverName[0] || firstLettersCloseEnough(driverName, name)))
                {
                    names.Add(name);
                }
            }
            return names.ToArray<string>();
        }
        public static FuzzyDriverNameResult MatchForOpponentName(string driverName, string[] availableDriverNames = null)
        {
            if (!useFuzzyDrivernameMatching || suppressFuzzyMatchesOnTheseNames.Contains(driverName.ToLower()))
            {
                var emptyResult = new FuzzyDriverNameResult();
                emptyResult.driverNameMatches = new List<string>();
                emptyResult.matched = false;
                return emptyResult;
            }
            if (availableDriverNames == null)
            {
                availableDriverNames = SoundCache.availableDriverNamesForUIAsArray; // files in AppData\Local\CrewChiefV4\sounds\driver_names
            }
            return PhonixFuzzyMatches(driverName, getAvailableNamesWithCloseFirstLetters(driverName, availableDriverNames), 1);
        }
        public static FuzzyDriverNameResult PhonixFuzzyMatches(string driverName, string[] availableDriverNames, int numberOfNamesRqd = 1)
        {
            bool multipleMatchesRequested = numberOfNamesRqd > 1;
            FuzzyDriverNameResult result = new FuzzyDriverNameResult();
            result.driverNameMatches = new List<string>();
            result.matched = false;
            if (driverName.Length < 2)
            {
                return result;
            }
            var soundex = new Soundex();
            var doubleMetaphone = new DoubleMetaphone();
            var matchRatingApproach = new MatchRatingApproach();
            var caverPhone = new CaverPhone();
            var metaphone = new Metaphone();

            // keep track of what we've matched so we don't add the same result twice
            HashSet<string> allMatches = new HashSet<string>();

            // Try to find a name where at least 2 algorithms find a match
            for (int threshold = 4; threshold > 1; threshold--)
            {
                List<string> matchesForThisThreshold = new List<string>();
                foreach (var availableName in availableDriverNames)
                {
                    string[] array = new string[] { driverName, availableName };
                    int matches = 0;
                    try
                    {
                        if (soundex.IsSimilar(array))
                        {
                            matches++;
                        }
                        if (doubleMetaphone.IsSimilar(array))
                        {
                            matches++;
                        }
                        //if (matchRatingApproach.IsSimilar(array))  Throws IndexOutOfRange a lot
                        //{
                        //    matches++;
                        //}
                        if (caverPhone.IsSimilar(array))
                        {
                            matches++;
                        }
                        if (metaphone.IsSimilar(array))
                        {
                            matches++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Exception(ex, "Phonix dll");
                    }
                    if (matches >= threshold && allMatches.Add(availableName))
                    {
                        // multiple match mode - add this match to the set and stop looking if we have enough
                        if (multipleMatchesRequested)
                        {
                            result.driverNameMatches.Add(availableName);
                            result.matched = true;
                            result.matchLevel = matches;
                            if (result.driverNameMatches.Count > numberOfNamesRqd)
                            {
                                // we have enough, stop looking and return - no need to do another run at a lower threshold
                                return result;
                            }
                        }
                        else
                        {
                            matchesForThisThreshold.Add(availableName);
                        }
                    }
                }
                // single match mode - get the best we have and stop looking
                if (!multipleMatchesRequested && matchesForThisThreshold.Count() > 0)
                {
                    // fuzzy match threshold can be more lenient when we have a great phonic match
                    int matchScoreThreshold = threshold == 4 ? 60 : threshold == 3 ? 72 : 75;
                    // get the best match from what we have, if it's good enough stop.
                    // "good enough" means it has to have a decent fuzzy match, and it can't be massively longer or shorter than the original
                    var fuzzyMatchesForThisThreshold = Process.ExtractTop(driverName, matchesForThisThreshold.ToArray(), limit: 1);
                    if (fuzzyMatchesForThisThreshold.Count() > 0 && fuzzyMatchesForThisThreshold.First().Score > matchScoreThreshold
                        && Math.Abs(fuzzyMatchesForThisThreshold.First().Value.Length - driverName.Length) < 5)
                    {
                        result.driverNameMatches.Add(fuzzyMatchesForThisThreshold.First().Value);
                        result.matched = true;
                        result.matchLevel = threshold;
                        result.fuzzyConfidence = fuzzyMatchesForThisThreshold.First().Score;
                        break;
                    }
                }
            }
            return result;
        }

        public static List<String> getUsableDriverNames(List<String> rawDriverNames)
        {
            usableNamesForSession.Clear();
            foreach (String rawDriverName in rawDriverNames)
            {
                getUsableDriverName(rawDriverName);                
            }
            return usableNamesForSession.Values.ToList();
        }

        private static bool firstLettersCloseEnough(string s1, string s2)
        {
            foreach (var pairs in closeFirstLetters)
            {
                if ((s1.StartsWith(pairs.Item1) && s2.StartsWith(pairs.Item2)) ||
                    (s1.StartsWith(pairs.Item2) && s2.StartsWith(pairs.Item1)))
                {
                    return true;
                }
            }
            return false;
        }
        internal static String getUnambiguousLastName(String fullName)
        {
            if (fullName.Count(Char.IsWhiteSpace) == 0)
            {
                return fullName;
            }
            else
            {
                String[] fullNameSplit = null;
                bool gotFromMiddleBit = false;
                foreach (String middleBit in middleBits)
                {
                    // if we have a something van der somethingelse, we want this to be { "something", "van der", "somethingelse" }
                    string middleBitWithSpaces = " " + middleBit + " ";
                    if (fullName.Contains(middleBitWithSpaces))
                    {
                        string start = fullName.Substring(0, fullName.IndexOf(middleBitWithSpaces));
                        string end = fullName.Substring(fullName.IndexOf(middleBitWithSpaces) + middleBitWithSpaces.Length);
                        List<string> splitList = new List<string>();
                        splitList.AddRange(start.Split(' '));
                        splitList.Add(middleBit);
                        splitList.AddRange(end.Split(' '));
                        fullNameSplit = splitList.ToArray();
                        gotFromMiddleBit = true;
                        break;
                    }
                }
                if (!gotFromMiddleBit)
                {
                    fullNameSplit = trimEmptyStrings(fullName.Split(' '));
                }
                // if the last part is 'uk' or something and there are 2 or more parts, lose the 'uk'.
                if (fullNameSplit.Length > 1 && ignored2PartSuffixes.Contains(fullNameSplit[fullNameSplit.Count() - 1]))
                {
                    fullNameSplit = fullNameSplit.Take(fullNameSplit.Count() - 1).ToArray();
                }
                // Lose the 'junior' only if there are 3 or more parts (note this is done separately so we fix cases
                // like "jim britton junior uk"
                if (fullNameSplit.Length > 2 && ignored3PartSuffixes.Contains(fullNameSplit[fullNameSplit.Count() - 1]))
                {
                    fullNameSplit = fullNameSplit.Take(fullNameSplit.Count() - 1).ToArray();
                }
                if (fullNameSplit.Count() == 1)
                {
                    return fullNameSplit[0];
                }
                if (fullNameSplit.Count() == 2)
                {
                    if (fullNameSplit[1].Count() > 1)
                    {
                        // "something somthingelse", just return "somethingelse"
                        return fullNameSplit[1];
                    }
                    else
                    {
                        // second part is 0 or 1 char, return the first part
                        return fullNameSplit[0];
                    }
                }
                else if (middleBits.Contains(fullNameSplit[fullNameSplit.Count() - 2]))
                {
                    return fullNameSplit[fullNameSplit.Count() - 2] + " " + fullNameSplit[fullNameSplit.Count() - 1];
                }
                else if (fullNameSplit[fullNameSplit.Count() - 2].Length == 1)
                {
                    return fullNameSplit[fullNameSplit.Count() - 1];
                }
                else if (fullNameSplit.Length > 3 && middleBits.Contains((fullNameSplit[fullNameSplit.Count() - 3] + " " + fullNameSplit[fullNameSplit.Count() - 2])))
                {
                    return fullNameSplit[fullNameSplit.Count() - 3] + " " + fullNameSplit[fullNameSplit.Count() - 2] + " " + fullNameSplit[fullNameSplit.Count() - 1];
                }
                // if there are more than 2 names, and the second to last name isn't one of the common middle bits, 
                // use the last part
                return fullNameSplit[fullNameSplit.Count() - 1];
            }
        }
        private static string SpacesFromCamel(string value)
        {
            if (value.Length > 2)
            {
                var result = new List<char>();
                char[] array = value.ToCharArray();
                result.Add(array[0]);
                for (int i = 1; i < array.Length - 1; i++)
                {
                    var prevItem = array[i - 1];
                    var item = array[i];
                    var nextItem = array[i + 1];
                    if (char.IsUpper(item)
                        && nextItem != ' ' && prevItem != ' '
                        && char.IsLower(prevItem)
                        && result.Count > 0)
                    {
                        result.Add(' ');
                    }
                    result.Add(item);
                    if (i == array.Length - 2)
                    {
                        result.Add(nextItem);
                    }
                }
                return new string(result.ToArray());
            }
            return value;
        }
        private static String[] trimEmptyStrings(String[] strings)
        {
            List<String> trimmedList = new List<string>();
            foreach (String str in strings) {
                if (str.Trim().Length > 0)
                {
                    trimmedList.Add(str.Trim());
                }
            }
            return trimmedList.ToArray();
        }

        public static void dumpUnvocalizedNames()
        {
            HashSet<String> existingNamesInFile = getNamesAlreadyInFile(getUnvocalizedDriverNamesFileLocation());
            existingNamesInFile.UnionWith(unvocalizedNames);
            List<String> namesToAdd = new List<String>(existingNamesInFile);
            namesToAdd.RemoveAll(alreadyRecorded => SoundCache.availableDriverNames.Contains(alreadyRecorded));
            namesToAdd.Sort();
            TextWriter tw = new StreamWriter(getUnvocalizedDriverNamesFileLocation(), false);
            foreach (String name in namesToAdd)
            {
                tw.WriteLine(name);
            }
            tw.Close();
        }

        private static HashSet<String> getNamesAlreadyInFile(String fullFilePath)
        {
            HashSet<String> names = new HashSet<string>();
            StreamReader file = null;
            try
            {
                file = new StreamReader(fullFilePath);
                String line;
                while ((line = file.ReadLine()) != null)
                {
                    names.Add(line.Trim());
                }
            }
            catch (Exception)
            {
                // ignore - file doesn't exist so it'll be created
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            return names;
        }

        private static String getUnvocalizedDriverNamesFileLocation()
        {
            return System.IO.Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments), "CrewChiefV4", "unvocalized_driver_names.txt");
        }
    }
}
