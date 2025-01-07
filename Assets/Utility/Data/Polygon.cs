using System;
using System.Collections.Generic;
using System.Linq;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace Griffty.Utility.Data
{
    [Serializable]
    public class Polygon
    {
        private const float InsetDist = 4;
        private static int _nextAvailableIndex = 0;
        public int Index => index;
        public List<Vertex> Vertices => vertices;
        public List<Vector2Int> VerticesAsCoordinates => PolygonUtils.VerticesAsListOfCoordinates(Vertices);
        
        [SerializeField] private int index;
        [SerializeField] private List<Vertex> vertices;

        public List<Vector2Int> InnerVertices => PolygonUtils.CalculateInnerPolygonParallel(PolygonUtils.VerticesAsListOfCoordinates(Vertices), InsetDist);
        public Vector2Int Center => Vector2Int.RoundToInt(PolygonUtils.Centroid(PolygonUtils.VerticesAsListOfCoordinates(Vertices)));
        public bool IsClockwise => PolygonUtils.IsPolyClockwise(PolygonUtils.VerticesAsListOfCoordinates(Vertices));
        public float Area => PolygonUtils.Area(PolygonUtils.VerticesAsListOfCoordinates(Vertices));
        public static int NextAvailableIndex => _nextAvailableIndex;
        public Polygon(List<Vertex> vertices, int index)
        {
            this.vertices = vertices;
            this.index = index;
            if (_nextAvailableIndex <= index)
            {
                _nextAvailableIndex = index + 1;
            }
        }
        
        public static Polygon FindByIndex(List<Polygon> polygons, int i)
        {
            return polygons.FirstOrDefault(polygon => polygon.Index == i);
        }
    }
}