using City_Gen;
using City_Gen.City;
using Griffty.Utility.Drawing;


public class zOldStreetsGenerator : CustomCityGenerator
{
    protected override void UseGenerator(City city)
    {
        foreach (var district in city.Districts)
        {
            SplitStreets(district);
        }
    }

    private void SplitStreets(District district)
    {
        // List<Segment> addedStreets = new List<Segment>();
        // int tries;
        // int i;
        // for (i = 0, tries = 0; i < Config.Instance.StreetSplitTries && tries < Config.Instance.MaxStreetSplitTries; i++, tries++)
        // {
        //     (Segment edge, Vector2Int splitPoint) = FindSplitPoint(district.DistrictBoundaries);
        //     Vector2 normal = PolygonUtils.InnerEdgeNormal(edge.Start, edge.End, district.OuterPolygon.IsClockwise);
        //
        //     if (SegmentDistrictIntersection(splitPoint + normal, splitPoint + normal * (edge.Length * 5), district,
        //             out Vector2Int intersection))
        //     {
        //         Segment newStreet = new Segment(splitPoint, intersection);
        //         if (!IsEvenlyDistributed(newStreet, addedStreets, district.DistrictBoundaries))
        //         {
        //             i--;
        //             continue;
        //         }
        //
        //         district.Streets.Add(newStreet);
        //         addedStreets.Add(newStreet);
        //     }
        //     else
        //     {
        //         i--;
        //     }
        // }
        // Debug.Log(tries);
    }

    // private bool IsEvenlyDistributed(Segment newStreet, List<Segment> addedStreets, List<Vector2Int> districtPoly)
    // {
    //     foreach (var street in addedStreets)
    //     {
    //         if (Vector2Int.Distance(newStreet.Start, street.Start) < Config.Instance.MinStreetDist)
    //         {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
    //
    // private (Segment, Vector2Int) FindSplitPoint(List<Vector2Int> districtPolygon)
    // {
    //     WeightedRandomItemGenerator<Segment> edgePicker = new WeightedRandomItemGenerator<Segment>();
    //     for (var i = 0; i < districtPolygon.Count; i++)
    //     {
    //         var start = districtPolygon[i];
    //         var end = districtPolygon[(i + 1) % districtPolygon.Count];
    //         Segment segment = new Segment(start, end);
    //         edgePicker.AddItem(segment, (int)segment.Length);
    //     }
    //     Segment edge = edgePicker.GetRandomItem();
    //     Vector2 point = (Vector2)(edge.End - edge.Start) * Random.value + edge.Start;
    //     if (!PolygonUtils.IsPointOnSegment(point, edge.Start, edge.End))
    //     {
    //         Debug.Log("Start of the street is not on street segment"); //todo check if this is a problem
    //     }
    //     
    //     return (edge, Vector2Int.RoundToInt(point));
    // }
    //
    // private bool SegmentDistrictIntersection(Vector2 start, Vector2 end, District district, out Vector2Int intersection)
    // {
    //     List<Vector2Int> intersections = new List<Vector2Int>();
    //     foreach (var street in district.Streets)
    //     {
    //         if (PolygonUtils.SegmentIntersection(start, end, street.Start, street.End, out intersection))
    //         {
    //             intersections.Add(intersection);
    //         }
    //     }
    //
    //     if (intersections.Count == 0)
    //         return PolygonUtils.SegmentPolygonIntersection(start, end, district.DistrictBoundaries,
    //             out intersection, out _);
    //     
    //     intersections.Sort((v1, v2) =>
    //         Vector2.Distance(start, v1).CompareTo(Vector2.Distance(start, v2)));
    //     intersection = intersections[0];
    //     return true;
    //
    // }


    public override void Draw(SimpleDrawer drawer, City city)
    {
        // foreach (var district in city.Districts)
        // {
        //     foreach (var street in district.Streets)
        //     {
        //         drawer.AddLine(street.Start, street.End, new Color(0.5f, 0.4f, 0.5f), 2);
        //     }
        // }
    }
}