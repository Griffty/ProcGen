using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Griffty.Utility.Static.GMath
{
    public static class MathUtils
    {
        public static int RandomModifier(int min, int max, Random random = null)
        {
            random ??= new Random();
            return 1 + RandomSign(random) * random.Next(min, max);
        }
        public static int RandomSign(Random random = null)
        {
            random ??= new Random();
            return random.Next(0, 2) * 2 - 1;
        }
        public static float SqDistance(Vector2 first, Vector2 second)
        {
            return SqDistance(first.x, first.y, second.x, second.y);
        }

        public static float SqDistance(float x1,  float y1, float x2, float y2)
        {
            float d;
            d = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
            return d;
        }
        public static float Distance(Vector2 first, Vector2 second)
        {
            return Vector2.Distance(first, second);
        }
        
        public static float Distance(float x1, float x2, float y1, float y2)
        {
            return Distance(new Vector2(x1, y1), new Vector2(x2, y2));
        }
        public static float Sum(List<float> numbers)
        {
            float sum = 0;
            foreach (var num in numbers)
                sum += num;
            return sum;
        }
    }
}