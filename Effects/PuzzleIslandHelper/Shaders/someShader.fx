float4x4 World;
float4x4 View;
float4x4 Projection;

float4 AmbientColor = float4(0.2f, 0.2f, 0.2f, 1);

float4 SpecularColor = float4(1, 1, 1, 1);
float SpecularPower = 32.0f;

float3 EyePosition;

float3 LightPositions[3];
float4 LightColors[3];

texture textureSampler : register(s0);

sampler2D textureSamplerSampler = sampler_state
{
    Texture = <textureSampler>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Point;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;

    float3 WorldPos : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    float3 worldNormal = normalize(mul(float4(input.Normal, 0), World).xyz);

    output.Position = mul(mul(worldPos, View), Projection);

    output.WorldPos = worldPos.xyz;
    output.WorldNormal = worldNormal;

    output.TexCoord = input.TexCoord;
    output.Color = input.Color;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR0
{
    float3 worldNormal = normalize(input.WorldNormal);
    float3 worldPos = input.WorldPos;
    float3 viewDir = normalize(EyePosition - worldPos);

    float4 lighting = AmbientColor;

    const float constant = 1.0;
    const float linearr = 0.1;
    const float quadratic = 0.02;

    for (int i = 0; i < 3; i++)
    {
        float3 lightVec = LightPositions[i] - worldPos;
        float dist = length(lightVec);

        float3 lightDir = normalize(lightVec);

        float attenuation = 1.0 / (constant + linearr * dist + quadratic * dist * dist);
        attenuation = saturate(attenuation);

        float NdotL = saturate(dot(worldNormal, lightDir));

        float4 diffuse =
            LightColors[i] *
            LightColors[i].a *
            NdotL *
            attenuation;

        float3 halfVec = normalize(lightDir + viewDir);

        float NdotH = saturate(dot(worldNormal, halfVec));

        float specularAmount = pow(NdotH, SpecularPower);

        float4 specular =
            SpecularColor *
            specularAmount *
            LightColors[i].a *
            attenuation;

        lighting += diffuse + specular;
    }

    float4 tex = tex2D(textureSamplerSampler, input.TexCoord);

    float3 albedo = pow(tex.rgb, 2.2);

    float3 finalColor = albedo * input.Color.rgb * lighting.rgb;

    finalColor = pow(saturate(finalColor), 1.0 / 2.2);

    return float4(finalColor, tex.a);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}