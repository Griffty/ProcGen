using System;
using System.Collections.Generic;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Patterns;
using UnityEngine;

namespace City_Gen
{
    public class MainHandler : MonoBehaviour // todo: turn 3 vertex poly into city suares
    {
        [SerializeField] private Config configScript;
        [SerializeField] private bool resetConfig;
        public SpriteRenderer spriteRenderer;
    
        [SerializeField] private City.City city;

        [SerializeField] private List<CustomCityGenerator> generators;
        public static bool OnPause { get; set; }
        public static int PauseCount { get; private set; }
        public SpriteRenderer SpriteRenderer => spriteRenderer;
        public City.City City => city;

        private void OnApplicationQuit()
        {
            MultipleStaticInstances<SimpleDrawer>.RemoveAllInstances();
            Config.Instance.Reset();
        }

        private void Awake()
        {
            if (configScript == null)
            {
                throw new Exception("Main handler does not contain reference to Config Object");
            }
            Config.SetUpConfig(configScript, resetConfig);
        }
    

        public void NextIter()
        {
            PauseCount++;
            OnPause = false;
            CityGeneration();
        }

        public void CityGeneration()
        {
            OnPause = false;
            PauseCount = 0;

            generators = new List<CustomCityGenerator>(GetComponents<CustomCityGenerator>());
        
            Config.SetUpConfig(configScript, resetConfig);
            
            Debugger.Timer.Start();
            SimpleDrawer drawingTool = MultipleStaticInstances<SimpleDrawer>.GetInstance("main");
            drawingTool.Texture2D = new Texture2D(Config.Instance.ActualCitySize, Config.Instance.ActualCitySize);
        
            List<Vertex> intersectionPoints = VoronoiGenerator.Instance.GenerateMap(configScript.ActualCitySize, configScript.VPx, configScript.VPy);
            city = new City.City {Vertices = intersectionPoints};
            Debugger.AddTimeRecord("Voronoi");
            foreach (var generator in generators)
            {
                generator.Generate(drawingTool, city);
                Debugger.AddTimeRecord(generator.GetType().Name);
            }
        
            SpriteRenderer.sprite = Sprite.Create(drawingTool.Texture2D,
                new Rect(0, 0, Config.Instance.ActualCitySize, Config.Instance.ActualCitySize), new Vector2(0.5f, 0.5f));
            Debugger.Timer.Stop();
            Debugger.PrintDebugInfo();
            Debugger.ClearRecords();
        }
    }
}

