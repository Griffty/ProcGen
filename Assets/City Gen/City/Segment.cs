using System;
using Griffty.Utility.Static.GMath;
using UnityEngine;

namespace City_Gen.City
{
    [Serializable]
    public class Segment
    {
        [SerializeField] private Vector2Int start;
        [SerializeField] private Vector2Int end;
    
        public Vector2Int Start => start;
        public Vector2Int End => end;
        public Vector2 Direction => Vector2Utils.DirectionFromPositions(start, end);

        public float Length => Vector2Int.Distance(start, end); 
    
        public Segment(Vector2Int start, Vector2Int end)
        {
            this.start = start;
            this.end = end;
        }
    }
}