float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float3 LightPosition;
float4x4 WorldInverseTranspose;
float4x4 InverseCamera;
texture2D Texture;
float4 AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity = 1;
float Shininess;

sampler2D ParticleSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct FireParticleVertexInput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParamater : POSITION2;
};

struct VertexShaderInput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 ParticlePosition : POSITION1;
    float4 ParticleParamater : POSITION2;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

// ==========================Phong Shader==========================
VertexShaderOutput PhongVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space (for rasterization)
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Pass world position to pixel shader (needed for view vector calculation)
    output.WorldPosition = worldPosition;
    
    // Pass transformed normal to pixel shader (will be interpolated)
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    // Color is not calculated here - leave it for pixel shader
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // N - Normal vector (normalize because interpolation can change length)
    float3 N = normalize(input.Normal.xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    
    // L - Light vector (direction to light, so negate the light direction)
    float3 L = normalize(-LightPosition);
    
    // R - Reflection vector (reflect light direction around normal)
    float3 R = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    float4 specular = SpecularIntensity * SpecularColor *
                      pow(max(0, dot(R, V)), Shininess);
    
    // Combine all lighting
    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    
    return color;
}

// ==========================Fire Particle Shader==========================
VertexShaderOutput FireParticleVertexShader(FireParticleVertexInput input)
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

technique Phong
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongVertexShaderFunction();
        PixelShader = compile ps_4_0 PhongPixelShaderFunction();
    }
}

technique FireParticleTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 FireParticleVertexShader();
        PixelShader = compile ps_4_0 FireParticlePixelShader();
    }
}