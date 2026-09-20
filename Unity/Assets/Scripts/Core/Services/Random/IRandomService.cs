namespace Gamelab.Services.Random
{
    public interface IRandomService
    {
        public double SampleGaussian(double mu, double sigma);

        /// <summary>Uniform in [0, 1). Equivalent of Src Random.NextSingle().</summary>
        public float NextSingle();
    }
}
