using System;
using System.Linq;
using Griffty.Utility.Static.GMath;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace City_Gen.Data.VoronoiSettings
{
    [CreateAssetMenu(menuName = "Generation/Voronoi/GridSettings", fileName = "Voronoi Grid Settings", order = 0)]
    public class GridVoronoiGenerationSettings : VoronoiGenerationSettings
    {
        [SF] private int gridSize;
        [SF] private int minPointsDistance;
        [SF] private int maxRetries;
        [Range(0,1)][SF] private float distributionModifier;
        protected override void GeneratePoints()
        {
            int reTryCount = 0;
            vPy = new int[gridSize * gridSize];
            vPx = new int[gridSize * gridSize];
        
            int cellSize = MapSize / gridSize;
            for (int x = 0; x < gridSize; x++)
            {
                for(int y = 0; y < gridSize; y++)
                {
                    Roll(x, y, cellSize);

                    while (!ValidPoint(x * gridSize + y))
                    {
                        if (reTryCount > maxRetries)
                        {
                            Debug.LogWarning("Rolling again");
                            GeneratePoints();
                            throw new Exception("Could not generate valid points");
                        }
                        reTryCount++;
                        Roll(x, y, cellSize);
                    }
                }
            }
        }

        private void Roll(int x, int y, int cellSize)
        {
            vPy[x * gridSize + y] = RandomUtils.NextGaussianInt(0, cellSize, distributionModifier) + y * cellSize;
            vPx[x * gridSize + y] = RandomUtils.NextGaussianInt(0, cellSize, distributionModifier) + x * cellSize;
        }
    
        private bool ValidPoint(int index)
        {
            return vPx.Where((t, i) => Vector2.Distance(new Vector2(t, vPy[i]), new Vector2(vPx[index], vPy[index])) < minPointsDistance).Count() == 1;
        }

        public override void SetCellsAmount(int amount)
        {
            gridSize = amount;
        }
    }
}
