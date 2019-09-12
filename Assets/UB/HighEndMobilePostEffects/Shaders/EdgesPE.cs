
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class EdgesPE : EffectBase
    {
        public float Threshold = .25f;
        public Color Color = new Color(.5f, .5f, .5f, 1f);
        public Color BackgroundColor = new Color(0f, 0f, 0f, 1f);

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
                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
                if (_material.HasProperty("_BackColor"))
                {
                    _material.SetColor("_BackColor", BackgroundColor);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}