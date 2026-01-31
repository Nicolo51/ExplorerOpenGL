Texture2D FontTexture;
sampler TextureSampler = sampler_state 
{ 
    texture = <FontTexture>; 
    magfilter = LINEAR; 
    minfilter = LINEAR; 
    mipfilter = LINEAR; 
    AddressU = clamp; 
    AddressV = clamp; 
};

float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 originalColor = tex2D(TextureSampler, texCoord);
    return originalColor*color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}