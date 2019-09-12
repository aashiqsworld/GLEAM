
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class FocusedBlursPE : EffectBase
    {
        public Vector3 Focus = new Vector3(0.5f, 0.5f, 1f);
        public float Step = 10;
        [Range(0f, 0.5f)]
        public float Radius = 0.1f;
        public float NeighbourPixels = 10f;

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

                if (_material.HasProperty("_Focus"))
                {
                    _material.SetVector("_Focus", Focus);
                }
                if (_material.HasProperty("_Step"))
                {
                    _material.SetFloat("_Step", Step);
                }
                if (_material.HasProperty("_Radius"))
                {
                    _material.SetFloat("_Radius", Radius);
                }
                if (_material.HasProperty("_NeighbourPixels"))
                {
                    _material.SetFloat("_NeighbourPixels", NeighbourPixels);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}