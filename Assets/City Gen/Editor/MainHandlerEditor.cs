using City_Gen;
using Griffty.Utility.Patterns;
using Griffty.Utility.Drawing;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainHandler))]
public class MainHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Generate"))
        {
            if (Config.IsSetUp)
            {
                Config.Instance.Reset();
            }
            
            MainHandler mainHandler = (MainHandler) target;
            mainHandler.CityGeneration();
        }
        
        if (GUILayout.Button("Next Iteration"))
        {
            MainHandler mainHandler = (MainHandler) target;
            mainHandler.NextIter();
        }
    }
}
