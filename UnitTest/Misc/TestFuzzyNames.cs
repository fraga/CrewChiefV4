using NUnit.Framework;

using Assert = NUnit.Framework.Assert;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

using CrewChiefV4;
using CrewChiefV4.UserInterface.Models;
using static CrewChiefV4.DriverNameHelper;

namespace UnitTest.Misc
{
    [TestFixture]
    public class TestFuzzyNames
    {
        string[] availableDriverNames = null;
        [SetUp]
        public void Init()
        {
            StreamReader sr = new StreamReader(@"../../misc/driverNameFiles.txt");
            var line = sr.ReadLine();
            List<string> aDN = new List<string>();
            while (line != null) 
            {
                if (line.Contains(".wav"))
                { // aachban.wav -> Aachban
                    line = line.Substring(0,line.Length - ".wav".Length);
                    line = char.ToUpper(line[0]) + line.Substring(1).ToLower();
                    aDN.Add(line);
                }
                line = sr.ReadLine();
            }
            availableDriverNames = aDN.ToArray();
            readRawNamesToUsableNamesFile(@"../../../CrewChiefV4/sounds/driver_names", "names.txt", lowerCaseRawNameToUsableName);
        }

        #region Test_FuzzyMatches
        // good matches...
        [TestCase("waissman", "weissmann")]
        [TestCase("gwilherme", "guilherme")]
        [TestCase("gwilherm", "guilherme")]
        [TestCase("Klemenz", "klemenz")]    // the following are taken from
        [TestCase("Holmes", "holmes")]      // sounds\driver_names\names.txt with small edits
        [TestCase("Gómez", "g�mez")]
        [TestCase("Mueller", "muller")]
        [TestCase("Clarke", "clark")]
        [TestCase("Fischer", "fischer")]
        [TestCase("Anderson", "Andersson")]
        [TestCase("Chaves", "chavez")]
        [TestCase("Christoph", "christophe")]
        [TestCase("Christopherssen", "christopherson")]
        [TestCase("Christoffersen", "christofferson")]
        [TestCase("D]Alessandro", "dalessandro")]
        [TestCase("Frank", "franke")]
        [TestCase("Low", "louw")]
        [TestCase("Laughton", "lawton")]
        [TestCase("Leigh", "lee")]
        [TestCase("McDonald", "macdonald")]
        [TestCase("Moor", "moore")]
        [TestCase("Sergent", "sargent")]
        [TestCase("Sunn", "sun")]
        [TestCase("Tomas", "thomas")]
        [TestCase("Webber", "weber")]
        [TestCase("Hundt", "hunt")]
        [TestCase("Bryan", "brien")]
        [TestCase("Clement", "clemente")]
        [TestCase("De Campos", "decampos")]
        [TestCase("Foulds", "faulds")]
        [TestCase("Folett", "follett")]
        [TestCase("Graeme", "graham")]
        [TestCase("Grey", "gray")]
        [TestCase("Healey", "haley")]
        [TestCase("Hugues", "hughes")]
        [TestCase("Larsson", "larsen")]
        [TestCase("Manson", "mansson")]
        [TestCase("Mellor", "meller")]
        [TestCase("Prince", "prinz")]
        [TestCase("Stephenson", "stevenson")]
        [TestCase("Villa", "vila")]
        [TestCase("Yung", "young")]
        [TestCase("Holtz", "holts")]
        [TestCase("Holtze", "holts")]
        [TestCase("webber", "weber")]
        [TestCase("olley", "olly")]
        [TestCase("plocke", "pollock")]
        [TestCase("müslie", "mosley")]
        [TestCase("morrie", "mori")]
        [TestCase("rahal", "rahhal")]
        [TestCase("rezende", "resende")]
        [TestCase("bouille", "boule")]
        [TestCase("senna", "senna")]

