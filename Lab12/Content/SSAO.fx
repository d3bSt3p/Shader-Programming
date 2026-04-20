float4x4 World;
float4x4 View;
float4x4 Projection;
float4x4 WorldInverseTranspose;
float offset;
float rad = 0.01f;
texture RandomNormalTexture;
texture DepthAndNormalTexture;

sampler randomMap = sampler_state
{
    Texture = <RandomNormalTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
};

sampler normalMap = sampler_state
{
    Texture = <DepthAndNormalTexture>;
    MipFilter = NONE;
    MinFilter = POINT;
    MagFilter = POINT;
};

struct VS_OUTPUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT RenderSceneVS(float4 vPos : POSITION, float2 TexCoord : TEXCOORD0)
{
    VS_OUTPUT Output;
    Output.Position = vPos;
    vPos.xy = sign(vPos.xy);
    Output.TexCoord.x = (vPos.x + 1.0f) * 0.5f;
    Output.TexCoord.y = 1.0f - (vPos.y + 1.0f) * 0.5f;
    return Output;
}

float4 RenderScenePS0(VS_OUTPUT Input) : COLOR0
{
    float3 pSphere[16] =
    {
        float3(0.53812504, 0.18565957, -0.43192),
        float3(0.13790712, 0.24864247, 0.44301823),
        float3(0.33715037, 0.56794053, -0.005789503),
        float3(-0.6999805, -0.04511441, -0.0019965635),
        float3(0.06896307, -0.15983082, -0.85477847),
        float3(0.056099437, 0.006954967, -0.1843352),
        float3(-0.014653638, 0.14027752, 0.0762037),
        float3(0.010019933, -0.1924225, -0.034443386),
        float3(-0.35775623, -0.5301969, -0.43581226),
        float3(-0.3169221, 0.106360726, 0.015860917),
        float3(0.010350345, -0.58698344, 0.0046293875),
        float3(-0.08972908, -0.49408212, 0.3287904),
        float3(0.7119986, -0.0154690035, -0.09183723),
        float3(-0.053382345, 0.059675813, -0.5411899),
        float3(0.035267662, -0.063188605, 0.54602677),
        float3(-0.47761092, 0.2847911, -0.0271716)        
     };
    float4 Output;
    
    // *** Step1: Get a random normal
    float3 randomNormal = normalize((tex2D(randomMap, Input.TexCoord * offset).xyz * 2.0) - 1.0f);
    //g_screen_size * uv / random_size
    // *** Step2: Get the normal and depth from sampler (G-buffer)
    float depth = tex2D(normalMap, Input.TexCoord).a;
    float3 normal = tex2D(normalMap, Input.TexCoord).xyz * 2.0f - 1.0f;
    float bl = 0.0;
    float radD = rad / depth; // Update the size of radius
    for (int i = 0; i < 16; ++i)
    {
        // *** Step3: Get a sample ray
        float3 ray = radD * reflect(pSphere[i], randomNormal);
        // *** Step4: Get the UV of sample ray
        float2 sampleCoord = Input.TexCoord + sign(dot(ray, normal)) * ray.xy * float2(1.0f, -1.0f);
        // *** Step5: Get the depth and normal of the sample ray
        float sampleDepth = tex2D(normalMap, sampleCoord).a; //[0,1]
        float3 occNormal = tex2D(normalMap, sampleCoord).xyz * 2.0f - 1.0f; //[-1,1]
        
        // *** Step6: Calcualte the diff between sample and fragment
        float depthDifference = max(depth - sampleDepth, 0);
        // *** Step7: Update the color for each sample
        float normDiff = (1.0 - dot(occNormal, normal));
        bl += normDiff * (1.0 - depthDifference);
    }
    float ao = 1.0 - bl / 16;
    Output = float4(ao, ao, ao, 1.0f);
    return Output;
}
technique RenderScene0
{
    pass P0
    {
        VertexShader = compile vs_4_0 RenderSceneVS();
        PixelShader = compile ps_4_0 RenderScenePS0();
    }
}