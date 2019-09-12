
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class HorizontalWavesPE : EffectBase
    {
        public float Speed = 10f;
        public float SizeX = 30f;
        public float SizeY = 10f;

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

                if (_material.HasProperty("_SizeX"))
                {
                    _material.SetFloat("_SizeX", SizeX);
                }
                if (_material.HasProperty("_SizeY"))
                {
                    _material.SetFloat("_SizeY", SizeY);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}