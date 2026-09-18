// Unity/Assets/Tests/EditMode/RandomServiceTests.cs
using NUnit.Framework;
using Gamelab.Services.Random;

public class RandomServiceTests
{
    [Test]
    public void SampleGaussian_WithZeroSigma_ReturnsMu()
    {
        var service = new RandomService();

        double result = service.SampleGaussian(5.0, 0.0);

        Assert.AreEqual(5.0, result, 0.0001);
    }
}
