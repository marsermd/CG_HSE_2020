Shader "0_Custom/Diffuse+Ambient"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0, 0, 0, 1)
        _AmbientColor ("Ambient Color", Color) = (0, 0, 0, 1)
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

            struct appdata
            {
                float4 vertex : POSITION;
                fixed3 normal : NORMAL;
            };

            struct v2f
            {
                float4 clip : SV_POSITION;
                fixed3 normal : NORMAL;
            };

            float4 _BaseColor;
            float4 _AmbientColor;
            
            half3 CalculateLight(float3 normal)
            {
                half cosTheta = dot(normal, _WorldSpaceLightPos0.xyz);
                half attenuation = max(0, cosTheta);
                half3 diffuse = attenuation * _LightColor0;
                
                return diffuse + _AmbientColor.rgb;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.clip = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                i.normal = normalize(i.normal);
                return half4(_BaseColor.rgb * CalculateLight(i.normal), 1);
            }
            ENDCG
        }
    }
}
