sampler2D implicitInput : register(s0);

/// <summary>Input size</summary>
/// <type>Size</type>
/// <minValue>10,10</minValue>
/// <maxValue>100,100</maxValue>
/// <defaultValue>#01000000</defaultValue>
float4 dxdyShift : register(c1);

/// <summary>Input size</summary>
/// <type>double</type>
/// <minValue>0</minValue>
/// <maxValue>10</maxValue>
/// <defaultValue>3</defaultValue>
float Detail : register(c2);

/// <summary>Input size</summary>
/// <type>double</type>
/// <minValue>1</minValue>
/// <maxValue>2</maxValue>
/// <defaultValue>2</defaultValue>
float edgeColorAmount : register(c3);

static const float BlurWeights[13] =
{
	0.002216,
	0.008764,
	0.026995,
	0.064759,
	0.120985,
	0.176033,
	0.199471,
	0.176033,
	0.120985,
	0.064759,
	0.026995,
	0.008764,
	0.002216,
};

float4 main(float2 uv : TEXCOORD) : COLOR
{
	//color to return
	float4 outcolor = { 1.0f, 0.0f, 0.0f, 1.0f };

	//pixel step vectors
	float2 xdisp = dxdyShift.xy;
	float2 ydisp = dxdyShift.zw;

	//grab surrounding px colors
	float4 tl = tex2D(implicitInput, uv - xdisp - ydisp);
	float4 l = tex2D(implicitInput, uv - xdisp);
	float4 bl = tex2D(implicitInput, uv - xdisp + ydisp);
	float4 t = tex2D(implicitInput, uv - xdisp);
	float4 b = tex2D(implicitInput, uv + ydisp);
	float4 tr = tex2D(implicitInput, uv + xdisp - ydisp);
	float4 r = tex2D(implicitInput, uv + xdisp);
	float4 br = tex2D(implicitInput, uv + xdisp + ydisp);

	//go to grayscale
	float3 grayscale = float3(0.3, 0.59, 0.11);
	tl = dot(tl.rgb, grayscale);
	l = dot(l.rgb, grayscale);
	bl = dot(bl.rgb, grayscale);
	t = dot(t.rgb, grayscale);
	b = dot(b.rgb, grayscale);
	tr = dot(tr.rgb, grayscale);
	r = dot(r.rgb, grayscale);
	br = dot(br.rgb, grayscale);

	// compute dx with Sobel operator
	// -1 0 1
	// -2 0 2
	// -1 0 1
	float dX = -tl.a - 2.0f*l.a - bl.a + tr.a + 2.0f*r.a + br.a;

	// compute dy with Sobel operator
	// -1 -2 -1
	// 0   0  0
	// 1   2  1
	float dY = -tl.a - 2.0f*t.a - tr.a + bl.a + 2.0f*b.a + br.a;

	//compute the magnitude
	float grad = abs(dX) + abs(dY);

	grad = saturate(grad*0.2);

	// compute horizontal gaussian blur
	float4 color = 0;

		for (int i = 0; i < 13; i++) {
			color += tex2D(implicitInput, uv.xy + (i - 6) * dxdyShift.xy) * BlurWeights[i];
		}

	float4 pixelColor = tex2D(implicitInput, uv);
		outcolor = (grad*Detail*(1 - edgeColorAmount) + grad*Detail*pixelColor*edgeColorAmount + color);

	outcolor.a = pixelColor.a;

	return outcolor;
}

