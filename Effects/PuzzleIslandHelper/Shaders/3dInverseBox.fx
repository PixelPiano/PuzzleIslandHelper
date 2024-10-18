
#define DECLARE_TEXTURE(Name, index) \
    texture Name: register(t##index); \
    sampler Name##Sampler: register(s##index)

#define SAMPLE_TEXTURE(Name, texCoord) tex2D(Name##Sampler, texCoord)
#define PI 3.14
#define UVFIX true
uniform float Time; // level.TimeActive
uniform float2 CamPos; // level.Camera.Position
uniform float2 Dimensions; // new floattor2(320, 180)
uniform float4x4 TransformMatrix;
uniform float4x4 ViewMatrix;
uniform float Amplitude;
const float3 cameraPosition = float3(0, 0, 15.0);
const float3 cameraDirection = float3(0.0, 0.0, -1.0);
const float3 cameraUp = float3(0.0, 1.0, 0.0);
const float fov = 50.0;

const float3 deltax = float3(1 ,0, 0);
const float3 deltay = float3(0 ,1, 0);
const float3 deltaz = float3(0 ,0, 1);
#define M_PI 3.14159265358979323846

// ray computation vars
float3 computeLambert(float3 hit, float3 surfaceNormal, float3 light){
    float a = dot(normalize(light-hit), surfaceNormal);
    return float3(a,a,a);
}
float rand(float2 co){return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);}
float rand (float2 co, float l) {return rand(float2(rand(co), l));}
float rand (float2 co, float l, float t) {return rand(float2(rand(co, l), t));}
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
float3 createLight(float x, float y, float z, float3 angle)
{
    return float3(x * sin(angle.x), y * cos(angle.y), z * cos(angle.z));
}
float veMax(float3 v, float m)
{
    return min(max(v.x,max(v.y,v.z)),m);
}
float3 spRelativity(float3 p, float t)
{
    return pow(t / (t * (1 / (sqrt(1 - (p.x * p.x) / (p.y * p.y))))),p.z * t);
}
float3 whut(float3 p, float t)
{
    return step(0,tan(acos(p.x + Time) + asin(p.y + Time))) / p.z;
}
float eq1(float p, float t)
{
    return frac(sin(p * log(t / p)));
}
float bumps(float3 p, float t)
{
   return pow(sin(p.x + t) * cos(p.y + t) / atan(p.z + t),log10(t)) * 2.112553;
}
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

float3 eq(float3 p, float t)
{
    return 1;
}

float3 displacement(float3 p, float3 n, float h)
{
    float s = Time;
    float3 e = eq(p, Time);
    float3 result = float3
    (
        e.x * n.x / h,
        e.y * n.y / h,
        e.z * n.z / h
    );
    return result;
}
float3 sdPlane( float3 p, float3 n, float h )
{
    float3 d1 = dot(p,n) + h; //plane equation
    return d1;
    //float3 d2 = displacement(p, n, h);
    //return d1 + dot(d2, n); //each displacement should only affect the plane passed in
}
//=====================================================

float distanceToNearestSurface(float3 ray)
{
    float pl = sdPlane(ray, float3(-1.0,0.0, 1.0), 3); //left plane
    float pr = sdPlane(ray, float3(1.0, 0.0, 1.0), 3); //right plane
    float pt = sdPlane(ray, float3(0.0,-1.0, 1.0), 1.5); //top plane
    float pb = sdPlane(ray, float3(0.0, 1.0, 1.0), 1.5); //bottom plane
    float pf = sdPlane(ray, float3(0.0, 0.0, 1.0),-1);   //front plane
    return min(min(min(pl, pr), min(pt,pb)),pf); //return the smallest distance
}
float3 computeSurfaceNormal(float3 hit, float s){
    float d = distanceToNearestSurface(hit);

    return normalize(float3(
        distanceToNearestSurface(hit+deltax * s)-d,
        distanceToNearestSurface(hit+deltay * s)-d,
        distanceToNearestSurface(hit+deltaz * s)-d
    ));
}
float3 intersectWithWorld(float3 cam, float3 dir)
{
    float dist = 0;
    float3 light = createLight(24,24,12,float3(Time,Time, 0));//float3(Time,Time, 0));
    float amount = 0;
    for(int i = 0; i < 20; i++)
    {
        float3 hit = cam+dir*dist;
        float nearest = distanceToNearestSurface(hit);
        if(nearest < 0.01)
        {
            float3 normal = computeSurfaceNormal(hit, 0.1);
            return max(computeLambert(hit, normal, light),0.3);
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