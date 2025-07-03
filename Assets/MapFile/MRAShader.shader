Shader "Custom/MRA_LitShader"
{
    Properties
    {
        _MainTex("Albedo", 2D) = "white" {}
        _MRA("MRA Texture (R:Metallic, G:Roughness, B:AO)", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _BumpScale("Normal Scale", Range(0,1)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            // 필요한 pragma 설정
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _NORMALMAP
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // 구조체: 정점 입력
            struct Attributes
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                float3 normal   : NORMAL;
                float4 tangent  : TANGENT;
            };

            // 구조체: 정점에서 픽셀로 전달
            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float2 uvMain       : TEXCOORD0;
                float3 worldPos     : TEXCOORD1;
                float3 normalWS     : TEXCOORD2;
                float3 tangentWS    : TEXCOORD3;
                float3 bitangentWS  : TEXCOORD4;
            };

            // 프로퍼티와 샘플러 선언
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MRA);
            SAMPLER(sampler_MRA);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);

            float4 _MainTex_ST;
            float4 _MRA_ST;
            float4 _NormalMap_ST;
            float _BumpScale;

            // 정점 셰이더: 월드 변환 및 텍스처 좌표 계산
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.vertex);
                OUT.uvMain = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.worldPos = TransformObjectToWorld(IN.vertex);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normal);
                OUT.tangentWS = TransformObjectToWorldDir(IN.tangent.xyz);
                // 비터전트 계산 (Tangent Space → World Space)
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangent.w;
                return OUT;
            }

            // 픽셀 셰이더: 라이팅 계산 및 MRA 텍스처 사용
            half4 frag(Varyings IN) : SV_Target
            {
                // 기본 알베도 샘플링
                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain);

                // 노멀맵 샘플링 및 언팩
                #if defined(_NORMALMAP)
                float3 normalTex = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, TRANSFORM_TEX(IN.uvMain, _NormalMap)));
                #else
                float3 normalTex = float3(0,0,1);
                #endif

                // MRA 텍스처 샘플링
                float4 mraSample = SAMPLE_TEXTURE2D(_MRA, sampler_MRA, TRANSFORM_TEX(IN.uvMain, _MRA));
                // 각 채널 추출: R = Metallic, G = Roughness, B = Ambient Occlusion
                float metallic = mraSample.r;
                float roughness = mraSample.g;
                float ambientOcclusion = mraSample.b;

                // URP Lit에서는 스무스니스(smoothness)를 사용하므로 roughness의 반전 값을 사용 (smoothness = 1 - roughness)
                float smoothness = 1.0 - roughness;

                // 노멀 계산: 기본 월드 노멀과 노멀맵 결합
                float3 N = normalize(IN.normalWS + normalTex * _BumpScale);

                // 간단한 라이팅 계산 (예제용: 단일 방향광 사용)
                float3 lightDir = normalize(float3(0.5, 0.5, -1.0));
                float NdotL = saturate(dot(N, lightDir));
                float3 diffuse = albedo.rgb * NdotL;

                // 간단한 블린-폰 Specular 계산 (metallic 및 smoothness 적용)
                float3 viewDir = normalize(_WorldSpaceCameraPos - IN.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(N, halfDir));
                float specularIntensity = pow(NdotH, smoothness * 128.0) * metallic;

                // 최종 색상에 앰비언트 오클루전 반영
                float3 finalColor = (diffuse + specularIntensity) * ambientOcclusion;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}
