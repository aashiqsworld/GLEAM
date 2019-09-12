
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class GlitchsPE : EffectBase
    {
        public float Noise = 0.04f;
        public float Speed = 10f;
        public float Size = 3f;

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

                if (_material.HasProperty("_Noise"))
                {
                    _material.SetFloat("_Noise", Noise);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}