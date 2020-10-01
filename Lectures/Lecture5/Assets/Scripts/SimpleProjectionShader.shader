// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ProjectionShader"
{
    Properties
    {
        _XAlbedo ("Albedo X (RGB)", 2D) = "white" {}
        _YAlbedo ("Albedo Y (RGB)", 2D) = "white" {}
        _ZAlbedo ("Albedo Z (RGB)", 2D) = "white" {}
        
        _Scale ("Scale", float) = 1
    }
    SubShader
    {
        Cull Back
        Pass
        {
            // indicate that our pass is the "base" pass in forward
            // rendering pipeline. It gets ambient and main directional
            // light data set up; light direction in _WorldSpaceLightPos0
            // and color in _LightColor0
            Tags {"LightMode"="ForwardBase"}
        
            CGPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // for UnityObjectToWorldNormal
            #include "UnityLightingCommon.cginc" // for _LightColor0

            struct vIn // CPU To Vertex Shader
            {
             float4 vertex   : POSITION;  // The vertex position in model space.
             float3 normal   : NORMAL;    // The vertex normal in model space.
             float4 texcoord : TEXCOORD0; // The first UV coordinate.
            };
            
            struct v2f // Vertex To Fragment Shader
            {
                float4 pos : SV_POSITION; // Vertex position in clipping space
                float2 uv : TEXCOORD0; // Texture coordinate
                fixed3 normal : NORMAL; // Normal in world space
                float4 wPos : TEXCOORD1; // Vertex position in world space. Calculate it by yourself
            };
            
            sampler2D _XAlbedo;
            sampler2D _YAlbedo;
            sampler2D _ZAlbedo;
            
            float _Scale;
           
            v2f vert (vIn v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Equivalient of mul(UNITY_MATRIX_MVP, vertex)
                o.uv = v.texcoord;
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                // Calculate world space position of the vertex here and pass it to a vertex shader.
                
                return o;
            }
            
            half3 getLighting(v2f i)
            {
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                half3 light = nl * _LightColor0;
                light += ShadeSH9(half4(i.normal, 1));
                return light;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                i.normal = normalize(i.normal);
                
                // Calculate albedo by projecting _XAlbedo, _YAlbedo, _ZAlbedo on world position. Asjust texel to world space ratio using _Scale.
                fixed4 albedo = fixed4(0.5, 0.5, 0.5, 1);
                
                return float4(albedo.rgb * getLighting(i), 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
