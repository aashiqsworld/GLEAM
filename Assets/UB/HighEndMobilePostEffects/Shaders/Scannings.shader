// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/Scannings" {
    Properties {
    	[HideInInspector]_Luminance("Luminance", Color) = (0.2126, 0.7152, 0.0722, 1)
    	[HideInInspector]_Color("Color", Color) = (1.0, 0.5490, 0.0392, 1)
		[HideInInspector]_Speed("Speed", Float) = 2
		[HideInInspector]_Threshold("Threshold", Float) = 0.7
		[HideInInspector]_ScanWidth("Scan Width", Float) = 200
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
			float4 _Luminance;
			float _Speed;
			float _ScanWidth;
			float _Threshold;
			float4 _Color;
			
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

			float RGB2Luminance(in float3 rgb)
			{
			    return _Luminance.r * rgb.r + _Luminance.g * rgb.g + _Luminance.b * rgb.b;
			}

			half4 frag(vertexOutput IN) : SV_Target
			{
				float2 pixelSize = float2(1.0,1.0) / _ScreenParams.xy;

			    float tl = RGB2Luminance(tex2D(_MainTex, IN.uv - pixelSize.xy).rgb);
			    float t = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(0.0, -pixelSize.y)).rgb);
			    float tr = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(pixelSize.x, -pixelSize.y)).rgb);

			    float cl = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(-pixelSize.x, 0.0)).rgb);
				float c = RGB2Luminance(tex2D(_MainTex, IN.uv).rgb);
			    float cr = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(pixelSize.x, 0.0)).rgb);

			    float bl = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(-pixelSize.x, pixelSize.y)).rgb);
				float b = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(0.0, pixelSize.y)).rgb);
			    float br = RGB2Luminance(tex2D(_MainTex, IN.uv + float2(pixelSize.x, pixelSize.y)).rgb);

			    float sobelX = tl * -1.0 + tr * 1.0 + cl * -2.0 + cr * 2.0 + bl * -1.0 + br * 1.0;
			    float sobelY = tl * -1.0 + t * -2.0 + tr * -1.0 + bl * 1.0 + b * 2.0 + br * 1.0;

			    float sobel = sqrt(sobelX * sobelX + sobelY * sobelY);

			    float scanlineX = sin(_Time.y * _Speed) * 0.5 + 0.5;
			    float4 textureColor = tex2D(_MainTex, IN.uv);
			    float pixelWidth = 1.0 / _ScreenParams.x;
			    float x = IN.uv.x;

			    float distanceToScanline = clamp(0.0, pixelWidth * _ScanWidth, 
			    	distance(scanlineX, x)) / (pixelWidth * _ScanWidth);

			    if (scanlineX > x - pixelWidth * _ScanWidth && scanlineX < 
			    	x + pixelWidth * _ScanWidth)
			    {
			        if (sobel < _Threshold)
			        {
			            return float4(lerp(float3(c,c,c), textureColor.rgb, 
			            	smoothstep(0.4, 0.6, distanceToScanline)), 1.0);
			        }
			        else
			        {
			            return float4(lerp(_Color.xyz, textureColor.rgb, 
			            	smoothstep(0.1, 0.9, distanceToScanline)), 1.0);
			        }
			    }
			    else
			    {
					return float4(textureColor.rgb, 1.0);
			    }
			}
			ENDCG
		}
	}
}