// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/TwoColorMixes" {

	Properties	{
		[HideInInspector]_Luminance("Luminance", float) = 1
		[HideInInspector]_Color1("Color1", Color) = (0.1, 0.36, 0.8, 1)
		[HideInInspector]_Color2("Color2", Color) = (1.0, 0.8, 0.55,1)
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

			float _Luminance;
			float4 _Color1;
			float4 _Color2;
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
				float mix = dot(tex2D(_MainTex, i.uv).rgb, float3(0.333333, 0.333333, 0.333333));

				float3 col = lerp(_Color1.rgb * (1.0 - 2.0*abs(mix - 0.5)), _Color2.rgb, 1.0 - mix)*_Luminance;

				return float4(col, 1);
			}
			ENDCG
		}
	}
}



