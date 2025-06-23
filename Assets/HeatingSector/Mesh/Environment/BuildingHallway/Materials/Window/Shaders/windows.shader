Shader "4ik0/FakeWindow/FakeWindow_URP"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)
        _MainTex ("Outside Window (RGB)", 2D) = "white" {}
        _BumpMap("Bumpmap (RGB)", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionMap("Emission Texture", 2D) = "white" {}
        _Scale("Simulated Distance", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" "RenderType" = "Opaque" }
        LOD 200
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float4 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);

            float4 _Color;
            float _Glossiness;
            float _Metallic;
            float _Scale;
            float4 _EmissionColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 worldTangent = TransformObjectToWorldDir(IN.tangentOS.xyz);
                float3 worldBitangent = cross(OUT.normalWS, worldTangent) * IN.tangentOS.w;
                
                OUT.tangentWS = float4(worldTangent, 0);
                OUT.bitangentWS = worldBitangent;
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS.xyz));
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample normal map and convert from tangent space to world space
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv));
                half3x3 TBN = half3x3(IN.tangentWS.xyz, IN.bitangentWS, IN.normalWS);
                half3 normalWS = normalize(mul(normalTS, TBN));

                // View-dependent offset calculation
                half scale = (1 - _Scale);
                half coeff = dot(abs(IN.viewDirWS), normalWS);
                half3 offset = (IN.viewDirWS + normalWS) * scale / 2 * (2 - coeff);

                // Adjust UV for fake parallax effect
                half2 uv = IN.uv * _Scale + scale / 2 - offset.xy;

                // Sample textures
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
                half4 emiss = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv) * _EmissionColor;

                // Calculate lighting
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;

                // Standard PBR lighting calculations
                half3 reflectDir = reflect(-IN.viewDirWS, normalWS);
                half3 halfDir = normalize(IN.viewDirWS + lightDir);
                half NdotL = saturate(dot(normalWS, lightDir));
                half NdotH = saturate(dot(normalWS, halfDir));

                // Specular reflection calculation
                half specularStrength = pow(NdotH, _Glossiness * 128.0) * _Glossiness;
                half3 specular = specularStrength * lightColor;

                // Apply metallic effect
                half3 diffuse = baseColor.rgb * (1.0 - _Metallic);
                half3 finalColor = diffuse * NdotL * lightColor + specular + emiss.rgb;

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}