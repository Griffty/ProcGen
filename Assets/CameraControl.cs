using System.Collections;
using System.Collections.Generic;
using System.Linq;
using City_Gen;
using City_Gen.City;
using Griffty.Utility.Data;
using Griffty.Utility.Drawing;
using Griffty.Utility.Patterns;
using Griffty.Utility.Static.GMath;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private Camera cam;
    [SerializeField] private MainHandler mainHandler;
    [SerializeField] private Texture2D texture;
    private SimpleDrawer drawer;
    void Start()
    {
        cam = GetComponent<Camera>();
        drawer = MultipleStaticInstances<SimpleDrawer>.GetInstance("debug");
        texture = new Texture2D(Config.Instance.ActualCitySize, Config.Instance.ActualCitySize);
        texture.SetPixels(mainHandler.SpriteRenderer.sprite.texture.GetPixels());
        texture.Apply();
        drawer.ClearOnNewCanvas = true;
        drawer.ClearOnDispatch = true;
    }
    
    void Update()
    {
        Movement();
        Click();
    }

    private void Click()
    {
        if (Input.GetMouseButtonDown(0))
        {
            drawer.Texture2D = GetBasicTexture();
            Vector2 textureCord = GetTextureCordFromMousePosition();
            Vector2Int textureCordInt = new Vector2Int((int)textureCord.x + Config.Instance.ActualCitySize/2, (int)textureCord.y + Config.Instance.ActualCitySize/2);
            Debug.Log(textureCordInt);
            drawer.AddPoint(textureCordInt, Color.red, 2, 1000);
            City city = mainHandler.City;
            int index = -1;
            for (int i = 0; i < city.Districts.Count; i++)
            {
                if (PolygonUtils.IsPointInsidePolygon(textureCordInt, city.Districts[i].DistrictBoundaries))
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                Debug.Log("No district found");
                mainHandler.SpriteRenderer.sprite = Sprite.Create(drawer.Texture2D,
                    new Rect(0, 0, Config.Instance.ActualCitySize, Config.Instance.ActualCitySize), new Vector2(0.5f, 0.5f));
                return;
            }
            
            District district = city.Districts[index];
            drawer.AddMultipleLines(district.DistrictBoundaries, Color.magenta, 2, true, 1000);
            
            Debug.Log("District:" + index);
            
            foreach (var neighborhood in district.Neighborhoods.Where(neighborhood => PolygonUtils.IsPointInsidePolygon(textureCordInt, neighborhood.VerticesAsCoordinates)))
            { 
                drawer.AddMultipleLines(neighborhood.VerticesAsCoordinates, Color.green, 1, true, 1000);
                drawer.AddMultiplePoints(neighborhood.VerticesAsCoordinates, Color.cyan, 2, 1000);
            }
            mainHandler.SpriteRenderer.sprite = Sprite.Create(drawer.Texture2D,
                new Rect(0, 0, Config.Instance.ActualCitySize, Config.Instance.ActualCitySize), new Vector2(0.5f, 0.5f));
        }
    }
    private Vector2 GetTextureCordFromMousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        Vector3 localPos = mainHandler.SpriteRenderer.transform.InverseTransformPoint(worldPos);

        Rect spriteRect = mainHandler.SpriteRenderer.sprite.rect;
        Vector2 textureCord = new Vector2(
            (localPos.x / mainHandler.SpriteRenderer.bounds.size.x) * spriteRect.width,
            (localPos.y / mainHandler.SpriteRenderer.bounds.size.y) * spriteRect.height
        );
        
        return textureCord;
    }

    private Texture2D GetBasicTexture()
    {
        Texture2D baseTexture = new Texture2D(Config.Instance.ActualCitySize, Config.Instance.ActualCitySize);
        baseTexture.SetPixels(texture.GetPixels());
        baseTexture.Apply();
        return baseTexture;
    }

    private void Movement()
    {
        var vector3 = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            vector3.y += 1;
        }        
        if (Input.GetKey(KeyCode.S))
        {
            vector3.y += -1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            vector3.x += -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            vector3.x += 1;
        }

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            cam.orthographicSize -= Input.mouseScrollDelta.y / 2;
            cam.orthographicSize = Mathf.Max(cam.orthographicSize, 0.5f);
        }

        transform.position += vector3.normalized * speed;
    }
}
