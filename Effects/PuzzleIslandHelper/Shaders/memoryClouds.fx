//inspired by https://www.shadertoy.com/view/WtjyzR
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
#define NUM_LAYERS 80.
#define ITER 23

float4 tex(float3 p)
{
    float t = Time+78.;
    float4 o = float4(p.xyz,3.*sin(t*.1));
    float4 dec = float4 (1.,.9,.1,.15) + float4(.06*cos(t*.1),0,0,.14*cos(t*.23));
    for (int i=0 ; i++ < ITER;) o.xzyw = abs(o/dot(o,o)- dec);
    return o;
}

DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{  
    float3 col = float3(0,0,0);   
    float t= Time* .3;
    
	for(float i=0.; i<=1.; i+=1./NUM_LAYERS)
    {
        float d = frac(i+t); // depth
        float s = lerp(5.,.5,d); // scale
        float f = d * smoothstep(1.,.9,d); //fade
        col+= tex(float3(uv*s,i*4.)).xyz*f;
    }
    
    col/=NUM_LAYERS;
    col*=float3(2,1.,2.);
   	col=pow(col,float3(.5,.5,.5 ));
    float4 re = float4(col, 0);
    float4 te = SAMPLE_TEXTURE(text, uv);
    return re;
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