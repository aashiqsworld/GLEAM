// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/Mosaics" {

	Properties	{
		_BlockSize("Block Size", Float) = 15.0
		[HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
	}

	Subshader{

		Pass {
			Tags { "Queue" = "Opaque" }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			uniform float _BlockSize;
			uniform sampler2D _ReflectionTex;

			struct vertexOutput {
				float4 vertex : POSITION;
				float4 screen_pos : TEXCOORD0;
			};

			vertexOutput vert(appdata_img v)
			{
				vertexOutput o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screen_pos = ComputeScreenPos(o.vertex);
				return o;
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				float2 screenUV = i.screen_pos.xy / i.screen_pos.w;

				float2 size = (_ScreenParams.zw - 1.0) * _BlockSize;

				screenUV = screenUV - fmod(screenUV, size) + size*0.5;

				return tex2D(_ReflectionTex, screenUV);
			}
			ENDCG
		}
	}
}