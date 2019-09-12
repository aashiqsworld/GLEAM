
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class DotsPE : EffectBase
    {
        public float BlockSize = 15f;
        public float DotRadius = 5f;
        public Color EmptyColor = new Color(0f, 0f, 0f, 1f);

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
                if (_material.HasProperty("_DotRadius"))
                {
                    _material.SetFloat("_DotRadius", DotRadius);
                }
                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", EmptyColor);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}