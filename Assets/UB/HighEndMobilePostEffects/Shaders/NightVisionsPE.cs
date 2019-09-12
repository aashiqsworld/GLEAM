
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class NightVisionsPE : EffectBase
    {
        public float Noise = 0.5f;
        public float Power = 0.5f;
        public float Flicker = 0.1f;

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
                if (_material.HasProperty("_Power"))
                {
                    _material.SetFloat("_Power", Power);
                }
                if (_material.HasProperty("_Flicker"))
                {
                    _material.SetFloat("_Flicker", Flicker);
                }
                //if (_material.HasProperty("_Vignette"))
                //{
                //    _material.SetFloat("_Vignette", Vignette);
                //}
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}