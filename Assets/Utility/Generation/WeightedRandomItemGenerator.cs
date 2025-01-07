using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Griffty.Utility.Generation
{
    public class WeightedRandomItemGenerator<T>
    {
        private readonly SortedList<float, T> items = new();
        private int totalWeight;

        public T GetRandomItem()
        {
            if (items.Count == 0)
                throw new InvalidOperationException("No items have been added to the generator.");

            float randomWeight = Random.Range(0, totalWeight);
            int index = BinarySearchForWeight(randomWeight);
            return items.Values[index];
        }

        public void AddItem(T item, int weight)
        {
            if (weight <= 0)
                throw new ArgumentException("Weight must be greater than 0");

            totalWeight += weight;
            items.Add(totalWeight, item);
        }
    
        private int BinarySearchForWeight(float targetWeight)
        {
            int left = 0;
            int right = items.Count - 1;

            while (left <= right)
            {
                int middle = left + (right - left) / 2;
                float middleWeight = items.Keys[middle];

                if (Math.Abs(middleWeight - targetWeight) < 0.1)
                {
                    return middle;
                }

                if (middleWeight < targetWeight)
                {
                    left = middle + 1;
                }
                else
                {
                    right = middle - 1;
                }
            }

            return left;
        }
    }
}