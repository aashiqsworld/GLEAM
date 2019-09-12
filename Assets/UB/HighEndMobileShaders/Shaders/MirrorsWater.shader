
Shader "UB/WatersMirror" {
    Properties {
        _Specular ("Specular", float) = 1
        _Gloss ("Gloss", float) = 0.2
        _Color ("Main color", Color) = (1,1,1,1)      
        _NormalMap("Normal map", 2D) = "bump" {}
        _NoiseTex ("Noise (RGB)", 2D) = "white" {}
        _ReflAmount ("Reflection amount", Range (0, 1)) = 1
        _ReflDistort("Reflection distort", float) = 0.08
        _Speed("Wave speed", float) = 0.6
        _TB("Top-Bottom Transparency", Range (0, 1)) = 0.5
        [HideInInspector]_ReflectionTex ("Reflection", 2D) = "white" { }
        [HideInInspector]_ReflectionTex2 ("Reflection2", 2D) = "white" { }
    }
   
    SubShader {
        Tags { 
            "RenderType" = "Opaque"
        }

        CGPROGRAM

        #pragma surface surf WetSpecular fullforwardshadows

        struct Input {
            float2 uv_NormalMap;
            float2 uv_NoiseTex;
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

        uniform fixed4 _Color;
        uniform sampler2D _NormalMap;
        uniform sampler2D _NoiseTex;
        uniform float _ReflAmount;
        uniform float _ReflDistort;
        uniform float _Speed;
        uniform float _Specular;
        uniform float _Gloss;
        uniform float _TB;
        uniform sampler2D _ReflectionTex;
        uniform sampler2D _ReflectionTex2;

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308

        void surf (Input IN, inout SurfaceOutput o) {
           
            fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);

            half3 col_orig = tex2D(_NoiseTex, IN.uv_NoiseTex + _Speed*_Time.y).rgb;
            screenUV += col_orig.r*_ReflDistort- _ReflDistort/2;
            fixed4 refl = tex2D(_ReflectionTex, screenUV);
            fixed4 refl2 = tex2D(_ReflectionTex2, screenUV);

            float2 waveUV = IN.uv_NoiseTex+col_orig.g*_ReflDistort - _ReflDistort / 2;

            fixed3 nor = UnpackNormal(tex2D(_NormalMap, waveUV));
            
            o.Normal = nor;
            //o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex))* (1-mask.a)+normalize(tex2D(_MainTex, p*_Wave))*mask.a;
            o.Albedo = _Color.rgb* (1-_ReflAmount);//tex.rgb *_Color.rgb;
            o.Emission = (refl*_TB + refl2*(1-_TB)) *_ReflAmount;
            o.Specular = _Specular;
            o.Gloss =  _Gloss; 
        }
        ENDCG
    }
   
    FallBack "Diffuse"
}
