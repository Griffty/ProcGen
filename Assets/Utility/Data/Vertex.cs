using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Griffty.Utility.Data
{
    [Serializable]
    public class Vertex
    {
        public int Index => index;
        public Vector2Int Position
        {
            get => position;
            set => position = value;
        }

        public IEnumerable<int> ConnectedVertices
        {
            get => connectedVertices;
            set => connectedVertices = (List<int>)value;
        }

        public IEnumerable<int> AdjacentPolygons => adjacentPolygons;

        [SerializeField] private int index;
        [SerializeField] private Vector2Int position;
        [SerializeField] private List<int> connectedVertices;
        [SerializeField] private List<int> adjacentPolygons;

        public Vertex(Vector2Int position, int index, List<int> adjacentPolygons, List<int> connectedVertices = null)
        {
            connectedVertices ??= new List<int>();
        
            this.position = position;
            this.connectedVertices = connectedVertices;
            this.adjacentPolygons = adjacentPolygons;
            this.index = index;
        }
        public void AddConnectedVertex(int index)
        {                                                          
            connectedVertices.Add(index);
        }
        public void RemoveConnectedVertex(int index)
        {                                                          
            connectedVertices.Remove(index);
        }
    
        public static Vertex FindByIndex(List<Vertex> vertices, int i)
        {
            return vertices.FirstOrDefault(vertex => vertex.Index == i);
        }
    }
}