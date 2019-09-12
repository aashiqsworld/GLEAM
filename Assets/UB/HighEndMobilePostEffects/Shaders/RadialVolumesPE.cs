//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class RadialVolumesPE : EffectBase
    {
        public float Size = 45;
        public float Step = 10;
        [Range(0, 0.5f)]
        public float Smooth = 0.08f;

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

                if (_material.HasProperty("_Size"))
                {
                    _material.SetFloat("_Size", Size);
                }
                if (_material.HasProperty("_Step"))
                {
                    _material.SetFloat("_Step", Step);
                }
                if (_material.HasProperty("_Smooth"))
                {
                    _material.SetFloat("_Smooth", Smooth);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}