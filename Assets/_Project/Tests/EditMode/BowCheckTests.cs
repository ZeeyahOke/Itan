using Itan.Detection;
using NUnit.Framework;

namespace Itan.Tests
{
    public class BowCheckTests
    {
        [Test]
        public void BowHeldLongEnough_Passes()
        {
            var bow = new BowCheck(25f, 0.5f);
            Assert.IsFalse(bow.Update(30f, 0.3f));
            Assert.IsTrue(bow.Update(30f, 0.3f));
        }

        [Test]
        public void ShallowNod_DoesNotPass()
        {
            var bow = new BowCheck(25f, 0.5f);
            Assert.IsFalse(bow.Update(15f, 2f));
        }

        [Test]
        public void LiftingEarly_RestartsTheCount()
        {
            var bow = new BowCheck(25f, 0.5f);
            bow.Update(30f, 0.4f);
            bow.Update(5f, 0.1f);           // head comes up
            Assert.IsFalse(bow.Update(30f, 0.4f));
        }
    }
}
