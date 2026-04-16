namespace Gamelab.Services.Random;

public interface IRandomService
{
    public double SampleGaussian(double mu, double sigma);
}