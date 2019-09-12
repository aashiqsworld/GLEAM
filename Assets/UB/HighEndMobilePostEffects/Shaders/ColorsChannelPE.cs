//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class ColorsChannelPE : EffectBase
    {
        [Range(0, 1)]
        public float Red = 1;
        [Range(0, 1)]
        public float Green = 1;
        [Range(0, 1)]
        public float Blue = 1;
        public float Luminance = 1;

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

                if (_material.HasProperty("_Red"))
                {
                    _material.SetFloat("_Red", Red);
                }
                if (_material.HasProperty("_Green"))
                {
                    _material.SetFloat("_Green", Green);
                }
                if (_material.HasProperty("_Blue"))
                {
                    _material.SetFloat("_Blue", Blue);
                }
                if (_material.HasProperty("_Luminance"))
                {
                    _material.SetFloat("_Luminance", Luminance);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}