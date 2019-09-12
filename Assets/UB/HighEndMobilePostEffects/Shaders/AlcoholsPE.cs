//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class AlcoholsPE : EffectBase
    {
        public bool SeperateColorChannels = true;
        public float Size = .02f;
        public float MainMultiplier = 1f;
        public float RedMultiplier = .3f;
        public float GreenMultiplier = .3f;
        public float BlueMultiplier = .3f;

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

                if (_material.HasProperty("_Seperate"))
                {
                    _material.SetFloat("_Seperate", SeperateColorChannels ? 1 : 0);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Main"))
                {
                    _material.SetFloat("_Main", MainMultiplier);
                }
                if (_material.HasProperty("_Red"))
                {
                    _material.SetFloat("_Red", RedMultiplier);
                }
                if (_material.HasProperty("_Green"))
                {
                    _material.SetFloat("_Green", GreenMultiplier);
                }
                if (_material.HasProperty("_Blue"))
                {
                    _material.SetFloat("_Blue", BlueMultiplier);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}