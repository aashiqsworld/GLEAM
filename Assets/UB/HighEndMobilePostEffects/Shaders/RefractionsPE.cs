
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class RefractionsPE : EffectBase
    {
        public float ReflDistort = 0.05f;
        public Texture2D Overlay;
        public Texture2D BumpMap;
        public float TileX = 1f;
        public float TileY = 1f;
        [Range(0,1)]
        public float Luminance = 1f;

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

                if (_material.HasProperty("_ReflDistort"))
                {
                    _material.SetFloat("_ReflDistort", ReflDistort);
                }
                if (_material.HasProperty("_Overlay"))
                {
                    _material.SetTexture("_Overlay", Overlay);
                }
                if (_material.HasProperty("_BumpMap"))
                {
                    _material.SetTexture("_BumpMap", BumpMap);
                }
                if (_material.HasProperty("_TileX"))
                {
                    _material.SetFloat("_TileX", TileX);
                }
                if (_material.HasProperty("_TileY"))
                {
                    _material.SetFloat("_TileY", TileY);
                }
                if (_material.HasProperty("_Luminance"))
                {
                    _material.SetFloat("_Luminance", Luminance);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}