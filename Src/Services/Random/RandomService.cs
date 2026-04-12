using System;

namespace Gamelab.Services.Random;

public class RandomService : IRandomService
{
    private System.Random random = System.Random.Shared;
    
    public double SampleGaussian(double mu, double sigma)
    {
        double u1 = 1.0 - random.NextDouble();
        double u2 = 1.0 - random.NextDouble();
    
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); 
    
        return mu + sigma * (float)randStdNormal;
    }
}