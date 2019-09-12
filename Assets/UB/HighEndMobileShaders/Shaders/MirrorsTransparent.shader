Shader "UB/MirrorsTransparent"
{
        Properties
        {
            _Color("Main Color", Color) = (1,1,1,1)
            _MainTex("Texture", 2D) = "white" {}
            _BumpMap("Normalmap", 2D) = "bump" {}
            _ReflAmount("Reflection amount", float) = 0.5
            _ReflDistort("Reflection distort", float) = 0.25
            _Blur("Blur Iteration", float) = 0
            _BlurNeighbour("Blur Neighbour Pixel", float) = 1
            [HideInInspector]_ReflectionTex("Reflection", 2D) = "white" { }
        }

        SubShader
        {
            Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
            Lighting Off
            Fog{ Mode Off }
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100

        Pass
            {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _BumpMap;
            float4 _BumpMap_ST;
            sampler2D _ReflectionTex;
            float4 _Color;
            float _ReflAmount;
            float _ReflDistort;
            float _Blur; 
            float _BlurNeighbour;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _BumpMap);
                return o;
            }

            float2 Circle(float mStart, float mPoints, float mPoint)
            {
                float Rad = (3.141592 * 2.0 * (1.0 / mPoints)) * (mPoint + mStart);
                return float2(sin(Rad), cos(Rad));
            }

            #define FLT_MAX 3.402823466e+38
            #define FLT_MIN 1.175494351e-38
            #define DBL_MAX 1.7976931348623158e+308
            #define DBL_MIN 2.2250738585072014e-308

            fixed4 frag(v2f i) : COLOR
            {
                fixed3 nor = UnpackNormal(tex2D(_BumpMap, i.uv1));
            
                fixed2 screenUV = (i.screenPos.xy) / (i.screenPos.w+FLT_MIN);

                screenUV.xy += nor *_ReflDistort;
                fixed4 refl = tex2D(_ReflectionTex, screenUV);

                fixed3 _check = fixed3(0, 0, 0);
                fixed4 _transparent = fixed4(0, 0, 0, 0);
                fixed4 _color;  

                float stepX = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).x);
                float stepY = (1.0 / (_ScreenParams.xy / _ScreenParams.w+FLT_MIN).y);
                fixed4 blur = fixed4(0, 0, 0, 0); 
                fixed2 copyUV;
                for (int x = 1; x <= _Blur; x++)
                {
                    copyUV = screenUV + Circle(0, _Blur, x)*float2(stepX, stepY)*_BlurNeighbour;
                    fixed4 neighbour = tex2D(_ReflectionTex, copyUV);
                    if (any(neighbour.rgb == _check)) { //do not blur outside of the boundaries
                        blur += tex2D(_ReflectionTex, screenUV) / _Blur; //get orijinal color
                    }
                    else {
                        blur += neighbour / _Blur; //get neighbour
                    }
                }
                
                if (any(refl.rgb == _check)) {
                    _color = _transparent;
                }
                else{
                    if (_Blur > 0)
                    {
                        refl = blur;
                    }
                    fixed4 tex = tex2D(_MainTex, i.uv);
                    _color = refl*_ReflAmount + (1 - _ReflAmount)*tex*_Color;
                    _color.a = _Color.a;
                }
                return _color;
            }
            ENDCG
        }
    }
}


