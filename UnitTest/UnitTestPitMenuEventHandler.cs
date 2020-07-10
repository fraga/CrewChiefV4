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
        public void Test_EventHandler()
        {
            bool result;
            var pmh = new PitManager();

            result = pmh.EventHandler(PitManagerEvent.AeroFrontSetToX);
            Assert.IsTrue(result);
            result = pmh.EventHandler(PitManagerEvent.RepairFast);
            Assert.IsFalse(result);

            result = pmh.EventHandler(PitManagerEvent.TyreCompoundWet);
            Assert.IsTrue(result);

            pmh.AmountHandler(32);
            result = pmh.EventHandler(PitManagerEvent.FuelAddXlitres);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Test_EventHandlerUndo()
        {
            bool result;
            var pmh = new PitManager();

            result = pmh.EventHandler(PitManagerEvent.TyreChangeNone);
            Assert.IsTrue(result);

            pmh.AmountHandler(0);
            result = pmh.EventHandler(PitManagerEvent.FuelAddXlitres);
            Assert.IsTrue(result);
        }

        [TestClass]
        public class TestPitManagerInCC
        {
            private static MainWindow hwnd;
            public CrewChief ccObj;
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
            public void TestVoice()
            {
                bool result;
                var pmh = new PitManager();

                //PitManagerVoiceCmds.respond("pitstop change all tyres");

            }

        }
    }
}
