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

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Normal : TEXCOORD0;
    float4 WorldPosition : TEXCOORD1;
};


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
          
    // N - Normal vector (normalize the surface normal)
    float3 N = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    
    // V - View vector (from surface point to camera)
    float3 V = normalize(CameraPosition - worldPosition.xyz);
    
    // L - Light vector (direction to light, so negate the light direction)
    float3 L = normalize(-DiffuseLightDirection);
    
    // R - Reflection vector (reflect light around normal)
    float3 R = reflect(-L, N);
    
    // Calculate lighting components
    float4 ambient = AmbientColor * AmbientIntensity;
    
    float4 diffuse = DiffuseIntensity * DiffuseColor * max(0, dot(N, L));
    
    // Specular
    float4 specular = SpecularIntensity * SpecularColor * 
                      pow(max(0, dot(R, V)), Shininess);
    
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


// ========================== TOON (Cel-Shading) ==========================

VertexShaderOutput ToonVertexShaderFunction(VertexShaderInput input)
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
    float3 R = reflect(-L, N);
    
    // D - Dot product of View and Reflection (specular factor)
    float D = dot(V, R);
    
    // Toon shading algorithm: discrete lighting levels
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
technique Toon
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 ToonVertexShaderFunction();
        PixelShader = compile ps_4_0 ToonPixelShaderFunction();
    }
}

