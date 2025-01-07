using City_Gen;
using City_Gen.City;
using Griffty.Utility.Drawing;

public class BuildingGenerator : CustomCityGenerator
{
    protected override void UseGenerator(City city)
    {
        throw new System.NotImplementedException();
    }

    public override void Draw(SimpleDrawer drawer, City city)
    {
        throw new System.NotImplementedException();
    }

    // [SerializeField] private int maxWallsAngel = 90;
    // [SerializeField] private int maxIterForAngle = 180;
    //
    //
    // public void Generate(City city)
    // {
    //     foreach (var district in city.Districts)
    //     {
    //         MakeBuildings(district);
    //     }
    // }
    //
    // private void MakeBuildings(District district)
    // {
    //     if (district.Yard == null)
    //     {
    //         district.Buildings = new List<Building> {new (district.DistrictBoundaries) };
    //         return;
    //     }
    //     int numberOfBuildings = GetNumberOfBuildingsForThisDistrict(district, Config.Instance.AvgBuildingAmount, Config.Instance.RandomBuildingAmountMultiplier);
    //     (List<Vector2Int> buildingEdge, List<int> buildingEdgePosition) = PolygonUtils.GenerateRandomSegmentsOnPoly(district.DistrictBoundaries, numberOfBuildings, Config.Instance.MinBuildingSize);
    //     List<Building> buildings = new List<Building>();
    //     (Vector2Int, Vector2Int)? lastWall = null;
    //     Vector2Int wallStart = default;
    //     Vector2Int wallEnd = default;
    //     if (buildingEdge.Count == 0)
    //     {
    //         district.Buildings = new List<Building> {new (district.DistrictBoundaries) };
    //         return;
    //     }
    //     for (var i = 0; i < buildingEdge.Count; i++)
    //     {
    //         wallStart = buildingEdge[i];
    //         MakeSingleWall(district, wallStart, district.DistrictBoundaries[buildingEdgePosition[i]], 
    //             district.DistrictBoundaries[(buildingEdgePosition[i] + 1) % district.DistrictBoundaries.Count], out wallEnd);
    //         if (lastWall != null)
    //         {
    //             buildings.Add(new Building(new List<Vector2Int>{wallStart, wallEnd, lastWall.Value.Item2, lastWall.Value.Item1}));
    //         }
    //         lastWall = (wallStart, wallEnd);
    //     }
    //
    //     buildings.Add(new Building(new List<Vector2Int> { wallStart, wallEnd, buildings[0].Corners[2], buildings[0].Corners[3]}));
    //     district.Buildings = buildings;
    // }
    //
    // private void MakeSingleWall(District district, Vector2Int wallStart, Vector2Int edgeStart, Vector2Int edgeEnd, out Vector2Int wallEnd)
    // {
    //     Vector2 edgeNormal = PolygonUtils.InnerEdgeNormal(edgeStart, edgeEnd, district.IsClockwise);
    //
    //     Vector2 closestIntersection = default;
    //     float distToClosestIntersection = float.MaxValue;
    //     Directions directions = new Directions(edgeNormal, maxWallsAngel, maxIterForAngle);
    //     for (int a = 0; a < maxIterForAngle; a++)
    //     {
    //         Vector2 nextDir = directions.GetNextDirection();
    //         for (int j = 0; j < district.Yard.Count; j++)
    //         {
    //             Vector2Int yardEdgeStart = district.Yard[j];
    //             Vector2Int yardEdgeEnd = district.Yard[(j+1) % district.Yard.Count];
    //             if (PolygonUtils.LineIntersection(wallStart, wallStart + nextDir, yardEdgeStart, yardEdgeEnd, out Vector2Int intersection))
    //             {
    //                 if (PolygonUtils.IsPointInRange(intersection, yardEdgeStart, yardEdgeEnd))
    //                 {
    //                     float distToThisIntersection = GMath.Distance(wallStart, intersection);
    //                     if (distToClosestIntersection > distToThisIntersection)
    //                     {
    //                         distToClosestIntersection = distToThisIntersection;
    //                         closestIntersection = intersection;
    //                     }   
    //                 }
    //             }
    //         }
    //     }
    //
    //     wallEnd = Vector2Int.RoundToInt(closestIntersection);
    // }
    //
    // private int GetNumberOfBuildingsForThisDistrict(District district, int avgNumOfBuildings, int buildingAmountMultiplayer)
    // {
    //     avgNumOfBuildings = (int)(avgNumOfBuildings * (1 + buildingAmountMultiplayer / 100f));
    //     float area = district.Area;
    //     float expectedArea = Config.Instance.AverageBuildingArea * Config.Instance.AreaBuildingAmountMultiplier;
    //     if (area < expectedArea)
    //     {
    //         avgNumOfBuildings = (int)(avgNumOfBuildings * Mathf.Pow(area / expectedArea, 0.5f));
    //     }
    //
    //     return avgNumOfBuildings;
    // }
}
