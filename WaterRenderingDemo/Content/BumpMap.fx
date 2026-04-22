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

sampler tsampler1 = sampler_state{
	texture = <normalMap>;
	magfilter = LINEAR; // None, POINT, LINEAR, Anisotropic
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = Wrap; // Clamp, Mirror, MirrorOnce, Wrap, Border
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
    float4 Color : COLOR0;
    float3 WorldPosition : TEXCOORD1;
    float3 Normal : NORMAL;       
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    float3 Binormal : BINORMAL0;
};

VertexOutput BumpMapVertexShader(VertexInput input)
{
    VertexOutput output;
    
    // Transform position to screen space
    float4 worldPos = mul(input.Position, World);
    float4 viewPos = mul(worldPos, View);
    output.Position = mul(viewPos, Projection);

    output.Normal = mul(input.Normal, (float3x3)WorldInverseTranspose);
    output.Tangent = mul(input.Tangent, (float3x3)WorldInverseTranspose);
    output.Binormal = mul(input.Binormal, (float3x3)WorldInverseTranspose);

    output.WorldPosition = worldPos.xyz;

    output.TexCoord = input.TexCoord;
    
    return output;
}


float4 BumpMapPixelShader(VertexOutput input) : COLOR
{
    
    float3 normalTexture = (tex2D(tsampler1, input.TexCoord).xyz - float3(0.5, 0.5, 0.5)) * 2.0;

    float3 lightDirection = normalize(LightPosition - input.WorldPosition);

    // calculate the bump normal
    float3 bumpNormal = normalize(input.Normal + normalTexture.x * input.Tangent + normalTexture.y * input.Binormal);
      
    // normalize bump normal and calculate the dot product with the light direction
    float4 diffuse = DiffuseColor * DiffuseIntensity * dot(bumpNormal, lightDirection);

    return diffuse;
}

technique Reflection
{
    pass Pass1
    {
        VertexShader = compile vs_4_0 BumpMapVertexShader();
        PixelShader = compile ps_4_0 BumpMapPixelShader();
    }
}