//
//   Project:           Shaders
//
//   Description:       Parametric tint pixel shader for Coding4Fun.
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

/// <description>Parametric tint pixel shader for Coding4Fun.</description>

/// <summary>The tint color.</summary>
/// <type>Color</type>
/// <minValue>0,0,0,0,</minValue>
/// <maxValue>1,1,1,1</maxValue>
/// <defaultValue>0.9,0.7,0.3,1</defaultValue>
float4 TintColor : register(C0);

// Sampler
sampler2D TextureSampler : register(S0);

// Shader
float4 main(float2 uv : TEXCOORD) : COLOR
{
   // Sample the original color at the coordinate
   float4 color = tex2D(TextureSampler, uv);

   // Convert the color to gray
   float gray = dot(color, float4(0.3, 0.59, 0.11, 0)); 
    
   // Create the gray color with the original alpha value
   float4 grayColor = float4(gray, gray, gray, color.a); 
    
   // Return the tinted pixel
   return grayColor * TintColor;
}
