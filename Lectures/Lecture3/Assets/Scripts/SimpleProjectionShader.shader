Shader "Custom/ProjectionShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
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
                fixed3 normal : NORMAL; // Normal in forld space
            };
            
            sampler2D _MainTex;
           
            v2f vert (vIn v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.normal = normalize(i.normal);
                
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                half3 light = nl * _LightColor0;
                light += ShadeSH9(half4(i.normal, 1));
                
                fixed4 col = tex2D(_MainTex, i.uv);
                col.rgb *= light;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
