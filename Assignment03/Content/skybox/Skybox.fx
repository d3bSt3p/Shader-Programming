float4x4 World;
float4x4 View;
float4x4 Projection; 
float3 CameraPosition;
Texture SkyBoxTexture;

samplerCUBE SkyBoxSampler = sampler_state 
{ 
 texture = <SkyBoxTexture>; 
 magfilter = LINEAR; 
 minfilter = LINEAR; 
 mipfilter = LINEAR; 
 AddressU = Mirror; 
 AddressV = Mirror; 
};

struct VertexInput
{
	float4 Position: POSITION0;	
};

struct VertexOutput
{
	float4 Position: POSITION0;
	float3 TextureCoordinate: TEXCOORD0;
};

VertexOutput SkyboxVertexShader (VertexInput input)
{
	VertexOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);	
	float4 VertexPosition = worldPos;
	output.TextureCoordinate = VertexPosition.xyz - CameraPosition;
	return output;
}
float4 SkyboxPixelShader (VertexOutput input ) : COLOR
{
	return texCUBE(SkyBoxSampler, normalize(input.TextureCoordinate));
}

technique Skybox
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 SkyboxVertexShader();
		PixelShader = compile ps_4_0 SkyboxPixelShader();
	}
}