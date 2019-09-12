
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class HeatsPE : EffectBase
    {
        public float Threshold = 0.3f;
        public float Noise = 0.8f;
        public float Light = 0.7f;

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

                if (_material.HasProperty("_Threshold"))
                {
                    _material.SetFloat("_Threshold", Threshold);
                }
                if (_material.HasProperty("_Noise"))
                {
                    _material.SetFloat("_Noise", Noise);
                }
                if (_material.HasProperty("_Light"))
                {
                    _material.SetFloat("_Light", Light);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}