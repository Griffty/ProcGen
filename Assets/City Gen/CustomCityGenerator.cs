using Griffty.Utility.Drawing;
using UnityEngine;

namespace City_Gen
{
    public abstract class CustomCityGenerator : MonoBehaviour
    {
        [SerializeField] private bool useGenerator;
        [SerializeField] private bool useDrawer;
        public void Generate(SimpleDrawer drawer, City.City city)
        {
            if (useGenerator)
            {
                if (!MainHandler.OnPause)
                {
                    UseGenerator(city);
                }

                if (useDrawer)
                {
                    Draw(drawer, city);
                }
            }
        }
        protected abstract void UseGenerator(City.City city);
        public abstract void Draw(SimpleDrawer drawer, City.City city);
    }
}