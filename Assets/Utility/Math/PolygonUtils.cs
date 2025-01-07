using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLib;
using Griffty.Utility.Data;
using UnityEngine;
using Random = System.Random;
using Math = System.Math;
namespace Griffty.Utility.Static.GMath
{
    public static class PolygonUtils
    {
        public static bool IsPointInRange(Vector2 point, Vector2 start, Vector2 end)
        {
            float minX = Mathf.Min(start.x, end.x);
            float maxX = Mathf.Max(start.x, end.x);
            float minY = Mathf.Min(start.y, end.y);
            float maxY = Mathf.Max(start.y, end.y);
            bool a = point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
            return a;
        }

        public static bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            // Check if point is on the line determined by start and end
            double lineVal = (end.y - start.y) * point.x + (start.x - end.x) * point.y +
                             (end.x * start.y - start.x * end.y);
            if (Math.Abs(lineVal) > Mathf.Epsilon)
            {
                return false;
            }

            // Check if point is between start and end
            double minX = Math.Min(start.x, end.x);
            double maxX = Math.Max(start.x, end.x);
            double minY = Math.Min(start.y, end.y);
            double maxY = Math.Max(start.y, end.y);

            return point.x >= minX && point.x <= maxX && point.y >= minY && point.y <= maxY;
        }

        public static (List<Vector2Int>, List<int>) GenerateRandomSegmentsOnPoly(List<Vector2Int> polygon, int n,
            float minDistance, Random random = null)
        {
            random ??= new Random();
            List<Vector2Int> segmentPoints = new();
            List<int> vertexPosition = new();
            float totalLength = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                totalLength += Vector2.Distance(polygon[i], polygon[(i + 1) % polygon.Count]);
            }

            float averageLength = totalLength / n;

            List<float> randomLengths = new List<float>();

            for (int i = 0; i < n - 1; i++)
            {
                randomLengths.Add(random.Next(7, 14) / 10f * averageLength);
            }

            float totalRandomLength = 0;
            foreach (var len in randomLengths)
                totalRandomLength += len;

            List<float> normalizedLengths = new List<float>();

            foreach (var len in randomLengths)
            {
                normalizedLengths.Add(len / totalRandomLength * totalLength);
            }

            normalizedLengths.Add(totalLength - MathUtils.Sum(normalizedLengths));

            float currentLength = 0;
            int currentVertex = 0;

            foreach (var length in normalizedLengths)
            {
                currentLength += length;

                while (currentLength >
                       Vector2.Distance(polygon[currentVertex], polygon[(currentVertex + 1) % polygon.Count]) &&
                       currentVertex < polygon.Count - 1)
                {
                    currentLength -= Vector2.Distance(polygon[currentVertex],
                        polygon[(currentVertex + 1) % polygon.Count]);
                    currentVertex++;
                }

                Vector2 direction = ((Vector2)(polygon[(currentVertex + 1) % polygon.Count] - polygon[currentVertex]))
                    .normalized;
                Vector2 pointLocation = polygon[currentVertex] + direction * currentLength;

                if ((segmentPoints.Count == 0 || Vector2.Distance(pointLocation, segmentPoints[^1]) >= minDistance))
                {
                    if (PointIsOnVertex(pointLocation, polygon))
                    {
                        if (random.Next(101) > 85)
                        {
                            segmentPoints.Add(Vector2Int.RoundToInt(pointLocation));
                            vertexPosition.Add(currentVertex);
                        }
                    }
                    else
                    {
                        segmentPoints.Add(Vector2Int.RoundToInt(pointLocation));
                        vertexPosition.Add(currentVertex);
                    }
                }
            }

