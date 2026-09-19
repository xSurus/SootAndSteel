using NUnit.Framework;
using Gamelab.Services.Random;

namespace Gamelab.Tests.Bullets
{
    public class SpreadSamplingTests
    {
        [Test]
        public void SampleGaussian_WithZeroSpread_IsExactlyZero()
        {
            IRandomService random = new RandomService();

            double sample = random.SampleGaussian(0, 0f / 3f);

            Assert.AreEqual(0.0, sample);
        }
    }
}
