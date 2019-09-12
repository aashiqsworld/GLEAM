//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class PulsesPE : EffectBase
    {
        public float Speed = 5;
        public float Iteration = 10;
        public float Min = .1f;
        public float Max = .3f;

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

                if (_material.HasProperty("_Speed"))
                {
                    _material.SetFloat("_Speed", Speed);
                }
                if (_material.HasProperty("_Iteration"))
                {
                    _material.SetFloat("_Iteration", Iteration);
                }
                if (_material.HasProperty("_Min"))
                {
                    _material.SetFloat("_Min", Min);
                }
                if (_material.HasProperty("_Max"))
                {
                    _material.SetFloat("_Max", Max);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}