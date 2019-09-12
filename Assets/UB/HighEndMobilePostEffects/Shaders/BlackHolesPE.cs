//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class BlackHolesPE : EffectBase
    {
        public float HoleSize = 0.05f;
        public Vector2 Position = new Vector2(0.3f, 0.5f);
        public float Size = 75;
        public float Radius = 10;
        public float Speed = 50;
        public float Luminance = 2;

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

                if (_material.HasProperty("_HoleSize"))
                {
                    _material.SetFloat("_HoleSize", HoleSize);
                }
                if (_material.HasProperty("_Position"))
                {
                    _material.SetVector("_Position", new Vector4(Position.x, Position.y, 0, 0));
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Radius"))
                {
                    _material.SetFloat("_Radius", Radius);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
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