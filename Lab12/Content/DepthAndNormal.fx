float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Position2D : TEXCOORD0;
    float4 Normal : NORMAL;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Position2D : TEXCOORD0;
    float3 Normal : TEXCOORD1;
};

VertexShaderOutput DepthMapVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(mul(mul(input.Position, World), View), Projection);
    output.Position2D = output.Position;
    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose).xyz);
    return output;
}

float4 DepthMapPixelShader(VertexShaderOutput input) : COLOR0
{
    float4 projTexCoord = input.Position2D / input.Position2D.w;
    projTexCoord.xy = 0.5 * projTexCoord.xy + float2(0.5, 0.5);
    projTexCoord.y = 1.0 - projTexCoord.y; // invert Y direction
    float depth = 1 - projTexCoord.z; // invert depth Z
    float4 color;
    color.rgb = (normalize(input.Normal.xyz)) / 2.0f + 0.5f;
    color.a = (depth > 0) ? depth : 0; // culling
    return color;
}

technique DepthAndNormalMap
{
	pass P0
	{
        VertexShader = compile vs_4_0 DepthMapVertexShader();
        PixelShader = compile ps_4_0 DepthMapPixelShader();
    }
}