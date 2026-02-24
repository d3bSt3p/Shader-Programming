// create VertexPositionColor struct
struct VertexPositionColor
{
	float4 position : POSITION;
	float4 color : COLOR;
};

// vertex shader that passes through position and color from the VertexPositionColor struct
VertexPositionColor MyVertexShader(VertexPositionColor input)
{
	VertexPositionColor output;
	output.position = input.position;
	output.color = input.color;
	return output;
}

// pixel shader that outputs the color passed through the vertex shader from the VertexPositionColor struct
float4 MyPixelShader(VertexPositionColor input): COLOR
{
	float4 color = input.color;

	// set the red channel to 0
	//color.r = 0.0f;
	

	return color;
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


