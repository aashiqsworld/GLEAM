//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class CrosshatchsPE : EffectBase
    {
        public Color Color1 = new Color(0f, 0f, 0f, 1f);
        public Color Color2 = new Color(1f, 1f, 1f, 1f);
        [Range(0,20)]
        public int Size = 5;
        [Range(0, 20)]
        public int Detail = 1;

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

                if (_material.HasProperty("_Color1"))
                {
                    _material.SetColor("_Color1", Color1);
                }
                if (_material.HasProperty("_Color2"))
                {
                    _material.SetColor("_Color2", Color2);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetInt("_Size", Size);
                }
                if (_material.HasProperty("_Detail"))
                {
                    _material.SetInt("_Detail", Detail);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}