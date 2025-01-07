using System.Collections.Generic;
using System.Linq;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.Generators
{
    public class CityGenerator : CustomCityGenerator
    {
        protected override void UseGenerator(City.City city)
        {
            (city.Boundaries, city.CityCenter) = PolygonUtils.FindBoundingBox(PolygonUtils.VerticesAsListOfCoordinates(city.Vertices));
            FindCityParameters(city);
            PolygonsStepExtraction(city);
        }

        public override void Draw(SimpleDrawer drawer, City.City city)
        {
            drawer.AddRectangle(city.Boundaries, new Color(0.5f, 0.2f,0.2f));
            foreach (var polygon in city.Polygons)
            {
                drawer.AddMultipleLines(PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices), Color.black, 1, true, -1);
            }
            drawer.AddMultiplePoints(PolygonUtils.VerticesAsListOfCoordinates(city.Vertices), Color.red, 1, 1);
        }

        private void FindCityParameters(City.City city)
        {
            int minX = int.MaxValue, maxX = 0, minY = int.MaxValue, maxY = 0;
            foreach (var vertex in city.Vertices)
            {
                if (vertex.Position.x > maxX) maxX = vertex.Position.x;
                if (vertex.Position.y > maxY) maxY = vertex.Position.y;
                if (vertex.Position.x < minX) minX = vertex.Position.x;
                if (vertex.Position.y < minY) minY = vertex.Position.y;
            }

            city.Boundaries = new Vector4(minX, minY, maxX, maxY);
            city.CityCenter = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        }

        private void PolygonsStepExtraction(City.City city)
        {
            List<int> parsedPolygons = new List<int>();

            for (var i = 0; i < city.Vertices.Count; i++)
            {
                var vertex = city.Vertices[i];
                foreach (int connectedIndex in vertex.ConnectedVertices)
                {
                    List<int> sharedPolygons = vertex.AdjacentPolygons
                        .Intersect(Vertex.FindByIndex(city.Vertices, connectedIndex).AdjacentPolygons)
                        .ToList();

                    foreach (int polygonIndex in sharedPolygons)
                    {
                        if (parsedPolygons.Contains(polygonIndex))
                        {
                            continue;
                        }

                        if (TryConstructPolygon(vertex, polygonIndex, city.Vertices, out var region))
                        {
                            city.Polygons.Add(new Polygon(region, polygonIndex));
                            parsedPolygons.Add(polygonIndex);
                        }
                    }
                }
            }

            city.CityPolygons = city.Polygons.Where(p =>
                Vector2.Distance(p.Center, city.CityCenter) < Config.Instance.MinDistrictDistanceFromCenter).ToList();
        }

        private bool TryConstructPolygon(Vertex startVertex, int polygonIndex, List<Vertex> vertices,
            out List<Vertex> region)
        {
            region = new List<Vertex>();
            Vertex currentPoint = startVertex;
            region.Add(currentPoint);

            Vertex nextPoint = GetNextPolygonVertex(currentPoint, polygonIndex, vertices, null);
            int i = 0;

            while (nextPoint != null && nextPoint != startVertex && i < vertices.Count)
            {
                i++;
                region.Add(nextPoint);

                Vertex lastPoint = currentPoint;
                currentPoint = nextPoint;
                nextPoint = GetNextPolygonVertex(currentPoint, polygonIndex, vertices, lastPoint);

                if (nextPoint == startVertex)
                {
                    return region.Count > 2; 
                }
            }

            if (i > 100)
            {
                Debug.Log("polygons iterations: " + i);
            }

            region.Clear();
            return false;
        }

        private Vertex GetNextPolygonVertex(Vertex currentVertex, int polygonIndex, List<Vertex> vertices,
            Vertex excludeVertex)
        {
            return vertices
                .FirstOrDefault(vertex => vertex != excludeVertex &&
                                          vertex.AdjacentPolygons.Contains(polygonIndex) &&
                                          currentVertex.ConnectedVertices.Contains(vertex.Index));
        }
    }
}