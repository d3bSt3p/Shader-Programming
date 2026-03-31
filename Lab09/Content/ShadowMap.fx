float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;
float3 CameraPosition;
float3 LightPosition;

sampler ShadowMapSampler = sampler_state {
	texture = <ProjectiveTexture>;
	magfilter = LINEAR;
	minfilter = LINEAR;
	mipfilter = LINEAR;
	AddressU = border;
	AddressV = border;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float4 Position2D : TEXCOORD0;
};


VertexShaderOutput ShadowMapVertexShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	output.Position = mul(mul(input.Position, LightViewMatrix), LightProjectionMatrix);
	output.Position2D = output.Position;
	return output;
}

float4 ShadowMapPixelShader(VertexShaderOutput input) : COLOR0
{

	float4 projTexCoord = input.Position2D / input.Position2D.w;
	
	projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5); // [-1, 1] --> [0, 1]
	projTexCoord.y = 1.0 - projTexCoord.y; // invert Y direction

	float depth = 1.0 - projTexCoord.z; // invert depth Z
	float4 color = (depth>0) ? depth : 0; // culling
	return color;
}


technique ShadowMap
{
	pass Pass0
	{
		VertexShader = compile vs_4_0 ShadowMapVertexShader();
		PixelShader = compile ps_4_0 ShadowMapPixelShader();
	}
}