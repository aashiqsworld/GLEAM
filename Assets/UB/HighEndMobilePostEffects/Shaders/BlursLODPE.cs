
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class BlursLODPE : EffectBase
    {
        public int Lod = 5;
        [Range(1, 20)]
        public int Iterations = 1;
        public float NeighbourPixels = 10;

        public Shader Shader;
        private Material _material;

        private RenderTexture _reflectionTexture1;
        private RenderTexture _reflectionTexture2;

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

                if (_material.HasProperty("_CustomFloatParam2"))
                {
                    _material.SetFloat("_CustomFloatParam2", NeighbourPixels);
                }
                if (_material.HasProperty("_Lod"))
                {
                    _material.SetFloat("_Lod", Lod);
                }
            }

            CreateRenderTextures(source);

            if (Shader != null && _material != null)
            {
                for (int i = 1; i <= Iterations; i++)
                {
                    if (_material.HasProperty("_CustomFloatParam1"))
                        _material.SetFloat("_CustomFloatParam1", i);

                    if (i == 1) //on first iteration use source
                    {
                        Graphics.Blit(source, _reflectionTexture1, _material);
                        _reflectionTexture1.GenerateMips();
                    }
                    else if (i == 2) //on second iteration use _ref1
                    {
                        Graphics.Blit(_reflectionTexture1, _reflectionTexture2, _material);
                        _reflectionTexture2.GenerateMips();
                    }
                    else if (i % 2 == 1)
                    {
                        Graphics.Blit(_reflectionTexture2, _reflectionTexture1, _material);
                        _reflectionTexture1.GenerateMips();
                    }
                    else
                    {
                        Graphics.Blit(_reflectionTexture1, _reflectionTexture2, _material);
                        _reflectionTexture2.GenerateMips();
                    }

                }

                if (Iterations % 2 == 1)
                {
                    Graphics.Blit(_reflectionTexture1, destination, _material);
                }
                else
                {
                    Graphics.Blit(_reflectionTexture2, destination, _material);
                }
            }
        }

        private void CreateRenderTextures(RenderTexture source)
        {
            if (!_reflectionTexture1 || !_reflectionTexture2)
            {
                if (_reflectionTexture1)
                    DestroyImmediate(_reflectionTexture1);
                _reflectionTexture1 = new RenderTexture(source.width, source.height, source.depth, source.format);
                _reflectionTexture1.name = "__MirrorReflection1" + GetInstanceID();
                _reflectionTexture1.isPowerOfTwo = true;
                _reflectionTexture1.hideFlags = HideFlags.DontSave;
                _reflectionTexture1.useMipMap = true;
                _reflectionTexture1.autoGenerateMips = false;

                if (_reflectionTexture2)
                    DestroyImmediate(_reflectionTexture2);
                _reflectionTexture2 = new RenderTexture(source.width, source.height, source.depth, source.format);
                _reflectionTexture2.name = "__MirrorReflection2" + GetInstanceID();
                _reflectionTexture2.isPowerOfTwo = true;
                _reflectionTexture2.hideFlags = HideFlags.DontSave;
                _reflectionTexture2.useMipMap = true;
                _reflectionTexture2.autoGenerateMips = false;
            }
        }
    }
}