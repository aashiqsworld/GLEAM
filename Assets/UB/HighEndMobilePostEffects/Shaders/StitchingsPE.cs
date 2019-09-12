//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class StitchingsPE : EffectBase
    {
        public bool Invert = true;
        public float Size = 8f;
        public Color Color = new Color(0.0f, 0.0f, 0.0f);

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

                if (_material.HasProperty("_Invert"))
                {
                    _material.SetFloat("_Invert", Invert ? 1 : 0);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
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