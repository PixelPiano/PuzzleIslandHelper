#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float2 Center = float2(0.5,0.5);
uniform float Amplitude;
uniform float Speed = 20;
uniform float Size = 0.01;
float2 Circle(float2 uv, float2 center, float size)
{
    float r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
}
#define PI 3.14159265358979323846

uniform float3 ORANGE = float3(1.0, 0.6, 0.2);
uniform float3 PINK   = float3(0.7, 0.1, 0.2); 
uniform float3 BLUE   = float3(0.4, 0.2, 0.9);
uniform float3 GREEN  = float3(0.1, 0.9, 0.4); 
uniform float3 BLACK  = float3(0.0, 0.0, 0.3);


float hash( float n ) {
    return frac(sin(n)*43758.5453123);   
}



float noise1( in float2 x ){
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
    f += 0.5000 * noise1(p); p = mul(p,m) * 2.02;
    f += 0.2500 * noise1(p); p = mul(p,m) * 2.03;
    f += 0.1250 * noise1(p); p = mul(p,m) * 2.01;
    f += 0.0625 * noise1(p);
    f /= 0.9375;
    return f;
}
float random (in float2 st) {
    return frac(sin(dot(st.xy,
                         float2(12.9898,78.233)))
                * 43758.5453123);
}

//value noise func
float noise (float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);
    float2 u = f*f*(3.-2.*f);
    
    return lerp( lerp(random(i +float2(0.,0.)), random(i+ float2(1.,0.)),u.x),
                lerp(random(i +float2(0.,1.)), random(i+ float2(1.,1.)),u.x), u.y);
}

float2x2 rotate2d(float angle){
    return float2x2(cos(angle),-sin(angle),
                sin(angle),cos(angle));
}

float2 lines (in float2 pos, float b)
{
    float scale = 1;
    pos*=scale;
    return smoothstep(0., 0.5+b*0.5, abs((sin(pos.x*3.1415)+b*2.))*.5);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float4 color = SAMPLE_TEXTURE(text, uv);
	float amount = Time % 1;
	float2 worldPos = (uv * Dimensions) + CamPos;
    float dist = Circle(uv, Center, Size * Amplitude);
	// Normalized pixel coordinates (from 0 to 1)
	if(dist < Size * Amplitude)
	{
		float2 p = uv/Dimensions;
    	p.y*= Dimensions.y/Dimensions.x;
	
    	float2 pos = p *(float2)2000;
    	float2 pattern = pos;
    	pos = mul(pos,mul(rotate2d(1),rotate2d(noise(pos + Time))));
    	pattern = lines(pos,0.1);
		pattern *= 5;
		return SAMPLE_TEXTURE(text, Center - Size + Size * pattern);
	}
	return float4(0,0,0,0);
    
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