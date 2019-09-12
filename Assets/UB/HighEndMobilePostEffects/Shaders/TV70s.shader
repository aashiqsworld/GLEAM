// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/TV70s" {
	Properties{
		[HideInInspector]_Corner("Corner", Float) = 50
		[HideInInspector]_Scan("Scan", Float) = 10
		[HideInInspector]_ShiftAmount("Shift Amount", Float) = 0.025
		[HideInInspector]_MainTex("Base (RGB)", 2D) = "white" {}
		[HideInInspector]_NoiseTex("Noise", 2D) = "white" {}
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
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float _Corner;
			float _Scan;
			float _ShiftAmount;

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
			
			float scanline(float2 uv) {
				return sin(_ScreenParams.y * uv.y * 0.7 - _Time.y * 10.0);
			}

			float slowscan(float2 uv) {
				return sin(_ScreenParams.y * uv.y * 0.02 + _Time.y * _Scan);
			}

			float noise(float2 uv) {
				return clamp(tex2D(_NoiseTex, uv.xy + _Time.y*6.0).r +
					tex2D(_NoiseTex, uv.xy - _Time.y*4.0).g, 0.96, 1.0);
			}

			float2 crt(float2 coord, float bend)
			{
				coord = (coord - 0.5) * 2.0;

				coord *= 0.5;

				coord.x *= 1.0 + pow((abs(coord.y) / bend), 2.0);
				coord.y *= 1.0 + pow((abs(coord.x) / bend), 2.0);

				coord = (coord / 1.0) + 0.5;

				return coord;
			}

			float2 colorshift(float2 uv, float amount, float rand) {

				return float2(
					uv.x,
					uv.y + amount * rand
				);
			}

			float2 scandistort(float2 uv) {
				float scan1 = clamp(cos(uv.y * 2.0 + _Time.y), 0.0, 1.0);
				float scan2 = clamp(cos(uv.y * 2.0 + _Time.y + 4.0) * 10.0, 0.0, 1.0);
				float amount = scan1 * scan2 * uv.x;

				uv.x -= 0.05 * lerp(tex2D(_NoiseTex, float2(uv.x, amount)).r * amount, amount, 0.9);

				return uv;

			}

            //remove this? and use 
			float vignette(float2 uv) {
				uv = (uv - 0.5) * 0.98;
				return clamp(pow(cos(uv.x * 3.1415), 1.2) * pow(cos(uv.y * 3.1415), 1.2) * _Corner, 0.0, 1.0);
			}

			half4 frag(vertexOutput i) : SV_Target
			{
				float2 sdUv = scandistort(i.uv);
				float2 crtUv = crt(sdUv, 2.0);

				float4 color;

				float4 rand = tex2D(_NoiseTex, float2(_Time.y * 0.01, _Time.y * 0.02));

				color.r = tex2D(_MainTex, crt(colorshift(sdUv, _ShiftAmount, rand.r), 2.0)).r;
				color.g = tex2D(_MainTex, crt(colorshift(sdUv, 0.01, rand.g), 2.0)).g;
				color.b = tex2D(_MainTex, crt(colorshift(sdUv, _ShiftAmount, rand.b), 2.0)).b;
				color.a = 1;

				float scanlineColor = scanline(crtUv);
				float slowscanColor = slowscan(crtUv);

				return lerp(color, lerp(float4(scanlineColor, scanlineColor, scanlineColor, scanlineColor), float4(slowscanColor, slowscanColor, slowscanColor, slowscanColor), 0.5), 0.05) *
					vignette(i.uv) *
					noise(i.uv);
			}
			ENDCG
		}
	}
}