        // not so good...
        [TestCase("rits", "raats")]
        [TestCase("ride", "reid")]
        [TestCase("piper", "pepper")]
        [TestCase("burdon", "burton")]
        [TestCase("marrazzo", "merazzi")]
        [TestCase("pallotti", "pauletta")]
        [TestCase("matter", "mader")]
        [TestCase("bowles", "blues")]
        [TestCase("wilhelm", "wilhelmsson")]
        [TestCase("koep", "koop")]
        [TestCase("moura", "moreau")]
        [TestCase("manuel", "muanle")]
        [TestCase("marín", "morn�")]
        [TestCase("goerg", "gourg")]
        [TestCase("dearman", "dorman")]
        [TestCase("neubauer", "neber")]
        [TestCase("francani", "frankonia")]

        // no matches
        [TestCase("saridis", null)]
        [TestCase("snow", null)]
        [TestCase("dobal", null)]
        [TestCase("canaille", null)]
        [TestCase("staveley", null)]
        [TestCase("iusan", null)]
        [TestCase("sancio", null)]
        [TestCase("trundle", null)]
        [TestCase("etoo", null)]
        [TestCase("ditte", null)]
        [TestCase("messina", null)]
        [TestCase("tarta", null)]
        [TestCase("bekesi", null)]
        [TestCase("de pauw", null)]
        [TestCase("sachse", null)]
        [TestCase("fenstermacher", null)]
        [TestCase("boltzmann", null)]
        [TestCase("berenger", null)]
        [TestCase("prunes", null)]

        public void test_FuzzyMatches(string driverName, string wavFile)
        {
            var result = DriverNameHelper.MatchForOpponentName(driverName, availableDriverNames);
            if (wavFile == null && result.matched)
            {
                Assert.Fail($"expected no match for {driverName} but got {result.driverNameMatches[0]} " +
                    $"with confidence {result.fuzzyConfidence} from phonic match count {result.matchLevel}");
            }
            else if (result.matched)
            {
                var filename = result.driverNameMatches[0];
                if (driverName.ToLower() != filename.ToLower())
                {
                    Assert.AreEqual(wavFile.ToLower(), filename.ToLower());
                }
            }
            else if (wavFile != null)
            {
                Assert.Fail($"{driverName} returned no match, expected {wavFile}");
            }
        }
        [TestCase("foulds", "faulds")]
        [TestCase("waissman", "weissmann")]
        public void test_PhonixFuzzyMultipleMatches(string driverName, string wavFile)
        {
            var result = DriverNameHelper.PhonixFuzzyMatches(driverName, availableDriverNames, 10);
            var filename = result.driverNameMatches[0];
            if (driverName.ToLower() != filename.ToLower())
            {
                Assert.AreEqual(wavFile.ToLower(), filename.ToLower());
            }
            Assert.Greater(result.driverNameMatches.Count, 0);
            for (int i = 0; i < result.driverNameMatches.Count; i++)
            {
                driverName = result.driverNameMatches[i];
                Log.Commentary(driverName);
            }
        }
        [Test]
        [Ignore("Takes forever!")]
        public void test_unvocalisedNames()
        {
            StreamReader sr = new StreamReader(@"../../misc/unvocalisedNames.h");
            var line = sr.ReadLine();
            while (line != null)
            {
                if (line.Contains(","))
                { // "a h", null
                    var uvn = line.Split(',')[0].Trim(new char[] { ' ', '"' });
                    var result = DriverNameHelper.MatchForOpponentName(uvn, availableDriverNames);
                    if (result.matched)
                    {
                        Console.WriteLine($"'{uvn}' fuzzy matched with {result.driverNameMatches[0]}");
                    }
                    else
                    {
                        Console.WriteLine($"'{uvn}' didn't match");
                    }
                }
                line = sr.ReadLine();
            }
        }
        #endregion Test_FuzzyMatches

