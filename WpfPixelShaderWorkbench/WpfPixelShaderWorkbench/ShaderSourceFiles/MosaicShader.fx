//
//   Project:           Shaders
//
//   Description:       Mosaic Shader for Coding4Fun.
//
//   Changed by:        $Author$
//   Changed on:        $Date$
//   Changed in:        $Revision$
//   Project:           $URL$
//   Id:                $Id$
//
//
//   Copyright (c) 2010 Rene Schulte
//

/// <description>Mosaic Shader for Coding4Fun.</description>

/// <summary>The number pixel blocks.</summary>
/// <type>Single</type>
/// <minValue>2</minValue>
/// <maxValue>500</maxValue>
/// <defaultValue>50</defaultValue>
float BlockCount : register(C0);

/// <summary>The rounding of a pixel block.</summary>
/// <type>Single</type>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.2</defaultValue>
float Min : register(C1);

/// <summary>The rounding of a pixel block.</summary>
/// <type>Single</type>
/// <minValue>0</minValue>
/// <maxValue>1</maxValue>
/// <defaultValue>0.45</defaultValue>
float Max : register(C2);

/// <summary>The aspect ratio of the image.</summary>
/// <type>Single</type>
/// <minValue>0</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>1</defaultValue>
float AspectRatio : register(C3);

// Sampler
sampler2D input : register(S0);

// Static computed vars for optimization
static float2 BlockCount2 = float2(BlockCount, BlockCount  / AspectRatio);
static float2 BlockSize2 = 1.0f / BlockCount2; 

// Shader
float4 main(float2 uv : TEXCOORD) : COLOR
{
	// Calculate block center
    float2 blockPos = floor(uv * BlockCount2);
	float2 blockCenter = blockPos * BlockSize2 + BlockSize2 * 0.5;
	
	// Scale coordinates back to original ratio for rounding
	float2 uvScaled = float2(uv.x * AspectRatio, uv.y);
	float2 blockCenterScaled = float2(blockCenter.x * AspectRatio, blockCenter.y);
		
	// Round the block by testing the distance of the pixel coordinate to the center
	float dist = length(uvScaled - blockCenterScaled) * BlockCount2;
	if(dist < Min || dist > Max)
	{
		return 0;
	}
	
	// Sample color at the calculated coordinate
	return tex2D(input, blockCenter);
}
