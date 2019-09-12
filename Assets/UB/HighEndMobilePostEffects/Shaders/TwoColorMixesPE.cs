
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class TwoColorMixesPE : EffectBase
    {
        public float Luminance = 1f;
        public Color Color1 = new Color(0.1f, 0.36f, 0.8f, 1f);
        public Color Color2 = new Color(1.0f, 0.8f, 0.55f, 1f);

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
                    _material.SetFloat("_Luminance", Luminance);
                }
                if (_material.HasProperty("_Color1"))
                {
                    _material.SetColor("_Color1", Color1);
                }
                if (_material.HasProperty("_Color2"))
                {
                    _material.SetColor("_Color2", Color2);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}