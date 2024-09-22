#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)

float4x4 World;
uniform float Time;
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};
float fbm( in float x, in float H )
{    
    float t = 0.0;
    for( int i=0; i<20; i++ )
    {
        float f = pow( 2.0, float(i) );
        float a = pow( f, -H );
        t += a*noise(f*x);
    }
    return t;
}
#define PI 3.14159265359
float test()
{
    return 2.9;
}
struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 uv : TEXCOORD0;
};
#define M_PI 3.14159265358979323846

float rand(float2 co){return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);}
float rand (float2 co, float l) {return rand(float2(rand(co), l));}
float rand (float2 co, float l, float t) {return rand(float2(rand(co, l), t));}
float2 hash( in float2 x )   // this hash is not production ready, please
{                        // replace this by something better
    const float2 k = float2( 0.3183099, 0.3678794 );
    x = x*k + k.yx;
    return -1.0 + 2.0*frac( 16.0 * k*frac( x.x*x.y*(x.x+x.y)) );
}
float perlin(float2 p, float dim, float time) {
	float2 pos = floor(p * dim);
	float2 posx = pos + float2(1.0, 0.0);
	float2 posy = pos + float2(0.0, 1.0);
	float2 posxy = pos + float2(1.0,1.0);
	
	float c = rand(pos, dim, time);
	float cx = rand(posx, dim, time);
	float cy = rand(posy, dim, time);
	float cxy = rand(posxy, dim, time);
	
	float2 d = frac(p * dim);
	d = -0.5 * cos(d * M_PI) + 0.5;
	
	float ccx = lerp(c, cx, d.x);
	float cycxy = lerp(cy, cxy, d.x);
	float center = lerp(ccx, cycxy, d.y);
	
	return center * 2.0 - 1.0;
}
float3 noised( in float2 p )
{

    float2 i = floor( p );
    float2 f = frac( p );


    // quintic interpolation
    float2 u = f*f*f*(f*(f*6.0-15.0)+10.0);
    float2 du = 30.0*f*f*(f*(f-2.0)+1.0);
 
    float2 ga = hash( i + float2(0.0,0.0) );
    float2 gb = hash( i + float2(1.0,0.0) );
    float2 gc = hash( i + float2(0.0,1.0) );
    float2 gd = hash( i + float2(1.0,1.0) );
    
    float va = dot( ga, f - float2(0.0,0.0) );
    float vb = dot( gb, f - float2(1.0,0.0) );
    float vc = dot( gc, f - float2(0.0,1.0) );
    float vd = dot( gd, f - float2(1.0,1.0) );

    return float3( va + u.x*(vb-va) + u.y*(vc-va) + u.x*u.y*(va-vb-vc+vd),   // value
                 ga + u.x*(gb-ga) + u.y*(gc-ga) + u.x*u.y*(ga-gb-gc+gd) +  // derivatives
                 du * (u.yx*(va-vb-vc+vd) + float2(vb,vc) - va));
}
VertexShaderOutput _VertexShader(VertexShaderInput input)
{
    VertexShaderOutput result;

	float4 newPos = input.Position;
	//newPos.z += perlin(input.Position.xy,10,Time * 0.000001) * 0.3;
	newPos.z += noised(input.Position.xy * Time * 0.001);
	result.uv = float2(input.Position.x / 320.,input.Position.y / 180.);
    newPos = mul(newPos, World);
	result.Color = input.Color;
	result.Color = lerp(result.Color, float4(0,0,0,1),newPos.z);
	newPos.z = 0;
    result.Position = newPos;
    return result;
}

float4 _PixelShader(VertexShaderOutput input) : COLOR0
{
    return input.Color;
}

technique Primitive
{
    pass Main
    {
        VertexShader = compile vs_3_0 _VertexShader();
        PixelShader = compile ps_3_0 _PixelShader();
    }
};