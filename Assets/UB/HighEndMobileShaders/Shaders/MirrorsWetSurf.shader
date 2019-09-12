Shader "UB/MirrorsWetSurf" {
    Properties {
        _Specular ("Specular", float) = 1
        _Gloss ("Gloss", float) = 0.2
        _Color ("Main color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap("Normal map", 2D) = "bump" {}
        _Mask ("Mask map", 2D) = "white" {}            
        _WetBumpMap("Wet normal map", 2D) = "bump" {}
        _NoiseTex ("Noise (RGB)", 2D) = "white" {}
        _CutOff ("Alpha cutoff", Range (0, 1)) = 0.
        _ReflAmount ("Reflection amount", float) = 0.5
        _ReflDistort("Reflection distort", float) = 0.08
        _Speed("Wave speed", Float) = 0.6
        [HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }

        [Space(20)]
        [Header(Ripple Settings)]
        [Toggle] _Animate("Animate ripple?", float) = 1
        _DropColor ("Drop color", Color) = (1,1,1,1)
        _Size("Size", float) = 0.5
        _Blur("Blur", float) = 4
        _Expansion("Expansion", float) = 3
        _RippleSpeed("Ripple speed", float) = 0.6
        _RingCount("Ring count", float) = 2
        _Iterations("Iterations", float) = 2
        _Power("Power", float) = 0.3
    }
   
    SubShader {
        Tags { 
            "RenderType" = "Opaque"
        }

        CGPROGRAM

        #pragma surface surf WetSpecular fullforwardshadows

        struct Input {
            float2 uv_MainTex;
            float2 uv_Mask;
            float4 screenPos;
            float3 viewDir;
            float3 worldPos;
        }; 

        half4 LightingWetSpecular (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
          half3 h = normalize (lightDir + viewDir);
          half diff = max (0, dot (s.Normal, lightDir));
          float nh = max (0, dot (s.Normal, h));
          float spec = pow (nh, s.Specular*128.0)* s.Gloss;
          half4 c;
          c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * (atten * 2);
          c.a = s.Alpha;
          return c;
      }

        inline fixed4 LightingWetSpecular_SingleLightmap (SurfaceOutput s, fixed4 color) {
            half3 lm = DecodeLightmap (color);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingWetSpecular_DualLightmap (
            SurfaceOutput s, fixed4 totalColor, fixed4 indirectOnlyColor, half indirectFade) {
            half3 lm = lerp (DecodeLightmap (indirectOnlyColor), DecodeLightmap (totalColor), indirectFade);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingWetSpecular_StandardLightmap (
            SurfaceOutput s, fixed4 color, fixed4 scale, bool surfFuncWritesNormal) {
            UNITY_DIRBASIS

            half3 lm = DecodeLightmap (color);
            half3 scalePerBasisVector = DecodeLightmap (scale);

            if (surfFuncWritesNormal)
            {
                half3 normalInRnmBasis = saturate (mul (unity_DirBasis, s.Normal));
                lm *= dot (normalInRnmBasis, scalePerBasisVector);
            }

            return fixed4(lm, 0);
        }
       
        uniform fixed4 _Color;
        uniform sampler2D _MainTex;
        uniform sampler2D _BumpMap;
        uniform sampler2D _WetBumpMap;
        uniform sampler2D _Mask;
        uniform sampler2D _NoiseTex;
        uniform float _ReflAmount;
        uniform float _ReflDistort;
        uniform float _Speed;
        uniform float _CutOff;
        uniform float _Specular;
        uniform float _Gloss;
        uniform sampler2D _ReflectionTex;

        uniform float _Animate;
        uniform float _Size;
        uniform float _Blur;
        uniform float _Expansion;
        uniform float _RippleSpeed;
        uniform float _RingCount;
        uniform float _Iterations;
        uniform float _Power;
        uniform fixed4 _DropColor;

        float randFloat(float2 inVal) {
            return frac(sin(dot(float2(inVal.x, inVal.y), float2(89.4516, 35.516))) * 13554.3651);

        }

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308

        void surf (Input IN, inout SurfaceOutput o) {
           
            fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
           
            fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);

            half3 col_orig = tex2D(_NoiseTex, IN.uv_Mask + _Speed*_Time.y).rgb;
            screenUV += col_orig.r*_ReflDistort- _ReflDistort/2;
            fixed4 refl = tex2D(_ReflectionTex, screenUV);

            float2 waveUV = IN.uv_Mask+col_orig.g*_ReflDistort - _ReflDistort / 2;

            fixed4 mask = tex2D(_Mask, IN.uv_Mask);
            mask = smoothstep(mask, 0, _CutOff);

            float4 animColor = float4(0., 0., 0., 0.);
            if(_Animate == 1)
            {
                const float pi = 3.141592;
                float time = _Time.y*_RippleSpeed;
            
                float2 uv;
                float2 uvStep;
            
                for (float i = 0.; i < _Iterations; i++) {
                    for (float xpos = -1.; xpos <= 1.; xpos++) {
                        for (float ypos = -1.; ypos <= 1.; ypos++) {
                            uv = IN.uv_Mask / _Size;
                            uv += i*float2(3.21, 2.561);
                            uv += float2(xpos*0.3333, ypos*0.3333);
                            uvStep = (ceil((uv*1.0 - float2(.5, .5))) / 1.);
                            uvStep += float2(xpos, ypos)*100.;
                            uv = float2(frac(uv.x + 0.5) - .5, frac(uv.y + 0.5) - .5);
            
                            float rand = randFloat(uvStep);
                            float loops = frac(time + rand);
                            //float iter = floor(time + rand);

                            float ringMap = _Blur*9.*distance(uv, sin(screenUV)*0.5);//randVec(iter + uvStep.x + uvStep.y)*0.5);
                            //float ringMap = _Blur*9.*distance(uv, randVec(iter + uvStep.x + uvStep.y)*0.5);
                            float clampMinimum = -(1. + ((_RingCount - 1.)*2.0));
                            ringMap = clamp((ringMap - _Expansion*_Blur*loops) + 1., clampMinimum, 1.);
            
                            float rings = (cos((ringMap + time)*pi) + 1.0) / 2.;
                            rings *= pow(1. - loops, 2.);
                            float bigRing = sin((ringMap - clampMinimum) / (1. - clampMinimum)*pi);
                            float result = rings * bigRing;
                            animColor += float4(result, result, result, result)*_Power;
                        }
                    }
                }
            }

            fixed3 texNor = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
            fixed3 nor = UnpackNormal(tex2D(_WetBumpMap, waveUV));
            
            o.Normal = texNor*(1-mask.a) + nor*mask.a;
            //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex))* (1-mask.a)+normalize(tex2D(_MainTex, p*_Wave))*mask.a;
            o.Albedo = tex.rgb *_Color.rgb* (1-mask.a);
            o.Emission = refl *_ReflAmount * mask.a + tex.rgb *_Color.rgb* (1-_ReflAmount) + animColor * mask.a * _DropColor;//
            o.Specular = _Specular;// *_ReflAmount;
            o.Gloss =  _Gloss;// * _ReflAmount;
        }
        ENDCG
    }
   
    FallBack "Diffuse"
}
