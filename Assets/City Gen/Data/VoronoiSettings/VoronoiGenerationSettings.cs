using UnityEngine;
using SF = UnityEngine.SerializeField;
namespace City_Gen.Data.VoronoiSettings
{
    public abstract class VoronoiGenerationSettings : ScriptableObject
    {
        [SF] protected int[] vPx;
        [SF] protected int[] vPy;
    
        [SF] protected int mapSize;
        public (int[], int[]) Points => (vPx, vPy);

        public (int[], int[]) Generate()
        {
            GeneratePoints();
            return Points;
        }
    
        protected abstract void GeneratePoints();
    
        public abstract void SetCellsAmount(int amount);
        public int MapSize
        {
            get => mapSize;
            set => mapSize = value;
        }
    }
}