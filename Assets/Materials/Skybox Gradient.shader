Shader "Skybox Gradient"
{
    Properties
    {
        _Top("Top", Color) = (1,1,1,0)
        _Bottom("Bottom", Color) = (0,0,0,0)
        _mult("mult", Float) = 1
        _pwer("pwer", Float) = 1
        [Toggle(_SCREENSPACE_ON)] _Screenspace("Screen space", Float) = 0
        _Rotation("Rotation", Vector) = (0,0,0,0) // New rotation property
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGINCLUDE
        #pragma target 3.0
        ENDCG
        Blend Off
        Cull Back
        ColorMask RGBA
        ZWrite On
        ZTest LEqual
        Offset 0 , 0
        
        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #pragma shader_feature_local _SCREENSPACE_ON

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
                float4 ase_texcoord1 : TEXCOORD1;
                float4 ase_texcoord2 : TEXCOORD2;
            };

            uniform float4 _Bottom;
            uniform float4 _Top;
            uniform float _mult;
            uniform float _pwer;
            uniform float3 _Rotation; // Rotation uniform
            
            float3 RotateVectorByEuler(float3 v, float3 rotation)
            {
                float3 rad = radians(rotation);
                float c1 = cos(rad.y), s1 = sin(rad.y);
                float c2 = cos(rad.x), s2 = sin(rad.x);
                float c3 = cos(rad.z), s3 = sin(rad.z);
                float3x3 rotMatrix = float3x3(
                    c1 * c3, -c1 * s3, s1,
                    s2 * s1 * c3 + c2 * s3, -s2 * s1 * s3 + c2 * c3, -s2 * c1,
                    -c2 * s1 * c3 + s2 * s3, c2 * s1 * s3 + s2 * c3, c2 * c1
                );
                return mul(rotMatrix, v);
            }
            
            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                float3 rotatedVertex = RotateVectorByEuler(v.vertex.xyz, _Rotation);
                v.vertex.xyz = rotatedVertex;

                float4 ase_clipPos = UnityObjectToClipPos(v.vertex);
                float4 screenPos = ComputeScreenPos(ase_clipPos);
                o.ase_texcoord2 = screenPos;
                
                o.ase_texcoord1 = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                fixed4 finalColor;
                float4 screenPos = i.ase_texcoord2;
                float4 ase_screenPosNorm = screenPos / screenPos.w;
                ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
                #ifdef _SCREENSPACE_ON
                float staticSwitch13 = ase_screenPosNorm.y;
                #else
                float staticSwitch13 = i.ase_texcoord1.xyz.y;
                #endif
                float4 lerpResult3 = lerp(_Bottom, _Top, pow(saturate(staticSwitch13 * _mult), _pwer));
                
                finalColor = lerpResult3;
                return finalColor;
            }
            ENDCG
        }
    }
    CustomEditor "ASEMaterialInspector"
}