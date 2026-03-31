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

texture normalMap;
texture environmentMap;

sampler normalSampler = sampler_state {
    texture = <normalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

samplerCUBE envSampler = sampler_state {
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

// ── Shared vertex structures ──────────────────────────────────────────────────

struct VertexInput
{
    float4 Position  : POSITION;
    float4 Normal    : NORMAL;
    float2 TexCoord  : TEXCOORD0;
    float3 Tangent   : TANGENT0;
    float3 Binormal  : BINORMAL0;
};

struct VertexOutput
{
    float4 Position      : POSITION0;
    float3 WorldPosition : TEXCOORD1;
    float3 Normal        : TEXCOORD2;
    float2 TexCoord      : TEXCOORD0;
    float3 Tangent       : TEXCOORD3;
    float3 Binormal      : TEXCOORD4;
};

// ── Shared vertex shader ──────────────────────────────────────────────────────

VertexOutput BumpMapVS(VertexInput input)
{
    VertexOutput output;

    float4 worldPos  = mul(input.Position, World);
    float4 viewPos   = mul(worldPos, View);
    output.Position  = mul(viewPos, Projection);

    output.WorldPosition = worldPos.xyz;
    output.TexCoord      = input.TexCoord;

    output.Normal   = mul(input.Normal.xyz,   (float3x3)WorldInverseTranspose);
    output.Tangent  = mul(input.Tangent,       (float3x3)WorldInverseTranspose);
    output.Binormal = mul(input.Binormal,      (float3x3)WorldInverseTranspose);

    return output;
}

// ── Helper: reconstruct bump normal from normal map ───────────────────────────

float3 BumpNormal(VertexOutput input)
{
    // Remap [0,1] -> [-1,1]
    float3 texN = (tex2D(normalSampler, input.TexCoord).xyz - 0.5) * 2.0;

    float3 N = normalize(input.Normal);
    float3 T = normalize(input.Tangent);
    float3 B = normalize(input.Binormal);

    // Tangent-space normal -> world space
    return normalize(texN.x * T + texN.y * B + texN.z * N);
}

// ── Helper: refract (Snell's law) ─────────────────────────────────────────────

float3 RefractDir(float3 I, float3 N, float eta)
{
    float cosI  = dot(-I, N);
    float cosT2 = 1.0 - eta * eta * (1.0 - cosI * cosI);
    float3 T    = eta * I + (eta * cosI - sqrt(abs(cosT2))) * N;
    return T * (float)(cosT2 > 0);
}

// ── F1: Visualize tangent-space normal map as RGB ─────────────────────────────

float4 F1_VisualizeNormalMapPS(VertexOutput input) : COLOR
{
    // Raw normal map sample is already in [0,1] – display directly as RGB
    float3 texN = tex2D(normalSampler, input.TexCoord).xyz;
    return float4(texN, 1.0);
}

// ── F2: Visualize bump normal in world space as RGB ───────────────────────────

float4 F2_VisualizeWorldNormalPS(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);
    // Remap [-1,1] -> [0,1] for display
    return float4(bumpN * 0.5 + 0.5, 1.0);
}

// ── F3: Tangent-space bump mapping (diffuse + specular) ───────────────────────

float4 F3_BumpDiffuseSpecularPS(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 L = normalize(LightPosition - input.WorldPosition);
    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 H = normalize(L + V);   // Blinn-Phong half-vector

    float4 ambient  = AmbientColor * AmbientIntensity;
    float4 diffuse  = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));
    float4 specular = SpecularColor * SpecularIntensity
                      * pow(max(0.0, dot(bumpN, H)), Shininess);

    return saturate(ambient + diffuse + specular);
}

// ── F4: Reflective bump mapping ───────────────────────────────────────────────

float4 F4_ReflectiveBumpPS(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 L = normalize(LightPosition - input.WorldPosition);
    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 H = normalize(L + V);
    float3 I = -V;

    // Diffuse + specular from bump normal
    float4 ambient  = AmbientColor * AmbientIntensity;
    float4 diffuse  = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));
    float4 specular = SpecularColor * SpecularIntensity
                      * pow(max(0.0, dot(bumpN, H)), Shininess);
    float4 lighting = saturate(ambient + diffuse + specular);

    // Cube-map reflection using bump normal
    float3 R = reflect(I, bumpN);
    float4 reflectionColor = texCUBE(envSampler, R);

    return lerp(lighting, reflectionColor, 0.5);
}

// ── F5: Refractive bump mapping ───────────────────────────────────────────────

float4 F5_RefractiveBumpPS(VertexOutput input) : COLOR
{
    float3 bumpN = BumpNormal(input);

    float3 V = normalize(CameraPosition - input.WorldPosition);
    float3 I = -V;

    // Refraction through the bump normal (eta ≈ 0.66 for glass-like look)
    float3 T = RefractDir(I, bumpN, 0.66);
    float4 refractionColor = texCUBE(envSampler, T);

    // Add a small diffuse term so the shape remains visible
    float3 L = normalize(LightPosition - input.WorldPosition);
    float4 diffuse = DiffuseColor * DiffuseIntensity * max(0.0, dot(bumpN, L));

    return saturate(lerp(refractionColor, diffuse, 0.3));
}

// ── Techniques ────────────────────────────────────────────────────────────────

technique VisualizeNormalMap
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVS();
        PixelShader  = compile ps_4_0 F1_VisualizeNormalMapPS();
    }
}

technique VisualizeWorldNormal
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVS();
        PixelShader  = compile ps_4_0 F2_VisualizeWorldNormalPS();
    }
}

technique BumpMapping
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVS();
        PixelShader  = compile ps_4_0 F3_BumpDiffuseSpecularPS();
    }
}

technique ReflectiveBump
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVS();
        PixelShader  = compile ps_4_0 F4_ReflectiveBumpPS();
    }
}

technique RefractiveBump
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVS();
        PixelShader  = compile ps_4_0 F5_RefractiveBumpPS();
    }
}