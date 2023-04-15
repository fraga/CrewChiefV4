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

        [TestCase("waissman", "weissmann")]
        [TestCase("gwilherme", "guilherme")]
        [TestCase("gwilherm", "guilherme")] // fails
        [TestCase("Klemenz", "klemenz")]    // the following are taken from
        [TestCase("Holmes", "holmes")]      // sounds\driver_names\names.txt with small edits
        [TestCase("Gómez", "g�mez")]
        [TestCase("Mueller", "muller")]
        [TestCase("Clarke", "clark")]
        [TestCase("Fischer", "fischer")]
        [TestCase("Anderson", "Andersson")]
        [TestCase("Chaves", "chavez")]
        [TestCase("Christoph", "christophe")]
        [TestCase("Christopherssen", "christofferson")]
        [TestCase("Christoffersen", "christofferson")]
        [TestCase("D]Alessandro", "dalessandro")]
        [TestCase("Frank", "franke")]
        [TestCase("Low", "lau")] //!!
        [TestCase("Laughton", "lawton")]
        [TestCase("Leigh", "lee")]
        [TestCase("McDonald", "macdonald")]
        [TestCase("Moor", "moore")]
        [TestCase("Sergent", "sargent")]
        [TestCase("Sunn", "son")]
        [TestCase("Tomas", "thomas")]
        [TestCase("Webber", "weber")]
        [TestCase("Hundt", "hand")] //!!
        [TestCase("Bryan", "brien")]
        [TestCase("Clement", "clemente")]
        [TestCase("De Campos", "decampos")]
        [TestCase("Foulds", "faulds")]
        [TestCase("Folett", "follett")]
        [TestCase("Graeme", "graham")]
        [TestCase("Grey", "gray")]
        [TestCase("Healey", "heasley")] //!!
        [TestCase("Hugues", "hakes")]
        [TestCase("Larsson", "larsen")]
        [TestCase("Manson", "mansson")]
        [TestCase("Mellor", "meller")]
        [TestCase("Prince", "price")] //!!
        [TestCase("Stephenson", "stevenson")]
        [TestCase("Villa", "vila")]
        [TestCase("Yung", "young")]
        [TestCase("Holtz", "holts")]
        [TestCase("Holtze", "holts")]
        [TestCase("webber", "weber")]
        public void test_FuzzyMatches(string driverName, string wavFile)
        {
            var result = DriverNameHelper.FuzzyMatch(driverName, availableDriverNames);
            if (result.matched)
            {
                var filename = result.driverNameMatches[0];
                if (driverName.ToLower() != filename.ToLower())
                {
                    Assert.AreEqual(wavFile.ToLower(), filename.ToLower());
                }
            }
            else
            {
                Log.Error($"{driverName} did not find a match");
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
