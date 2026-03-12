float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;
float3 DiffuseLightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

// Lab 4
float3 CameraPosition;
float Shininess;
float4 SpecularColor;
float SpecularIntensity = 1;

// Lab 6 - Reflection shader parameters
texture decalMap;
texture environmentMap;

// Refraction parameters
float RefractionIndex = 0.66f;
float DisplacementAmount = 0.1f;

// Refraction dispersion eta ratios for each color channel
float EtaRatioRed = 0.95f;
float EtaRatioGreen = 1.0f;
float EtaRatioBlue = 1.05f;

// Fresnel parameters
float FresnelBias = 0.1f;
float FresnelScale = 0.9f;
float FresnelPower = 5.0f;

// Flag to control whether to apply texture
bool applyTexture = false;

// regular sampler for the model texture
sampler tsampler1 = sampler_state {
    texture = <decalMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Cube sampler for the environment map (skybox)
samplerCUBE SkyBoxSampler = sampler_state {
    texture = <environmentMap>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Mirror;
    AddressV = Mirror;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float3 R : TEXCOORD3;
};


// Helper function for refraction using Snell's Law
float3 refract(float3 I, float3 N, float etaRatio)
{
    float cosI = dot(-I, N);
    float cosT2 = 1.0f - etaRatio * etaRatio * (1.0f - cosI * cosI);
    float3 T = etaRatio * I + ((etaRatio * cosI - sqrt(abs(cosT2))) * N);
    return T * (float3)(cosT2 > 0);
}


// ========================== GOURAUD (Per-Vertex Lighting) ==========================

VertexShaderOutput GouraudVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Store world position for pixel shader
    output.WorldPosition = worldPosition;
    
    // Transform normal to world space
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
          
    // N - Normal vector (normalize the surface normal)
    float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - worldPosition.xyz);
    
    // L - Light vector (direction to light, so negate the light direction)
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector (reflect light around normal)
    float3 ReflectionVector = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    // Specular
    float4 specular = SpecularIntensity * SpecularColor * 
                      pow(max(0, dot(ReflectionVector, V)), Shininess);
    
    // Combine all lighting
    output.Color = saturate(ambient + diffuse + specular);
    
    return output;
}

float4 GouraudPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Simply return the color from the vertex shader
    return input.Color;
}


// ========================== PHONG (Per-Fragment Lighting) ==========================

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
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
    
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
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector (reflect light direction around normal)
    float3 ReflectionVector = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    float4 specular = SpecularIntensity * SpecularColor * 
                      pow(max(0, dot(ReflectionVector, V)), Shininess);
    
    // Combine all lighting
    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    
    return color;
}


// ========================== PHONG-BLINN ==========================

VertexShaderOutput PhongBlinnVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Pass world position to pixel shader
    output.WorldPosition = worldPosition;
    
    // Pass transformed normal to pixel shader
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
    
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 PhongBlinnPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // N - Normal vector
    float3 N = normalize(input.Normal.xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    
    // L - Light vector (direction to light)
    float3 L = normalize(-DiffuseLightDirection);
    
    // H - Halfway vector (between view and light)
    float3 H = normalize(L + V);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    // Blinn-Phong uses N·H instead of R·V
    float4 specular = SpecularIntensity * SpecularColor * 
                      pow(max(0, dot(N, H)), Shininess);
    
    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    
    return color;
}


// ========================== SCHLICK ==========================
VertexShaderOutput SchlickVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Pass world position to pixel shader
    output.WorldPosition = worldPosition;
    
    // Pass transformed normal to pixel shader
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
    
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 SchlickPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // N - Normal vector
    float3 N = normalize(input.Normal.xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    
    // L - Light vector (direction to light)
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector
    float3 ReflectionVector = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    // Schlick approximation for specular
    float RdotV = max(0, dot(ReflectionVector, V));
    float schlickFactor = RdotV / (Shininess - Shininess * RdotV + RdotV);
    float4 specular = SpecularIntensity * SpecularColor * schlickFactor;
    
    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    
    return color;
}


// ========================== TOON (Cel-Shading) ==========================
VertexShaderOutput ToonVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    output.WorldPosition = worldPosition;
    
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
    
    // Color not needed for vertex shader in Toon shading
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 ToonPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // N - Normal vector (normalize the interpolated normal)
    float3 N = normalize(input.Normal.xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    
    // L - Light vector (direction to light, so negate the light direction)
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector (reflect light direction around normal)
    float3 ReflectionVector = reflect(-L, N);
    
    // D - Dot product of View and Reflection (specular factor)
    float D = dot(V, ReflectionVector);

    // Darkest
    if (D < -0.7)
    {
        return float4(0, 0, 0, 1); 
    }
    // Dark gray
    else if (D < 0.2)
    {
        return float4(0.25, 0.25, 0.25, 1); 
    }
    // Medium gray
    else if (D < 0.97)
    {
        return float4(0.5, 0.5, 0.5, 1); 
    }
    // Bright white
    else
    {
        return float4(1, 1, 1, 1); 
    }
}


