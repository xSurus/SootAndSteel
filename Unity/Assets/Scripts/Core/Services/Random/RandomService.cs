using System;

namespace Gamelab.Services.Random
{
    public class RandomService : IRandomService
    {
        // System.Random.Shared doesn't exist on netstandard2.1 (this project's compile
        // target), so new Random() is the only option here, not a stylistic choice.
        // If this service is ever instantiated more than once, note that Random.Shared
        // (were it available) is thread-safe and avoids correlated sequences between
        // instances created in the same clock tick; a plain new Random() has neither
        // guarantee.
        private System.Random random = new System.Random();

        public float NextSingle() => (float)random.NextDouble() * 0.99999994f; // keep the cast below 1.0

        public double SampleGaussian(double mu, double sigma)
        {
            double u1 = 1.0 - random.NextDouble();
            double u2 = 1.0 - random.NextDouble();

            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            return mu + sigma * (float)randStdNormal;
        }
    }
}
