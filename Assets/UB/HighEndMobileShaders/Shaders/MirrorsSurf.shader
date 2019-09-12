	Shader "UB/MirrorsSurf" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _ReflAmount ("Reflection amount", float) = 0.5
        _ReflDistort ("Reflection distort", float) = 0.25
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
            float2 uv_BumpMap;
            float4 screenPos;
        }; 
       
        uniform fixed4 _Color;
        uniform sampler2D _MainTex;
        uniform sampler2D _BumpMap;
        uniform float _ReflAmount;
        uniform float _ReflDistort;
        uniform sampler2D _ReflectionTex;

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

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308
          
        void surf (Input IN, inout SurfaceOutputStandard o) {
           
            fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
            fixed3 nor = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
           
            fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);
           
            screenUV.xy += nor *_ReflDistort;
           
            fixed4 refl = tex2D (_ReflectionTex, screenUV);
           
            o.Albedo = tex.rgb *_Color.rgb;
            o.Normal = nor.rgb;
            o.Emission = refl *_ReflAmount; 
        }
        ENDCG
    }
   
    FallBack "Diffuse"
}

