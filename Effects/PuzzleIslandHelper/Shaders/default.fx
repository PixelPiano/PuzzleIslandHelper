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
uniform float Speed = 0.035;
uniform float Amplitude;
uniform float2 BoxCenter;
uniform float2 MaxOffset;
uniform int MaxSize;
uniform int StartSize;
uniform float2 Offset;
float length(float2 pos)
{
    return sqrt(pos.x * pos.x + pos.y * pos.y);
}
bool inBox(float2 a,float2 pos, float2 dim)
{
    return a.x > pos.x && a.x < pos.x + dim.x && a.y > pos.y && a.y < pos.y + dim.y;
}
float2 worldToUv(float2 pos)
{
    return (pos - CamPos) / Dimensions;
}
float2 uvToWorld(float2 pos)
{
    return (pos * Dimensions) + CamPos;
}
bool xyEquals(float2 a, float2 b, float m)
{
    return a.x >= b.x - m/2 && a.x <= b.x + m/2 && a.y >= b.y - m/2 && a.y <= b.y + m/2;

}
DECLARE_TEXTURE(text, 0);
float4 SpritePixelShader(float2 uv : TEXCOORD0) : COLOR0
{
	float2 worldPos = uvToWorld(uv);
    float amplitude = Amplitude;
    float2 puv = worldToUv(BoxCenter);
    float2 maxSize = (float2)MaxSize;
    float2 startSize = (float2)StartSize;
    float2 boxSize = startSize + ((maxSize-startSize) * amplitude);
    boxSize.x = boxSize.x / Dimensions.x;
    boxSize.y = boxSize.y / Dimensions.y;
    float2 offset = worldToUv(Offset);

    float2 targetA = float2(0.5 - boxSize.x - offset.x, offset.y);
    float2 targetB = float2(0.5 + offset.x, offset.y);
    float2 basePos = (puv - boxSize/2);
    float2 boxPosA = basePos + (targetA - basePos) * amplitude;
    float2 boxPosB = basePos + (targetB - basePos) * amplitude;
        if(inBox(uv,boxPosA,boxSize))
        {
            float2 pos = (puv - boxPosA) + (uv - boxSize/2);
            
            return SAMPLE_TEXTURE(text, pos);
        }
        if(inBox(uv,boxPosB,boxSize))
        {
            float2 pos = (puv - boxPosB) + (uv - boxSize/2);
            
            return SAMPLE_TEXTURE(text, pos);
        }

    return float4(0,0,0,1);
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