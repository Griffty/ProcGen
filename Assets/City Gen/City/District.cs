using System;
using System.Collections.Generic;
using Griffty.Utility.Data;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.City
{
    [Serializable]
    public class District
    {
        [SerializeField] private DistrictType type;
        [SerializeField] private List<Vector2Int> districtBoundaries;
        [SerializeField] private List<Polygon> neighborhoods;
    
        [SerializeField] private Vector2Int center;
        [SerializeField] private float area;
        [SerializeField] private bool isClockwise;
        

        public List<Vector2Int> DistrictBoundaries
        {
            get => districtBoundaries;
            set
            {
                districtBoundaries = value;
                Recalculate();
            }
        }
    
        public List<Polygon> Neighborhoods
        {
            get => neighborhoods;
            set => neighborhoods = value;
        }
        
        public DistrictType Type
        {
            get => type;
            set => type = value;
        }
    

        public Vector2Int Center => center;
        public float Area => area;
        public bool IsClockwise => isClockwise;

        public District(List<Vector2Int> districtBoundaries)
        {
            this.districtBoundaries = districtBoundaries; 
            Neighborhoods = new List<Polygon>();
            Recalculate();
        }

        private void Recalculate()
        {       
            center = Vector2Int.RoundToInt(PolygonUtils.Centroid(districtBoundaries));
            area = PolygonUtils.Area(districtBoundaries);
            isClockwise = PolygonUtils.IsPolyClockwise(districtBoundaries);
        }
    }
}
