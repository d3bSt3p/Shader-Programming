// offset global variable
float3 offset;

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

// vertex shader that passes through position and texture coordinates from the VertexPositionTexture struct
VertexPositionTexture MyVertexShader(VertexPositionTexture input)
{	
	input.position.xyz += offset;
	return input;
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