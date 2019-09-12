// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Mosaics" {

	Properties	{
		[HideInInspector]_BlockSize("Block Size", Float) = 15.0
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

			float _BlockSize;
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

			half4 frag(vertexOutput i) : SV_Target
			{
				float2 size = (_ScreenParams.zw - 1.0) * _BlockSize;
				i.uv = i.uv - fmod(i.uv, size) + size*0.5;
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}