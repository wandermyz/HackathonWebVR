Shader "VRLineRenderer/MeshChain - Alpha Blended"
{
	Properties 
	{
		_Color("Color Tint", COLOR) = (1,1,1,1)
		_lineSettings ("Line Thickness Settings", VECTOR) = (0, 1, .5, 1)

		_lineRadius("Line Radius Scale, Min, Max", VECTOR) = (1, 0, 100)

		// Local space or world space data
		[HideInInspector] _WorldData("__worlddata", Float) = 0.0

		// Depth effects line width
		[HideInInspector] _LineDepthScale("__linedepthscale", Float) = 1.0
	}
	SubShader
	{
		Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
		LOD 100

		// We don't want the line segments and caps to draw overtop
		// one another as it breaks the continous segment illusion
		// To alpha blend with the background, we use a two-pass technique
		Pass
		{
			// In the first pass we write only to the alpha channel.
			// This lets us punch a hole in the background that our 
			// line color then shows through
			Blend One One
			BlendOp Min
			Cull Off
			Lighting Off
			ZWrite On
			ColorMask A
			Offset 0, -.1

			CGPROGRAM

				#pragma vertex vert
				#pragma fragment fragAlphaMask
				#pragma multi_compile LINE_PERSPECTIVE_WIDTH LINE_FIXED_WIDTH
				#pragma multi_compile LINE_MODEL_SPACE LINE_WORLD_SPACE

				#include "UnityCG.cginc"
				#include "MeshChain.cginc"

			ENDCG
		}
		Pass
		{
			// In this second pass, we write our line color only as much
			// as the alpha value we wrote before allows through.  To
			// prevent overlapping lines from adding too much color,
			// we set the alpha value to one after visiting a pixel.
			Blend OneMinusDstAlpha DstAlpha, One One
			BlendOp Add
			Cull Off
			Lighting Off
			ZWrite Off
			Offset 0, -.1

			CGPROGRAM

				#pragma vertex vert
				#pragma fragment fragColor
				#pragma multi_compile LINE_PERSPECTIVE_WIDTH LINE_FIXED_WIDTH
				#pragma multi_compile LINE_MODEL_SPACE LINE_WORLD_SPACE

				#include "UnityCG.cginc"
				#include "MeshChain.cginc"

			ENDCG
		}
	}
	FallBack "Diffuse"
	CustomEditor "MeshChainShaderGUI"
}
