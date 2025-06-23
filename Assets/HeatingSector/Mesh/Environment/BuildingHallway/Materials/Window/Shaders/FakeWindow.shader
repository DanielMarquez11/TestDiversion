Shader "4ik0/FakeWindow/FakeWindow_URP"
{
    Properties
    {
        _Color ("Color Tint", Color) = (1,1,1,1)
        _MainTex ("Outside Window (RGB)", 2D) = "white" {}
        _BumpMap("Bumpmap (RGB)", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionMap("Emission Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Scale("Simulated Distance", Range(0, 1)) = 0.1
        _Lightmap ("Lightmap Texture", 2D) = "white" {}  // Lightmap texture for baked lighting
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
                float2 lightmapUV : TEXCOORD1; // Lightmap UV
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float2 lightmapUV : TEXCOORD3; // Pass lightmap UV to fragment shader
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
            TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
            TEXTURE2D(_Lightmap); SAMPLER(sampler_Lightmap); // Lightmap texture

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
                OUT.viewDirWS = normalize(_WorldSpaceCameraPos - TransformObjectToWorld(IN.positionOS.xyz));
                OUT.lightmapUV = IN.lightmapUV; // Pass lightmap UV to fragment shader
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample normal map
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uv));
                half3 normalWS = normalize(IN.normalWS + normalTS); 

                // View-dependent offset calculation
                half scale = (1 - _Scale);
                half coeff = dot(abs(IN.viewDirWS), normalWS);
                half3 offset = (IN.viewDirWS + normalWS) * scale / 2 * (2 - coeff);
                
                // Ensure UV remains 2D and calculate offset separately
                half2 uv = IN.uv * _Scale + scale / 2 - offset.xy; // Only 2D UV manipulation

                // Sample textures
                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv) * _Color;
                half4 emiss = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv) * _EmissionColor;

                // Sample baked lighting from lightmap (using the lightmap UVs)
                half4 lightmapColor = SAMPLE_TEXTURE2D(_Lightmap, sampler_Lightmap, IN.lightmapUV);
                
                // Apply lighting using baked lightmap and dynamic lighting
                half3 lighting = max(0.2, dot(normalWS, _MainLightPosition.xyz)) * _MainLightColor.rgb + lightmapColor.rgb;
                half3 finalColor = baseColor.rgb * lighting + emiss.rgb;

                return half4(finalColor, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}