            return (segmentPoints, vertexPosition);
        }

        private static bool PointIsOnVertex(Vector2 pointLocation, List<Vector2Int> polygon)
        {
            foreach (var vertex in polygon)
            {
                if (MathUtils.Distance(pointLocation, vertex) < 3)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<IntPoint> ValidInnerPoly(List<List<IntPoint>> simplifiedPoly, Vector2Int center)
        {
            double maxArea = double.MinValue;
            List<IntPoint> largestPolygon = null;

            foreach (var poly in simplifiedPoly)
            {
                double area = Clipper.Area(poly);
                if (area > maxArea)
                {
                    maxArea = area;
                    largestPolygon = poly;
                }
            }

            return IsPointInsidePolygon(center, largestPolygon) ? largestPolygon : null;
        }
        
        public static bool IsPointInsidePolygon(Vector2Int point, List<Vector2Int> polygon)
        {
            return IsPointInsidePolygon(point, Vector2IntToIntPointPoly(polygon));
        }

        public static bool IsPointInsidePolygon(Vector2Int point, List<IntPoint> polygon)
        {
            bool isInside = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                if (((polygon[i].Y > point.y) != (polygon[j].Y > point.y)) &&
                    (point.x < (polygon[j].X - polygon[i].X) * (point.y - polygon[i].Y) /
                        (polygon[j].Y - polygon[i].Y) + polygon[i].X))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }

        public static List<Vector2Int> CalculateInnerPolygonWithBisector(List<Vector2Int> polygon, int distFromVertices)
        {
            List<Vector2Int> innerPoints = new List<Vector2Int>();
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2Int thisVertex = polygon[i];
                Vector2Int lastVertex = polygon[(i - 1 + polygon.Count) % polygon.Count];
                Vector2Int nextVertex = polygon[(i + 1) % polygon.Count];

                float angelToLast = Mathf.Atan2(lastVertex.y - thisVertex.y, lastVertex.x - thisVertex.x);
                float angelToNext = Mathf.Atan2(nextVertex.y - thisVertex.y, nextVertex.x - thisVertex.x);

                float angleDifference = angelToNext - angelToLast;

                if (angleDifference > Mathf.PI)
                    angleDifference -= 2 * Mathf.PI;
                else if (angleDifference <= -Mathf.PI)
                    angleDifference += 2 * Mathf.PI;

                float bisectorAngel = angelToLast + angleDifference / 2;

                Vector2 point = new Vector2(Mathf.Cos(bisectorAngel), Mathf.Sin(bisectorAngel)) * distFromVertices;

                var pos = point + thisVertex;

                innerPoints.Add(new Vector2Int((int)pos.x, (int)pos.y));
            }

            return innerPoints;
        }

        public static List<Vector2Int> CalculateInnerPolygonParallel(List<Vector2Int> polygon, float distFromVertices)
        {
            List<Vector2Int> innerPolygon = new List<Vector2Int>();
            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                Vector2 curr = polygon[i];
                Vector2 next = polygon[(i + 1) % n];
                bool isClockwise = IsPolyClockwise(polygon);

                Vector2 edgeNormal = OuterEdgeNormal(next, curr, isClockwise);

                Vector2 displacedCurr = curr + edgeNormal * distFromVertices;
                Vector2 displacedNext = next + edgeNormal * distFromVertices;

                Vector2 prev = polygon[(i - 1 + n) % n];
                Vector2 prevEdgeDir = (curr - prev).normalized;
                Vector2 prevEdgeNormal = isClockwise
                    ? new Vector2(prevEdgeDir.y, -prevEdgeDir.x)
                    : new Vector2(-prevEdgeDir.y, prevEdgeDir.x);

                if (LineIntersection(prev + prevEdgeNormal * distFromVertices, curr + prevEdgeNormal * distFromVertices,
                        displacedCurr, displacedNext, out Vector2Int intersection))
                {
                    innerPolygon.Add(Vector2Int.RoundToInt(intersection));
                }
                else
                {
                    innerPolygon.Add(Vector2Int.RoundToInt(displacedCurr));
                }
            }

            return innerPolygon;
        }

        public static Vector2 OuterEdgeNormal(Vector2 next, Vector2 curr, bool isClockwise)
        {
            Vector2 edgeDir = (next - curr).normalized;
            Vector2 edgeNormal = isClockwise ? new Vector2(edgeDir.y, -edgeDir.x) : new Vector2(-edgeDir.y, edgeDir.x);
            return edgeNormal;
        }

        public static Vector2 InnerEdgeNormal(Vector2 next, Vector2 curr, bool isClockwise)
        {
            Vector2 edgeDir = (next - curr).normalized;
            Vector2 edgeNormal = isClockwise ? new Vector2(-edgeDir.y, edgeDir.x) : new Vector2(edgeDir.y, -edgeDir.x);
            return edgeNormal;
        }

        public static bool IsPolyClockwise(List<Vector2Int> polygon)
        {
            float sum = 0.0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 v1 = polygon[i];
                Vector2 v2 = polygon[(i + 1) % polygon.Count];
                sum += (v2.x - v1.x) * (v2.y + v1.y);
            }

            return sum > 0;
        }

        public static bool LineIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2Int intersection)
        {
            intersection = Vector2Int.zero;

            float det = (p2.x - p1.x) * (q2.y - q1.y) - (p2.y - p1.y) * (q2.x - q1.x);
            if (Mathf.Abs(det) < Mathf.Epsilon)
            {
                // Lines are parallel
                return false;
            }

            float alpha = ((p1.y - q1.y) * (q2.x - q1.x) - (p1.x - q1.x) * (q2.y - q1.y)) / det;
            intersection = Vector2Int.RoundToInt(p1 + alpha * (p2 - p1));

            return true;
        }

        public static bool SegmentPolygonIntersection(Vector2 p1, Vector2 p2, List<Vector2Int> polygon,
            out Vector2Int intersection, out int edgeId)
        {
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 q1 = polygon[i];
                Vector2 q2 = polygon[(i + 1) % polygon.Count];
                if (SegmentIntersection(p1, p2, q1, q2, out intersection))
                {
                    edgeId = i;
                    return true;
                }
            }
            intersection = Vector2Int.zero;
            edgeId = -1;
            return false;
        }
        
        public static bool SegmentPolygonIntersection(Vector2 p1, Vector2 p2, List<Vector2Int> polygon,
            out List<Vector2Int> intersections, out List<int> edgeIds)
        {
            intersections = new List<Vector2Int>();
            edgeIds = new List<int>();
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 q1 = polygon[i];
                Vector2 q2 = polygon[(i + 1) % polygon.Count];
                if (SegmentIntersection(p1, p2, q1, q2, out Vector2Int intersection))
                {
                    intersections.Add(intersection);
                    edgeIds.Add(i);   
                }
            }

            return intersections.Count > 0;
        }
        
        public static bool SegmentIntersection(Vector2 p1, Vector2 p2, Vector2 q1, Vector2 q2, out Vector2Int intersection)
        {
            if (LineIntersection(p1, p2, q1, q2, out intersection))
            {
                return IsPointInRange(intersection, q1, q2) && IsPointInRange(intersection, p1, p2);
            }
            intersection = Vector2Int.zero;
            return false;
        }

        public static Vector2 Centroid(List<Vector2Int> p)
        {
            int numPoints = p.Count;
            if (numPoints < 3)
                throw new ArgumentException("At least 3 points required for a polygon to determine the center.");

            Vector2 c = Vector2.zero;
            float signedArea = 0.0f;

            for (int i = 0; i < numPoints; i++)
            {
                int nextIndex = (i + 1) % numPoints;
                float a = p[i].x * p[nextIndex].y - p[nextIndex].x * p[i].y;
                signedArea += a;
                c.x += (p[i].x + p[nextIndex].x) * a;
                c.y += (p[i].y + p[nextIndex].y) * a;
            }

            signedArea *= 0.5f;
            c.x /= (6.0f * signedArea);
            c.y /= (6.0f * signedArea);

            return c;
        }



        public static void AlignPolygons(List<Vector2Int> outerPolygon, List<Vector2Int> innerPolygon)
        {
            float AngularDifference(Vector2 a, Vector2 b)
            {
                return Vector2.Angle(a, b);
            }

            // Calculate the total angular difference between two polygons
            float TotalAngularDifference(List<Vector2Int> outer, List<Vector2Int> inner)
            {
                float totalDifference = 0;

                for (int i = 0; i < outer.Count; i++)
                {
                    Vector2 outerDir = ((Vector2)(outer[(i + 1) % outer.Count] - outer[i])).normalized;
                    Vector2 innerDir = ((Vector2)(inner[(i + 1) % inner.Count] - inner[i])).normalized;

                    totalDifference += AngularDifference(outerDir, innerDir);
                }

                return totalDifference;
            }

            if (outerPolygon.Count != innerPolygon.Count)
            {
                return;
            }

            float bestDifference = float.MaxValue;
            int bestRotation = 0;

            // Try all rotations of the outer polygon
            for (int r = 0; r < outerPolygon.Count; r++)
            {
                float difference = TotalAngularDifference(outerPolygon, innerPolygon);
                if (difference < bestDifference)
                {
                    bestDifference = difference;
                    bestRotation = r;
                }

                // Rotate the outer polygon
                Vector2Int temp = outerPolygon[0];
                outerPolygon.RemoveAt(0);
                outerPolygon.Add(temp);
            }

            // Now, rearrange the outer polygon with the best rotation found
            for (int r = 0; r < bestRotation; r++)
            {
                Vector2Int temp = outerPolygon[0];
                outerPolygon.RemoveAt(0);
                outerPolygon.Add(temp);
            }
        }
        [Obsolete("Function is not working as intended", true)]
        public static void SplitPolygon(List<Vector2Int> polygon, int edgeIndex, out List<Vector2Int> firstPoly,
            out List<Vector2Int> secondPoly)
        {
            int nextIndex = (edgeIndex + 1) % polygon.Count;
            // Find midpoint of the selected edge
            Vector2Int midpoint = new Vector2Int(
                (polygon[edgeIndex].x + polygon[nextIndex].x) / 2,
                (polygon[edgeIndex].y + polygon[nextIndex].y) / 2
            );

            // Create two new polygons
            List<Vector2Int> polygon1 = new List<Vector2Int>();
            List<Vector2Int> polygon2 = new List<Vector2Int>();

            for (int i = 0; i <= edgeIndex; i++)
            {
                polygon1.Add(polygon[i]);
            }

            polygon1.Add(midpoint);

            polygon2.Add(midpoint);
            for (int i = nextIndex; i < polygon.Count; i++)
            {
                polygon2.Add(polygon[i]);
            }

            firstPoly = polygon1;
            secondPoly = polygon2;
        }
        [Obsolete("Function is not working as intended", true)]
        public static (Vector2Int midpoint, Vector2Int newPoint) SplitPolygonWithNormal(List<Vector2Int> polygon,
            int edgeIndex, out List<Vector2Int> firstPoly, out List<Vector2Int> secondPoly)
        {
            int nextIndex = (edgeIndex + 1) % polygon.Count;
            // Find midpoint of the selected edge
            Vector2Int midpoint = new Vector2Int(
                (polygon[edgeIndex].x + polygon[nextIndex].x) / 2,
                (polygon[edgeIndex].y + polygon[nextIndex].y) / 2
            );

            Vector2 midPointNormal = InnerEdgeNormal(polygon[edgeIndex], polygon[nextIndex], IsPolyClockwise(polygon));
            // return midpoint + Vector2Int.RoundToInt(midPointNormal)*5;
            Vector2Int newPoint = default;
            int newPointEdgeIndex = -1;
            for (int i = 0; i < polygon.Count; i++)
            {
                if (i == edgeIndex)
                {
                    continue;
                }

                Vector2Int edgeStart = polygon[i];
                Vector2Int edgeEnd = polygon[(i + 1) % polygon.Count];
                if (LineIntersection(midpoint, midpoint + midPointNormal, edgeStart, edgeEnd, out Vector2Int intersection))
                {
                    if (IsPointInRange(intersection, edgeStart, edgeEnd))
                    {
                        newPointEdgeIndex = i;
                        newPoint = Vector2Int.RoundToInt(intersection);
                        break;
                    }
                }
            }

            if (newPoint == default)
            {
                throw new Exception("No intersection found");
            }

            int midpointIndex = nextIndex;
            int newPointIndex = newPointEdgeIndex + 2;

            // Create two new polygons
            List<Vector2Int> polygon1 = new List<Vector2Int>();
            List<Vector2Int> polygon2 = new List<Vector2Int>();


            int firstPoint = midpointIndex > newPointIndex ? newPointIndex : midpointIndex;
            int secondPoint = firstPoint == midpointIndex ? newPointIndex : midpointIndex;

            // Add vertices to the first polygon
            int index = firstPoint;
            do
            {
                polygon1.Add(polygon[index]);
                index = (index + 1) % polygon.Count;
            } while (index != secondPoint);

            polygon1.Add(polygon[secondPoint]);

            // Add vertices to the second polygon
            index = secondPoint;
            do
            {
                polygon2.Add(polygon[index]);
                index = (index + 1) % polygon.Count;
            } while (index != firstPoint);

            polygon2.Add(polygon[firstPoint]);

            firstPoly = polygon1;
            secondPoly = polygon2;
            return (midpoint, newPoint);
        }

        public static float Area(List<Vector2Int> districtBoundaries)
        {
            return Mathf.Abs((float)Clipper.Area(Vector2IntToIntPointPoly(districtBoundaries)));
        }

        public static List<IntPoint> Vector2IntToIntPointPoly(List<Vector2Int> poly)
        {
            return poly.Select(vertex => new IntPoint(vertex.x, vertex.y)).ToList();
        }

        public static List<Vector2Int> IntPointToVector2IntPoly(List<IntPoint> poly)
        {
            return poly.Select(point => new Vector2Int((int)point.X, (int)point.Y)).ToList();
        }

        public static List<Vector2Int> VerticesAsListOfCoordinates(List<Vertex> vertices)
        {
            return vertices.Select(vertex => vertex.Position).ToList();
        }

        public static int LargestEdge(List<Vector2Int> streets, List<int> usedEdges, out float length)
        {
            int largestEdgeIndex = -1;
            length = float.NegativeInfinity;
            for (int i = 0; i < streets.Count; i++)
            {
                if (MathUtils.SqDistance(streets[i], streets[(i + 1) % streets.Count]) > length && !usedEdges.Contains(i))
                {
                    length = MathUtils.SqDistance(streets[i], streets[(i + 1) % streets.Count]);
                    largestEdgeIndex = i;
                }
            }

            return largestEdgeIndex;
        }

        public static (Vector4, Vector2) FindBoundingBox(List<Vector2Int> vertices)
        {
            int minX = int.MaxValue, maxX = 0, minY = int.MaxValue, maxY = 0;
            foreach (var vertex in vertices)
            {
                if (vertex.x > maxX) maxX = vertex.x;
                if (vertex.y > maxY) maxY = vertex.y;
                if (vertex.x < minX) minX = vertex.x;
                if (vertex.y < minY) minY = vertex.y;
            }

            return (new Vector4(minX, minY, maxX, maxY), new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f));
        }

        public static List<Vertex> PolygonsAsListOfVertices(List<Polygon> polygons)
        {
            return polygons.SelectMany(polygon => polygon.Vertices).ToList();
        }
        
        public static float PointPolygonShortestDistance(List<Vector2Int> polygon, Vector2Int point, out Vector2 closest)
        {
            float shortestDistance = float.MaxValue;
            closest = Vector2.zero;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2Int start = polygon[i];
                Vector2Int end = polygon[(i + 1) % polygon.Count];
                float distance = PointSegmentShortestDistance(point, start, end, out Vector2 closestPoint);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest = closestPoint;
                }
            }

            return shortestDistance;
        }
        
        public static float PointSegmentShortestDistance(Vector2Int point, Vector2Int segmentStart, Vector2Int segmentEnd, out Vector2 closest)
        {
            Vector2 segmentVector = segmentEnd - segmentStart;
            Vector2 pointVector = point - segmentStart;

            float segmentLengthSquared = segmentVector.sqrMagnitude; // Magnitude squared for efficiency

            if (segmentLengthSquared == 0.0f)
            {
                // Segment is a point; return the distance from the point to this point
                closest = segmentStart;
                return Vector2.Distance(point, segmentStart);
            }

            // Project pointVector onto segmentVector
            float t = Vector2.Dot(pointVector, segmentVector) / segmentLengthSquared;

            // Clamp t to the range [0, 1] to stay within the segment
            t = Mathf.Clamp01(t);

            // Find the closest point on the segment to the point
            closest = segmentStart + t * segmentVector;

            // Return the distance from the point to the closest point on the segment
            return Vector2.Distance(point, closest);
        }

        public static float PointVerticesShortestDistance(Vector2Int point, List<Vector2Int> vertices, out Vector2Int closest)
        {
            closest = Vector2Int.zero;
            float shortestDistance = float.MaxValue;
            foreach (var vertex in vertices)
            {
                float distance = Vector2.Distance(point, vertex);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closest = vertex;
                }
            }

            return shortestDistance;
        }
    }
}