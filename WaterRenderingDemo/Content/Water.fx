float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
float3 LightPosition;

// Lighting
float AmbientColor;
float AmbientIntensity;
float4 DiffuseColor;
float DiffuseIntensity;
float4 SpecularColor;
float SpecularIntensity;
float Shininess;

// Water parameters
texture heightMap;
texture heightMap2;
texture heightMap3;
texture heightMap4;
texture normalMap;
float Time;
float WaveAmplitude;
float WaveSpeed;
float WaterLevel;
float TextureScale;
float NormalMapScale;
float2 WindDirection;
float WaterAlpha;

sampler heightSampler = sampler_state
{
    Texture = <heightMap>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};


sampler heightSampler2 = sampler_state
{
    Texture = <heightMap2>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler heightSampler3 = sampler_state
{
    Texture = <heightMap3>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler heightSampler4 = sampler_state
{
    Texture = <heightMap4>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler normalSampler = sampler_state
{
    Texture = <normalMap>;
    MagFilter = LINEAR;
    MinFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
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
    float2 TexCoord : TEXCOORD0;
    float3 Normal : NORMAL;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
};

VertexOutput WaterVertexShader(VertexInput input)
{
    VertexOutput output;

    // Animated UV coordinates 
    // Scroll UVs over time in WindDirection to animate the waves
    float2 uv = input.TexCoord * TextureScale + WindDirection * Time * WaveSpeed;
    
    // Sample four height map octaves (Fourier synthesis)
    
    // Octave 1 — large, slow waves (primary height map)
    float h1 = tex2Dlod(heightSampler, float4(uv, 0, 0)).r;

    // Octave 2 — medium waves, offset to avoid mirroring octave 1
    float h2 = tex2Dlod(heightSampler2, float4(uv * 0.5 + float2(0.3, 0.7), 0, 0)).r;

    // Octave 3 — fine ripples, doubled frequency + phase offset
    float h3 = tex2Dlod(heightSampler3, float4(uv * 2.0 + float2(0.1, 0.5), 0, 0)).r;

    // Octave 4 — very fine ripples, quadrupled frequency + phase offset
    float h4 = tex2Dlod(heightSampler4, float4(uv * 4.0 + float2(0.8, 0.2), 0, 0)).r;
    
    // Combine octaves
    // Weights sum to 1.0, higher octaves carry less energy
    float combinedHeight = h1 * 0.50 + h2 * 0.25 + h3 * 0.15 + h4 * 0.10;
    
    // Displace vertex vertically 
    // Color is [0,1]; map to [-0.5, 0.5] then scale by amplitude
    float4 displacedPos = input.Position;
    displacedPos.y = WaterLevel + (combinedHeight - 0.5) * WaveAmplitude;

    float4 worldPos = mul(displacedPos, World);
    output.Position = mul(mul(worldPos, View), Projection);
    output.WorldPosition = worldPos.xyz;

    output.Normal = mul(input.Normal, (float3x3) WorldInverseTranspose);
    output.Tangent = mul(input.Tangent, (float3x3) WorldInverseTranspose);
    output.Binormal = mul(input.Binormal, (float3x3) WorldInverseTranspose);
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 WaterPixelShader(VertexOutput input) : COLOR
{
    // Read normal map — NormalMapScale controls tiling density independently of height map
    float2 scrollUV = input.TexCoord * NormalMapScale + WindDirection * Time * WaveSpeed;
    float3 normalTex = (tex2D(normalSampler, scrollUV).xyz - float3(0.5, 0.5, 0.5)) * 2.0;
    
    // Build bump normal
    float3 bumpNormal = normalize(
                    input.Normal +
                    normalTex.x *
                    input.Tangent +
                    normalTex.y *
                    input.Binormal);

    // Diffuse lighting 
    float3 lightDir = normalize(LightPosition - input.WorldPosition);
    float diffAmt = saturate(dot(bumpNormal, lightDir));
    float4 diffuse = DiffuseColor * DiffuseIntensity * diffAmt;
    
    // Specular 
    float3 viewDir = normalize(CameraPosition - input.WorldPosition);
    float3 reflect = normalize(2.0 * dot(bumpNormal, lightDir) * bumpNormal - lightDir);
    float specAmt = pow(saturate(dot(reflect, viewDir)), Shininess);
    float4 specular = SpecularColor * SpecularIntensity * specAmt;
    
    // Ambient 
    float4 ambient = float4(AmbientColor, AmbientColor, AmbientColor, 1.0) * AmbientIntensity;
    
    // Water color 
    float4 waterColor = float4(0.1, 0.4, 0.8, WaterAlpha);

    float4 result = waterColor * (ambient + diffuse) + specular;
    result.a = WaterAlpha;
    return result;
}

technique WaterRendering
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 WaterVertexShader();
        PixelShader = compile ps_4_0 WaterPixelShader();
    }
}