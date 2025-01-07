using System;
using System.Collections.Generic;
using System.Linq;
using City_Gen.City;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace Griffty.Utility.Drawing
{
    public class SimpleDrawer
    {
        public void AddRectangle(Vector4 rect, Color color, int thickness = 1, int layer = 0)
        {
            AddLine(new Vector2Int((int)rect.x, (int)rect.y), new Vector2Int((int)rect.x, (int)rect.w), color, thickness, layer);
            AddLine(new Vector2Int((int)rect.x, (int)rect.w), new Vector2Int((int)rect.z, (int)rect.w), color, thickness, layer);
            AddLine(new Vector2Int((int)rect.z, (int)rect.w), new Vector2Int((int)rect.z, (int)rect.y), color, thickness, layer);
            AddLine(new Vector2Int((int)rect.z, (int)rect.y), new Vector2Int((int)rect.x, (int)rect.y), color, thickness, layer);
        }
        public void AddLine(Vector2Int start, Vector2Int end, Color color, int thickness = 1, int layer = 0)
        {
            linesToDraw.Add(new LineToDraw(start, end, color, thickness, layer));
        }
        
        public void AddLine(Segment segment, Color color, int thickness = 1, int layer = 0)
        {
            AddLine(segment.Start, segment.End, color, thickness, layer);
        }
        public void AddPoint(Vector2Int position, Color color, int radius = 1, int layer = 0)
        {
            pointsToDraw.Add(new PointToDraw(position, color, radius, layer));
        }
        public void AddMultiplePoints(IEnumerable<Vector2Int> positions, Color color, int radius = 1, int layer = 0)
        {
            foreach (Vector2Int position in positions)
            {
                AddPoint(position, color, radius, layer);
            }
        }
        public void AddMultipleLines(IEnumerable<Vector2Int> vertices, Color color,  int thickness = 1, bool polygon = true, int layer = 0)
        {
            List<Vector2Int> verticesList = vertices.ToList();
            for (int i = 0; i < verticesList.Count; i++)
            {
                if (!polygon && i == verticesList.Count - 1)
                {
                    break;
                }
                AddLine(verticesList[i], verticesList[(i + 1) % verticesList.Count], color, thickness, layer);
            }
        }
        public void AddPolygons(IEnumerable<IEnumerable<Vector2Int>> polygons, Color color, int thickness = 1, int layer = 0)
        {
            foreach (IEnumerable<Vector2Int> polygon in polygons)
            {
                AddMultipleLines(polygon, color,  thickness, true, layer);
            }
        }
        private Texture2D texture2D;
        private readonly List<LineToDraw> linesToDraw = new();
        private readonly List<PointToDraw> pointsToDraw = new();
        public bool ClearOnDispatch { get; set; }
        public bool ClearOnNewCanvas { get; set; } = true;

        public Texture2D Texture2D
        {
            get
            {
                if (texture2D == null)
                {
                    throw new InvalidOperationException(
                        "You need to set an empty texture, before trying to get a new one");
                }

                DispatchDrawQueue();
                texture2D.Apply();
                return texture2D;
            }
            set
            {
                if (ClearOnNewCanvas)
                {
                    ClearPoints();
                }

                texture2D = value;
            }
        }

        private void DispatchDrawQueue()
        {
            if (linesToDraw.Count > 0)
            {
                linesToDraw.Sort((l1, l2) => l1.Layer.CompareTo(l2.Layer));
            }

            if (pointsToDraw.Count > 0)
            {
                pointsToDraw.Sort((p1, p2) => p1.Layer.CompareTo(p2.Layer));
            }
            
            Vector2Int layerLimits = FindLayerLimits();

            int lineIndex = 0, pointIndex = 0;
            for (int layer = layerLimits.x; layer <= layerLimits.y; layer++)
            {
                while (lineIndex < linesToDraw.Count && linesToDraw[lineIndex].Layer == layer)
                {
                    DrawLine(linesToDraw[lineIndex]);
                    lineIndex++;
                }
                while (pointIndex < pointsToDraw.Count && pointsToDraw[pointIndex].Layer == layer)
                {
                    DrawPoint(pointsToDraw[pointIndex]);
                    pointIndex++;
                }
            }

            if (ClearOnDispatch)
            {
                ClearPoints();        
            }
        }
        
        private void ClearPoints()
        {
            linesToDraw.Clear(); 
            pointsToDraw.Clear();
        }
        
        private void DrawPoint(PointToDraw pointToDraw)
        {
            DrawPoint(pointToDraw.Position.x, pointToDraw.Position.y, pointToDraw.Color, pointToDraw.Radius);
        }
        
        //todo: implement as shader
        //todo: add float radius
        private void DrawPoint(int sx, int sy, Color color, int radius) 
        {
            for (float x = -radius; x <= radius; x++)
            {
                for (float y = -radius; y <= radius; y++)
                {
                    if (MathUtils.SqDistance(0, 0, x, y) < radius * radius) 
                    {
                        texture2D.SetPixel((int)x + sx,(int)y + sy, color);
                    }
                }   
            }
        }

        private void DrawLine(LineToDraw lineToDraw) // implement as shader
        {
            int x0 = lineToDraw.Start.x;
            int x1 = lineToDraw.End.x;
            int y0 = lineToDraw.Start.y;
            int y1 = lineToDraw.End.y;
            
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (;;)
            {
                DrawPoint(x0, y0, lineToDraw.Color, lineToDraw.Thickness);
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dy)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
        
        private Vector2Int FindLayerLimits()
        {
            try
            {
                int min = (linesToDraw.Count > 0 ? linesToDraw[0].Layer : 0) > (pointsToDraw.Count > 0 ? pointsToDraw[0].Layer : 0) 
                    ? (pointsToDraw.Count > 0 ? pointsToDraw[0].Layer : 0) 
                    : (linesToDraw.Count > 0 ? linesToDraw[0].Layer : 0);

                int max = (linesToDraw.Count > 0 ? linesToDraw[^1].Layer : 0) > (pointsToDraw.Count > 0 ? pointsToDraw[^1].Layer : 0) 
                    ? (linesToDraw.Count > 0 ? linesToDraw[^1].Layer : 0) 
                    : (pointsToDraw.Count > 0 ? pointsToDraw[^1].Layer : 0);
                
                return new Vector2Int(min, max);
            }
            catch (Exception)
            {
                Debug.Log("cannot find layer limits");
                return new Vector2Int(0, 0);
            }
        }

    }
}