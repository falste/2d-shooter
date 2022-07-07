using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoise3DTest : MonoBehaviour {
    public Texture2D texture;
    public int size;
    public float scaling = 0.05f;
    public float speed = 0.3f;

    void Start () {
        texture = new Texture2D(size, size);
    }
    
    void Update () {
        //float min = float.MaxValue;
        //float max = float.MinValue;

        for (int x = 0; x < size; x++) {
            for (int y = 0; y < size; y++) {
                float val = Utility.Noise.PerlinNoise3D(x * scaling, y * scaling, Time.time * speed);

                //min = Mathf.Min(min, val);
                //max = Mathf.Max(max, val);

                val = (val+1)/2;
                Color color = new Color(val, val, val);
                texture.SetPixel(x, y, color);
            }
        }
        //Debug.Log(min + ", " + max);
        texture.Apply();
    }
}
