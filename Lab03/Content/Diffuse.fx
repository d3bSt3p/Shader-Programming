float4x4 World;
float4x4 View;
float4x4 Projection;

float4x4 WorldInverseTranspose;

float4 AmbientColor;
float AmbientIntensity;

float3 DiffuseLightDirection;
float4 DiffuseColor;
float DiffuseIntensity;

struct VertexInput
{
	float4 Position: POSITION;
	float4 Normal: NORMAL;
};

struct VertexOutput
{
	float4 Position: POSITION;
	float4 Color: COLOR;
};

VertexOutput MyVertexShader (VertexInput input)
{
	
	VertexOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	float4 normal = mul(input.Normal, WorldInverseTranspose);

	float lightIntensity = dot(normal.xyz, DiffuseLightDirection); //rename to defuse
	output.Color = saturate(DiffuseColor * DiffuseIntensity * lightIntensity);
	return output;
}

float4 MyPixelShader (VertexOutput input ) : COLOR
{
	return input.Color + AmbientColor * AmbientIntensity;
}

technique Diffuse
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 MyVertexShader();
		PixelShader = compile ps_4_0 MyPixelShader();
	}
}

