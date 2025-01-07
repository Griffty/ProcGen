using System;
using UnityEngine;

namespace Griffty.Utility.Drawing
{
    [Serializable]
    public struct LineToDraw
    {
        [SerializeField] private Vector2Int start;
        [SerializeField] private Vector2Int end;
        [SerializeField] private Color color;
        [SerializeField] private int thickness;
        [SerializeField] private int layer;

        public LineToDraw(Vector2Int start, Vector2Int end, Color color, int thickness = 1, int layer = 0)
        {
            this.start = start;
            this.end = end;
            this.color = color;
            this.layer = layer;
            this.thickness = thickness;
        }

        public Vector2Int Start => start;
        public Vector2Int End => end;
        public Color Color => color;
        public int Layer => layer;
        public int Thickness => thickness;
    }
}