// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/UnderWaters" {

	Properties	{
		[HideInInspector]_VerticalStrength("VerticalStrength", Float) = 4
		[HideInInspector]_HorizontalStrength("HorizontalStrength", Float) = 5
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}
	}

	Subshader{

		Pass {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //Tags { "Queue" = "Opaque" }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float _VerticalStrength;
			float _HorizontalStrength;
			sampler2D _MainTex;
			float4 _MainTex_ST;

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
				float y = 
					0.7*sin((IN.uv.y + _Time.y) * _VerticalStrength) * 0.038 +
					0.3*sin((IN.uv.y + _Time.y) * _VerticalStrength*2) * 0.010 +
					0.05*sin((IN.uv.y + _Time.y) * _VerticalStrength*10) * 0.05;

				float x = 
					0.5*sin((IN.uv.y + _Time.y) * _HorizontalStrength) * 0.1 +
					0.2*sin((IN.uv.x + _Time.y) * _HorizontalStrength*2) * 0.05 +
					0.2*sin((IN.uv.x + _Time.y) * _HorizontalStrength*6) * 0.02;

				return tex2D(_MainTex, 0.79*(IN.uv + float2(y+0.11, x+0.11)));
			}
			ENDCG
		}
	}
}



