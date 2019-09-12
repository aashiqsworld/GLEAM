
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class UnderWatersPE : EffectBase
    {
        public float VerticalStrength = 4f;
        public float HorizontalStrength = 5f;

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

                if (_material.HasProperty("_VerticalStrength"))
                {
                    _material.SetFloat("_VerticalStrength", VerticalStrength);
                }
                if (_material.HasProperty("_HorizontalStrength"))
                {
                    _material.SetFloat("_HorizontalStrength", HorizontalStrength);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}