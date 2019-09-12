// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/SimpleDisplacements" {
    Properties {
		[HideInInspector]_Amount("Amount", Float) = 0.08
		[HideInInspector]_Speed("Speed", Float) = 0.2
		[HideInInspector]_MainTex ("Base (RGB)", 2D) = "white" {}
		[HideInInspector]_NoiseTex ("Noise (RGB)", 2D) = "white" {}
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
			float _Amount;
			float _Speed;
			
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
    
			    float2 p = i.uv.xy;

			    float2 p2 = p + float2(_Time.x*_Speed,_Time.x*_Speed);

			    p += (tex2D(_NoiseTex, p2).rb-float2(.5,.5))* _Amount;
			        
				return tex2D(_MainTex, p);
			}
			ENDCG
		}
	}
}