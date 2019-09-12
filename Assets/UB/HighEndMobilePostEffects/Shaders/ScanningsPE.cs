
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class ScanningsPE : EffectBase
    {
        public Color Luminance = new Color(0.2126f, 0.7152f, 0.0722f);
        public Color Color = new Color(1.0f, 0.5490f, 0.0392f);
        public float Speed = 2f;
        public float Threshold = 0.7f;
        public float ScanWidth = 200f;

        public Shader Shader;
        private Material _material;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {

            if (_material)
            {
                DestroyImmediate(_material);
                _material = null;
            }
            if (Shader)
            {
                _material = new Material(Shader);
                _material.hideFlags = HideFlags.HideAndDontSave;

                if (_material.HasProperty("_Luminance"))
                {
                    _material.SetColor("_Luminance", Luminance);
                }
                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_Threshold"))
                {
                    _material.SetFloat("_Threshold", Threshold);
                }
                if (_material.HasProperty("_ScanWidth"))
                {
                    _material.SetFloat("_ScanWidth", ScanWidth);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}