// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "UB/PostEffects/BlackHoles"
{
    Properties
    {
        [HideInInspector]_HoleSize("HoleSize", Float) = 0.05
        [HideInInspector]_Position("Position", Vector) = (0.3,0.5,0)
        [HideInInspector]_Size("Size", Float) = 75
        [HideInInspector]_Radius("Radius", Float) = 10
        [HideInInspector]_Speed("Speed", Float) = 50
        [HideInInspector]_Luminance("Luminance", Float) = 2
        [HideInInspector]_MainTex("Base (RGB)", 2D) = "" {}
    }

    SubShader
    {
        Pass
        {
            Tags{ "Queue" = "Opaque" }
            Cull Off ZWrite Off ZTest Always
            //ZTest Always 
            //Cull Off
            //Fog{ Mode off }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _HoleSize;
            float _Size;
            float _Radius;
            float3 _Position;
            float _Speed;
            float _Luminance;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                //_Position += sin(_Time.y)/10; //TEST
                float2 uv = i.uv;
                float dist = distance(_Position.xy, i.uv);
                float hole = saturate(_HoleSize*distance(_Position.xy*_ScreenParams.xy, uv*_ScreenParams.xy) - 1.5);
                float holeToRotateSize = (_Size * 3) / _ScreenParams.x;

                float2 warp = normalize(_Position.xy - uv) *
                    pow(dist, -_Radius /_ScreenParams.x) * _Size / _ScreenParams.xy;
                
                uv = uv + warp;
                
                //rotating UV through around hole for N*HoleSize
                if (dist < holeToRotateSize)
                {
                    float distanceToSize = (dist / holeToRotateSize)*10;

                    const float Deg2Rad = (UNITY_PI * 2.0) / 360.0;

                    float rotationRadians = _Time.y * _Speed * Deg2Rad; // convert degrees to radians
                    
                    float s = sin(rotationRadians); // sin and cos take radians, not degrees
                    float c = cos(rotationRadians);

                    float2x2 rotationMatrix = float2x2(c, -s, s, c); // construct simple rotation matrix

                    half4 color = tex2D(_MainTex, uv) *hole; //save original color to blend

                    uv -= _Position.xy; // offset UV so we rotate around _Position and not 0.0
                    uv = mul(rotationMatrix, uv); // apply rotation matrix
                    uv += _Position.xy; // offset UV again so UVs are in the correct location
                    
                    return tex2D(_MainTex, uv) *hole*(1 - distanceToSize / 10)*_Luminance +color*distanceToSize/10;
                }
                else {
                    return tex2D(_MainTex, uv) *hole;
                }
            }
            ENDCG
        }
    }
}
