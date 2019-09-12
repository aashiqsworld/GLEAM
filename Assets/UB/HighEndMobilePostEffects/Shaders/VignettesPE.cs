
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class VignettesPE : EffectBase
    {
        public float Size = 5.8f;
        public float Blackness = 0.32f;

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
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Blackness"))
                {
                    _material.SetFloat("_Blackness", Blackness);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}