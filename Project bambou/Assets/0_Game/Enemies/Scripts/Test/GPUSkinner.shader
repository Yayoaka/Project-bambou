Shader "Custom/GPUSkinnedLit_Textured"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalRenderPipeline" "Queue"="Opaque" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            StructuredBuffer<float3> _SkinnedPositions;
            StructuredBuffer<float3> _SkinnedNormals;
            StructuredBuffer<float2> _UV;

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct appdata
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 nrm : TEXCOORD0;
                float2 uv  : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                float3 p = _SkinnedPositions[v.vertexID];
                float3 n = _SkinnedNormals[v.vertexID];

                o.pos = TransformObjectToHClip(p);
                o.nrm = TransformObjectToWorldNormal(n);
                o.uv  = _UV[v.vertexID];
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
            }

            ENDHLSL
        }
    }
}
