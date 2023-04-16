using CrewChiefV4.Audio;

using FuzzySharp;
using FuzzySharp.Extractor;

using Phonix;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/**
 * Utility class to ease some of the pain of managing driver names.
 */
namespace CrewChiefV4
{
    class DriverNameHelper
    {
        public static HashSet<String> unvocalizedNames = new HashSet<string>();

        // if there's more than 2 names, and the second to last name isn't one of the common middle bits, 
        // use the last part
        private static Boolean optimisticSurnameExtraction = true;

        private static String[] middleBits = new String[] { "de la", "de le", "van der", "van de", "van", "de", "da", "le", "la", "von", "di", "eg", "du", "el", "del", "saint", "st" };

        private static String[] juniorSuffixes = new string[] { "jr", "junior" };

        // provide a hint to the last-ditch phonics matcher - only allow names who's first letters match these pairs
        private static string[][] closeFirstLetters = new string[][] {
            new string[] { "C", "K" }, new string[] { "J", "G" }, new string[] { "W", "V" }, new string[] { "Z", "S" },
            new string[] { "Sh", "Ch" }, new string[] { "Ch", "Sch" }, new string[] { "Th", "T" }, new string[] { "Ts", "S" },
            new string[] { "X", "Z" }, new string[] { "Dj", "J" }};

        private static Dictionary<String, String> lowerCaseRawNameToUsableName = new Dictionary<String, String>();

        private static Dictionary<String, String> usableNamesForSession = new Dictionary<String, String>();

        private static Boolean useLastNameWherePossible = true;

        private static string generatedDriverNamesPath;

        public static void readRawNamesToUsableNamesFiles(String soundsFolderName)
        {
            readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\additional_names.txt");
            readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\names.txt");
            // Generating fuzzy match driver names is costly so they're stored once
            // they're generated
            readRawNamesToUsableNamesFile(soundsFolderName, @"driver_names\generated_names.txt");
            generatedDriverNamesPath = Path.Combine(soundsFolderName, @"driver_names\generated_names.txt");
        }

        private static void readRawNamesToUsableNamesFile(String soundsFolderName, String filename)
        {
            Console.WriteLine("Reading driver name mappings");
            int counter = 0;
            string line;
            try
            {
                StreamReader file = new StreamReader(Path.Combine(soundsFolderName, filename));
                while ((line = file.ReadLine()) != null)
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
                    counter++;
                }
                file.Close();
                Console.WriteLine("Read " + counter + " driver name mappings");
            }
            catch (IOException)
            {}
        }
        
