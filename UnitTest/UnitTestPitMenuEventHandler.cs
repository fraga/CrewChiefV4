﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrewChiefV4;
using CrewChiefV4.PitManager;
using System.Resources;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using PitMenuAPI;

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
        private static readonly PitMenuAbstractionLayer Pmal = new PitMenuAbstractionLayer();
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
    }
}
