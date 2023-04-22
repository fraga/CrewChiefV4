using NUnit.Framework;

using CrewChiefV4;
using System;

using Assert = NUnit.Framework.Assert;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using SharpDX;

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
        }

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
        
        public void test_FuzzyMatches(string driverName, string wavFile)
        {
            // var result = DriverNameHelper.FuzzyMatch(driverName, availableDriverNames);
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
    }
}
