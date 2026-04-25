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
float4 PhongParticleColor = float4(1, 1, 1, 1);

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
    float4 ParticleParamater : POSITION2; // X=Age, Y=MaxAge, Z=Size
};

struct PhongVertexInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
    float4 WorldPosition : TEXCOORD1;
    float4 Normal : TEXCOORD2;
};

// ========================== Phong Shader ==========================
VertexShaderOutput PhongVertexShaderFunction(PhongVertexInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.WorldPosition = worldPosition;
    output.Normal = mul(float4(input.Normal, 0), WorldInverseTranspose);
    output.TexCoord = input.TexCoord;
    output.Color = float4(1, 1, 1, 1);

    return output;
}

float4 PhongPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float3 N = normalize(input.Normal.xyz);
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    float3 L = normalize(-LightPosition);
    float3 R = reflect(-L, N);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    float4 specular = SpecularIntensity * SpecularColor * pow(max(0, dot(R, V)), Shininess);

    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    return color;
}

// ========================== Shared billboard helper ==========================
// Builds a billboard quad vertex in world space, scaled by particle Size (param.Z).
float4 BillboardWorldPos(FireParticleVertexInput input)
{
    float pSize = max(0.01, input.ParticleParamater.z);
    float4 worldPos = mul(input.Position, InverseCamera); // orient quad to face camera
    worldPos.xyz *= pSize;
    worldPos += input.ParticlePosition;
    return worldPos;
}

// Age-based fade: 1 at birth → 0 at death
float AgeFade(FireParticleVertexInput input)
{
    return 1.0 - saturate(input.ParticleParamater.x / max(0.001, input.ParticleParamater.y));
}

// ========================== Phong Particle Shader ==========================
VertexShaderOutput PhongParticleVertexShader(FireParticleVertexInput input)
{
    VertexShaderOutput output;

    float4 worldPos = BillboardWorldPos(input);
    float4 wp = mul(worldPos, World);
    output.Position = mul(mul(wp, View), Projection);
    output.WorldPosition = wp;

    // Surface normal always faces camera
    float3 camForward = normalize(CameraPosition - wp.xyz);
    output.Normal = float4(camForward, 0);

    output.TexCoord = input.TexCoord;
    float fade = AgeFade(input);
    output.Color = float4(fade, fade, fade, fade);

    return output;
}

float4 PhongParticlePixelShader(VertexShaderOutput input) : COLOR0
{
    float3 N = normalize(input.Normal.xyz);
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    float3 L = normalize(-LightPosition);
    float3 R = reflect(-L, N);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    float4 specular = SpecularIntensity * SpecularColor * pow(max(0, dot(R, V)), Shininess);

    float4 color = saturate(ambient + diffuse + specular);
    color *= PhongParticleColor;
    color.a = input.Color.a * PhongParticleColor.a;
    return color;
}

// ========================== Fire Particle Shader ==========================
VertexShaderOutput FireParticleVertexShader(FireParticleVertexInput input)
{
    VertexShaderOutput output;

    float4 worldPos = BillboardWorldPos(input);
    output.Position = mul(mul(mul(worldPos, World), View), Projection);
    output.TexCoord = input.TexCoord;

    float fade = AgeFade(input);
    output.Color = float4(fade, fade, fade, fade);
    output.WorldPosition = mul(worldPos, World);
    output.Normal = float4(0, 0, 1, 0);

    return output;
}

float4 FireParticlePixelShader(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(ParticleSampler, input.TexCoord);
    color *= input.Color;
    return color;
}

// ========================== Techniques ==========================
technique Phong
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongVertexShaderFunction();
        PixelShader = compile ps_4_0 PhongPixelShaderFunction();
    }
}

technique PhongParticleTechnique
{
    pass Pass0
    {
        VertexShader = compile vs_4_0 PhongParticleVertexShader();
        PixelShader = compile ps_4_0 PhongParticlePixelShader();
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