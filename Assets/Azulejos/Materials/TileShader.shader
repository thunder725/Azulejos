// Shader for using some tile image as a mask to show two different textures behind

Shader "KT/TileShader" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_BackTex ("Background (RGB)", 2D) = "white" {}
	_Mask ("Pattern Mask (R)", 2D) = "black" {}
	_Invert("Invert", Range(0,1)) = 0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
// Mobile improvement: noforwardadd
// http://answers.unity3d.com/questions/1200437/how-to-make-a-conditional-pragma-surface-noforward.html
// http://gamedev.stackexchange.com/questions/123669/unity-surface-shader-conditinally-noforwardadd
#pragma surface surf Lambert

// Declare the variables used here
sampler2D _MainTex;
sampler2D _BackTex;
sampler2D _Mask;
half _Invert;

// No idea why, but it works to just keep the same uv_TextureName system in here
struct Input {
	float2 uv_MainTex;
	float2 uv_BackTex;
	float2 uv_Mask;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 m = tex2D(_MainTex, IN.uv_MainTex);
	fixed4 b = tex2D(_BackTex, IN.uv_BackTex);
	fixed mask = tex2D(_Mask, IN.uv_Mask).r; // The mask is single-colour, so 
	if (_Invert < 0.5){
		o.Albedo = lerp(m.rgb, b.rgb, mask);
	}
	else {
		o.Albedo = lerp(b.rgb, m.rgb, mask);
	}
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
