using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class InvokeFillHoles {
    // Use this for initialization
    public static void init () {
        InitFHoles();

        //For testing
        tex = new Texture2D(128, 128);
        //RandomizeColors();
    }
	
	// Update is called once per frame
	void Update () {

	}

    
   
    //For testing
    public static RawImage outputImage;
    static Texture2D tex;
    //


    static public ComputeShader _shader;
    private static int kernelFHoles;
    static ComputeBuffer colorsIn;
    static ComputeBuffer colorsOut;

    static void InitFHoles()
    {
        colorsIn = new ComputeBuffer(128 * 128 * 4, sizeof(float));
        colorsOut = new ComputeBuffer(128 * 128 * 4, sizeof(float));
        _shader = Resources.Load<ComputeShader>("CSFillHoles");
        kernelFHoles = _shader.FindKernel("FHoles");
        _shader.SetBuffer(kernelFHoles, "colorsA", colorsIn);
        _shader.SetBuffer(kernelFHoles, "colorsB", colorsOut);
        
    }

    public static void FHoles(Color[] colors)
    {
        colorsIn.SetData(colors);
        for (int i = 0; i < 15; i++)
        {
            _shader.Dispatch(kernelFHoles, 4, 4, 1);
        }
        colorsIn.GetData(colors);
    }
    

    Color[] RandomizeColors()
    {
        Color[] colors = new Color[128*128] ;
        for (int i = 0; i < 128*128; i++) {
            if (UnityEngine.Random.value > 0.9f) {
                colors[i] = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
            }
            else
            {
                colors[i] = new Color(0, 0, 0, 0);
            }
        }
        
        return colors;
    }
    Color[] cols;
    public void RandomizeUI()
    {
        cols = RandomizeColors();
        tex.SetPixels(cols);
        tex.Apply();
    }
    public void FillHolesUI()
    {
        //Color[] cols = RandomizeColors();
        //FillHoles2(cols);
        long startTime = CurrentTimeMillis();
        long elapsed = CurrentTimeMillis() - startTime;
        Debug.Log(elapsed);
        tex.SetPixels(cols);
        tex.Apply();
        outputImage.texture = tex;
    }





    private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long CurrentTimeMillis()
    {
        return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
    }
    public void OnDestroy()
    {
        colorsIn.Dispose();
        colorsOut.Dispose();
    }
}
