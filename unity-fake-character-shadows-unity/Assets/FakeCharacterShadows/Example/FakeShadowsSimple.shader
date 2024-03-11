Shader "Unlit/FakeShadowsSimple"
{
    Properties
    {
        _BaseColor ("Base color", Color) = (1, 1, 1, 1)
        _PlayerPosition ("Player Position", Vector) = (0, 0, 0, 0)
        _PlayerRadius ("Player Radius", Float) = 0.5
        _LightPosition ("Light Position", Vector) = (0, 0, 0, 0)
        _LightRadius ("Light Radius", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varying
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float fogCoord : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half3 _PlayerPosition;
                half _PlayerRadius;
                half3 _LightPosition;
                half _LightRadius;
            CBUFFER_END

            Varying vert(Attributes IN)
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                Varying OUT = (Varying)0;
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;

                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }

            const half epsilon = 0.0001;

            half CalcFakeShadowPerLight(half4 light, half3 playerPos, half playerRad, half3 posToPlayer, half3 posWS)
            {
                // Calc dot
                half3 playerToLight = normalize(light.xyz - playerPos);
                half d = dot(posToPlayer, playerToLight);
                float r = 1 - playerRad;
                d = saturate((d - r) / (1 - r)); // remap range: r~1 -> 0~1

                // // Attenuation
                half distLightToPos = distance(posWS, light.xyz);
                half atten = 1 - saturate(distLightToPos / (light.w + epsilon)); // Apply light radius
                atten = atten * atten; // Inverse Square Law

                // // Adjust attenuation and reverse
                return 1 - saturate(d * atten);
            }

            half4 frag(Varying IN) : SV_Target
            {
                half4 color = _BaseColor;
                half3 posToPlayer = normalize(_PlayerPosition - IN.positionWS);
                half shadow = CalcFakeShadowPerLight(half4(_LightPosition, _LightRadius), _PlayerPosition, _PlayerRadius, posToPlayer, IN.positionWS);
                color.rgb *= shadow * shadow * shadow;
                color.rgb = MixFog(color.rgb, IN.fogCoord);
                return color;
            }
            ENDHLSL
        }
    }
}