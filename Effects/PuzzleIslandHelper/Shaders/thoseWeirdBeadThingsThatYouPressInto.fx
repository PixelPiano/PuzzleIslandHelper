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

float sdVerticalCapsule( float3 p, float h, float r )
{
  p.y -= clamp( p.y, 0.0, h );
  return length( p ) - r;
}
float vecMax(float3 v)
{
    return max(v.x,max(v.y,v.z));
}
float vecMin(float3 v)
{
    return min(v.x,min(v.y,v.z));
}
float sdBox(float3 p, float3 s)
{
    float3 q = abs(p) - s;
    return length(max(q,0.0)) + min(vecMax(q),0.0);
}
float repeated( float3 p, float space)
{
    p.x = p.x - round(p.x);
    p.y = p.y - round(p.y);
    return length(p) - space;
}
float repeatedBox(float3 p, float3 v,float3 s)
{
    p.x = p.x - round(p.x / s.x) * s.x;
    p.y = p.y - round(p.y / s.y) * s.y;
    //p.z = p.z - round(p.z / l) * l;
    float3 q = abs(p) - v;
    return length(max(q,0.0)) + min(vecMax(q),0.0);
}
float grid(float3 p, float3 t, float3 n, float3 s)
{
    float x = repeatedBox(p, float3(t.x, n.y, t.z),s);
    float y = repeatedBox(p, float3(n.x, t.y, t.z),s);
    float z = repeatedBox(p, float3(t.z, t.y, n.z),s);
    float m = min(x,y);
    return min(m,z);
}
float grid(float3 p, float3 t, float3 e, float3 s, float b, float eS)
{   
    float x = repeatedBox(p, float3(t.x, b, t.z),s);
    float y = repeatedBox(p, float3(b, t.y, t.z),s);
    float r = length(e - p);
    float th = 2;
    float z = 0;
    float m = min(x,y);
    float dist = distance(x, y);
    if(p.z > 0 && r < th && dist < t.x && dist < t.y)
    {
        z = repeatedBox(p, float3(t.x, t.y, ((r / th)) * eS),s);
        return min(m, z);
    }
    return m;
}
float distanceToNearestSurface(float3 ray)
{
    float angle = Time;
    float3 e = float3(sin(angle) * 4, cos(angle) * 2, 0);
    float3 size = float3(0.5, 0.5, 4);
    return grid(ray, 0.05, e, size, 15., 3);
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
    for(int i = 0; i < 20; i++)
    {
        float nearest = distanceToNearestSurface(cam + dir*dist);
        if(nearest < 0.01)
        {
            float3 hit = cam+dir*dist;
            float3 light = createLight(50,50,25,0);//float3(Time,Time, 0));
            return max(computeLambert(hit, computeSurfaceNormal(hit, 0.1), light),0.01);
        }
        dist += nearest;
    }
    return (float3)0;
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