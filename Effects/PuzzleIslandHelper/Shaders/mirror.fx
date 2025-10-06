#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform float2 Position;
uniform float Modifier;
float2 Circle(float2 uv, float2 center, float2 size)
{
    float2 r = size; //define the radius of the circle
    float invAr = Dimensions.x / Dimensions.y;
	float x = (center.x-uv.x)*invAr;
	float y = (center.y-uv.y);
    float2 d = x*x + y*y; //pythagoreum theorum to find hypoteneuse(dist to middle)
    d = d * 2 - r; // subtract the radius from the dist
    return d;
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
float random (in float2 st) {
    return frac(sin(dot(st.xy,
                         float2(12.9898,78.233)))
                * 43758.5453123);
}

float noise (float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);
    float2 u = f*f*(3.-2.*f);
    
    return lerp( lerp(random(i +float2(0.,0.)), random(i+ float2(1.,0.)),u.x),
                lerp(random(i +float2(0.,1.)), random(i+ float2(1.,1.)),u.x), u.y);
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = (uv * Dimensions) + CamPos;
	float4 color = SAMPLE_TEXTURE(text, uv);
	float2 circle = Circle(uv, Position, Amplitude * 8);
	float4 invert = float4(1 - color.rgb, color.a);
	float2 p = uv/Dimensions;
    p.y*= Dimensions.y/Dimensions.x;
    float2 pos = p *(float2)2000;
    float2 pattern = pos;
    pos = mul(pos,mul(rotate2d(1),rotate2d(noise(pos + Time))));
    pattern = lines(pos,0.1) * 5;
	float4 other = SAMPLE_TEXTURE(text, Position - 1 + pattern);

	invert = lerp(invert, other,Modifier);
	// Normalized pixel coordinates (from 0 to 1)

	float4 output = lerp(invert,color,step(Amplitude, circle.x));
	return output;
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