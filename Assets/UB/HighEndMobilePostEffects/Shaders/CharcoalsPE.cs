//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class CharcoalsPE : EffectBase
    {
        public float Strength = 100;
        public Color LineColor = new Color(0.328125f, 0.328125f, 0.328125f, 0.0f);

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

                if (_material.HasProperty("_Strength"))
                {
                    _material.SetFloat("_Strength", Strength);
                }
                if (_material.HasProperty("_LineColor"))
                {
                    _material.SetColor("_LineColor", LineColor);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}