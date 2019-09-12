// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Charcoals" {
    Properties{
        [HideInInspector]_LineColor("LineColor", Color) = (0,0,0,1)
        [HideInInspector]_Strength("Strength", Float) = 100
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
            float4 _MainTex_TexelSize;
            uniform fixed4 _LineColor;
            uniform float _Strength;
            
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
                half xP = _MainTex_TexelSize.x * 2;
                half yP = _MainTex_TexelSize.y * 2;
                fixed4 main = tex2D(_MainTex, i.uv);
                fixed3 tex0 = tex2D(_MainTex, i.uv + float2(xP, 0)).rgb;
                fixed3 tex1 = tex2D(_MainTex, i.uv + float2(0, yP)).rgb;
                half charcoal = 0;
                charcoal += abs(main.r - tex0.r);
                charcoal += abs(main.g - tex0.g);
                charcoal += abs(main.b - tex0.b);
                charcoal += abs(main.r - tex1.r);
                charcoal += abs(main.g - tex1.g);
                charcoal += abs(main.b - tex1.b);
                charcoal -= (1/_Strength);
                charcoal = saturate(charcoal);
                main.rgb = (1 - charcoal) + _LineColor.rgb * charcoal;
                return main;
            }
            ENDCG
        }
    }
}