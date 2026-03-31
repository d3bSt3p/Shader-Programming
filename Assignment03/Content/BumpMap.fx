float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
float3 LightPosition;
float AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;

// Mip-map toggle
bool UseMipMap;

// Refraction
float RefractionIndex;

// Bump tiling (UV scale) and height
float BumpTilingU;
float BumpTilingV;
float BumpHeight;

texture normalMap;
texture environmentMap;


// Sampler  
sampler normalSamplerNoMip = sampler_state
{
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = NONE;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Sampler mip-mapping 
sampler normalSamplerMip = sampler_state
{
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Sampler environment map
samplerCUBE envSampler = sampler_state
{
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexInput
{
    float4 Position : POSITION;
    float4 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
};

struct VertexOutput
{
    float4 Position : POSITION0;
    float3 WorldPosition : TEXCOORD1;
    float3 Normal : TEXCOORD2;
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TEXCOORD3;
    float3 Binormal : TEXCOORD4;
};

VertexOutput BumpMapVertexShader(VertexInput input)
{
    VertexOutput output;

    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.WorldPosition = worldPos.xyz;
    output.TexCoord = input.TexCoord;

    output.Normal = mul(input.Normal.xyz, (float3x3) WorldInverseTranspose);
    output.Tangent = mul(input.Tangent, (float3x3) WorldInverseTranspose);
    output.Binormal = mul(input.Binormal, (float3x3) WorldInverseTranspose);

    return output;
}

//sample normal map using the active mip-map mode
float3 SampleNormalMap(float2 texCoord)
{
    // Apply UV tiling scale
    float2 tiledCoord = texCoord * float2(BumpTilingU, BumpTilingV);

    if (UseMipMap)
        return tex2D(normalSamplerMip, tiledCoord).xyz;
    else
        return tex2D(normalSamplerNoMip, tiledCoord).xyz;
}

// reconstruct bump normal from normal map
float3 BumpNormal(VertexOutput input)
{
    // Scale xy components by BumpHeight to control bump intensity
    float3 texN = (SampleNormalMap(input.TexCoord) - 0.5) * 2.0;
    texN.xy *= BumpHeight;

    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent);
    float3 B = normalize(input.Binormal);

    return normalize(texN.x * T + texN.y * B + texN.z * N);
}

// refraction 
float3 RefractDir(float3 I, float3 N, float eta)
{
    float cosI = dot(-I, N);
    float cosT2 = 1.0 - eta * eta * (1.0 - cosI * cosI);
    float3 T = eta * I + (eta * cosI - sqrt(abs(cosT2))) * N;
    return T * (float) (cosT2 > 0);
}

// ==========================Visualize Normal Map==========================

float4 VisualizeNormalMapPixelShader(VertexOutput input) : COLOR
{
    float3 texN = SampleNormalMap(input.TexCoord);
    return float4(texN, 1.0);
}

// ==========================Visualize bump normal in world space as RGB==========================

float4 VisualizeWorldNormalPixelShader(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);
    return float4(bumpN * 0.5 + 0.5, 1.0);
}

// ==========================Tangent-space bump mapping (diffuse + specular)==========================

float4 BumpDiffuseSpecularPixelShader(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 L = normalize(LightPosition - input.WorldPosition);
    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 H = normalize(L + V);

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));
    float4 specular = SpecularColor * SpecularIntensity * pow(max(0.0, dot(bumpN, H)), Shininess);

    return saturate(ambient + diffuse + specular);
}

// ==========================Reflective bump mapping==========================

float4 ReflectiveBumpPixelShader(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 L = normalize(LightPosition - input.WorldPosition);
    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 H = normalize(L + V);
    float3 I = -V;

    float4 ambient = AmbientColor * AmbientIntensity;
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));
    float4 specular = SpecularColor * SpecularIntensity * pow(max(0.0, dot(bumpN, H)), Shininess);
    float4 lighting = saturate(ambient + diffuse + specular);

    float3 R = reflect(I, bumpN);
    float4 reflectionColor = texCUBE(envSampler, R);

    return lerp(lighting, reflectionColor, SpecularIntensity);
}

// ==========================Refractive bump mapping==========================

float4 RefractiveBumpPixelShader(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 I = -V;

    float3 T = RefractDir(I, bumpN, RefractionIndex);
    float4 refractionColor = texCUBE(envSampler, T);

    float3 L = normalize(LightPosition - input.WorldPosition);
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));

    return saturate(lerp(refractionColor, diffuse, 0.3));
}

// I'm gonna pass on doing the advanced shaders

technique VisualizeNormalMap
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 VisualizeNormalMapPixelShader();
    }
}

technique VisualizeWorldNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 VisualizeWorldNormalPixelShader();
    }
}

technique BumpMapping
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 BumpDiffuseSpecularPixelShader();
    }
}

technique ReflectiveBump
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 ReflectiveBumpPixelShader();
    }
}

technique RefractiveBump
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 RefractiveBumpPixelShader();
    }
}

