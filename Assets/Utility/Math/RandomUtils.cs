using System;
using Random = UnityEngine.Random;

namespace Griffty.Utility.Static.GMath
{
    public class RandomUtils
    {
        public static int NextCentralLimitInt(int iter, int max)
        {
            return NextCentralLimitInt(iter, 0 , max);
        }
        
        public static int NextCentralLimitInt(int iter, int min, int max)
        {
            if (max < min)
            {
                (max, min) = (min, max);
            }
            int diff = max - min;
            int num = 0;
            for (int i = 0; i < iter; i++)
            {
                num += Random.Range(0, diff);
            }

            return num/iter + min;
        }
        
        public static int NextGaussianInt(int min, int max, float modifier)
        {
            if (max < min)
            {
                (max, min) = (min, max);
            }

            int diff = max - min;
            double mean =  diff / 2.0;
            double standardDeviation = modifier * diff;
            double gaussValue;
            do
            {
                gaussValue = mean + Gaussian() * standardDeviation;
            } while (gaussValue < 0 || gaussValue >= diff);

            return min + (int)gaussValue;
        }
        
        public static double Gaussian()
        {
            double u1 = 1.0 - Random.value;
            double u2 = 1.0 - Random.value;
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }
    }
}