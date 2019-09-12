Shader "UB_Old/MirrorsAlphaSurf" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
		_Mask ("Maskmap", 2D) = "white" {}
		_ReflAmount ("Reflection amount", float) = 0.5
		_Animate("Animate 1 or 0", float) = 1
		_DropColor ("Drop Color", Color) = (1,1,1,1)
		_Size("Size", float) = 0.5
		_Blur("Blur", float) = 4
		_Expansion("Expansion", float) = 3
		_Speed("Speed", float) = 0.6
		_RingCount("RingCount", float) = 2
		_Iterations("Iterations", float) = 2
		_Power("Power", float) = 0.3
        [HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }
    }
   
    SubShader {
        Tags { 
        	"RenderType" = "Opaque"
        }

        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows

        struct Input {
            float2 uv_MainTex;
			float2 uv_Mask;
			float4 screenPos;
			float3 viewDir;
        }; 

        half4 LightingStandard (SurfaceOutputStandard s, half3 lightDir, half atten) {
            half NdotL = dot (s.Normal, lightDir);
            half4 c; c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten * 2);
            c.a = s.Alpha;
            return c;
        }

        inline fixed4 LightingStandard_SingleLightmap (SurfaceOutputStandard s, fixed4 color) {
            half3 lm = DecodeLightmap (color);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingStandard_DualLightmap (SurfaceOutputStandard s, fixed4 totalColor, fixed4 indirectOnlyColor, half indirectFade) {
            half3 lm = lerp (DecodeLightmap (indirectOnlyColor), DecodeLightmap (totalColor), indirectFade);
            return fixed4(lm, 0);
        }

        inline fixed4 LightingStandard_StandardLightmap (SurfaceOutput s, fixed4 color, fixed4 scale, bool surfFuncWritesNormal) {
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
		uniform sampler2D _Mask;
		uniform float _ReflAmount;
        uniform float _Animate;
        uniform float _Size;
        uniform float _Blur;
        uniform float _Expansion;
        uniform float _Speed;
        uniform float _RingCount;
        uniform float _Iterations;
        uniform float _Power;
        uniform fixed4 _DropColor;
        uniform sampler2D _ReflectionTex;

        float2 randVec(float inVal) {

			return float2(frac(sin(dot(float2(inVal*1.1, 2352.75053), float2(12.9898, 78.233))) * 43758.5453) - 0.5,
				frac(sin(dot(float2(715.23515, inVal), float2(27.2311, 31.651))) * 65161.6513) - 0.5);

		}

		float randFloat(float2 inVal) {
			return frac(sin(dot(float2(inVal.x, inVal.y), float2(89.4516, 35.516))) * 13554.3651);

		}

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308
       
        void surf (Input IN, inout SurfaceOutputStandard o) {
           
            fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
           
            fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);

            fixed4 refl = tex2D (_ReflectionTex, screenUV);

			fixed4 mask = tex2D(_Mask, IN.uv_Mask);
			 
			float4 animColor = float4(0., 0., 0., 0.);
			if(_Animate == 1)
			{
				const float pi = 3.141592;
				float time = _Time.y*_Speed;
            
				float2 uv;
				float2 uvStep;
			
				for (float i = 0.; i < _Iterations; i++) {
					for (float xpos = -1.; xpos <= 1.; xpos++) {
						for (float ypos = -1.; ypos <= 1.; ypos++) {
							uv = IN.uv_MainTex / _Size;
							uv += i*float2(3.21, 2.561);
							uv += float2(xpos*0.3333, ypos*0.3333);
							uvStep = (ceil((uv*1.0 - float2(.5, .5))) / 1.);
							uvStep += float2(xpos, ypos)*100.;
							uv = float2(frac(uv.x + 0.5) - .5, frac(uv.y + 0.5) - .5);
            
							float rand = randFloat(uvStep);
							float loops = frac(time + rand);
							float iter = floor(time + rand);
            

							float ringMap = _Blur*9.*distance(uv, randVec(iter + uvStep.x + uvStep.y)*0.5);
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

            o.Metallic = _ReflAmount;
            o.Occlusion =  1 - _ReflAmount;
            //o.Normal = nor;
            o.Albedo = tex.rgb *_Color.rgb* (1-mask.a);
            //o.Emission = refl *_ReflAmount * mask.a + animColor * mask.a * _DropColor + tex.rgb *_Color.rgb* (1 - _ReflAmount);//
            o.Emission = refl *_ReflAmount * mask.a + tex.rgb *_Color.rgb* (1 - _ReflAmount);//


        }
        ENDCG
    }
   
    FallBack "Diffuse"
}
