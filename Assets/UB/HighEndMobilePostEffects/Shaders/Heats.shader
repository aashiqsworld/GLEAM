// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/Heats" {

	Properties	{
		[HideInInspector]_Threshold("Threshold", Float) = 0.3
		[HideInInspector]_Noise("Noise", Float) = 0.8
		[HideInInspector]_Light("Light", Float) = 0.7
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _Threshold;
			float _Noise;
			float _Light;

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

			float rand(in float n) { return frac(sin(n)*43758.5453123); }

			float3 heat(float input) {
			    float output = 1.0 - input;
			    return (0.5+0.5*smoothstep(0.0, 0.1, output))*float3(
			      	smoothstep(0.5, 0.3, output),
			      	output < _Threshold ? smoothstep(0.0, 0.3, output) : smoothstep(1.0, 0.6, output),
			    	smoothstep(0.4, 0.6, output)
				);
			}

			half4 frag(vertexOutput IN) : SV_Target
			{
			    float3 color = tex2D(_MainTex, IN.uv).rgb;
			    color = heat((color.x+color.y+color.z)/3);
			    if (_Noise!=0)
			    {
				    float2 u = IN.uv * 2. - 1.;
				    float2 n = u * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
				    color+=rand((rand(n.x) + n.y) * _Time.y) * _Noise;
			    }
			    color*=_Light;

			    return float4(color, 1.0);    
			}
			ENDCG
		}
	}
}



