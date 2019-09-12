Shader "UB/Skyboxes" {
    Properties{
        _Tint("Tint Color", Color) = (.5, .5, .5, .5)
        [Gamma] _Exposure("Exposure", Range(0, 8)) = 1.0
        _Rotation("Rotation", Range(0, 360)) = 0
        _Cube("Environment Map   (HDR)", Cube) = "grey" {}
    }

    SubShader{
        Tags{ "Queue" = "Background" }

        Pass{
            //ZWrite Off
            Cull Front

            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag 

            #include "UnityCG.cginc"

            uniform samplerCUBE _Cube;
            uniform half4 _Cube_HDR;
            uniform half4 _Tint;
            uniform half _Exposure;
            uniform float _Rotation;

            struct vertexInput {
                float4 vertex : POSITION;
            };

            struct vertexOutput {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD1;
            };

            float4 RotateAroundYInDegrees(float4 vertex, float degrees)
            {
                float alpha = degrees * UNITY_PI / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float4(mul(m, vertex.xz), vertex.yw).xzyw;
            }

            vertexOutput vert(vertexInput input)
            {
                vertexOutput output;

                float4x4 modelMatrix = unity_ObjectToWorld;
                output.viewDir = mul(modelMatrix, input.vertex).xyz
                    - _WorldSpaceCameraPos;
                output.pos = UnityObjectToClipPos(RotateAroundYInDegrees(input.vertex, _Rotation));
                return output;
            }

            float4 frag(vertexOutput input) : COLOR
            {
                half4 tex = texCUBE(_Cube, input.viewDir);
                half3 c = DecodeHDR(tex, _Cube_HDR);
                c = c * _Tint.rgb * unity_ColorSpaceDouble;
                c *= _Exposure;
                return half4(c, 1);
            }

            ENDCG
        }
    }
}