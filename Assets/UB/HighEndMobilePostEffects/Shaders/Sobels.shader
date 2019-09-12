// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Sobels" {
    Properties {
    	[HideInInspector]_Color("Color", Color) = (1.0, 0.4, 0.0, 1)
		[HideInInspector]_Detail("Detail", Float) = 5
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
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
			float4 _Color;
			float _Detail;
			
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

			half4 frag(vertexOutput IN) : SV_Target
			{
				float glow = 0.0;
				float2 e = float2(0.1, 0.1) * 0.04;	
				float gx = 0.0, gy = 0.0;
				float push = _Detail;
				
				gx += tex2D(_MainTex, IN.uv + float2(-e.x, 0)).r * 1.0 * push;
				gx += tex2D(_MainTex, IN.uv + float2(e.x, 0)).r * -1.0 * push;
				gy += tex2D(_MainTex, IN.uv + float2(0, -e.x)).r * 1.0 * push;
				gy += tex2D(_MainTex, IN.uv + float2(0, e.x)).r * -1.0 * push;

				glow = gx + gy;
				glow = max(-0.1, glow-0.1);

				float3 col = _Color * (glow);

				return float4(col, 1.0);
			}
			ENDCG
		}
	}
}