//=============================================================================
// Basic.fx by Frank Luna (C) 2011 All Rights Reserved.
//
// Basic effect that currently supports transformations, lighting, and texturing.
//=============================================================================

#include "LightHelper.fx"

cbuffer cbPerFrame
{
    float3 gEyePosW;
    float3 gPointLightSource;
};

cbuffer cbPerObject
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float gLineWidth;
    Material gMaterial;
};

struct VertexIn
{
	float3 PosL      : POSITION0;
	float3 NextL     : POSITION1;
	float3 PrevL     : POSITION2;
	float3 NormalL   : NORMAL;
	float4 Color     : COLOR0;
	float4 NextColor : COLOR1;
};

struct VertexOut
{
	float3 PosW       : POSITION0;
	float3 NextW      : POSITION1;
	float3 PrevW      : POSITION2;
	float3 NormalW    : NORMAL;
	float4 Color      : COLOR0;
	float4 NextColor  : COLOR1;
};

struct GeoOut
{
	float4 PosH    : SV_POSITION;
	float3 PosW    : POSITION;
	float3 NormalW : NORMAL;
	float4 Color   : COLOR;
	uint   PrimID  : SV_PrimitiveID;
};

VertexOut VS(VertexIn vin)
{
	// Just pass data over to geometry shader.
	VertexOut vout;
	vout.PosW = vin.PosL;
	vout.NextW = vin.NextL;
	vout.PrevW = vin.PrevL;
	vout.NormalW = vin.NormalL;
	vout.Color = vin.Color;
	vout.NextColor = vin.NextColor;
	return vout;
}

[maxvertexcount(4)]
void GS(point VertexOut gin[1],
	    uint primID : SV_PrimitiveID,
	    inout TriangleStream<GeoOut> triangleStream)
{
	float3 position = gin[0].PosW;
	float3 next = gin[0].NextW;
	float3 prev = gin[0].PrevW;

	float3 forward = normalize(next - position);
	float3 right = normalize(cross(forward, gin[0].NormalW));

	float3 forwardPrev = normalize(position - prev);
	float3 rightPrev = normalize(cross(forwardPrev, gin[0].NormalW));

	float4 vertices[4];
	vertices[0] = float4(position - gLineWidth * 0.5 * rightPrev, 1.0f);
	vertices[1] = float4(position + gLineWidth * 0.5 * rightPrev, 1.0f);
	vertices[2] = float4(next - gLineWidth * 0.5 * right, 1.0f);
	vertices[3] = float4(next + gLineWidth * 0.5 * right, 1.0f);

	//Transform quad vertices to world space and output them as a triangle strip.
	GeoOut gout;
	[unroll]
	for (int i = 0; i < 2; ++i)
	{
		gout.PosH = mul(vertices[i], gWorldViewProj);
		gout.PosW = mul(vertices[i], gWorld).xyz;
		gout.NormalW = mul(gin[0].NormalW, (float3x3)gWorldInvTranspose);
		gout.Color = gin[0].Color;
		gout.PrimID = primID;
		triangleStream.Append(gout);
	}

	[unroll]
	for (i = 2; i < 4; ++i)
	{
		gout.PosH = mul(vertices[i], gWorldViewProj);
		gout.PosW = mul(vertices[i], gWorld).xyz;
		gout.NormalW = mul(gin[0].NormalW, (float3x3)gWorldInvTranspose);
		gout.Color = gin[0].NextColor;
		gout.PrimID = primID;
		triangleStream.Append(gout);
	}
}

float4 PS(GeoOut pin, uniform bool gUseLighting) : SV_Target
{
	float4 color = pin.Color;
    pin.NormalW = normalize(pin.NormalW);

    if (gUseLighting)
    {
        float3 toEye = gEyePosW - pin.PosW;
        float distToEye = length(toEye);
        toEye /= distToEye;

        float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
        float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
        float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

        PointLight pointLight;
        pointLight.Ambient = float4(0.3f, 0.3f, 0.3f, 1.0f);
        pointLight.Diffuse = float4(0.7f, 0.7f, 0.7f, 1.0f);
        pointLight.Specular = float4(0.0f, 0.0f, 0.0f, 0.0f);
        pointLight.Position = gPointLightSource;
        //pointLight.Range = 1000;
        pointLight.Att = float3(1.0f, 0.0f, 0.0f);

        Material material;
        material.Ambient = float4(0.5f, 0.5f, 0.5f, 1.0f);
        material.Diffuse = float4(1.0f, 1.0f, 1.0f, 1.0f);
        material.Specular = float4(0.6f, 0.6f, 0.6f, 16.0f);
        material.Reflect = float4(0, 0, 0, 0);
        
        float4 A, D, S;
        ComputePointLight(material, pointLight, pin.PosW, pin.NormalW, toEye, A, D, S);
        ambient += A;
        diffuse += D;
        spec += S;
        color = pin.Color * (ambient + diffuse) + spec;
        color.a = gMaterial.Diffuse.a * pin.Color.a;
    }
    else
    {
	    color.a = 0.75;
    }

	return color;
}

technique11 Orbit
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(CompileShader(gs_5_0, GS()));
		SetPixelShader(CompileShader(ps_5_0, PS(false)));
	}
}

technique11 PlanetRing
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VS()));
        SetGeometryShader(CompileShader(gs_5_0, GS()));
        SetPixelShader(CompileShader(ps_5_0, PS(false)));
    }
}