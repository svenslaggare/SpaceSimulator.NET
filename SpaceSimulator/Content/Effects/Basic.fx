//=============================================================================
// Basic.fx by Frank Luna (C) 2011 All Rights Reserved.
//
// Basic effect that currently supports transformations, lighting, and texturing.
//=============================================================================

#include "LightHelper.fx"

cbuffer cbPerFrame
{
	DirectionalLight gDirLights[3];
	float3 gEyePosW;
	float3 gPointLightSource;

	float  gFogStart;
	float  gFogRange;
	float4 gFogColor;

	float gBlurSizeX;
	float gBlurSizeY;
};

cbuffer cbPerObject
{
	float4x4 gWorld;
	float4x4 gWorldInvTranspose;
	float4x4 gWorldViewProj;
	float4x4 gTexTransform;
	Material gMaterial;
};

// Nonnumeric values cannot be added to a cbuffer.
Texture2D gDiffuseMap;

SamplerState samAnisotropic
{
	Filter = ANISOTROPIC;
	MaxAnisotropy = 4;

	AddressU = WRAP;
	AddressV = WRAP;
};

struct VertexIn
{
	float3 PosL    : POSITION;
	float3 NormalL : NORMAL;
	float2 Tex     : TEXCOORD;
};

struct VertexOut
{
	float4 PosH    : SV_POSITION;
	float3 PosW    : POSITION;
	float3 NormalW : NORMAL;
	float2 Tex     : TEXCOORD;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	// Transform to world space.
	vout.PosW = mul(float4(vin.PosL, 1.0f), gWorld).xyz;
	vout.NormalW = mul(vin.NormalL, (float3x3)gWorldInvTranspose);

	// Transform to homogeneous clip space.
	vout.PosH = mul(float4(vin.PosL, 1.0f), gWorldViewProj);

	// Output vertex attributes for interpolation across triangle.
	vout.Tex = mul(float4(vin.Tex, 0.0f, 1.0f), gTexTransform).xy;

	return vout;
}

float4 PS(VertexOut pin, uniform int gLightCount, uniform bool gUseTexture) : SV_Target
{
	//Interpolating normal can unnormalize it, so normalize it.
	pin.NormalW = normalize(pin.NormalW);
	float3 toEye = gEyePosW - pin.PosW;
	float distToEye = length(toEye);
	toEye /= distToEye;

	float4 texColor = float4(1, 1, 1, 1);
	if (gUseTexture)
	{
		texColor = gDiffuseMap.Sample(samAnisotropic, pin.Tex);
	}

	//Lighting
	float4 litColor = texColor;
	if (gLightCount > 0)
	{
		// Start with a sum of zero. 
		float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
		float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
		float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

		// Sum the light contribution from each light source.  
		[unroll]
		for (int i = 0; i < gLightCount; ++i)
		{
			float4 A, D, S;
			ComputeDirectionalLight(gMaterial, gDirLights[i], pin.NormalW, toEye, A, D, S);
			ambient += A;
			diffuse += D;
			spec += S;
		}

		// Modulate with late add.
		litColor = texColor * (ambient + diffuse) + spec;
	}
	else
    {
        litColor = gMaterial.Diffuse;
    }

	// Common to take alpha from diffuse material and texture.
	litColor.a = gMaterial.Diffuse.a * texColor.a;
	return litColor;
}

float4 PlanetPS(VertexOut pin, uniform bool gUseLighting, uniform bool gUseTexture) : SV_Target
{
	// Interpolating normal can unnormalize it, so normalize it.
	pin.NormalW = normalize(pin.NormalW);

    float4 texColor = float4(1, 1, 1, 1);
    if (gUseTexture)
    {
        texColor = gDiffuseMap.Sample(samAnisotropic, pin.Tex);
    }

	//Lighting
	float4 litColor = texColor;

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

		float4 A, D, S;
		ComputePointLight(gMaterial, pointLight, pin.PosW, pin.NormalW, toEye, A, D, S);
		ambient += A;
		diffuse += D;
		spec += S;
		litColor = texColor * (ambient + diffuse) + spec;
    }

	// Common to take alpha from diffuse material and texture.
	litColor.a = gMaterial.Diffuse.a * texColor.a;
	return litColor;
}

