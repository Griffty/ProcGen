using System.Collections.Generic;
using System.Linq;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.Generators
{
    public class WallGenerator : CustomCityGenerator
    {
        protected override void UseGenerator(City.City city)
        {
            List<Vertex> wall = new List<Vertex>();
            Vertex startVertex = FindFurthest(PolygonUtils.PolygonsAsListOfVertices(city.CityPolygons), city.CityCenter);
            int lastVertex = startVertex.Index;
            Vertex currentVertex = startVertex;
            Vertex nextVertex;
            int i = 0;
            do
            {
                nextVertex = Vertex.FindByIndex(city.Vertices, currentVertex.ConnectedVertices.Where(v =>
                {
                    Vertex vertex = Vertex.FindByIndex(city.Vertices, v);
                    List<int> sharedPolygons = currentVertex.AdjacentPolygons
                        .Intersect(vertex.AdjacentPolygons)
                        .ToList();
                    int c = sharedPolygons.Intersect(city.CityPolygons.Select(p => p.Index)).Count();
                    return (sharedPolygons.Count == 2 &&
                            c == 1 && lastVertex != vertex.Index);
                }).FirstOrDefault());
                wall.Add(currentVertex);
                lastVertex = currentVertex.Index;
                currentVertex = nextVertex;
            }while (startVertex.Index != nextVertex.Index && i++ < 1000);

            if (i > 500)
            {
                Debug.LogError("wall iterations: " + i);
            }

            city.Wall = wall;
        }

        public override void Draw(SimpleDrawer drawer, City.City city)
        {
            drawer.AddMultipleLines(PolygonUtils.VerticesAsListOfCoordinates(city.Wall), new Color(0.5f, 0.3f, 0.1f), 3);
            
        }

        private Vertex FindFurthest(List<Vertex> cityVertices, Vector2 center)
        {
            Vertex furthestVertex = null;
            float maxDistance = 0;
            foreach (var vertex in cityVertices)
            {
                float distance = Vector2.Distance(vertex.Position, center);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestVertex = vertex;
                }
            }

            return furthestVertex;
        }       
    }
}