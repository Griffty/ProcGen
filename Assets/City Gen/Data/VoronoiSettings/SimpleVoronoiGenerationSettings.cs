using System;
using UnityEngine;
using Random = UnityEngine.Random;
using SF = UnityEngine.SerializeField;
namespace City_Gen.Data.VoronoiSettings
{
    [CreateAssetMenu(menuName = "Generation/Voronoi/SimpleSettings", fileName = "Simple Voronoi Settings", order = 0)]
    public class SimpleVoronoiGenerationSettings : VoronoiGenerationSettings
    {
        [SF] private int polygonsAmount;
        protected override void GeneratePoints()
        {
            vPy = new int[polygonsAmount];
            vPx = new int[polygonsAmount];

            float threshold = MapSize / 1.4f / polygonsAmount ;
            for (int i = 0; i < polygonsAmount; i++)
            {
                vPx[i] = Random.Range(0, MapSize);
                vPy[i] = Random.Range(0, MapSize);

                for (int j = 0; j < 15000; j++)
                {
                    if (ValidPoint(i, j))
                    {
                        break;
                    }

                    vPx[i] = Random.Range(0, MapSize);
                    vPy[i] = Random.Range(0, MapSize);
                }
            }
            bool ValidPoint(int i, int a)
            {
                for (int j = 0; j < i; j++)
                {
                    a = 1 - a / 10000;
                    if (Math.Abs(vPx[i] - vPx[j]) < threshold * a && Math.Abs(vPy[i] - vPy[j]) < threshold * a )
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public override void SetCellsAmount(int amount)
        {
            polygonsAmount = amount * amount;
        }
    }
}
