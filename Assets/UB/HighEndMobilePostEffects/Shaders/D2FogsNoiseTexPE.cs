//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class D2FogsNoiseTexPE : EffectBase
    {
        public Color Color = new Color(1f, 1f, 1f, 1f);
        public Texture2D Noise;
        public float Size = 1f;
        public float HorizontalSpeed = 0.2f;
        public float VerticalSpeed = 0f;
        [Range(0.0f,5)]
        public float Density = 2f;
        //public bool DarkMode = false;
        //public float DarkMultiplier = 1f;

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

                if (_material.HasProperty("_Color"))
                {
                    _material.SetColor("_Color", Color);
                }
                if (_material.HasProperty("_NoiseTex"))
                {
                    _material.SetTexture("_NoiseTex", Noise);
                }
                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", HorizontalSpeed);
                }
                if (_material.HasProperty("_VSpeed"))
                {
                    _material.SetFloat("_VSpeed", VerticalSpeed);
                }
                //if (_material.HasProperty("_DarkMode"))
                //{
                //    _material.SetFloat("_DarkMode", DarkMode == true ? 1 : 0);
                //}
                //if (_material.HasProperty("_DarkMultiplier"))
                //{
                //    _material.SetFloat("_DarkMultiplier", DarkMultiplier);
                //}
                if (_material.HasProperty("_Density"))
                {
                    _material.SetFloat("_Density", Density);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}