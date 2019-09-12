// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/BleachsBypass" {
    Properties{
        [HideInInspector]_Scale("Scale", Float) = 2
        [HideInInspector]_Color("Color",Color) = (0.328125, 0.328125, 0.328125, 0.0)
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
            float _Scale;
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

            float4 bleach(float4 dotP, float4 tex)
            {
                float4 a = float4(1,1,1,1);
                float4 b = float4(2,2,2,2);
                float dotColor = dot(tex, _Color);
                float sat = saturate((dotColor - 0.45) * 10.0);
                float4 t = b * tex * dotP;
                float4 w = a - (b * (a - tex) * (a - dotP));
                float4 to = lerp(t, w, float4(sat, sat, sat, sat));
                return lerp(tex, to, float4(_Scale, _Scale, _Scale, _Scale));
            }

            half4 frag(vertexOutput i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float d = dot(tex, _Color);
                float4 dotP = float4(float3(d, d, d), tex.a);
                return bleach(dotP, tex);
            }
            ENDCG
        }
    }
}