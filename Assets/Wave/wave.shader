// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "wave"
{
    Properties
    {
        _Diffuse ("Diffuse", Color) = (1, 1, 1, 1)
        _Crest ("（波峰）Crest", Range(0.1, 1.0)) = 0.1
        _Cycle ("（周期）Cycle", Range(1.0, 100.0)) = 1
    }
    SubShader
    {
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Lighting.cginc"
            
            fixed4 _Diffuse;
            uniform float _Crest;
            uniform float _Cycle;
            
            struct a2v
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float3 worldNormal: TEXCOORD0;
                float3 worldPos: TEXCOORD1;
            };
            
            v2f vert(a2v v)
            {
                v2f o;
      
                o.pos = UnityObjectToClipPos(v.vertex);
                
        
                o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
                
    
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				// float offset1 = _Crest * sin(_Time.y * _Cycle + v.vertex.x + v.vertex.z);
                // o.pos.y += offset1;
				// o.pos.y +=  _Crest * sin(length(v.vertex.xz)*10 + _Time.y * _Cycle);


				o.pos.y +=  _Crest * sin(10 + _Time.y * _Cycle);
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                // Get ambient term
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
                
                fixed3 worldNormal = normalize(i.worldNormal);
                fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);
                
                // Compute diffuse term
                fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * max(0, dot(worldNormal, worldLightDir));
                
                // Get the view direction in world space
                fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
                // Get the half direction in world space
                fixed3 halfDir = normalize(worldLightDir + viewDir);
                // Compute specular term
                fixed3 specular = _LightColor0.rgb * max(0, dot(worldNormal, halfDir));
                
                return fixed4(ambient + diffuse, 1.0);
            }
            
            ENDCG
            
        }
    }
    FallBack "Specular"
}
