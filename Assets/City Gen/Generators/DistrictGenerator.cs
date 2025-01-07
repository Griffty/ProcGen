using System;
using System.Collections.Generic;
using City_Gen.City;
using ClipperLib;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.Generators
{
    public class DistrictGenerator : CustomCityGenerator
    {
        // private void Start()
        // {
        //     _drawingTool = new DrawingTool(autoClear);
        //     DrawPolygon();
        // }
        //
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.C))
        //     {
        //         streets.Clear();
        //         district = null;
        //         _drawingTool.ClearPoint();
        //         DrawPolygon();
        //     }
        //
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         Vector2 uv = GetTextureCoordFromMousePosition(spriteRenderer, cam);
        //         selectedVertex = FindClosestVertex(uv, streets);
        //     }
        //
        //     if (Input.GetMouseButtonUp(0))
        //     {
        //         selectedVertex = -1;
        //     }
        //
        //     if (selectedVertex != -1)
        //     {
        //         Vector2 uv = GetTextureCoordFromMousePosition(spriteRenderer, cam);
        //         streets[selectedVertex] = new Vector2Int((int)uv.x, (int)uv.y);
        //         MakeDistrict();
        //         DrawPolygon();
        //     }
        //
        //     if (Input.GetMouseButtonDown(2))
        //     {
        //         Vector2 uv = GetTextureCoordFromMousePosition(spriteRenderer, cam);
        //         streets.Add(new Vector2Int((int)uv.x, (int)uv.y));
        //         MakeDistrict();
        //         DrawPolygon();
        //     }
        // }
        protected override void UseGenerator(City.City city)
        {
            foreach (var polygon in city.CityPolygons)
            {
                
                District d = MakeDistrict(polygon);
                if (d != null)
                {
                    city.Districts.Add(d);
                }
            }
        }

        public override void Draw(SimpleDrawer drawer, City.City city)
        {
            foreach (var district in city.Districts)
            {
                drawer.AddPoint(district.Center, Color.yellow, 3);
                drawer.AddMultipleLines(district.DistrictBoundaries, new Color(0.2f, 0.2f, 0.2f), 3);
            }
        }

        private District MakeDistrict(Polygon polygon)
        {
            List<Vector2Int> streets = PolygonUtils.VerticesAsListOfCoordinates(polygon.Vertices);
            if (streets.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(streets), streets.Count, "Cannot make district for polygon with less than 3 vertices");
            }
        
            List<Vector2Int> districtCorners = PolygonUtils.CalculateInnerPolygonParallel(streets, Config.Instance.DistrictDistFromCorners);
        
            districtCorners = PolygonUtils.IntPointToVector2IntPoly(
                Clipper.CleanPolygon(PolygonUtils.Vector2IntToIntPointPoly(districtCorners), Config.Instance.DistrictVertexDiffThreshold)); // if too close - merge
        
            // float area = PolygonUtils.Area(districtCorners);
            // int distFromVertices = (int)(Config.Instance.BuildingsDistFromCorners * Mathf.Pow(area / Config.Instance.AverageBuildingArea, 0.35f));
            // Vector2Int center = Vector2Int.RoundToInt(PolygonUtils.Centroid(districtCorners));
            // List<Vector2Int> innerBuildingCorners = PolygonUtils.CalculateInnerPolygonParallel(districtCorners, distFromVertices); //center
            //
            // for (int i = 0; i < districtCorners.Count; i++)
            // {
            //     if (GMath.SqDistance(districtCorners[i], center) < GMath.SqDistance(districtCorners[i], innerBuildingCorners[i]))
            //     {
            //         return new District(streets, districtCorners, distFromVertices);
            //     }
            // }
            //
            // List<List<IntPoint>> simplifiedPoly = Clipper.SimplifyPolygon(PolygonUtils.Vector2IntToIntPointPoly(innerBuildingCorners));
            // List<IntPoint> validYard = PolygonUtils.ValidInnerPoly(simplifiedPoly, center);
            //
            //
            // List<Vector2Int> yard = null;
            // if (validYard != null)
            // {
            //    validYard = Clipper.CleanPolygon(validYard, Config.Instance.YardVertexDiffThreshold); // if too close - merge
            //    yard = PolygonUtils.IntPointToVector2IntPoly(validYard);
            //    if (PolygonUtils.IsPolyClockwise(yard) != PolygonUtils.IsPolyClockwise(districtCorners))
            //    {
            //        yard.Reverse();
            //    }
            //    PolygonUtils.AlignPolygons(districtCorners, yard);
            //    if (PolygonUtils.Area(yard) < Config.Instance.MinYardArea)
            //    {
            //        yard = null;
            //    }
            // }
            
            return new District(districtCorners);
        }
    }
}
