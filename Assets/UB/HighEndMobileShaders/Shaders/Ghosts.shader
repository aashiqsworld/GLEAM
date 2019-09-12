Shader "UB/Ghosts" {
	Properties{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
        _NoiseTex ("Noise (RGB)", 2D) = "white" {}
        _Speed("Wave speed", Float) = 0.6
		_ReflAmount ("Reflection amount", float) = 0.5
        _ReflDistort ("Reflection distort", float) = 0.25
		[HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
	}

	SubShader{

		Tags{
			"RenderType" = "Transparent"
			"IgnoreProjector" = "True"
			"Queue" = "Transparent"
		}

		CGPROGRAM

		#pragma surface surf Lambert alpha

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float4 screenPos;
		};

		uniform fixed4 _Color;
		uniform sampler2D _MainTex;
		uniform sampler2D _BumpMap;
        uniform sampler2D _NoiseTex;
		uniform float _ReflAmount;
		uniform float _ReflDistort;
        uniform float _Speed;
		uniform sampler2D _ReflectionTex;

        #define FLT_MAX 3.402823466e+38
        #define FLT_MIN 1.175494351e-38
        #define DBL_MAX 1.7976931348623158e+308
        #define DBL_MIN 2.2250738585072014e-308

		void surf(Input IN, inout SurfaceOutput o) {
			fixed2 screenUV = (IN.screenPos.xy) / (IN.screenPos.w+FLT_MIN);
    
		    float4 tex = tex2D(_MainTex, IN.uv_MainTex);
		    fixed3 nor = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));

		    //screenUV.xy += nor *_ReflDistort;

		    //float4 refl = tex2D(_ReflectionTex, screenUV);
            half3 col_orig = tex2D(_NoiseTex, IN.uv_MainTex + _Speed*_Time.y).rgb;
            screenUV += col_orig.r*_ReflDistort- _ReflDistort/2;

            //half3 col_orig = tex2D(_NoiseTex, 
            //    float2(IN.uv_MainTex.x,IN.uv_MainTex.y+_Speed*_Time.y)).rgb;
            //screenUV = float2(
            //    screenUV.x+col_orig.b*_ReflDistort,
            //    screenUV.y+col_orig.g*_ReflDistort);
            fixed4 refl = tex2D(_ReflectionTex, screenUV);

		 	o.Albedo = tex.rgb*(1-_ReflAmount);
		    o.Emission = refl * _Color *_ReflAmount;
		    o.Normal = nor.rgb;
		    o.Alpha = tex.a;
		}
		ENDCG
	}

	FallBack "Diffuse"
}

