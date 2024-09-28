#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define PI 3.14
uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float2 Position;
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
texture2D lights_texture;
sampler2D lights_sampler = sampler_state
{
    Texture = <lights_texture>;
    MagFilter = Point;
    MinFilter = Point;
    AddressU = Clamp;
    AddressV = Clamp;
};
float hashOld22( float2 p )
{
	p = float2( dot(p,float2(127.1,311.7)),
			  dot(p,float2(269.5,183.3)));

	return frac(sin(p)*43758.5453123);
}
float distToEdge(float p,float plane)
{
	return smoothstep(0, plane, abs(p - plane/2));
}
float fuzz(float2 uv, float light, float distToOne, float from)
{
	float ratio = 1/distToOne;
	float r = hashOld22(uv * Time);
	float x =  (distToEdge(uv.x, 1) - from) / 0.5;
	//smoothstep(0.4, 0.5,abs(uv.x - 0.5));
	//float y = smoothstep(0.4, 0.5,abs(uv.y - 0.5));
	float y = (distToEdge(uv.y, 1) - from) / 0.5;
	float limit = Time % 1;
	float val = 1 - clamp(max(x, y), 0, ratio) / ratio;

	return val;
}
float fuzz(float2 uv, float amount)
{
	float r = hashOld22(uv * Time) * 0.1;
	float x = smoothstep(0.5 - amount + r, 0.5, abs(uv.x - 0.5));
	float y = smoothstep(0.5 - amount + r, 0.5, abs(uv.y - 0.5));
	float val = max(x, y);
	return val;
}
float randomize(float val, float2 seed)
{
	float r = hashOld22(seed) * val;
	return step(1 - val, r) * val;
	return smoothstep(0,0., r * val);
}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	float4 color = tex2D(lights_sampler, uv);
	float t = Time * 1.2;
	int xDir = sign(uv.x - 0.5);
	int yDir = sign(uv.y - 0.5);
	float wave = ((uv.x + (uv.y * xDir + t) * 10));
	float wave2 = ((uv.y + (uv.x * -yDir + t) * 10));
	float s = cos(wave) * 0.002;
	float s2 = sin(wave2) * 0.002;
	uv.x += s;
	uv.y += s2;
	float r = hashOld22(uv * Time);
	float val = fuzz(uv, 0.1);
	float v = randomize(1 - max(val,color.a), uv * Time);
	float v2 = hashOld22(uv * 2.222555* Time) * v;
	v2 = round(v2) * 0.2;
	return float4(v2,v2,v2,round(v + v2));
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