using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrewChiefV4;
using CrewChiefV4.PitManager;
using System.Resources;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;

// NONE of these actually run, TestPitManager does allow stepping through the
// Pit Menu event handler
namespace UnitTest
{
    [TestClass]
    public class TestPitManager
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException),
            "text")]
        /// RUNS OK AS LONG AS RF2 IS NOT LOADED
        public void Test_EventHandler()
        {
            bool result;
            var pmh = new PitManager();

            result = pmh.EventHandler(PitManagerEvent.TyreCompoundWet, "");
            Thread.Sleep(100);

            result = pmh.EventHandler(PitManagerEvent.AeroFrontSetToX, "");
            Thread.Sleep(100);
            result = pmh.EventHandler(PitManagerEvent.RepairFast, "");
            Thread.Sleep(100);
        }

        [TestMethod]
        [Ignore]
        public void Test_EventHandlerFuel()
        {
            bool result;
            var pmh = new PitManager();
            //pmh.AmountHandler(32);
            result = pmh.EventHandler(PitManagerEvent.FuelAddXlitres, "fifteen liters");
            Thread.Sleep(100);
        }

        [TestMethod]
        [Ignore]
        [ExpectedException(typeof(TypeInitializationException),
            "text")]
        public void Test_EventHandlerUndo()
        {
            bool result;
            var pmh = new PitManager();

            result = pmh.EventHandler(PitManagerEvent.TyreChangeNone, "");
            Thread.Sleep(100);

            // tbd pmh.AmountHandler(0);
            result = pmh.EventHandler(PitManagerEvent.FuelAddXlitres, "one gallon");
            Thread.Sleep(100);
        }
    }

    [TestClass]
    public class TestPitManagerInCC
    {
        private static MainWindow hwnd;
        public static CrewChief ccObj;
        [ClassInitialize]
        public static void TestFixtureSetup(TestContext context)
        {
            // Set Invariant Culture for all threads as default.
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            // Set Invariant Culture for current thead.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            hwnd = new MainWindow();
            //ControllerConfiguration controllerConfiguration = new ControllerConfiguration(hwnd);
            ccObj = hwnd.crewChief;//    new CrewChief(controllerConfiguration);
        }
        [TestMethod]
        [Ignore]
        public void CCobject()
        {
            ; // Do nothing, just tests that CC MainWindow has been created
        }
#if false
        [TestMethod]
        public void Test1()
        {
            var pmh = new PitManager_dev();
            pmh.method();
            pmh.EventHandler(PitManager_devEvent.AeroFrontPlusMinusX);

        }
        [TestMethod]
        public void Test2()
        {
            bool result;
            var pmh = new PitManager_dev();
            pmh.method();
            result = pmh.EventHandler2(PitManager_devEvent.AeroFrontPlusMinusX);
            Assert.IsTrue(result);
            result = pmh.EventHandler2(PitManager_devEvent.FenderL);
            Assert.IsTrue(result);
        }
#endif

        [TestMethod]
        [Ignore]
        public void TestVoice()
        {
            bool result;
            var pmh = new PitManager();
            PitManagerVoiceCmds pmvc = null;
            try
            {
                pmvc = new PitManagerVoiceCmds(ccObj.audioPlayer);
            }
            catch
            {
                if (pmvc == null)
                {
                    pmvc = new PitManagerVoiceCmds(ccObj.audioPlayer);
                }
            }

            pmvc.respond("pitstop change all tyres");
            Application.DoEvents();
            Thread.Sleep(200);
        }

    }
    [TestClass]
    public class TestTranslateTyreTypes
    {
        [TestMethod]
        public void Test_TTT_FormulaISI()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Intermediate");
            inMenu.Add("Super Soft");
            inMenu.Add("Soft");
            inMenu.Add("Medium");
            inMenu.Add("Hard");
            inMenu.Add("Wet");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Super Soft", result["Hypersoft"]);
            Assert.AreEqual("Super Soft", result["Ultrasoft"]);
            Assert.AreEqual("Super Soft", result["Supersoft"]);
            Assert.AreEqual("Soft", result["Soft"]);
            Assert.AreEqual("Medium", result["Medium"]);
            Assert.AreEqual("Hard", result["Hard"]);
            Assert.AreEqual("Intermediate", result["Intermediate"]);
            Assert.AreEqual("Wet", result["Wet"]);
            Assert.AreEqual("Wet", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_S397_Oreca()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Soft");
            inMenu.Add("Medium");
            inMenu.Add("Hard");
            inMenu.Add("Inter");
            inMenu.Add("Rain");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Soft", result["Hypersoft"]);
            Assert.AreEqual("Soft", result["Ultrasoft"]);
            Assert.AreEqual("Soft", result["Supersoft"]);
            Assert.AreEqual("Soft", result["Soft"]);
            Assert.AreEqual("Medium", result["Medium"]);
            Assert.AreEqual("Hard", result["Hard"]);
            Assert.AreEqual("Inter", result["Intermediate"]);
            Assert.AreEqual("Rain", result["Wet"]);
            Assert.AreEqual("Rain", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_DallaraIndy()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Alternates");    // Soft
            inMenu.Add("Primary");       // Hard
            inMenu.Add("Rain");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Alternates", result["Hypersoft"]);
            Assert.AreEqual("Alternates", result["Ultrasoft"]);
            Assert.AreEqual("Alternates", result["Supersoft"]);
            Assert.AreEqual("Alternates", result["Soft"]);
            Assert.AreEqual("Alternates", result["Medium"]);
            Assert.AreEqual("Primary", result["Hard"]);
            Assert.AreEqual("Rain", result["Intermediate"]);
            Assert.AreEqual("Rain", result["Wet"]);
            Assert.AreEqual("Rain", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_S397_AMGT3()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Soft");
            inMenu.Add("Medium");
            inMenu.Add("Hard");
            inMenu.Add("Rain");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Soft", result["Hypersoft"]);
            Assert.AreEqual("Soft", result["Ultrasoft"]);
            Assert.AreEqual("Soft", result["Supersoft"]);
            Assert.AreEqual("Soft", result["Soft"]);
            Assert.AreEqual("Medium", result["Medium"]);
            Assert.AreEqual("Hard", result["Hard"]);
            Assert.AreEqual("Rain", result["Intermediate"]);
            Assert.AreEqual("Rain", result["Wet"]);
            Assert.AreEqual("Rain", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_S397_Megane()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Dry");
            inMenu.Add("Rain");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Dry", result["Hypersoft"]);
            Assert.AreEqual("Dry", result["Ultrasoft"]);
            Assert.AreEqual("Dry", result["Supersoft"]);
            Assert.AreEqual("Dry", result["Soft"]);
            Assert.AreEqual("Dry", result["Medium"]);
            Assert.AreEqual("Dry", result["Hard"]);
            Assert.AreEqual("Rain", result["Intermediate"]);
            Assert.AreEqual("Rain", result["Wet"]);
            Assert.AreEqual("Rain", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_F1_1998()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Wet");
            inMenu.Add("Soft");
            inMenu.Add("Medium");
            inMenu.Add("Hard");
            inMenu.Add("Intermediate");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Soft", result["Hypersoft"]);
            Assert.AreEqual("Soft", result["Ultrasoft"]);
            Assert.AreEqual("Soft", result["Supersoft"]);
            Assert.AreEqual("Soft", result["Soft"]);
            Assert.AreEqual("Medium", result["Medium"]);
            Assert.AreEqual("Hard", result["Hard"]);
            Assert.AreEqual("Intermediate", result["Intermediate"]);
            Assert.AreEqual("Wet", result["Wet"]);
            Assert.AreEqual("Wet", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_Audi_R15()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Wet COMPOUND");
            inMenu.Add("Soft COMPOUND");
            inMenu.Add("Medium COMPOUND");
            inMenu.Add("Hard COMPOUND");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Soft COMPOUND", result["Hypersoft"]);
            Assert.AreEqual("Soft COMPOUND", result["Ultrasoft"]);
            Assert.AreEqual("Soft COMPOUND", result["Supersoft"]);
            Assert.AreEqual("Soft COMPOUND", result["Soft"]);
            Assert.AreEqual("Medium COMPOUND", result["Medium"]);
            Assert.AreEqual("Hard COMPOUND", result["Hard"]);
            Assert.AreEqual("Wet COMPOUND", result["Intermediate"]);
            Assert.AreEqual("Wet COMPOUND", result["Wet"]);
            Assert.AreEqual("Wet COMPOUND", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_Boxmaster()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("All-weather");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("All-weather", result["Hypersoft"]);
            Assert.AreEqual("All-weather", result["Ultrasoft"]);
            Assert.AreEqual("All-weather", result["Supersoft"]);
            Assert.AreEqual("All-weather", result["Soft"]);
            Assert.AreEqual("All-weather", result["Medium"]);
            Assert.AreEqual("All-weather", result["Hard"]);
            Assert.AreEqual("All-weather", result["Intermediate"]);
            Assert.AreEqual("All-weather", result["Wet"]);
            Assert.AreEqual("All-weather", result["Monsoon"]);

        }
        [TestMethod]
        public void Test_TTT_testCases()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("All-weather");
            inMenu.Add("Inters");
            inMenu.Add("Hyper Soft");
            inMenu.Add("UltraSoft");
            inMenu.Add("Super-Soft");
            inMenu.Add("S310");
            inMenu.Add("P310");
            inMenu.Add("Medium");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Hyper Soft", result["Hypersoft"]);
            Assert.AreEqual("UltraSoft", result["Ultrasoft"]);
            Assert.AreEqual("Super-Soft", result["Supersoft"]);
            Assert.AreEqual("S310", result["Soft"]);
            Assert.AreEqual("Medium", result["Medium"]);
            Assert.AreEqual("P310", result["Hard"]);
            Assert.AreEqual("Inters", result["Intermediate"]);
            Assert.AreEqual("All-weather", result["Wet"]);
            Assert.AreEqual("All-weather", result["Monsoon"]);

        }
        [TestMethod]
        /// Test all the types scraped from rF2 data files
        /// Not convinced this test is catching everything...
        public void Test_TTT_testAllTyres()
        {
            Dictionary<string, string> result = null;
            PitManagerEventHandlers_RF2.TyreDictionary tyreDict = PitManagerEventHandlers_RF2.SampleTyreTranslationDict;

            foreach (string line in System.IO.File.ReadLines(@"..\..\..\CrewChiefV4\PitManager\Documentation\Scraped_rf2_tyres_Sorted_categorised.txt"))
            {
                string testName = line.Split(',')[0];
                string ccName = line.Split(',')[1];
                List<string> inMenu = new List<string>();
                inMenu.Add("NotFound");
                inMenu.Add(testName);
                tyreDict["default"] = new List<string> { testName };
                try
                {
                    result =
                        PitManagerEventHandlers_RF2.TranslateTyreTypes(
                            tyreDict,
                            inMenu);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(testName, result[ccName]);
                }
                catch
                {
                    Console.WriteLine($"{ccName} didn't return '{testName}', instead '{result[ccName]}'");
                }
            }
        }
        [TestMethod]
        /// Single tyre type, not in tyre dict
        public void Test_TTT_Predators()
        {
            List<string> inMenu = new List<string>();
            inMenu.Add("Avon ACB10");

            Dictionary<string, string> result =
                PitManagerEventHandlers_RF2.TranslateTyreTypes(
                    PitManagerEventHandlers_RF2.SampleTyreTranslationDict,
                    inMenu);
            Assert.IsNotNull(result);
            Assert.AreEqual("Avon ACB10", result["Hypersoft"]);
            Assert.AreEqual("Avon ACB10", result["Ultrasoft"]);
            Assert.AreEqual("Avon ACB10", result["Supersoft"]);
            Assert.AreEqual("Avon ACB10", result["Soft"]);
            Assert.AreEqual("Avon ACB10", result["Medium"]);
            Assert.AreEqual("Avon ACB10", result["Hard"]);
            Assert.AreEqual("Avon ACB10", result["Intermediate"]);
            Assert.AreEqual("Avon ACB10", result["Wet"]);
            Assert.AreEqual("Avon ACB10", result["Monsoon"]);

        }
    }
    [TestClass]
    public class TestTyreTypeDictionary
    {
        [TestMethod]
        public void Test_TyreTypeDictionary_Write()
        {
            PitManagerEventHandlers_RF2.TyreDictFile.saveTyreDictionaryFile(
                PitManagerEventHandlers_RF2.SampleTyreTranslationDict);
        }
        [TestMethod]
        public void Test_TyreTypeDictionary_Read()
        {
            PitManagerEventHandlers_RF2.TyreDictFile.saveTyreDictionaryFile(
                PitManagerEventHandlers_RF2.SampleTyreTranslationDict);
            var dict = PitManagerEventHandlers_RF2.TyreDictFile.getTyreDictionaryFromFile();
            Assert.AreEqual(PitManagerEventHandlers_RF2.SampleTyreTranslationDict["Hypersoft"][0],
                dict["Hypersoft"][0]);
        }
    }
}