        #region Test_getUsableDriverName
        [TestCase("J1mBr1tt0n", "brltton")] // note 1 is substituted for L (not I)
        [TestCase("JimBri5tol", "bristol")]
        [TestCase("JimBritton", "britton")]
        [TestCase("jimBRITTON", "britton")]
        [TestCase("jim BRITTON", "britton")]
        [TestCase("jim BRITTON UK", "britton")]
        [TestCase("JIM BRITTON", "britton")]
        [TestCase("JIM_BRITTON!!!", "britton")]
        [TestCase("BRITTON", "britton")]
        [TestCase("JimBritton!!!!", "britton")]
        [TestCase("Jim Britton!!!!", "britton")]
        [TestCase("jim vanderbritton", "vanderbritton")]
        [TestCase("Jim_Van_Der_Britton", "van der britton")]
        [TestCase("jim_von_britton", "von britton")]
        [TestCase("Jim Van Der Britton", "van der britton")]
        [TestCase("JimVanDerBritton", "van der britton")]
        [TestCase("Jim McShit", "mc shit")]
        [TestCase("Jim Mcshit", "mcshit")]
        [TestCase("Jim MacShit", "mac shit")]
        [TestCase("Dave Mackay", "mackay")]
        [TestCase("bobbyMoore", "moore")]
        [TestCase("Jim Britton uk", "britton")]
        [TestCase("JimBritton UK", "britton")]
        [TestCase("Jim Britton [UK]", "britton")]
        [TestCase("Jim Britt0n 69 UK", "britton")]
        [TestCase("Jim Britton 69", "britton")]
        [TestCase("Jim Britton Junior UK", "britton")]
        [TestCase("Jim Britton Junior", "britton")]
        [TestCase("Jim Britton jr", "britton")]
        [TestCase("Jim Junior", "junior")]
        [TestCase("Jim LeClerc", "le clerc")]
        [TestCase("Jim le Clerc", "le clerc")]
        [TestCase("Charles Leclerc", "leclerc")]
        [TestCase("Jim Ng", "ng")]
        [TestCase("jim", "jim")]
        [TestCase("a", null)]
        [TestCase("ba", "ba")]
        [TestCase("aaaa", "aaaa")]
        [TestCase("345hf9237f", "hff")]
        [TestCase("9999", null)]
        [TestCase("Jim Britton DIV 2", "britton")]
        [TestCase("jimBritton DIV 2", "britton")]
        [TestCase("jimBritton pro", "britton")]
        [TestCase("jimBritton proam", "britton")]
        [TestCase("jim DIV 2", "jim")]
        [TestCase("Jim Britton division 2", "britton")]
        [TestCase("Jim [da man] Britton", "britton")]
        [TestCase("JimBritton {some nonsense} UK", "britton")]
        [TestCase("Britton {some other nonsense} UK", "britton")]
        [TestCase("Jim <boss> Britton (is ace)", "britton")]
        // [TestCase("Jim <boss> Britton [][]<>{} ()", "britton")]  fails because it removes everything between the first < and the last >
        [TestCase("Jim Britton [dude]", "britton")]
        [TestCase("<ejit> JimBritton", "britton")]
        [TestCase("<ejit> Jim Britton [dipstick]{smelly}", "britton")]
        [TestCase("Jim   ARG", "jim")]
        public void Test_getUsableDriverName(string rawDriverName,
            string usableDriverName)
        {
            var driverName = getUsableDriverName(rawDriverName);
            Assert.AreEqual(usableDriverName, driverName);
        }
        #endregion Test_getUsableDriverName

        #region Test_getUsableDriverNameAB
        static int BEFORE_usableNamesForSessionCount = 0;
        static int AFTER_usableNamesForSessionCount = 0;
        [TestCase("michael holtz", "holts", "holtz", 1)]
        [TestCase("Patricio Javier Alzamora", "alzamora", "alzamora", 1)] // Clause 1: A straight match
        // (Clause 2 is an error condition I can't see a way to generate)
        [TestCase("webber232", "weber", "weber", 1)]         // Clause 3: Using mapped driver name for cleaned up driver name
        [TestCase("andy weber", "weber", "weber", 1)]        // Clause 4: We have a sound file for the driver last name
        [TestCase("jim whatshisname", "whats his name", "whatshisname", 1)]       // Clause 5: Using mapped driver name for cleaned up driver (last) name
        [TestCase("andy wexxxer", "wexxxer", "wexxxer", 1)]    // Clause 6a: Using unmapped driver last name for raw driver name
        [TestCase("weyyyer", "weyyyer", "weyyyer", 1)]         // Clause 6b: Using unmapped driver name for raw driver name