float4 SunPS_Horizontal(VertexOut pin, uniform bool gUseTexture) : SV_Target
{
	// Interpolating normal can unnormalize it, so normalize it.
	pin.NormalW = normalize(pin.NormalW);

	// The toEye vector is used in lighting.
	float3 toEye = gEyePosW - pin.PosW;

	// Cache the distance to the eye from this surface point.
	float distToEye = length(toEye);

	// Normalize.
	toEye /= distToEye;

	// Default to multiplicative identity.
	float4 texColor = float4(1, 1, 1, 1);
	if (gUseTexture)
	{
		// Sample texture.
		texColor = gDiffuseMap.Sample(samAnisotropic, pin.Tex);
	}

	float4 litColor = texColor;

	// Common to take alpha from diffuse material and texture.
	litColor.a = gMaterial.Diffuse.a * texColor.a;

	// Blur
	//float blurSizeX = gBlurSizeX;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x - 3.0*blurSizeX, pin.Tex.y)) * 0.09f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x - 2.0*blurSizeX, pin.Tex.y)) * 0.11f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x - blurSizeX, pin.Tex.y)) * 0.18f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, pin.Tex) * 0.24f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x + blurSizeX, pin.Tex.y)) * 0.18f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x + 2.0*blurSizeX, pin.Tex.y)) * 0.11f;
	//litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x + 3.0*blurSizeX, pin.Tex.y)) * 0.09f;

	return litColor;
}

float4 SunPS_Vertical(VertexOut pin, uniform bool gUseTexture) : SV_Target
{
	// Interpolating normal can unnormalize it, so normalize it.
	pin.NormalW = normalize(pin.NormalW);

	// The toEye vector is used in lighting.
	float3 toEye = gEyePosW - pin.PosW;

	// Cache the distance to the eye from this surface point.
	float distToEye = length(toEye);

	// Normalize.
	toEye /= distToEye;

	// Default to multiplicative identity.
	float4 texColor = float4(1, 1, 1, 1);
	if (gUseTexture)
	{
		// Sample texture.
		texColor = gDiffuseMap.Sample(samAnisotropic, pin.Tex);
	}

	float4 litColor = texColor;

	// Common to take alpha from diffuse material and texture.
	litColor.a = gMaterial.Diffuse.a * texColor.a;

	// Blur
	float blurSizeY = gBlurSizeY;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y - 3.0*blurSizeY)) * 0.09f;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y - 2.0*blurSizeY)) * 0.11f;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y - blurSizeY)) * 0.18f;
	litColor += gDiffuseMap.Sample(samAnisotropic, pin.Tex) * 0.24f;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y + blurSizeY)) * 0.18f;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y + 2.0*blurSizeY)) * 0.11f;
	litColor += gDiffuseMap.Sample(samAnisotropic, float2(pin.Tex.x, pin.Tex.y + 3.0*blurSizeY)) * 0.09f;

	return litColor;
}

technique11 Light0
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(0, false)));
	}
}

technique11 Light1
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(1, false)));
	}
}

technique11 Light2
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(2, false)));
	}
}

technique11 Light3
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(3, false)));
	}
}

technique11 Light0Tex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(0, true)));
	}
}

technique11 Light1Tex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(1, true)));
	}
}

technique11 Light2Tex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(2, true)));
	}
}

technique11 Light3Tex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS(3, true)));
	}
}

technique11 PlanetNoLightTex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PlanetPS(false, true)));
	}
}

technique11 PlanetLight
{
    pass P0
    {
        SetVertexShader(CompileShader(vs_5_0, VS()));
        SetGeometryShader(NULL);
        SetPixelShader(CompileShader(ps_5_0, PlanetPS(true, false)));
    }
}

technique11 PlanetTexLight
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PlanetPS(true, true)));
	}
}

technique11 SunTex
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, SunPS_Horizontal(true)));
	}
}
