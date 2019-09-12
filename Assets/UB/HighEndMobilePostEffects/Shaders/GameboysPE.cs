
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class GameboysPE : EffectBase
    {
        public float BlockSize = 3;
        public Color Color1 = new Color(0.03137f, 0.09803f, 0.12549f);
        public Color Color2 = new Color(0.19607f, 0.41568f, 0.30980f);
        public Color Color3 = new Color(0.53725f, 0.75294f, 0.43529f);
        public Color Color4 = new Color(0.87450f, 0.96470f, 0.81568f);

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

                if (_material.HasProperty("_BlockSize"))
                {
                    _material.SetFloat("_BlockSize", BlockSize);
                }
                if (_material.HasProperty("_Color1"))
                {
                    _material.SetColor("_Color1", Color1);
                }
                if (_material.HasProperty("_Color2"))
                {
                    _material.SetColor("_Color2", Color2);
                }
                if (_material.HasProperty("_Color3"))
                {
                    _material.SetColor("_Color3", Color3);
                }
                if (_material.HasProperty("_Color4"))
                {
                    _material.SetColor("_Color4", Color4);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}