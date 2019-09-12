// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "UB/PostEffects/NightVisions" {

	Properties	{
		[HideInInspector]_Noise("Noise", Float) = 0.5
		[HideInInspector]_Power("Power", Float) = 0.5
		[HideInInspector]_Flicker("Flicker", Float) = 0.1
		//[HideInInspector]_Vignette("Vignette", Float) = 5
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

			float _Noise;
			float _Power;
			float _Flicker;
			//float _Vignette;
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

			float rand(in float n) { return frac(sin(n)*43758.5453123); }

			half4 frag(vertexOutput IN) : SV_Target
			{
				float2 u = IN.uv * 2. - 1.;
				float2 n = u * float2(_ScreenParams.x / _ScreenParams.y, 1.0);
				float3 color = tex2D(_MainTex, IN.uv).xyz;
			    
				color += rand((rand(n.x) + n.y) * _Time.y) * _Noise;
			    color *= smoothstep(0.001, 3.5, _Time.y) * _Power;
				
				color = dot(color, float3(0.2126, 0.7152, 0.0722)) 
				  * float3(0.2, 1.5 - rand(_Time.y) * _Flicker,0.4);

				float2 dist = (IN.uv - 0.5f);
        		//dist.x = 1 - dot(dist, dist)  * _Vignette * (_ScreenParams.y/_ScreenParams.x);
                dist.x = 1 - dot(dist, dist) * (_ScreenParams.y/_ScreenParams.x);
        		color *= dist.x;

        		//color *= 0.5 + 0.5*pow( 1.0*u.x*u.y*(1.0-u.x)*(1.0-u.y), 0.1 );
				
				return float4(color,1.0);
			}
			ENDCG
		}
	}
}



