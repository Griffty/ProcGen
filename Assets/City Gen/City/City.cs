using System;
using System.Collections.Generic;
using Griffty.Utility.Data;
using UnityEngine;

namespace City_Gen.City
{
    [Serializable]
    public class City
    {
        [SerializeField] private List<Polygon> cityPolygons;
        [SerializeField] private List<Polygon> polygons;
        [SerializeField] private List<Vertex> vertices;
        [SerializeField] private Vector4 boundaries;
        [SerializeField] private Vector2 cityCenter;
        [SerializeField] private List<District> districts;
        [SerializeField] private List<Vertex> wall;
    
        public List<Polygon> CityPolygons
        {
            get => cityPolygons;
            set => cityPolygons = value;
        }
        public List<Polygon> Polygons
        {
            get => polygons;
            set => polygons = value;
        }

        public List<Vertex> Vertices
        {
            get => vertices;
            set => vertices = value;
        }

        public Vector4 Boundaries
        {
            get => boundaries;
            set => boundaries = value;
        }

        public Vector2 CityCenter
        {
            get => cityCenter;
            set => cityCenter = value;
        }

        public List<District> Districts
        {
            get => districts;
            set => districts = value;
        }
    
        public List<Vertex> Wall
        {
            get => wall;
            set => wall = value;
        }
    
        public City()
        {
            Vertices = new List<Vertex>();
            Polygons = new List<Polygon>();
            Districts = new List<District>();
            Wall = new List<Vertex>();    
        }
    }
}