        // Rerun the same cases and no new names should be added
        // ...assuming the tests are run in this order
        [TestCase("michael holtz", "holts", "holtz", 0)]
        [TestCase("Patricio Javier Alzamora", "alzamora", "alzamora", 0)] // Clause 1: A straight match
        // (Clause 2 is an error condition I can't see a way to generate)
        //[TestCase("webber232", "weber", "webber", 0)]         // Clause 3: Using mapped driver name for cleaned up driver name
        [TestCase("andy weber", "weber", "weber", 0)]        // Clause 4: We have a sound file for the driver last name
        [TestCase("jim whatshisname", "whats his name", "whatshisname", 0)]       // Clause 5: Using mapped driver name for cleaned up driver (last) name
        [TestCase("andy wexxxer", "wexxxer", "wexxxer", 0)]    // Clause 6a: Using unmapped driver last name for raw driver name
        [TestCase("weyyyer", "weyyyer", "weyyyer", 0)]         // Clause 6b: Using unmapped driver name for raw driver name

        public void AB_Test_getUsableDriverName(string rawDriverName,
            string usableDriverName,
            string surname,
            int adds1to_usableNamesForSession)
        {
            Assert.AreEqual(BEFORE_usableNamesForSessionCount, AFTER_usableNamesForSessionCount);
            var driverName = BEFORE_DriverNameHelper.BEFORE_getUsableDriverName(rawDriverName);
            Assert.AreEqual(driverName, usableDriverName);
            BEFORE_usableNamesForSessionCount += adds1to_usableNamesForSession;
            Assert.AreEqual(BEFORE_usableNamesForSessionCount,
                BEFORE_DriverNameHelper.getSize_usableNamesForSession());

            // Old method passes, check the new method
            driverName = getUsableDriverName(rawDriverName);
            Assert.AreEqual(driverName, usableDriverName);
            var driverSurname = getUsableDriverNameForSRE(rawDriverName);
            Assert.AreEqual(driverSurname, surname);
            AFTER_usableNamesForSessionCount += adds1to_usableNamesForSession;
            Assert.AreEqual(AFTER_usableNamesForSessionCount,
                GetSize_usableNamesForSession());
        }
    }

    // Copy of getUsableDriverName() before refactoring.
    class BEFORE_DriverNameHelper
    {
        private static Dictionary<String, String> usableNamesForSession = new Dictionary<String, String>();
        private static bool useLastNameWherePossible = true;
        internal static int getSize_usableNamesForSession()
        {
            return usableNamesForSession.Count;
        }
        public static String BEFORE_getUsableDriverName(String rawDriverName)
        {
            if (!usableNamesForSession.ContainsKey(rawDriverName))
            {
                String usableDriverName = null;
                if (lowerCaseRawNameToUsableName.TryGetValue(rawDriverName.ToLower(), out usableDriverName))
                {
                    Console.WriteLine("BEFORE_Using mapped drivername " + usableDriverName + " for raw driver name " + rawDriverName);
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
                                    Console.WriteLine("BEFORE_Using mapped driver last name " + usableDriverName + " for raw driver last name " + lastName);
                                    usableNamesForSession.Add(rawDriverName, usableDriverName);
                                    usedLastName = true;
                                }
                                else
                                {
                                    Console.WriteLine("BEFORE_Using unmapped driver last name " + lastName + " for raw driver name " + rawDriverName);
                                    usableDriverName = lastName;
                                    usableNamesForSession.Add(rawDriverName, usableDriverName);
                                    usedLastName = true;
                                }
                            }
                        }
                        if (!usedLastName)
                        {
                            Console.WriteLine("BEFORE_Using unmapped drivername " + usableDriverName + " for raw driver name " + rawDriverName);
                            usableNamesForSession.Add(rawDriverName, usableDriverName);
                        }
                    }
                    else
                    {
                        Console.WriteLine("BEFORE_Unable to create a usable driver name for " + rawDriverName);
                    }
                }
                return usableDriverName;
            }
            else
            {
                return usableNamesForSession[rawDriverName];
            }
        }
        #endregion Test_getUsableDriverName
    }
}
