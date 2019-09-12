Shader "UB/Distortions" {
    Properties{
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (A)", 2D) = "white" {}
        _ReflDistort("Reflection distort", float) = 0.25
        [HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
    }

    SubShader{

        Tags{
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
        }

        CGPROGRAM

        #pragma surface surf Lambert alpha

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
        };

        uniform fixed4 _Color;
        uniform sampler2D _MainTex;
        uniform float _ReflAmount;
        uniform float _ReflDistort;
        uniform sampler2D _ReflectionTex;

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308

        void surf(Input IN, inout SurfaceOutput o) {

            float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x)*_ReflDistort;

            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);

            fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);

            screenUV.xy += tex*stepX; 

            fixed4 refl = tex2D(_ReflectionTex, screenUV);

            o.Emission = refl * _Color; 

            o.Alpha = tex.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