// ========================== HALF-LIFE ==========================

VertexShaderOutput HalfLifeVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    
    // Pass world position to pixel shader
    output.WorldPosition = worldPosition;
    
    // Pass transformed normal to pixel shader
    output.Normal = mul(input.Normal, WorldInverseTranspose);
    
    output.TexCoord = input.TexCoord;
    output.R = float3(0, 0, 0);
    
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 HalfLifePixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // N - Normal vector
    float3 N = normalize(input.Normal.xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - input.WorldPosition.xyz);
    
    // L - Light vector (direction to light)
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector
    float3 ReflectionVector = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    // Half-Life style specular uses (R·V)^2 / Shininess as the specular factor
    float RdotV = max(0, dot(ReflectionVector, V));
    float halfLifeFactor = pow(RdotV, 2) / Shininess;
    float4 specular = SpecularIntensity * SpecularColor * halfLifeFactor;
        
    float4 color = saturate(ambient + diffuse + specular);
    color.a = 1;
    
    return color;
}


// ========================== REFLECTION (Environment Mapping) ==========================

VertexShaderOutput ReflectionVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);
    output.TexCoord = input.TexCoord;
    
    float3 N = normalize(mul(input.Normal.xyz, WorldInverseTranspose).xyz);
    float3 I = normalize(worldPos.xyz - CameraPosition);
    
    // Calculate the reflection vector
    output.R = reflect(I, N);
    
    output.Color = float4(1, 1, 1, 1);
    output.Normal = input.Normal;
    output.WorldPosition = worldPos;
    
    return output;
}

float4 ReflectionPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Get the reflection color from the skybox and the texture
    float4 reflectionColor = texCUBE(SkyBoxSampler, input.R);
    float4 decalColor = tex2D(tsampler1, input.TexCoord);
    
    // Only apply texture if applyTexture flag is true
    if (applyTexture)
    {
        return lerp(decalColor, reflectionColor, 0.5);
    }
    else
    {
        return reflectionColor;
    }
}


// ========================== REFRACTION ==========================

VertexShaderOutput RefractionVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);
    output.TexCoord = input.TexCoord;
    
    // Transform normal to world space
    float3 N = normalize(mul(input.Normal.xyz, WorldInverseTranspose).xyz);
    
    // Incident vector: view direction (from camera to surface)
    float3 I = normalize(worldPos.xyz - CameraPosition);
    
    // Calculate the refraction vector using Snell's Law
    output.R = refract(I, N, RefractionIndex);
    
    output.Color = float4(1, 1, 1, 1);
    output.Normal = input.Normal;
    output.WorldPosition = worldPos;
    
    return output;
}

float4 RefractionPixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Get the refraction color from the skybox using the refraction vector
    float4 refractionColor = texCUBE(SkyBoxSampler, input.R);
    float4 decalColor = tex2D(tsampler1, input.TexCoord);
    
    // Blend refraction and model texture
    if (applyTexture)
    {
        return lerp(decalColor, refractionColor, 0.5);
    }
    else
    {
        return refractionColor;
    }
}


// ========================== REFRACTION + DISPERSION ==========================

struct VertexShaderOutputDispersion
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float3 I : TEXCOORD3;
    float3 N : TEXCOORD4;
};

VertexShaderOutputDispersion RefractionDispersionVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutputDispersion output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);
    output.TexCoord = input.TexCoord;
    
    // Transform normal to world space
    output.N = normalize(mul(input.Normal.xyz, WorldInverseTranspose).xyz);
    
    // Incident vector: view direction (from camera to surface)
    output.I = normalize(worldPos.xyz - CameraPosition);
    
    output.Color = float4(1, 1, 1, 1);
    output.Normal = input.Normal;
    output.WorldPosition = worldPos;
    
    return output;
}

