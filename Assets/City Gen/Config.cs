using System;
using City_Gen.Data.VoronoiSettings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace City_Gen
{
    [CreateAssetMenu(menuName = "Generation/Map Config", fileName = "Map Config", order = 0)]
    public class Config : ScriptableObject
    {
        public int Seed => seed;
        public int ActualCitySize => actualCitySize;
        public int CitySizeFactor => citySizeFactor;
        public int[] VPx => vPx;
        public int[] VPy => vPy;
        public int DistrictDistFromCorners => districtDistFromCorners;
        public int DistrictVertexDiffThreshold => districtVertexDiffThreshold;
        public float MinDistrictDistanceFromCenter { get; private set; }   
        public VoronoiGenerationSettings CityGenerationSettings => cityGenerationSettings;

        public float MaxNeighborhoodArea => maxNeighborhoodArea;
        public float MinNeighborhoodArea => minNeighborhoodArea;
        public float NeighborhoodVertexMergeDistance => neighborhoodVertexMergeDistance;
        public float MinTwistSplitAngle => minTwistSplitAngle;
        public float MinDirectSplitAngle => minDirectSplitAngle;
        public float MinTwistSplitPolygonDistance => minTwistSplitPolygonDistance;
        public float MinTwistSplitPointVertexDistance => minTwistSplitPointVertexDistance;
        public float MinPolygonSubSplitDistance => minPolygonSubSplitDistance;

        private static Config _instance;
        public static Config Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw new InvalidOperationException("Cannot get Config before it was set up.");
                }
                return _instance;
            }
        }
        public static bool IsSetUp => _instance != null;
    

        // City info
        [Header("City Info")]
        [SerializeField] private int seed;
        [SerializeField] private int actualCitySize;
    
        [Range(0,MaxSizeFactor)][SerializeField] private int citySizeFactor;

        // Voronoi Info
        [Header("Voronoi Info")]
        [SerializeField] private int[] vPx;
        [SerializeField] private int[] vPy;

        [SerializeField] private VoronoiGenerationSettings cityGenerationSettings;

        //BuildingGen info
    
        //DistrictGen info
        [Header("District Info")]
        [Range(0,1)][SerializeField] private float minDistrictDistanceFromCenter = 0.8f;
        [SerializeField] private int districtDistFromCorners = 10;
        [SerializeField] private int districtVertexDiffThreshold = 15;
    
        //StreetGen info
        [Header("Street Info")]
        [SerializeField] private float maxNeighborhoodArea = 10000;
        [SerializeField] private float minNeighborhoodArea = 1000;
        [SerializeField] private float neighborhoodVertexMergeDistance = 10;
        [SerializeField] private float minTwistSplitAngle = 80;
        [SerializeField] private float minDirectSplitAngle = 10;
        [SerializeField] private float minTwistSplitPolygonDistance = 22;
        [SerializeField] private float minTwistSplitPointVertexDistance = 10;
        [SerializeField] private float minPolygonSubSplitDistance = 30;

    
        private const int MaxSizeFactor = 20;
        public static void SetUpConfig(Config configScript, bool resetConfig)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Cannot set Config because it was already set up.");
            }
            if (configScript.seed == 0 || resetConfig)
            {
                configScript.seed = Random.Range(10000000, 100000000);
            }
        
        
            Random.InitState(configScript.seed);
            int citySize = configScript.citySizeFactor;
            if (citySize == 0)
            {
                citySize = Random.Range(1, MaxSizeFactor + 1);
            }

            int size = citySize * 155 + 250;
            int cells = (int)(2 + Mathf.Pow(citySize * 1.5f, 0.73f));//3 + (int)((citySize - 1) * 0.75f); 
        
            float rs = Random.Range(-10, 11) / 100f;
            size = (int)(size * (1 + rs));

            configScript.actualCitySize = size;
            configScript.citySizeFactor = citySize;
            configScript.MinDistrictDistanceFromCenter = configScript.actualCitySize * configScript.minDistrictDistanceFromCenter / 2;
        
            configScript.CityGenerationSettings.MapSize = configScript.actualCitySize;
            configScript.CityGenerationSettings.SetCellsAmount(cells);

            (configScript.vPx, configScript.vPy) = configScript.CityGenerationSettings.Generate();
        
            _instance = configScript;
        }

        public void Reset()
        {
            _instance = null;
        }
    
    }
}
