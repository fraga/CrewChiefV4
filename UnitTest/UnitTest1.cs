using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CrewChiefV4;
using CrewChiefV4.PitManager;
using System.Resources;

namespace UnitTest
{
    [TestClass]
    public class TestPitManager
    {
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
        public void Test3()
        {
            bool result;
            var pmh = new PitManager();
#if false // attempt to fake a whole CC
            var wh = new DummyForm();
            var controllerConfiguration = new ControllerConfiguration();
            CrewChief cc = new CrewChief(controllerConfiguration);
            //cc.gameDefinition =
#endif
            result = pmh.EventHandler(PitManagerEvent.AeroFrontSetToX);
            Assert.IsTrue(result);
            result = pmh.EventHandler(PitManagerEvent.RepairFast);
            Assert.IsFalse(result);
        }
    }
}
