// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Vintages" {
    Properties {
    	[HideInInspector]_Luminance("Luminance", float) = 1
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
			float _Luminance;

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
			    float4 color = tex2D(_MainTex,IN.uv);
			    float avg = (color.r + color.g + color.b) / 3.0;
				color.r *= abs(cos(avg));
			    color.g *= abs(sin(avg));
			    color.b *= abs(atan(avg) * sin(avg));
			    return color*_Luminance;
			}
			ENDCG
		}
	}
}