
//#define DEBUG_RENDER

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;

namespace UB
{
    [ExecuteInEditMode]
    public class BlursFastPE : EffectBase
    {
        [Range(1, 100)]
        public int Iterations = 1;
        public float LoopNStepFor360Degree = 9;
        public float JumpNPixelsFromOrigin = 6;

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

                if (_material.HasProperty("_Loop"))
                {
                    _material.SetFloat("_Loop", LoopNStepFor360Degree);
                }
                if (_material.HasProperty("_Jump"))
                {
                    _material.SetFloat("_Jump", JumpNPixelsFromOrigin);
                }
            }

            CreateRenderTextures(source);

            if (Shader != null && _material != null)
            {
                for (int i = 1; i <= Iterations; i++)
                {
                    if (Iterations != 1)
                    {
                        if (i == 1)
                        {
                            Graphics.Blit(source, _reflectionTexture1, _material);
                        }
                        else if (i % 2 == 1)
                        {
                            Graphics.Blit(_reflectionTexture2, _reflectionTexture1, _material);
                        }
                        else
                        {
                            Graphics.Blit(_reflectionTexture1, _reflectionTexture2, _material);
                        }
                    }
                }

                if (Iterations == 1) //just blit source and dest
                {
                    Graphics.Blit(source, destination, _material);
                }
                else //use properties
                {
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

                if (_reflectionTexture2)
                    DestroyImmediate(_reflectionTexture2);
                _reflectionTexture2 = new RenderTexture(source.width, source.height, source.depth, source.format);
                _reflectionTexture2.name = "__MirrorReflection2" + GetInstanceID();
                _reflectionTexture2.isPowerOfTwo = true;
                _reflectionTexture2.hideFlags = HideFlags.DontSave;
            }
        }
    }
}