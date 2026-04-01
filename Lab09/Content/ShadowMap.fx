float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;
float3 CameraPosition;
float3 LightPosition;

sampler ShadowMapSampler = sampler_state
{
    texture = <ProjectiveTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = border;
    AddressV = border;
};

//=====================  ShadowMap =====================
struct ShadowMapVertexShaderInput
{
    float4 Position : POSITION0;
};

struct ShadowMapVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Position2D : TEXCOORD0;
};

ShadowMapVertexShaderOutput ShadowMapVertexShader(ShadowMapVertexShaderInput input)
{
    ShadowMapVertexShaderOutput output;
    output.Position = mul(mul(mul(input.Position, World), LightViewMatrix), LightProjectionMatrix);
    output.Position2D = output.Position;
    return output;
}

float4 ShadowMapPixelShader(ShadowMapVertexShaderOutput input) : COLOR0
{
    float4 projTexCoord = input.Position2D / input.Position2D.w;

    projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5); // [-1, 1] --> [0, 1]
    projTexCoord.y = 1.0 - projTexCoord.y; // invert Y direction

    float depth = projTexCoord.z; // nearest z=0, farthest z=1
    float4 color = (depth > 0) ? depth : 0;
    return color;
}


//===================== ShadowedScene =====================
struct ShadowedSceneVertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TexCoords : TEXCOORD0;
};

struct ShadowedSceneVertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Pos2DAsSeenByLight : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPosition : TEXCOORD2;
    float2 TexCoords : TEXCOORD3;
};

ShadowedSceneVertexShaderOutput ShadowedSceneVertexShader(ShadowedSceneVertexShaderInput input)
{
    ShadowedSceneVertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(mul(worldPosition, View), Projection);
    output.Pos2DAsSeenByLight = mul(mul(worldPosition, LightViewMatrix), LightProjectionMatrix);
    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
    output.WorldPosition = worldPosition.xyz;
    output.TexCoords = input.TexCoords;

    return output;
}
float4 ShadowedScenePixelShader(ShadowedSceneVertexShaderOutput input) : COLOR0
{
    float4 projTexCoord = input.Pos2DAsSeenByLight / input.Pos2DAsSeenByLight.w;
    projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5);
    projTexCoord.y = 1.0 - projTexCoord.y;

    float realDistance = projTexCoord.z;

    float3 N = normalize(input.Normal);
    float3 L = normalize(LightPosition - input.WorldPosition.xyz);
    float4 diffuseLightingFactor = 0; // black 

    if (projTexCoord.x >= 0 && projTexCoord.x <= 1 &&
        projTexCoord.y >= 0 && projTexCoord.y <= 1 &&
        saturate(projTexCoord).x == projTexCoord.x &&
        saturate(projTexCoord).y == projTexCoord.y)
    {
        float depthStoredInShadowMap = tex2D(ShadowMapSampler, projTexCoord.xy).r;

        if (depthStoredInShadowMap + 1.0f / 10000.0f > realDistance)
        {
            diffuseLightingFactor = max(0, dot(N, L)); // gray 
        }
        else
        {
            diffuseLightingFactor = float4(1, 0, 0, 1); // red
        }
    }

    return diffuseLightingFactor;
}

technique ShadowMap
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 ShadowMapVertexShader();
        PixelShader = compile ps_4_0 ShadowMapPixelShader();
    }
}

technique ShadowedScene
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 ShadowedSceneVertexShader();
        PixelShader = compile ps_4_0 ShadowedScenePixelShader();
    }
}