Shader "UB/InvisiblesDirectional" {
    
        Properties{
            _Color("Shadow Color", Color) = (0,0,0,0.5)
            //[HideInInspector]_Cutoff("Alpha cutoff", Range(0,1)) = 0
        }

        SubShader{
            Tags{
            "Queue" = "AlphaTest"
            "IgnoreProjector" = "True"
            "RenderType" = "TransparentCutout"
            }
            LOD 200
            //ZWrite off
            Blend SrcAlpha OneMinusSrcAlpha//One One //Zero SrcColor

            CGPROGRAM
            #pragma surface surf ShadowOnly alphatest:_Cutoff

            fixed4 _Color;
            //float _ShadowInt;

            struct Input {
                float2 uv_MainTex;
            };

            inline fixed4 LightingShadowOnly(SurfaceOutput s, fixed3 lightDir, fixed atten)
            {
                fixed4 c;
                //c.rgb = lerp(s.Albedo, float3(1.0,1.0,1.0), atten);
                //c.rgb = lerp(_Color.rgb, float3(1.0, 1.0, 1.0), atten);
                c.rgb = s.Albedo; // _Color.rgb*atten;
                c.a = (1.0 - atten) *_Color.a;
                return c;
            }

            void surf(Input IN, inout SurfaceOutput o) {
                //o.Albedo = lerp(float3(1.0, 1.0, 1.0), _Color.rgb, _Color.a);
                o.Albedo = _Color.rgb;// *_Color.a;
                o.Alpha = 1;//_Color.a;
            }
            ENDCG
        }
    Fallback "Transparent/Cutout/VertexLit"
}
