// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/DistortedLens" {

	Properties	{
		[HideInInspector]_Power("Power", Float) = 0.7
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

			float _Power;
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
				float2 halfArea = float2(0.5,0.5);
				float2 distance = halfArea - i.uv;
				float l = length(distance);
				float2 uv = halfArea - distance*(1-l*_Power);

				return tex2D (_MainTex, uv);
			}
			ENDCG
		}
	}
}
