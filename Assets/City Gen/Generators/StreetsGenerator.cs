using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using City_Gen.City;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Patterns;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.Generators
{
    public class StreetsGenerator : CustomCityGenerator
    {
        protected override void UseGenerator(City.City city)
        {
            int uniquePolygonIndex = 0;
            Parallel.ForEach(city.Districts , district =>
            {
                HashSet<Polygon> failedPolygons = new HashSet<Polygon>();
                Polygon newPolygon = CreateNewPolygon(district, ref uniquePolygonIndex);
                (Polygon poly1, Polygon poly2) = TwistPolygonSplit(newPolygon, ref uniquePolygonIndex);
                if (poly1 == null || poly2 == null)
                {
                    Debug.Log("Split failed");
                    return;
                }
                
                district.Neighborhoods.Add(poly1);
                district.Neighborhoods.Add(poly2);

                int i = 0;
                while (i++ < 400)
                {
                    List<Polygon> polygonsToSplit = district.Neighborhoods.Where(neighborhood => neighborhood.Area > Config.Instance.MaxNeighborhoodArea && !failedPolygons.Contains(neighborhood)).ToList();
                    if (polygonsToSplit.Count == 0)
                    {
                        break;
                    }
                    
                    foreach (var polygon in polygonsToSplit)
                    {
                        (Polygon newPoly1, Polygon newPoly2) = TwistPolygonSplit(polygon, ref uniquePolygonIndex); //(Polygon newPoly1, Polygon newPoly2) = i % 2 == 0 ? TwistPolygonSplit(polygon, ref uniquePolygonIndex) : DirectPolygonSplit(polygon, ref uniquePolygonIndex);
                        
                        if (newPoly1 == null || newPoly2 == null)
                        {
                            failedPolygons.Add(polygon);
                            Debug.Log("Split failed");
                            MultipleStaticInstances<SimpleDrawer>.GetInstance("main").AddPoint(polygon.Center, Color.magenta, 2);
                        }
                        else
                        {
                            district.Neighborhoods.Remove(polygon);
                            district.Neighborhoods.Add(newPoly1);
                            district.Neighborhoods.Add(newPoly2);
                        }
                    }
                }
            });
            
            PostProcessPolygons(city, ref uniquePolygonIndex);
        }

        private Polygon CreateNewPolygon(District district, ref int uniquePolygonIndex)
        {
            List<Vertex> vertices = new List<Vertex>();
            int newIndex = 0;

            for (var i = 0; i < district.DistrictBoundaries.Count; i++)
            {
                var boundary = district.DistrictBoundaries[i];
                Vertex newVertex = new Vertex(boundary, newIndex, new List<int> {(i - 1 + district.DistrictBoundaries.Count) % district.DistrictBoundaries.Count, (i + 1) % district.DistrictBoundaries.Count });
                vertices.Add(newVertex);
                newIndex++;
            }

            return new Polygon(vertices, uniquePolygonIndex++);
        }

        private (Polygon, Polygon) TwistPolygonSplit(Polygon polygon, ref int uniquePolygonIndex, List<int> preUsedEdges = null)
        {
            List<EdgeCombo> edgeCombos = new List<EdgeCombo>(); //todo: replace with HashSet for faster lookup
            List<int> used = preUsedEdges != null ? new List<int>(preUsedEdges) : new List<int>();
            
            int edgeIndex;
            Vector2Int splitPoint;
            Vector2 normal;
            
            int newEdgeIndex = -1;
            Vector2Int newSplitPoint = Vector2Int.zero;
            Vector2 newNormal;
            
            Vector2Int intersection = Vector2Int.zero;
            bool valid = false;
            do
            {
                (edgeIndex, splitPoint, normal) = PickNewEdge(polygon, used);
                List<int> newUsed = new List<int>{edgeIndex};
                if (edgeIndex == -1)
                {
                    break;
                }
                do
                {
                    (newEdgeIndex, newSplitPoint, newNormal) = PickNewEdge(polygon, newUsed);
                    if (edgeCombos.Contains(new EdgeCombo(edgeIndex, newEdgeIndex)))
                    {
                        continue;
                    }
                    if (newEdgeIndex == -1)
                    {
                        break;
                    }
                    PolygonUtils.SegmentIntersection(splitPoint - normal * 10, splitPoint + normal * 500,
                        newSplitPoint - newNormal * 10, newSplitPoint + newNormal * 500, out intersection);
                    edgeCombos.Add(new EdgeCombo(edgeIndex, newEdgeIndex));
                    valid = ValidIntersection(polygon, intersection, splitPoint, normal, newSplitPoint, newNormal);
                } while (!valid);
            } while (!valid);

            if (!valid)
            {
                return DirectPolygonSplit(polygon, ref uniquePolygonIndex);
            }
            
            
            int vertexIndex = polygon.Vertices.Count;
            List<Vertex> polygon1Vertices = new List<Vertex>();
            List<Vertex> polygon2Vertices = new List<Vertex>();

            // Traverse through the original polygon vertices and assign them to the two new polygons
            bool addingToPolygon1 = true;

            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                // Add the current vertex to the appropriate polygon
                if (addingToPolygon1)
                {
                    polygon1Vertices.Add(polygon.Vertices[i]);
                }
                else
                {
                    polygon2Vertices.Add(polygon.Vertices[i]);
                }

                // Check if we've reached one of the split edges
                if (i == edgeIndex || i == newEdgeIndex) //todo: MAYBE make single vertex and assign for both polygons
                {
                    if (i == edgeIndex)
                    {
                        polygon1Vertices.Add(new Vertex(splitPoint, vertexIndex++, new List<int>()));
                        polygon2Vertices.Add(new Vertex(splitPoint, vertexIndex++, new List<int>()));
                    }
                    else
                    {
                        polygon1Vertices.Add(new Vertex(newSplitPoint, vertexIndex++, new List<int>()));
                        polygon2Vertices.Add(new Vertex(newSplitPoint, vertexIndex++, new List<int>()));
                    }
                    if (addingToPolygon1)
                    {
                        
                        polygon1Vertices.Add(new Vertex(intersection, vertexIndex++, new List<int>()));
                    }
                    else
                    {
                        polygon2Vertices.Add(new Vertex(intersection, vertexIndex++, new List<int>()));
                    }

                    // Switch to adding to the other polygon
                    addingToPolygon1 = !addingToPolygon1;
                }
            }

            
            Polygon polygon1 = new Polygon(polygon1Vertices, uniquePolygonIndex++);
            Polygon polygon2 = new Polygon(polygon2Vertices, uniquePolygonIndex++);

            if (polygon1.Area < Config.Instance.MinNeighborhoodArea || polygon2.Area < Config.Instance.MinNeighborhoodArea)
            {
                return TwistPolygonSplit(polygon, ref uniquePolygonIndex, used);
            }
            
            return (polygon1, polygon2);
        }

        private static (int newEdgeIndex, Vector2Int newSplitPoint, Vector2 newNormal) PickNewEdge(Polygon polygon, List<int> used)
        {
            if (used.Count == polygon.Vertices.Count)
            {
                return (-1, Vector2Int.zero, Vector2Int.zero);
            }
            int newEdgeIndex = PolygonUtils.LargestEdge(PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices), used, out float _);
            Vertex newStartVertex = polygon.Vertices[newEdgeIndex];
            Vertex newEndVertex = polygon.Vertices[(newEdgeIndex + 1) % polygon.Vertices.Count];
            Vector2Int newSplitPoint = (newStartVertex.Position + newEndVertex.Position) / 2;
            Vector2 newNormal = PolygonUtils.InnerEdgeNormal(newStartVertex.Position, newEndVertex.Position, PolygonUtils.IsPolyClockwise(PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices)));
            used.Add(newEdgeIndex);
            return (newEdgeIndex, newSplitPoint, newNormal);
        }

        private bool ValidIntersection(Polygon polygon, Vector2Int intersection, Vector2Int splitPoint, Vector2 normal, Vector2Int newSplitPoint, Vector2 newNormal)
        {
            if (!PolygonUtils.IsPointInsidePolygon(intersection, PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices)))
            {
                return false;
            }

            if (Mathf.Abs(Vector2Utils.AngleBetweenPoints(intersection, splitPoint, newSplitPoint, true)) < Config.Instance.MinTwistSplitAngle)
            {
                return false;
            }

            if (PolygonUtils.PointPolygonShortestDistance(PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices), intersection, out Vector2 _) < Config.Instance.MinTwistSplitPolygonDistance)
            {
                return false;
            }

            if (PolygonUtils.PointVerticesShortestDistance(splitPoint, polygon.VerticesAsCoordinates, out Vector2Int _) < Config.Instance.MinTwistSplitPointVertexDistance || PolygonUtils.PointVerticesShortestDistance(newSplitPoint, polygon.VerticesAsCoordinates, out Vector2Int _) < Config.Instance.MinTwistSplitPointVertexDistance)
            {
                return false;
            }

            PolygonUtils.SegmentPolygonIntersection(splitPoint - normal, intersection, polygon.VerticesAsCoordinates,
                out List<Vector2Int> intersections, out List<int> _);
            if (intersections.Count > 1)
            {
                return false;
            }
            
            PolygonUtils.SegmentPolygonIntersection(newSplitPoint - newNormal, intersection, polygon.VerticesAsCoordinates,
                out List<Vector2Int> newIntersections, out List<int> _);
            if (newIntersections.Count > 1)
            {
                return false;
            }
            
            return true;
        }

        private (Polygon, Polygon) DirectPolygonSplit(Polygon polygon, ref int uniquePolygonIndex)
        {
            List<int> usedEdges = new List<int>();
            List<Vector2Int> intersections;
            List<int> edgeIds;

            do
            {
                (int edgeIndex, Vector2Int splitPoint, Vector2 normal) = PickNewEdge(polygon, usedEdges);
                if (edgeIndex == -1)
                {
                    return (null, null);
                }

                PolygonUtils.SegmentPolygonIntersection(splitPoint - normal * 10, splitPoint + normal * 1000,
                    PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices), out intersections,
                    out edgeIds);
                
            } while (!ValidIntersection(intersections, edgeIds, polygon));

            List<Vertex> vertices1 = new List<Vertex>();
            List<Vertex> vertices2 = new List<Vertex>();

            bool addToFirstPolygon = true;
            int newIndex = polygon.Vertices.Count;

            int crossCount = 0;
            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                if (addToFirstPolygon)
                {
                    if (!intersections.Contains(polygon.Vertices[i].Position))
                    {
                        vertices1.Add(polygon.Vertices[i]);
                    }
                }
                else
                {
                    if (!intersections.Contains(polygon.Vertices[i].Position))
                    {
                        vertices2.Add(polygon.Vertices[i]);
                    }
                }

                if (edgeIds.Contains(i))
                {
                    addToFirstPolygon = !addToFirstPolygon;
                    Vertex intersectionVertex =
                        new Vertex(intersections[crossCount++], newIndex++, new List<int>());
                    vertices1.Add(intersectionVertex);
                    vertices2.Add(intersectionVertex);
                }
            }

            return (new Polygon(vertices1, uniquePolygonIndex++), new Polygon(vertices2, uniquePolygonIndex++));
        }

        private bool ValidIntersection(List<Vector2Int> intersections, List<int> edgeIds, Polygon polygon)
        {
            if (intersections.Count != 2)
            {
                return false;
            }

            if (PolygonUtils.PointVerticesShortestDistance(intersections[0], polygon.VerticesAsCoordinates, out Vector2Int _) < Config.Instance.MinTwistSplitPointVertexDistance || PolygonUtils.PointVerticesShortestDistance(intersections[1], polygon.VerticesAsCoordinates, out Vector2Int _) < Config.Instance.MinTwistSplitPointVertexDistance)
            {
                return false;
            }
            
            if (Mathf.Abs(Vector2Utils.AngleBetweenPoints(intersections[0], intersections[1], polygon.Vertices[edgeIds[0]].Position, true)) < Config.Instance.MinDirectSplitAngle
                || Mathf.Abs(Vector2Utils.AngleBetweenPoints(intersections[0], intersections[1], polygon.Vertices[(edgeIds[0] + 1) % polygon.Vertices.Count].Position, true)) < Config.Instance.MinDirectSplitAngle
                || Mathf.Abs(Vector2Utils.AngleBetweenPoints(intersections[1], intersections[0], polygon.Vertices[edgeIds[1]].Position, true)) < Config.Instance.MinDirectSplitAngle
                || Mathf.Abs(Vector2Utils.AngleBetweenPoints(intersections[1], intersections[0], polygon.Vertices[(edgeIds[1] + 1) % polygon.Vertices.Count].Position, true)) < Config.Instance.MinDirectSplitAngle)
            {
                return false;
            }
            
            return true;
        }

        private void PostProcessPolygons(City.City city, ref int uniquePolygonIndex)         
        {
            foreach (var district in city.Districts)
            {
                var neighborhoodsCopy = new List<Polygon>(district.Neighborhoods);
        
                foreach (var neighborhood in neighborhoodsCopy)
                {
                    MergeNearNeighbourVertices(neighborhood);
                    SubSplitPolygon(district, neighborhood, ref uniquePolygonIndex);
                }
                
                neighborhoodsCopy = new List<Polygon>(district.Neighborhoods);
                foreach (var neighborhood in neighborhoodsCopy)
                {
                    MergeNearNeighbourVertices(neighborhood);
                }
            }
        }

        private void SubSplitPolygon(District district, Polygon neighborhood, ref int uniquePolygonIndex) // todo: fix this
        {
            List<Vertex> vertices = neighborhood.Vertices;
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = 0; j < vertices.Count; j++)
                {
                    if (Mathf.Abs(i - j) <= 1 || Mathf.Abs(i - (j + 1) % vertices.Count) <= 1 || Mathf.Abs(i - Mathf.Abs(j + 1 - vertices.Count)) <= 1) //todo: also check for (j + 1) % vertices.Count for equal distance
                    {
                        continue;
                    }
                    Vector2Int edgeStart = vertices[j].Position;
                    Vector2Int edgeEnd = vertices[(j + 1) % vertices.Count].Position;
                    if (PolygonUtils.PointSegmentShortestDistance(vertices[i].Position, edgeStart, edgeEnd, out Vector2 closest) < Config.Instance.MinPolygonSubSplitDistance)
                    {
                        MultipleStaticInstances<SimpleDrawer>.GetInstance("main").AddLine(Vector2Int.RoundToInt(closest), vertices[i].Position, Color.green, 2, 10000);
                        MultipleStaticInstances<SimpleDrawer>.GetInstance("main").AddLine(edgeStart, edgeEnd, Color.red, 2, 10000);
                        (Polygon poly1, Polygon poly2) = SplitPolygonAtClosestPoint(neighborhood, i, j, closest, ref uniquePolygonIndex);
                        district.Neighborhoods.Remove(neighborhood);
                        district.Neighborhoods.Add(poly1);
                        district.Neighborhoods.Add(poly2);
                        return;
                    }
                }
            }
        }

        private void MergeNearNeighbourVertices(Polygon neighborhood)
        {
            List<Vertex> vertices = neighborhood.Vertices;
            for (int i = 0; i < vertices.Count; i++)
            {
                for (int j = i + 1; j < vertices.Count; j++)
                {
                    if (Vector2Int.Distance(vertices[i].Position, vertices[j].Position) < Config.Instance.NeighborhoodVertexMergeDistance)
                    {
                        vertices[i].Position = (vertices[i].Position + vertices[j].Position) / 2;
                        var connectedVerticesList = vertices[i].ConnectedVertices.ToList();

                        foreach (int connectedVertexIndex in vertices[j].ConnectedVertices)
                        {
                            if (!connectedVerticesList.Contains(connectedVertexIndex))
                            {
                                connectedVerticesList.Add(connectedVertexIndex);
                            }
                            Vertex connectedVertex = Vertex.FindByIndex(vertices, connectedVertexIndex);
                            if (connectedVertex != null)
                            {
                                var connectedVertexList = connectedVertex.ConnectedVertices.ToList();
                                connectedVertexList.Remove(vertices[j].Index);
                                if (!connectedVertexList.Contains(vertices[i].Index))
                                {
                                    connectedVertexList.Add(vertices[i].Index);
                                }
                                connectedVertex.ConnectedVertices = connectedVertexList;
                            }
                        }

                        vertices[i].ConnectedVertices = connectedVerticesList;

                        vertices.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private (Polygon, Polygon) SplitPolygonAtClosestPoint(Polygon polygon, int vertexIndex, int edgeIndex, Vector2 closestPoint, ref int uniquePolygonIndex)
        {
            List<Vertex> vertices1 = new List<Vertex>();
            List<Vertex> vertices2 = new List<Vertex>();

            bool addToFirstPolygon = true;
            int newIndex = polygon.Vertices.Count;

            for (int i = 0; i < polygon.Vertices.Count; i++)
            {
                if (i == vertexIndex)
                {
                    vertices1.Add(polygon.Vertices[i]);
                    vertices2.Add(polygon.Vertices[i]);
                    
                    addToFirstPolygon = !addToFirstPolygon;
                    continue;
                }

                if (addToFirstPolygon)
                {
                    vertices1.Add(polygon.Vertices[i]);
                }
                else
                {
                    vertices2.Add(polygon.Vertices[i]);
                }
                
                if (i == edgeIndex)
                {
                    Vertex intersectionVertex = new Vertex(Vector2Int.RoundToInt(closestPoint), newIndex++, new List<int>());
                    vertices1.Add(intersectionVertex);
                    vertices2.Add(intersectionVertex);

                    addToFirstPolygon = !addToFirstPolygon;
                }
            }

            return (new Polygon(vertices1, uniquePolygonIndex++), new Polygon(vertices2, uniquePolygonIndex++));
        }
        
        public override void Draw(SimpleDrawer drawer, City.City city)
        {
            foreach (var district in city.Districts)
            {
                foreach (var neighborhood in district.Neighborhoods)
                {
                    drawer.AddMultipleLines(neighborhood.InnerVertices, new Color(0.7f,0.4f,0.5f), 2, true, 5);
                }
            }
        }
    }


    struct EdgeCombo
    {
        public int First;
        public int Second;

        public EdgeCombo(int first, int second)
        {
            if (first > second)
            {
                First = second;
                Second = first;
            }else
            {
                First = first;
                Second = second;
            }
        }
    }
}