float4 RefractionDispersionPixelShaderFunction(VertexShaderOutputDispersion input) : COLOR0
{
    // Normalize vectors
    float3 N = normalize(input.N);
    float3 I = normalize(input.I);
    
    // Different refraction indices for each color channel (simulating dispersion)
    // Use the eta ratio parameters for each channel
    float3 refractRed = refract(I, N, RefractionIndex * EtaRatioRed);
    float3 refractGreen = refract(I, N, RefractionIndex * EtaRatioGreen);
    float3 refractBlue = refract(I, N, RefractionIndex * EtaRatioBlue);
    
    // Sample the environment map with each refracted direction
    float redChannel = texCUBE(SkyBoxSampler, refractRed).r;
    float greenChannel = texCUBE(SkyBoxSampler, refractGreen).g;
    float blueChannel = texCUBE(SkyBoxSampler, refractBlue).b;
    
    // Combine the channels
    float4 refractionColor = float4(redChannel, greenChannel, blueChannel, 1.0f);
    float4 decalColor = tex2D(tsampler1, input.TexCoord);
    
    // Blend refraction and model texture
    if (applyTexture)
    {
        return lerp(decalColor, refractionColor, 0.5);
    }
    else
    {
        return refractionColor;
    }
}


// ========================== FRESNEL ==========================

struct VertexShaderOutputFresnel
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
    float2 TexCoord : TEXCOORD2;
    float3 I : TEXCOORD3;
    float3 N : TEXCOORD4;
    float3 R : TEXCOORD5;
};

VertexShaderOutputFresnel FresnelVertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutputFresnel output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);
    output.TexCoord = input.TexCoord;
    output.WorldPosition = worldPos;
    
    // Transform normal to world space
    output.N = normalize(mul(input.Normal.xyz, WorldInverseTranspose).xyz);
    
    // Incident vector: view direction (from camera to surface)
    output.I = normalize(worldPos.xyz - CameraPosition);
    
    // Calculate reflection vector for reflected color sampling
    output.R = reflect(output.I, output.N);
    
    output.Color = float4(1, 1, 1, 1);
    
    return output;
}

float4 FresnelPixelShaderFunction(VertexShaderOutputFresnel input) : COLOR0
{
    // Normalize vectors
    float3 N = normalize(input.N);
    float3 I = normalize(input.I);
    float3 R = normalize(input.R);
    
    // Calculate Fresnel reflection coefficient
    // rC = max(0, min(1, bias + scale * pow(1 + dot(I, N), power)))
    float dotProduct = dot(I, N);
    float fresnelCoefficient = FresnelBias + FresnelScale * pow(1.0f + dotProduct, FresnelPower);
    fresnelCoefficient = max(0.0f, min(1.0f, fresnelCoefficient));
    
    // Get reflected color from environment map
    float4 reflectedColor = texCUBE(SkyBoxSampler, R);
    
    // Get refracted color from environment map
    float3 refractedDir = refract(I, N, RefractionIndex);
    float4 refractedColor = texCUBE(SkyBoxSampler, refractedDir);
    
    // Blend based on Fresnel coefficient
    // Cfinal = rC * Creflected + (1 - rC) * Crefracted
    float4 finalColor = fresnelCoefficient * reflectedColor + (1.0f - fresnelCoefficient) * refractedColor;
    finalColor.a = 1.0f;
    
    return finalColor;
}


technique Gouraud
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 GouraudVertexShaderFunction();
        PixelShader = compile ps_4_0 GouraudPixelShaderFunction();
    }
}

technique Phong
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongVertexShaderFunction();
        PixelShader = compile ps_4_0 PhongPixelShaderFunction();
    }
}

technique PhongBlinn
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 PhongBlinnVertexShaderFunction();
        PixelShader = compile ps_4_0 PhongBlinnPixelShaderFunction();
    }
}

technique Schlick
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 SchlickVertexShaderFunction();
        PixelShader = compile ps_4_0 SchlickPixelShaderFunction();
    }
}

technique Toon
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 ToonVertexShaderFunction();
        PixelShader = compile ps_4_0 ToonPixelShaderFunction();
    }
}

technique HalfLife
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 HalfLifeVertexShaderFunction();
        PixelShader = compile ps_4_0 HalfLifePixelShaderFunction();
    }
}

technique Reflection
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 ReflectionVertexShaderFunction();
        PixelShader = compile ps_4_0 ReflectionPixelShaderFunction();
    }
}

technique Refraction
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 RefractionVertexShaderFunction();
        PixelShader = compile ps_4_0 RefractionPixelShaderFunction();
    }
}

technique RefractionDispersion
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 RefractionDispersionVertexShaderFunction();
        PixelShader = compile ps_4_0 RefractionDispersionPixelShaderFunction();
    }
}

technique Fresnel
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 FresnelVertexShaderFunction();
        PixelShader = compile ps_4_0 FresnelPixelShaderFunction();
    }
}   