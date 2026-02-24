float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float3 CameraPosition;
texture decalMap;
texture environmentMap;

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

struct VertexInput
{
    float4 Position : POSITION;
    float4 Normal : NORMAL;
    float2 TexCoord : TEXCOORD0; 
};

struct VertexOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;   
    float3 R : TEXCOORD1;          
};

VertexOutput ReflectionVertexShader(VertexInput input)
{
    VertexOutput output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);
    output.TexCoord = input.TexCoord;
        
    float3 N = normalize(mul(input.Normal.xyz, WorldInverseTranspose).xyz);  
    float3 I = normalize(worldPos.xyz - CameraPosition);
    
    // Calculate the reflection vector
    output.R = reflect(I,N);
    
    return output;
}


float4 ReflectionPixelShader(VertexOutput input) : COLOR
{
    // use our samplers to get the reflection color from the skybox and the texture

    float4 reflectionColor = texCUBE(SkyBoxSampler, input.R);
        
    float4 decalColor = tex2D(tsampler1, input.TexCoord);
        
    return lerp(decalColor, reflectionColor, 0.5);
}

technique Reflection
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 ReflectionVertexShader();
        PixelShader = compile ps_4_0 ReflectionPixelShader();
    }
}