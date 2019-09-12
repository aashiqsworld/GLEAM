
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class EdgesBlendPE : EffectBase
    {
        [Range(1, 10)]
        public int Strength = 1;
        [Range(1, 100)]
        public int Smooth = 8;
        public Color Color = new Color(0f, 0f, 0f, 1f);

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

                if (_material.HasProperty("_Smooth"))
                {
                    _material.SetFloat("_Smooth", (float)Smooth);
                }
                if (_material.HasProperty("_Strength"))
                {
                    _material.SetFloat("_Strength", (float)Strength);
                }
                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}