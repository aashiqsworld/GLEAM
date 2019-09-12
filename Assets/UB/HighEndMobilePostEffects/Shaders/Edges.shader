// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Edges" {
	Properties{
        [HideInInspector]_Threshold("Threshold", Float) = .25
        [HideInInspector]_Color("Color", Color) = (.5, .5, .5, 1)
		[HideInInspector]_BackColor("Background Color", Color) = (0, 0, 0, 1)
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
            float4 _Color;
			float4 _BackColor;
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

			float threshold(in float val) {
				if (val < _Threshold) { return 0.0; }
				return val;
			}

			float avgIntensity(in float4 pix) {
				return (pix.r + pix.g + pix.b) / 3.;
			}

			float4 getPixel(in float2 uv, in float dx, in float dy) {
				return tex2D(_MainTex, uv + float2(dx, dy));
			}

			float isEdge(in float2 uv) {
				float dxtex = 1.0 / _ScreenParams.x;
				float dytex = 1.0 / _ScreenParams.y;
				float pix[9];
				int k = -1;

				for (int i = -1; i<2; i++) {
					for (int j = -1; j<2; j++) {
						k++;
						pix[k] = avgIntensity(getPixel(uv, float(i)*dxtex,
							float(j)*dytex));
					}
				}

				float delta = (abs(pix[1] - pix[7]) +
					abs(pix[5] - pix[3]) +
					abs(pix[0] - pix[8]) +
					abs(pix[2] - pix[6])
					) / 4.;

				return threshold(clamp(1.8*delta, 0.0, 1.0));
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				float edge = isEdge(i.uv);
                return (1-edge)*_BackColor+_Color*edge;
			}
			ENDCG
		}
	}
}

