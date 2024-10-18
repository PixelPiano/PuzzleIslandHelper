
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
float4 mod289(float4 x)
{
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 permute(float4 x)
{
  return mod289(((x*34.0)+10.0)*x);
}

float4 taylorInvSqrt(float4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

float2 fade(float2 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float cnoise(float2 P)
{
  float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
  float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
  Pi = mod289(Pi); // To avoid truncation effects in permutation
  float4 ix = Pi.xzxz;
  float4 iy = Pi.yyww;
  float4 fx = Pf.xzxz;
  float4 fy = Pf.yyww;

  float4 i = permute(permute(ix) + iy);

  float4 gx = frac(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
  float4 gy = abs(gx) - 0.5 ;
  float4 tx = floor(gx + 0.5);
  gx = gx - tx;

  float2 g00 = float2(gx.x,gy.x);
  float2 g10 = float2(gx.y,gy.y);
  float2 g01 = float2(gx.z,gy.z);
  float2 g11 = float2(gx.w,gy.w);

  float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
  g00 *= norm.x;  
  g01 *= norm.y;  
  g10 *= norm.z;  
  g11 *= norm.w;  

  float n00 = dot(g00, float2(fx.x, fy.x));
  float n10 = dot(g10, float2(fx.y, fy.y));
  float n01 = dot(g01, float2(fx.z, fy.z));
  float n11 = dot(g11, float2(fx.w, fy.w));

  float2 fade_xy = fade(Pf.xy);
  float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
  float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
  return 2.3 * n_xy;
}

// Classic Perlin noise, periodic variant
float pnoise(float2 P, float2 rep)
{
  float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
  float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
  Pi = Pi % rep.xyxy; // To create noise with explicit period 
  Pi = mod289(Pi);        // To avoid truncation effects in permutation
  float4 ix = Pi.xzxz;
  float4 iy = Pi.yyww;
  float4 fx = Pf.xzxz;
  float4 fy = Pf.yyww;

  float4 i = permute(permute(ix) + iy);

  float4 gx = frac(i * (1.0 / 41.0)) * 2.0 - 1.0 ;
  float4 gy = abs(gx) - 0.5 ;
  float4 tx = floor(gx + 0.5);
  gx = gx - tx;

  float2 g00 = float2(gx.x,gy.x);
  float2 g10 = float2(gx.y,gy.y);
  float2 g01 = float2(gx.z,gy.z);
  float2 g11 = float2(gx.w,gy.w);

  float4 norm = taylorInvSqrt(float4(dot(g00, g00), dot(g01, g01), dot(g10, g10), dot(g11, g11)));
  g00 *= norm.x;  
  g01 *= norm.y;  
  g10 *= norm.z;  
  g11 *= norm.w;  

  float n00 = dot(g00, float2(fx.x, fy.x));
  float n10 = dot(g10, float2(fx.y, fy.y));
  float n01 = dot(g01, float2(fx.z, fy.z));
  float n11 = dot(g11, float2(fx.w, fy.w));

  float2 fade_xy = fade(Pf.xy);
  float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
  float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
  return 2.3 * n_xy;
}
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
float3 eq(float3 p, float t)
{
    //uv = noised(uv*1.5 + iTime).rg;
    //p = float3(noise(p.x+t*0.1), noise(p.y+10.), noise(p.z + 1));
    //uv += iTime;
    float d = p.x - p.y - p.z;

    d = sin(d + t) * cos(d + t);
    d = d * 0.5 + 0.5;
    d = 1.0 - d;
    d = smoothstep(-1., 1., (d-.5));
    
    return float3(lerp(float3(0.1,0.1,0.1), float3(0.2, 0.2, 0.6), d));
    //col = float3(noised(uv*10.)*0.5+0.5);
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
    //return d1;
    float3 d2 = displacement(p, n, h);
    return d1 + dot(d2, n); //each displacement should only affect the plane passed in
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

float2 hash( float2 n)
{
    return frac(sin(float2(n.x,n.y+1.0))*float(13.5453123));
}
DECLARE_TEXTURE(text, 0);

float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{  
    //uv = noised(uv*1.5 + iTime).rg;
    uv.x -=1;
    uv.y -=1.5;
    uv *= (Dimensions.x/Dimensions.y);
    uv = float2(pnoise(uv + Time * 0.1,  uv.y + hash(uv.y)),pnoise(uv, 10));
    //uv += iTime;
    float d = uv.x - uv.y;
    d *= 30.;
    d = sin(d);
    d = d * 0.5 + 0.5;
    d = 1.0 - d;
    
    d = smoothstep(0.09, 0.1, d);
    
    float3 col = float3(lerp(float3(0.1,0.1,0.1), float3(0.2, 0.2, 0.6), d));
    //col = float3(noised(uv*10.)*0.5+0.5);

    return float4(col,1.0);
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