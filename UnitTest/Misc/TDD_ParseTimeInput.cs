using CrewChiefV4.Events;
using NUnit.Framework;
using System;
using Assert = NUnit.Framework.Assert;

namespace UnitTest.Misc
{
    [TestFixture]
    public class TDD_ParseTimeInput
    {
        [Test]
        [TestCase("9:45PM", 21, 45)]
        [TestCase("9:45 PM", 21, 45)]
        [TestCase("9:45AM", 9, 45)]
        [TestCase("9:45  AM", 9, 45)]
        [TestCase("21:25", 21, 25)]
        [TestCase("9:45", 9, 45)]
        [TestCase("945", 0, 0)]
        public void test_ParseTimeInput(string input, int hour, int minute)
        {
            DateTime dt = AlarmClock.ParseTimeInput(input);
            Assert.AreEqual(hour, dt.Hour);
            Assert.AreEqual(minute, dt.Minute);
        }
    }
}
