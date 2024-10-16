float2 center : register(C0);
float inner_radius: register(C2);
float magnification : register(c3);
float outer_radius : register(c4);

SamplerState  Input : register(S0);

float4 main( float2 uv : TEXCOORD) : COLOR
{
    float2 center_to_pixel = uv - center; // vector from center to pixel  
	float distance = length(center_to_pixel);
	float4 color;
	float2 sample_point;
	
	if(distance < outer_radius)
    {
      if( distance < inner_radius )
      {
         sample_point = center + (center_to_pixel / magnification);
	  }
	  else
      {
	      float radius_diff = outer_radius - inner_radius;
	      float ratio = (distance - inner_radius ) / radius_diff; // 0 == inner radius, 1 == outer_radius
	      ratio = ratio * 3.14159; //  -pi/2 .. pi/2	      
	      float adjusted_ratio = cos( ratio );  // -1 .. 1
	      adjusted_ratio = adjusted_ratio + 1;   // 0 .. 2
	      adjusted_ratio = adjusted_ratio / 2;   // 0 .. 1
	   
	      sample_point = ( (center + (center_to_pixel / magnification) ) * (  adjusted_ratio)) + ( uv * ( 1 - adjusted_ratio) );
	  }
	}
	else
    {
	   sample_point = uv;
	}

	return tex2D( Input, sample_point );	
}

