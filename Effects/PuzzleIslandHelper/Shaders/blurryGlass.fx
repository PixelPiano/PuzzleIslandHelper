#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
float hash( float n ) {
    return frac(sin(n)*43758.5453123);   
}
float noise( in float2 x ){
    float2 p = floor(x);
    float2 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
	float n = p.x + p.y;
	float lerpA = lerp(hash(n),hash(n + 1.0),f.x);
	float lerpB = lerp(hash(n + 57.0),hash(n + 58.0),f.x);
    return lerp(lerpA,lerpB, f.y);
}
float2x2 m = float2x2( 0.6, 0.6, -0.6, 0.8);
float fbm(float2 p){
    float f = 0.0;
    f += 0.5000 * noise(p); p = mul(p,m) * 2.02;
    f += 0.2500 * noise(p); p = mul(p,m) * 2.03;
    f += 0.1250 * noise(p); p = mul(p,m) * 2.01;
    f += 0.0625 * noise(p);
    f /= 0.9375;
    return f;
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
	return SAMPLE_TEXTURE(text, uv + fbm(Time * fbm(worldPos)) / Dimensions);
    
}
void SpriteVertexShader(inout float4 color: COLOR0,
	inout float2 texCoord : TEXCOORD0,
	inout float4 position : SV_Position)
{
	position = mul(position, ViewMatrix);
	position = mul(position, TransformMatrix);
}
technique Shader
{
	pass pass0
	{
		VertexShader = compile vs_3_0 SpriteVertexShader();
		PixelShader = compile ps_3_0 SpritePixelShader();
	}
}