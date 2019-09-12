// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/D2RainsFast" {
    Properties{
        [HideInInspector]_Density("Density", float) = 1.2
        [HideInInspector]_Speed("Speed", float) = 0.1
        [HideInInspector]_Exposure("Exposure", float) = 5
        [HideInInspector]_Direction("Dİrection", float) = -1.1
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
        [HideInInspector]_Color("Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_NoiseTex("Noise", 2D) = "white" {}
    }

    Subshader{

        Pass{
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags{ "Queue" = "Opaque" }

            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _Density;
            float _Speed;
            float _Exposure;
            float _Direction;
            float4 _Color;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct vertexOutput {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            vertexOutput vert(appdata v)
            {
                vertexOutput o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(vertexOutput i) : SV_Target
            {
                float2 st =  i.uv * float2(.5, .01)+float2(_Time.y*_Speed+i.uv.y*_Direction, _Time.y*_Speed);
                float r = tex2D(_NoiseTex, st).y * tex2D(_NoiseTex, st*.773).x * _Density;
                r = clamp(pow(abs(r), 23.0) * 13.0, 0.0, i.uv.y*.14);
                r *= _Exposure;
                float4 tex = tex2D(_MainTex, i.uv);
                return tex*(1-r)+r*_Color;
            }
            ENDCG
        }
    }
}