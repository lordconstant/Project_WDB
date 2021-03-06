﻿Shader "Custom/ColourChangeable" {
	Properties {
		[PerRendererData]_DamagedLerp ("Damaged Lerp", Float) = 0.0
		[PerRendererData]_DisableLerp ("Disable Lerp", Float) = 0.0
		[PerRendererData]_SelectionLerp ("Selection Lerp", Float) = 0.0
		_Color ("Color", Color) = (1,1,1,1)
		_DamageColor ("Damage Color", Color) = (0,0,0,0)
		_DisableColor ("Disable Color", Color) = (0.4,0.4,0.4,1)
		_SelectionColor ("Selection Color", Color) = (0.0,0.5,0.0,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		float _DamagedLerp;
		float _DisableLerp;
		float _SelectionLerp;
		fixed4 _Color;
		fixed4 _DamageColor;
		fixed4 _DisableColor;
		fixed4 _SelectionColor;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_CBUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_CBUFFER_END

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 blendedC = _Color;
			blendedC = (blendedC * (1.0f - _DisableLerp)) + (_DisableColor * _DisableLerp);
			blendedC = (blendedC * (1.0f - _SelectionLerp)) + (_SelectionColor * _SelectionLerp);
			fixed4 c = ((tex2D (_MainTex, IN.uv_MainTex) * (1.0f - _DamagedLerp)) * (1.0f - _DisableLerp)) * blendedC;

			if(_DamagedLerp > 0.0f)
			{
				o.Emission = (_DamageColor * _DamagedLerp);
			}

			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
