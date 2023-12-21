using CrewChiefV4.PitManager;
using NUnit.Framework;

using Assert = NUnit.Framework.Assert;

namespace UnitTest.Misc
{
    [TestFixture]
    public class TestNumberParser
    {
        [TestCase("box change tyre pressures twenty one point four five bar", 21.45f)]
        [TestCase("box change tyre pressures twenty one point fourty five psi", 21.45f)]
        [TestCase("box change tyre pressures zero point four five bar", 0.45f)]
        [TestCase("box change tyre pressures point four five bar", 0.45f)]
        [TestCase("box change tyre pressures nought point zero psi", 0.0f)]
        [TestCase("box change tyre pressures thirty point four five bar", 30.45f)]
        [TestCase("box change tyre pressures thirty point twenty bar", 30.20f)]
        [TestCase("box change tyre pressures thirty point bar", 30.0f)]
        [TestCase("box change tyre pressures oh point zero psi", 0.0f)]
        [TestCase("pit stop change tyre pressures thirty five psi", 35.0f)]
        [TestCase("pit stop change tyre pressures one hundred and thirty five psi", 135.0f)]
        [TestCase("pit stop change tyre pressures hundred psi", -1.0f)]
        [TestCase("pitstop add ten liters", 10.0f)]
        [TestCase("pit stop clear tyres", -1.0f)]
        [TestCase("twenty twenty two", 2022.0f)]
        [TestCase("point twenty twenty two", 0.2022f)]
        public void TestFloatsInStrings(string text, float result)
        {
            float parsed = CrewChiefV4.NumberProcessing.SpokenNumberParser.Parse(text);
            Assert.AreEqual(result, parsed);
            //parsed = CrewChiefV4.NumberProcessing.SpokenNumberParser.ParseSM(text);
            //Assert.AreEqual(result, parsed);
        }
#if onlyWorkInBetaBranch
        [TestCase("pitstop add ten liters", 10)]
        [TestCase("pitstop fill to twenty gallons", 20)]
        public void TestPitNumberHandling_processNumber(string text, int result)
        {
            int parsed = PitNumberHandling.processNumber(text);
            Assert.AreEqual(result, parsed);
        }
#endif
    }
}
