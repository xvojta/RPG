using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapGeneration : MonoBehaviour
{
    Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Generate(bool[][] map)
    {
        // Create a new 2x2 texture ARGB32 (32 bit with alpha) and no mipmaps
        texture = new Texture2D(map.Length, map.Length, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        // set the pixel values
        for(int x = 0; x < map.Length; x++)
        {
            for(int y = 0; y < map[x].Length; y++)
            {
                var color = map[x][y] ? Color.white : Color.black;
                texture.SetPixel(x, y, color);
            }
        }

        // Apply all SetPixel calls
        texture.Apply();

        // connect texture to material of GameObject this script is attached to
        GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }
}
