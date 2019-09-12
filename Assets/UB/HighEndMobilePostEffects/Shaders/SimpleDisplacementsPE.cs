
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class SimpleDisplacementsPE : EffectBase
    {
        public float Amount = 0.08f;
        public float Speed = 0.2f;
        public Texture2D Noise;

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

                if (_material.HasProperty("_Amount"))
                {
                    _material.SetFloat("_Amount", Amount);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_NoiseTex"))
                {
                    _material.SetTexture("_NoiseTex", Noise);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}