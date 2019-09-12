// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Thermals" {
    Properties{
        [HideInInspector]_Threshold("Threshold", Float) = 0.5
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
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
            float _Threshold;
            
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
                float4 tex = tex2D(_MainTex, i.uv);
                //float4 color[3];
                float4 color0 = float4(0., 0., 1., 1.);
                float4 color1 = float4(1., 1., 0., 1.);
                float4 color2 = float4(1., 0., 0., 1.);
                float lum = (tex.r + tex.g + tex.b) / 3.;
                int index = (lum < _Threshold) ? 0 : 1;
                float3 thermal;
                if (index==0)
                    thermal = lerp(color0, color1, (lum - float(index)*0.5) / 0.5);
                else
                    thermal = lerp(color1, color2, (lum - float(index)*0.5) / 0.5);
                return float4(thermal, 1.);
            }
            ENDCG
        }
    }
}