        private static String validateAndCleanUpName(String name)
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
                if (allCharsValid && name.Trim().Count() > 1)
                {
                    return name.Trim().ToLower();
                }
                else if (charsFromName.Trim().Count() > 1)
                {
                    return charsFromName.ToLower().Trim();
                }                
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
            return System.Text.RegularExpressions.Regex.Replace(name.Trim(), @"\s+", " ");
        }

        private static String cleanBrackets(String name)
        {
            if (name.EndsWith("]") && name.Contains("["))
            {
                name = name.Substring(0, name.IndexOf('['));
                name = name.Trim();
            }
            if (name.StartsWith("[") && name.Contains("]"))
            {
                name = name.Substring(name.LastIndexOf(']') + 1);
                name = name.Trim();
            }
            if (name.EndsWith(")") && name.Contains("("))
            {
                name = name.Substring(0, name.LastIndexOf('('));
                name = name.Trim();
            }
            if (name.StartsWith("(") && name.Contains(")"))
            {
                name = name.Substring(name.LastIndexOf(')') + 1);
                name = name.Trim();
            }
            if (name.EndsWith(">") && name.Contains("<"))
            {
                name = name.Substring(0, name.LastIndexOf('<'));
                name = name.Trim();
            }
            if (name.StartsWith("<") && name.Contains(">"))
            {
                name = name.Substring(name.LastIndexOf('>') + 1);
                name = name.Trim();
            }
            if (name.EndsWith("}") && name.Contains("{"))
            {
                name = name.Substring(0, name.LastIndexOf('{'));
                name = name.Trim();
            }
            if (name.StartsWith("{") && name.Contains("}"))
            {
                name = name.Substring(name.LastIndexOf('}') + 1);
                name = name.Trim();
            }
            return name;
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
                        if (ch == '1')
                        {
                            changedNumberForLetter = true;
                            nameWithLetterSubstitutions = nameWithLetterSubstitutions + 'l';
                        }
                        else if (ch == '3')
                        {
                            changedNumberForLetter = true;
                            nameWithLetterSubstitutions = nameWithLetterSubstitutions + 'e';
                        }
                        else if (ch == '0')
                        {
                            changedNumberForLetter = true;
                            nameWithLetterSubstitutions = nameWithLetterSubstitutions + 'o';
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
            if (!usableNamesForSession.ContainsKey(rawDriverName))
            {
                String usableDriverName = null;
                if (lowerCaseRawNameToUsableName.TryGetValue(rawDriverName.ToLower(), out usableDriverName))
                {
                    Console.WriteLine("Using mapped drivername " + usableDriverName + " for raw driver name " + rawDriverName);
                    usableNamesForSession.Add(rawDriverName, usableDriverName);
                }
                else
                {
                    usableDriverName = validateAndCleanUpName(rawDriverName);
                    if (usableDriverName != null)
                    {
                        Boolean usedLastName = false;
                        if (useLastNameWherePossible)
                        {
                            String lastName = getUnambiguousLastName(usableDriverName);
                            if (lastName != null && lastName.Count() > 1)
                            {
                                if (lowerCaseRawNameToUsableName.TryGetValue(lastName.ToLower(), out usableDriverName))
                                {
                                    Console.WriteLine("Using mapped driver name " + usableDriverName + " for raw driver last name " + lastName);
                                    usableNamesForSession.Add(rawDriverName, usableDriverName);
                                    usedLastName = true;
                                }
                                else
                                {
                                    var fuzzyDriverName = FuzzyMatch(lastName);
                                    if (fuzzyDriverName.matched)
                                    {
                                        usableDriverName = fuzzyDriverName.driverNameMatches[0].ToLower();
                                        if (fuzzyDriverName.fuzzy)
                                        {
                                            Utilities.AddLinesToFile(generatedDriverNamesPath,
                                                new List<string> { $"{lastName}:{usableDriverName}" });
                                        }
                                        lowerCaseRawNameToUsableName[rawDriverName] = usableDriverName;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Using unmapped driver last name " + lastName + " for raw driver name " + rawDriverName);
                                        usableDriverName = lastName;
                                    }
                                    usableNamesForSession.Add(rawDriverName, usableDriverName);
                                    usedLastName = true;
                                }
                            }
                        }
                        if (!usedLastName)
                        {
                            var fuzzyDriverName = FuzzyMatch(usableDriverName);
                            if (fuzzyDriverName.matched)
                            {
                                var driverName = fuzzyDriverName.driverNameMatches[0].ToLower();
                                if (fuzzyDriverName.fuzzy)
                                {
                                    Utilities.AddLinesToFile(generatedDriverNamesPath,
                                        new List<string> { $"{usableDriverName}:{driverName}" });
                                }
                                usableDriverName = driverName;
                                lowerCaseRawNameToUsableName[rawDriverName] = usableDriverName;
                            }
                            else
                            {
                                Console.WriteLine("Using unmapped drivername " + usableDriverName + " for raw driver name " + rawDriverName);
                            }
                            usableNamesForSession.Add(rawDriverName, usableDriverName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unable to create a usable driver name for " + rawDriverName);
                    }
                }
                return usableDriverName;
            }
            else
            {
                return usableNamesForSession[rawDriverName];
            }
        }
        public struct FuzzyDriverNameResult
        {
            public List<string> driverNameMatches;
            public int matchLevel;
            public bool matched;
            public bool fuzzy;
        }
        public static FuzzyDriverNameResult FuzzyMatch(string driverName, string[] availableDriverNames = null)
        {
            FuzzyDriverNameResult result = new FuzzyDriverNameResult();
            result.driverNameMatches = new List<string>();
            result.matched = false;

            if (availableDriverNames == null)
            {
                availableDriverNames = SoundCache.availableDriverNamesForUI.ToArray(); // files in AppData\Local\CrewChiefV4\sounds\driver_names
            }
            driverName = char.ToUpper(driverName[0]) + driverName.Substring(1);
            if (availableDriverNames.Contains(driverName))
            {
                result.driverNameMatches.Add(driverName);
                result.matched = true;
                result.fuzzy = false;
                return result;
            }

            string[] useAvailableDriverNames = availableDriverNames.Where(w => w.Length > driverName.Length/2).ToArray();
            
            var matches = Process.ExtractTop(driverName, availableDriverNames, limit: 1);
            if (matches.Count() > 0 &&
                matches.First<ExtractedResult<string>>().Score > 90)
                {
                    var usableDriverName = matches.First<ExtractedResult<string>>().Value;
                    Log.Commentary($"Driver name {driverName} fuzzy matched {usableDriverName}");
                    result.driverNameMatches.Add(usableDriverName);
                    result.matched = true;
                }
                else
                {   // FuzzySharp didn't return good enough matches, try the Phonix set of fuzzy matches
                    // Try names with same first letter first
                    string[] namesWithSameFirstLetter = useAvailableDriverNames.Where(w => w.StartsWith(driverName[0].ToString())).Select(w => w).ToArray<string>();
                    var phonix = PhonixFuzzyMatches(driverName, namesWithSameFirstLetter);
                    if (phonix.matched)
                    {
                        var usableDriverName = phonix.driverNameMatches[0];
                        Log.Commentary($"Driver name {driverName} fuzzy matched {usableDriverName} in names with the same first letter at level {phonix.matchLevel} ");
                    }
                    else
                    {
                        // getting desperate now, try to match anything provided the 2 first letters are close enough   
                        string[] namesWithCloseFirstLetters = useAvailableDriverNames.Where(w => firstLettersCloseEnough(w, driverName)).Select(w => w).ToArray<string>();
                        if (namesWithCloseFirstLetters.Count() > 0)
                        {
                            phonix = PhonixFuzzyMatches(driverName, namesWithCloseFirstLetters);
                            if (phonix.matched)
                            {
                                var usableDriverName = phonix.driverNameMatches[0];
                                Log.Commentary($"Driver name {driverName} fuzzy matched {usableDriverName} in all names at level {phonix.matchLevel} ");
                            }
                        }
                    }
                    result = phonix;

                    //Log.Commentary($"These fuzzy matches for '{driverName}' were not acceptable:");
                    //foreach (var match in matches)
                    //{
                    //    Log.Commentary($"  '{match.Value}', {match.Score}");
                    //}
                }
            result.fuzzy = true;
            return result;
        }
        public static FuzzyDriverNameResult PhonixFuzzyMatches(string driverName, string[] availableDriverNames, int numberOfNamesRqd = 1)
        {
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

            // Try to find a name where at least 2 algorithms find a match
            for (int threshold = 3; threshold > 1; threshold--)
            {
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
                    if (matches > threshold)
                    {
                        if (!result.driverNameMatches.Contains(availableName))
                        {
                            result.driverNameMatches.Add(availableName);
                            result.matched = true;
                            result.matchLevel = matches;
                            if (result.driverNameMatches.Count > numberOfNamesRqd)
                            {
                                break;
                            }
                        }
                    }
                }
                if (result.matched &&
                    result.driverNameMatches.Count > numberOfNamesRqd)
                {
                    break;
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
            foreach (string[] pairs in closeFirstLetters)
            {
                if ((s1.StartsWith(pairs[0]) && s2.StartsWith(pairs[1])) || (s1.StartsWith(pairs[1]) && s2.StartsWith(pairs[0])))
                {
                    return true;
                }
            }
            return false;
        }
        private static String getUnambiguousLastName(String fullName)
        {
            if (fullName.Count(Char.IsWhiteSpace) == 0) 
            {
                return fullName;
            } 
            else
            {
                foreach (String middleBit in middleBits) {
                    if (fullName.Contains(" " + middleBit + " ")) {
                        String[] split = fullName.Split(' ');
                        return middleBit + " " + split[split.Count() - 1];
                    }
                }
                String[] fullNameSplit = trimEmptyStrings(fullName.Split(' '));
                if (fullNameSplit.Count() == 2)
                {
                    String[] split = fullName.Split(' ');
                    if (split[1].Count() > 1)
                    {
                        return split[1];
                    }
                    else
                    {
                        return split[0];
                    }
                }
                else if (fullNameSplit[fullNameSplit.Count() - 2].Length == 1) 
                {
                    return fullNameSplit[fullNameSplit.Count() - 1];
                }
                else if (middleBits.Contains(fullNameSplit[fullNameSplit.Count() - 2].ToLower()))
                {
                    return fullNameSplit[fullNameSplit.Count() - 2] + " " + fullNameSplit[fullNameSplit.Count() - 1];
                }
                else if (fullNameSplit.Length > 3 && middleBits.Contains((fullNameSplit[fullNameSplit.Count() - 3] + " " + fullNameSplit[fullNameSplit.Count() - 2]).ToLower()))
                {
                    return fullNameSplit[fullNameSplit.Count() - 3] + " " + fullNameSplit[fullNameSplit.Count() - 2] + " " + fullNameSplit[fullNameSplit.Count() - 1];
                }
                else if (juniorSuffixes.Contains(fullNameSplit[fullNameSplit.Count() - 1].ToLower()))
                {
                    return fullNameSplit[fullNameSplit.Count() - 2];
                }
                else if (optimisticSurnameExtraction)
                {
                    return fullNameSplit[fullNameSplit.Count() - 1];
                }
            }
            return null;
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
