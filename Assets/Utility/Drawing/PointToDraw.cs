using System;
using UnityEngine;

namespace Griffty.Utility.Drawing
{
    [Serializable]
    public struct PointToDraw
    {
        [SerializeField] private Vector2Int position;
        [SerializeField] private Color color;
        [SerializeField] private int radius;
        [SerializeField] private int layer;
        
        public PointToDraw(Vector2Int position, Color color, int radius, int layer = 0)
        {
            this.position = position;
            this.color = color;
            this.radius = radius;
            this.layer = layer;
        }

        public Vector2Int Position => position;

        public Color Color => color;

        public int Radius => radius;

        public int Layer => layer;
    }
}