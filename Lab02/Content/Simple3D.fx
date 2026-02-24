// offset global variable
float3 offset;

// matrixes for 3d computer graphics
float4x4 World;
float4x4 View;
float4x4 Projection;

// define a sampler
texture MyTexture;
sampler MySampler = sampler_state
{
	Texture = <MyTexture>;
};

// create VertexPositionTexture struct
struct VertexPositionTexture
{	
	float4 position : POSITION;
	float2 textureCoordinate : TEXCOORD;
};

// multiply each matrix above
VertexPositionTexture MyVertexShader(VertexPositionTexture input)
{	
	VertexPositionTexture output;
	float4 worldPos = mul(input.position, World);
	float4 viewPos = mul(worldPos, View);
	output.position = mul(viewPos, Projection);
	output.textureCoordinate = input.textureCoordinate;
	return output;
}

// pixel shader that samples the texture using the texture coordinates (UV)passed through the vertex shader from the VertexPositionTexture struct
float4 MyPixelShader(VertexPositionTexture input): COLOR0 
{
    float2 textureCoordinate = input.textureCoordinate; 
	
    return tex2D(MySampler, textureCoordinate);
}

// apply the shaders in a technique
technique MyTechnique
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}