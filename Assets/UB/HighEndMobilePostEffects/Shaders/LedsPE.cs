
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class LedsPE : EffectBase
    {
        public float LedSize = 2f;
        public float Luminance = 1.5f;
        public Color ScanColor = new Color(0.5f, 0.5f, 0.5f, 1f);

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

                if (_material.HasProperty("_LedSize"))
                {
                    _material.SetFloat("_LedSize", LedSize);
                }
                if (_material.HasProperty("_Luminance"))
                {
                    _material.SetFloat("_Luminance", Luminance);
                }
                if (_material.HasProperty("_ScanColor"))
                {
                    _material.SetColor("_ScanColor", ScanColor);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}