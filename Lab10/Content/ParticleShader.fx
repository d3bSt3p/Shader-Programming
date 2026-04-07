float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4x4 WorldInverseTranspose;
float4x4 InverseCamera;
texture2D Texture;


sampler2D ParticleSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParamater : POSITION2; // x: Scale x/y: Color
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput FireParticleVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, InverseCamera);
    worldPosition.xyz = worldPosition.xyz * sqrt(input.ParticleParamater.x);
    worldPosition += input.ParticlePosition;
    
    output.Position = mul(mul(mul(worldPosition, World), View), Projection);
    output.TexCoord = input.TexCoord;
    output.Color = 1 - input.ParticleParamater.x / input.ParticleParamater.y;
        
    float fade = 1.0 - input.ParticleParamater.x / input.ParticleParamater.y;
    return output;     
}

float4 FireParticlePixelShader(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(ParticleSampler, input.TexCoord);
    color *= input.Color;
    return color;
}

technique FireParticleTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 FireParticleVertexShader();
        PixelShader = compile ps_4_0 FireParticlePixelShader();
    }
}