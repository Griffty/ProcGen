using System;
using System.Linq;
using Griffty.Utility.Static.GMath;
using UnityEngine;
using SF = UnityEngine.SerializeField;

namespace City_Gen.Data.VoronoiSettings
{
    [CreateAssetMenu(menuName = "Generation/Voronoi/GradientGridSettings", fileName = "Voronoi Gradient Grid Settings", order = 0)]
    public class GradientGridVoronoiGenerationSettings : VoronoiGenerationSettings
    {
        [SF] private int gridSize;
        [SF] private int minPointsDistance;
        [SF] private int maxRetries;
        [Range(0,1)][SF] private float distributionModifier;
        [Range(0,1)][SF] private float innerGradientModifier;
        [Range(0,1)][SF] private float outerGradientModifier;
    
        private float maxDist;
        protected override void GeneratePoints()
        {
            maxDist = Vector2.Distance(new (0,0), new (gridSize / 2f, gridSize / 2f));
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
            float distFromCenter = Vector2.Distance(new (x, y), new (gridSize / 2f, gridSize / 2f));
            float modifier = distFromCenter / maxDist * (outerGradientModifier - innerGradientModifier) + innerGradientModifier;
        
            vPy[x * gridSize + y] = RandomUtils.NextGaussianInt(0, cellSize, distributionModifier * modifier) + y * cellSize;
            vPx[x * gridSize + y] = RandomUtils.NextGaussianInt(0, cellSize, distributionModifier * modifier) + x * cellSize;
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
