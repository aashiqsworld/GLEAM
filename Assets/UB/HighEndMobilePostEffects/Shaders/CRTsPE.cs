
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class CRTsPE : EffectBase
    {
        public float NoiseX = 0;
        public Vector3 Offset = Vector3.zero;
        public float RGBNoise = 0;
        public float ScanLineTail = 0.5f;
        public float ScanLineSpeed = 100;

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

                if (_material.HasProperty("_NoiseX"))
                {
                    _material.SetFloat("_NoiseX", NoiseX);
                }
                if (_material.HasProperty("_Offset"))
                {
                    _material.SetVector("_Offset", Offset);
                }
                if (_material.HasProperty("_RGBNoise"))
                {
                    _material.SetFloat("_RGBNoise", RGBNoise);
                }
                if (_material.HasProperty("_ScanLineTail"))
                {
                    _material.SetFloat("_ScanLineTail", ScanLineTail);
                }
                if (_material.HasProperty("_ScanLineSpeed"))
                {
                    _material.SetFloat("_ScanLineSpeed", ScanLineSpeed);
                }
            }

            if (Shader != null && _material != null)
            {
                Graphics.Blit(source, destination, _material);
            }
        }
    }
}