using UnityEngine;
using Unity;

public static class NoiseGenerator
{
    // C# implementation for generating a Gaussian random number
    // using the Box-Muller transform (for a clean, reusable function)
    public static float GetGaussianNoise(float mean, float std)
    {
        // Two uniformly distributed random numbers between 0 and 1
        float u1 = 1.0f - Random.value;
        float u2 = 1.0f - Random.value;

        // Apply the Box-Muller transform
        float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

        // Scale and shift
        return mean + std * randStdNormal;
    }
    
    public static Vector3 GetGaussianVector3(float mean, float stdDev) {
        float noise1 = GetGaussianNoise(mean, stdDev);
        float noise2 = GetGaussianNoise(mean, stdDev);
        float noise3 = GetGaussianNoise(mean, stdDev);
        
        return new Vector3(noise1, noise2, noise3);
    }
}