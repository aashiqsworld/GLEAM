//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class BleachsBypassPE : EffectBase
    {
        public float Scale = 2;
        public Color Color = new Color(0.328125f, 0.328125f, 0.328125f, 0.0f);

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

                if (_material.HasProperty("_Scale"))
                {
                    _material.SetFloat("_Scale", Scale);
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