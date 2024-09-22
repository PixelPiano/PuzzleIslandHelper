#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define PI 3.14

uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
uniform float3 Offsets[27];

const float3 cameraPosition = float3(0.0, -.5, 10.0);
const float3 cameraDirection = float3(0.0, 0.0, -1.0);
const float3 cameraUp = float3(0.0, 1.0, 0.0);
const float fov = 50.0;

const float3 deltax = float3(1 ,0, 0);
const float3 deltay = float3(0 ,1, 0);
const float3 deltaz = float3(0 ,0, 1);

// ray computation vars
float3 computeLambert(float3 hit, float3 surfaceNormal, float3 light){
    float a = dot(normalize(light-hit), surfaceNormal);
    return float3(a,a,a);
}
float3 createLight(float x, float y, float z, float3 angle)
{
    return float3(x * sin(angle.x), y * cos(angle.y), z * cos(angle.z));
}

float sdBox( float3 p, float3 b )
{
  float3 q = abs(p) - b;
  return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}
float displacement(float3 p)
{
    float speed = 0.001;
    float t0 = Time*speed;
    float t1 = sin(t0);
    float t2 = 0.5*t1+0.5;
    float phase=1.1;
    float tho = length(p)*phase+t1;
    float thop = t0*20.;
    return  float3(tho * cos(tho - 1.25 * thop), tho * sin(tho - 1.15 * thop),tho * atan(tho - 1.05 * thop));

    float a = Time * 0.5;
    return atan(p.x * a) * sin(p.y * a) * atan(p.z * a);
}
float opDisplace(float3 p, float3 b)
{
    float d1 = sdBox(p,b);
    float d2 = displacement(p);
    return d1+d2;
}
float hashOld33( float3 p )
{
	p = float3( dot(p,float3(127.1,311.7, 74.7)),
			  dot(p,float3(269.5,183.3,246.1)),
			  dot(p,float3(113.5,271.9,124.6)));

	return frac(sin(p)*43758.5453123);
}
float3 createDisplacement(float3 p)
{
    float3 h =hashOld33(p); 
    p += Time;
    return float3(sin(p.x) * 0.1 * h.x,sin(p.y) * 0.1 * h.y, sin(p.z) * 0.1 * h.z);
}
float3 opLimitedRepetition( in float3 p, in float s, in float3 l)
{   
    float3 r = round(p/s);
    float3 c = clamp(r,-l,l);
    //int3 si = abs(sign(r));
    //float3 d = Offsets[si.x * s * si.y * s * s * si.z] * 0.1;
    float3 q = (p) - (s*c);
   // return opDisplace(q, 0.5);
    return sdBox(q, 0.5);
}

float distanceToNearestSurface(float3 ray)
{
    return opLimitedRepetition(ray,3, 1);
}
float3 computeSurfaceNormal(float3 hit, float s){
    float d = distanceToNearestSurface(hit);

    return normalize(float3(
        distanceToNearestSurface(hit+deltax * s)-d,
        distanceToNearestSurface(hit+deltay * s)-d,
        distanceToNearestSurface(hit+deltaz * s)-d
    ));
}
float staticScreen(float3 hit, float3 normal, float3 light)
{
    float a = max(computeLambert(hit, normal, light),0.3);
    float rand = hashOld33(hit * Time) * 0.8;
    float amount = 1 - step(0, -sign(normal.z));
    amount += amount * distance(a, normal);
     return lerp(a, rand, amount);
}
float3 intersectWithWorld(float3 cam, float3 dir)
{
    float dist = 0;
    float3 light = createLight(6,6,12,float3(Time * 20,Time * 20, 0));//float3(Time,Time, 0));
    float amount = 0;
    for(int i = 0; i < 20; i++)
    {
        float3 hit = cam+dir*dist;
        float nearest = distanceToNearestSurface(cam + dir*dist);
        if(nearest < 0.01)
        {
            float3 normal = computeSurfaceNormal(hit, 0.1);
            return staticScreen(hit, normal, light);
        }
        dist += nearest;
    }
    return (float3)amount;
}

DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{   
	const float fovx = PI * fov / 360.0;
    float fovy = fovx * Dimensions.y/Dimensions.x;
    float ulen = tan(fovx);
    float vlen = tan(fovy);    
    float2 camUV = uv*2.0 - float2(1.0, 1.0);
    float3 nright = normalize(cross(cameraUp, cameraDirection));
    float3 pixel = cameraPosition + cameraDirection + nright*camUV.x*ulen + cameraUp*camUV.y*vlen;


	float3 rayDirection = normalize(pixel - cameraPosition);

  	float3 curve = intersectWithWorld(cameraPosition, rayDirection);
    
  	return float4(curve, 1);
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