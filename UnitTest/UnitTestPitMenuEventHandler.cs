using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrewChiefV4;
using CrewChiefV4.PitManager;
using System.Resources;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

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
        public void Test_EventHandlerFuel()
        {
            bool result;
            var pmh = new PitManager();
            //pmh.AmountHandler(32);
            result = pmh.EventHandler(PitManagerEvent.FuelAddXlitres, "fifteen liters");
            Thread.Sleep(100);
        }

        [TestMethod]
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
        //[ExpectedException(typeof(TypeInitializationException),
        //    "text")]
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
}
