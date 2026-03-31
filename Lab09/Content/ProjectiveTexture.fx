float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

float4x4 LightViewMatrix;
float4x4 LightProjectionMatrix;

float3 CameraPosition;
float3 LightPosition;

// Light Uniforms
float AmbientColor;

texture ProjectiveTexture;
sampler ProjectiveTextureSampler = sampler_state
{
	Texture = <ProjectiveTexture>;
	MinFilter = none;
	MagFilter = none;
	MipFilter = none;
	AddressU = border;
	AddressV = border;
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
	float3 Normal : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float3 WorldPosition : TEXCOORD2;
};
//TBN rotation matrix per vertex for bumpmapping
VertexShaderOutput VSFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPos = mul(input.Position, World);
	float4 viewPosition = mul(worldPos, View);
	output.Position = mul(viewPosition, Projection);

	output.WorldPosition = worldPos.xyz;
	output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);  
	output.TexCoord = input.TexCoord;

	return output;
}
//projective texturing w/ ambient w/ culling
float4 PSFunction(VertexShaderOutput input) : COLOR0
{
	//Step1-2: calculate projective texture coordinate	
	float4 projTexCoord = mul(mul(float4(input.WorldPosition, 1.0), LightViewMatrix), LightProjectionMatrix);
    projTexCoord = projTexCoord / projTexCoord.w; //*** Important !!!
	//Step3: compress -1~1 to 0~1
	projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5);
	//Step4: inverse the y dimension
	projTexCoord.y = 1.0 - projTexCoord.y;

	//Step5: Depth shall be 1-z because z is inverse proportional to depth (e.g. near_plane = 1, far_plane = 0)
	float depth = 1.0 - projTexCoord.z;

	//Step6: reference to the prjective texture	
	float4 color = (depth>0) ? tex2D(ProjectiveTextureSampler, projTexCoord.xy) : 0;

	//(0,1,1) is transparent key **** NOT Necessary
	//if (color.x == 0 && color.y == 1 && color.z == 1)  color.xyz = float3(0, 0, 0);
	//back-face culling
	float3 N = normalize(input.Normal);
	float3 L = normalize(LightPosition - input.WorldPosition);  //light vector
	if (dot(L, N)<0) color = 0;
	//ambient
	color += float4(AmbientColor, AmbientColor, AmbientColor, 1.0);

	return color;
}
technique Technique1
{
	pass Pass1
	{
		VertexShader = compile vs_4_0 VSFunction();
		PixelShader = compile ps_4_0 PSFunction();
	}
}
