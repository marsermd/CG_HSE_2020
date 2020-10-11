Shader "0_Custom/Cubemap"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0, 0, 0, 1)
        _Roughness ("Roughness", Range(0.03, 1)) = 1
        _Cube ("Cubemap", CUBE) = "" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            
            #define EPS 1e-7

            struct appdata
            {
                float4 vertex : POSITION;
                fixed3 normal : NORMAL;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                float4 pos : TEXCOORD1;
                fixed3 normal : NORMAL;
            };

            float4 _BaseColor;
            float _Roughness;
            
            samplerCUBE _Cube;
            half4 _Cube_HDR;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.clip = UnityObjectToClipPos(v.vertex);
                o.pos = mul(UNITY_MATRIX_M, v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            uint Hash(uint s)
            {
                s ^= 2747636419u;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                s ^= s >> 16;
                s *= 2654435769u;
                return s;
            }
            
            float Random(uint seed)
            {
                return float(Hash(seed)) / 4294967295.0; // 2^32-1
            }
            
            float3 SampleColor(float3 direction)
            {   
                half4 tex = texCUBE(_Cube, direction);
                return DecodeHDR(tex, _Cube_HDR).rgb;
            }
            
            float Sqr(float x)
            {
                return x * x;
            }
            
            // Calculated according to NDF of Cook-Torrance
            float GetSpecularBRDF(float3 viewDir, float3 lightDir, float3 normalDir)
            {
                float3 halfwayVector = normalize(viewDir + lightDir);               
                
                float a = Sqr(_Roughness);
                float a2 = Sqr(a);
                float NDotH2 = Sqr(dot(normalDir, halfwayVector));
                
                return a2 / (UNITY_PI * Sqr(NDotH2 * (a2 - 1) + 1));
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.normal);
                
                float3 viewDirection = normalize(_WorldSpaceCameraPos - i.pos.xyz);
                
                // Replace this specular calculation by Montecarlo.
                // Normalize the BRDF in such a way, that integral over a hemysphere of (BRDF * dot(normal, w')) == 1
                // TIP: use Random(i) to get a pseudo-random value.
                float3 viewRefl = reflect(-viewDirection.xyz, normal);
                float3 specular = SampleColor(viewRefl);
                
                return fixed4(specular, 1);
            }
            ENDCG
        }
    }
}
