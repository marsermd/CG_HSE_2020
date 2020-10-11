Shader "0_Custom/Diffuse"
{
    Properties
    {
        _BaseColor ("Color", Color) = (0, 0, 0, 1)
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
                //half3 light : COLOR;
            };

            float4 _BaseColor;
            
            half3 CalculateLight(float3 normal)
            {
                half cosTheta = dot(normal, _WorldSpaceLightPos0.xyz);
                half attenuation = max(0, cosTheta);
                half3 diffuse = attenuation * _LightColor0;
                return diffuse;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.clip = UnityObjectToClipPos(v.vertex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                
                //o.light = CalculateLight(o.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //return half4(_BaseColor.rgb * i.light, 1);
                
                i.normal = normalize(i.normal);
                return half4(_BaseColor.rgb * CalculateLight(i.normal), 1);
            }
            ENDCG
        }
    }
}
