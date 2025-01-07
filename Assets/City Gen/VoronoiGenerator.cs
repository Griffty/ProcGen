using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Griffty.Utility.Data;
using Griffty.Utility.Patterns;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen
{
    public class VoronoiGenerator : UnitySingleton<VoronoiGenerator> // I have no idea why, but this is working, and working really fast, much faster that I thought
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private bool useOptimization;
        [SerializeField] private bool draw;
        public List<Vertex> GenerateMap(int size, int[] px, int[] py)
        {
            List<Vertex> intersections = new();
            PixelP[,] allPixels = new PixelP[size, size];

            int pointCount = px.Length;
            float inversePointCount = 1f / pointCount;
            int critSize = (int)(size / Mathf.Sqrt(pointCount) * 2);
        
            Parallel.For(0, size, x =>
            {
                for (int y = 0; y < size; y++)
                {
                    PixelP p = new PixelP();
                    p.AnchorPoint = FindClosestPoint(x, y, px, py, critSize);
                    allPixels[x, y] = p;
                }
            });
        

            ApplyTexture(allPixels, size);
            ProcessIntersections(intersections, allPixels, size);

            return intersections;
        }

        private int FindClosestPoint(int x, int y, int[] px, int[] py, int critSize)
        {
            int closestPointIndex = 0;
            float minDistance = MathUtils.SqDistance(px[0], py[0], x, y);

            for (byte i = 1; i < px.Length; i++)
            {
                if (useOptimization && Math.Abs(px[i] - x) > critSize && Math.Abs(py[i] - y) > critSize)
                {
                    continue;
                }

                float currentDistance = MathUtils.SqDistance(px[i], py[i], x, y);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance;
                    closestPointIndex = i;
                }
            }

            return closestPointIndex;
        }

        private void ApplyTexture(PixelP[,] allPixels, int size)
        {
            if (!draw)
            {
                return;
            }
            Texture2D texture2D = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
        
            int maxAnchor = allPixels.Cast<PixelP>().Max(p => p.AnchorPoint);
        
            Dictionary<int, Color> anchorPointColors = new Dictionary<int, Color>();
        
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int anchorPoint = allPixels[x, y].AnchorPoint;
                    if (!anchorPointColors.ContainsKey(anchorPoint))
                    {
                        anchorPointColors[anchorPoint] = new Color(0.3f, (float)anchorPoint / maxAnchor,0.5f);
                    }
                }
            }
        
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    int index = y * size + x;
                    int anchorPoint = allPixels[x, y].AnchorPoint;
                    colors[index] = anchorPointColors[anchorPoint];
                }
            }
        
            texture2D.SetPixels(colors);
            texture2D.Apply();
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
            spriteRenderer.sprite = sprite;
        }

        private void ProcessIntersections(List<Vertex> intersections, PixelP[,] allPixels, int size) // this is a mess, but it works, so I'm not going to touch it
        {
            int index = 0;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (CheckNeighborsBlack(x, y, allPixels, size))
                    {
                        if (CheckNeighborsIntersection(x, y, intersections, allPixels, size, index))
                        {
                            index++;   
                        }
                    }
                }
            }

            foreach (Vertex vertex1 in intersections)
            {
                foreach (Vertex vertex2 in intersections)
                {
                    if (vertex1 != vertex2)
                    {
                        int similar = vertex1.AdjacentPolygons.Intersect(vertex2.AdjacentPolygons).Count();
                        if (similar >= 2)
                        {
                            vertex1.AddConnectedVertex(vertex2.Index);
                        }
                    }
                }
            }

            SortIntersections(intersections);
            ClearFromUnconnected(intersections);
            if (HasDuplicateIndices(intersections))
            {
                Debug.Log("Duplicate indices");
            }
        }


        private void SortIntersections(List<Vertex> intersections)
        {
            float centroidX = (float)intersections.Average(p => p.Position.x);
            float centroidY = (float)intersections.Average(p => p.Position.y);
            Vector2 centroid = new Vector2(centroidX, centroidY);
            foreach (var point in intersections)
            {
                List<int> sorted = point.ConnectedVertices.ToList();
                sorted.Sort((p1, p2) =>
                    Vector2Utils.AngleToTarget(centroid, Vertex.FindByIndex(intersections, p1).Position)
                        .CompareTo(Vector2Utils.AngleToTarget(centroid, Vertex.FindByIndex(intersections, p2).Position)));
                point.ConnectedVertices = sorted;
            }
        }

        private void ClearFromUnconnected(List<Vertex> intersections)
        {
            bool changesMade;
            int i = 0;
            do
            {
                changesMade = false;
                foreach (var point in intersections.ToList())
                {
                    if (point.ConnectedVertices.Count() < 2)
                    {
                        intersections.Remove(point);
                        if (point.ConnectedVertices.Any())
                            Vertex.FindByIndex(intersections, point.ConnectedVertices.ElementAt(0)).RemoveConnectedVertex(point.Index);
                        changesMade = true;
                    }
                }
            } while (changesMade && i++ < 1000);

            if (i > 500)
            {
                Debug.Log("Voronoi neighbors iterations: " + i);
            }

        }

        private int Check(int x, int y, PixelP[,] allPixels)
        {
            return allPixels[x, y].AnchorPoint;
        }

        private bool CheckNeighborsBlack(int row, int col, PixelP[,] allPixels, int size)
        {
            int prevAnchor = allPixels[row, col].AnchorPoint;
            if (col < size && row < size)
            {
                if (col < size - 1 && prevAnchor != Check(row, col + 1, allPixels))
                {
                    return true;
                }

                if (row < size - 1 && prevAnchor != Check(row + 1, col, allPixels))
                {
                    return true;
                }
            }

            return false;
        }
  
        private bool CheckNeighborsIntersection(int row, int col, List<Vertex> intersections, PixelP[,] allPixels, int size, int index)
        {
            int prevAnchor1 = allPixels[row, col].AnchorPoint;
            int prevAnchor2 = allPixels[row, col].AnchorPoint;
            int prevAnchor3 = allPixels[row, col].AnchorPoint;

            if (col < size - 3 && col > 2 && row < size - 3 && row > 2)
            {
                int differentColors = 0;
                if (prevAnchor1 != Check(row, col + 1, allPixels))
                {
                    prevAnchor2 = Check(row, col + 1, allPixels);
                    differentColors++;
                    if (prevAnchor1 != Check(row + 1, col + 1, allPixels) && prevAnchor2 != Check(row + 1, col + 1, allPixels) && prevAnchor3 != Check(row + 1, col + 1, allPixels))
                    {
                        differentColors++;
                    }
                }
                if (prevAnchor1 != Check(row + 1, col, allPixels) && prevAnchor2 != Check(row + 1, col, allPixels))
                {
                    prevAnchor3 = Check(row + 1, col, allPixels);
                    differentColors++;
                    if (prevAnchor1 != Check(row + 1, col + 1, allPixels) && prevAnchor2 != Check(row + 1, col + 1, allPixels) && prevAnchor3 != Check(row + 1, col + 1, allPixels))
                    {
                        differentColors++;
                    }
                }
                if (differentColors >= 2)
                {
                    if (row < size - 7 && col < size - 7 && row > 6 && col > 6)
                    {
                        HashSet<int> polygons = new();
                        for (int x = row - 4; x < row + 5; x++)
                        {
                            for (int y = col - 4; y < col + 5; y++)
                            {
                                polygons.Add(Check(x, y, allPixels));
                            }
                        }
                        bool changesMade;
                        int i = 0;
                        do
                        {
                            changesMade = false;
                            foreach (var intersection in intersections)
                            {
                                if (MathUtils.SqDistance(intersection.Position, new Vector2(row, col)) < 50)
                                {
                                    foreach (var c in intersection.AdjacentPolygons)
                                    {
                                        polygons.Add(c);
                                    }

                                    intersections.Remove(intersection);
                                    changesMade = true;
                                    break;
                                }
                            }
                        } while (changesMade && i++ < 1000);

                        if (i > 500)
                        {
                            Debug.Log("Voronoi neighbors iterations: " + i);
                        }

                        intersections.Add(new Vertex(new Vector2Int(row, col), index, polygons.ToList()));
                        return true;
                    }
                }
            }

            return false;
        }
    
        public static bool HasDuplicateIndices(List<Vertex> intersections)
        {
            Dictionary<int, int> indexCount = new Dictionary<int, int>();

            foreach (var vertex in intersections)
            {
                if (indexCount.ContainsKey(vertex.Index))
                {
                    indexCount[vertex.Index]++;
                }
                else
                {
                    indexCount[vertex.Index] = 1;
                }
            }

            return indexCount.Values.Any(count => count > 1);
        }
    }

    public struct PixelP{
        public int AnchorPoint { get; set; }   
    }
}