float4x4 World;
float4x4 View;
float4x4 Projection;

extern bool FogEnabled;
extern float FogNear;
extern float FogFar;

extern float3 playerPos;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float4 Pos2 : TEXCOORD1;
};
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;
    
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;
    output.Pos2 = worldPosition;
    
    return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    
    float4 color = input.Color * input.Color.a;
    float3 fogColor = float3(0.529, 0.808, 0.922);
    float distance = length(float4(playerPos, 0) - input.Pos2);
    
    if (FogEnabled)
    {
        float f = lerp(0, 1, (distance - FogNear) / (FogFar - FogNear));
        f = clamp(f, 0, 1);
        f = pow(f, 2);
        color = lerp(color, float4(fogColor, 0), f);
    }

    return color;

}
technique Ambient
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}