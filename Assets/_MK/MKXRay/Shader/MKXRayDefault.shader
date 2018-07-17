Shader "MK/XRay" 
{
	Properties 
	{
		//Main
		_Color("_Color", Color) = (1,1,1,1)
		_MainTex("Base (RGBA)", 2D) = "white" {}

		//Normalmap
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Distortion("Distortion", Range(0,1)) = 0.30

		//XRay
		_XRayRimColor("Rim Color", Color) = (1,1,1,1)
		_XRayInside("Inside", Range(0,1) ) = 0.25
	    _XRayRimSize("Rim Size", Range(0,1) ) = 0.50

		//Noise
		[Toggle] _UseNoise ("Noise", int) = 0
		_NoiseAnimationSpeed("Noise Speed", Range(0,1) ) = 0.5

		//Dissolve 
		[Toggle] _UseDissolve ("Dissolve", int) = 0
	    _DissolveMap ("Dissolve (R)", 2D) = "white" {}
		_DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.5
		_DissolveAnimationDirection ("Dissolve Amount", Vector) = (0,0.5,0,0)

		//Emission
		_EmissionColor("Emission Color", Color) = (0,0,0)
		_EmissionMap("Emission (RGB)", 2D) = "white" {}

		//Editor
		[HideInInspector] _MKEditorShowMainBehavior ("Main Behavior", int) = 1
		[HideInInspector] _MKEditorShowXRayBehavior ("XRay Behavior", int) = 0
		[HideInInspector] _MKEditorShowNoiseBehavior ("Noise Behavior", int) = 0
		[HideInInspector] _MKEditorShowDissolveBehavior ("Dissolve Behavior", int) = 0
	}

	/////////////////////////////////////////////////////////////////////////////////////////////
	// SM Includes
	/////////////////////////////////////////////////////////////////////////////////////////////
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "Inc/MKXRayDef.cginc"

		//modified clip function for a complete discard when input equal zero oder smaller
		inline void Clip0(half c)
		{
			//if(any(c <= 0.0)) discard;
			clip(c);
		}

		#if MKXRAY_TC
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
		#endif

        uniform fixed4 _Color;
		uniform half4 _XRayRimColor;
		uniform half _XRayRimSize;
		uniform half _XRayInside;

		#if _MK_NOISE
			uniform half _NoiseAnimationSpeed;
		#endif

		#if _MK_NOISE
			//Add simple noise
			half Noise(float2 s)
			{
				return frac(dot(s.xy, float2(19.65, 59.63) + _Time.x * 0.01 * _NoiseAnimationSpeed) * 8314.22);
			}
		#endif

		#if _MKXRAY_DISSOLVE
			uniform fixed3 _DissolveColor;
			uniform sampler2D _DissolveMap;
			uniform half _DissolveAmount;
			uniform half4 _DissolveAnimationDirection;
		#endif
			
		//Emission
		#if _MKXRAY_EMISSION
			uniform fixed3 _EmissionColor;
			#if _MK_EMISSION_MAP
				uniform sampler2D _EmissionMap;
			#endif
		#endif

		/////////////////////////////////////////////////////////////////////////////////////////////
		// SM Input
		/////////////////////////////////////////////////////////////////////////////////////////////
		struct Input
		{
			float4 vertex : POSITION;
			#if MKXRAY_TC
				float2 texcoord : TEXCOORD0;
			#endif
			half3 normal : NORMAL;
			#if UNITY_VERSION >= 560
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
		};

		/////////////////////////////////////////////////////////////////////////////////////////////
		// SM Output
		/////////////////////////////////////////////////////////////////////////////////////////////
		struct Output
		{
			#if MKXRAY_TC
				float2 uv : TEXCOORD0;
			#endif
			float4 pos : SV_POSITION;
			half3 normal : TEXCOORD1;
			#if UNITY_VERSION >= 560
				UNITY_VERTEX_INPUT_INSTANCE_ID
			#endif
			UNITY_VERTEX_OUTPUT_STEREO
		};
			
		/////////////////////////////////////////////////////////////////////////////////////////////
		// Vertex
		/////////////////////////////////////////////////////////////////////////////////////////////
		Output vert (Input v)
		{
			#if UNITY_VERSION >= 560
				UNITY_SETUP_INSTANCE_ID(v);
			#endif
			Output o;
			UNITY_INITIALIZE_OUTPUT(Output, o);
			#if UNITY_VERSION >= 560
				UNITY_TRANSFER_INSTANCE_ID(v,o);
			#endif
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			o.pos = UnityObjectToClipPos (v.vertex);

			o.normal = v.normal;
			#if MKXRAY_TC
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
			#endif

			return o;
		}

		/////////////////////////////////////////////////////////////////////////////////////////////
		// Fragment
		/////////////////////////////////////////////////////////////////////////////////////////////
		fixed4 frag(Output o) : SV_TARGET
		{
			#if UNITY_VERSION >= 560
				UNITY_SETUP_INSTANCE_ID(o);
			#endif
			_Color.a = _XRayRimColor.a = 1.0;

			#if _MKXRAY_DISSOLVE
				half sg = tex2D (_DissolveMap, o.uv + float2(_DissolveAnimationDirection.xy) * _Time.x).r - _DissolveAmount;
				Clip0(sg);
			#endif

			fixed4 colorOut;
			float3 uv = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, o.normal));

			#if _MK_ALBEDO_MAP
				fixed4 t = tex2D(_MainTex, o.uv.xy);
			#else
				fixed4 t = 1.0;
			#endif

			#if _MK_NOISE
				_XRayInside = _XRayInside*0.5;
				half rF = 1.0;
			#else
				_XRayInside = _XRayInside*0.25;
				half rF = 0.5;
			#endif

			colorOut = lerp(fixed4(0,0,0,0), t * _Color, _XRayInside);
			colorOut.rgb = lerp(colorOut.rgb, t * _XRayRimColor, rF * saturate(1 - pow(uv.z, _XRayRimSize)));

			#if _MKXRAY_EMISSION
				#if _MK_EMISSION_DEFAULT
					colorOut.rgb += _EmissionColor;
				#elif _MK_EMISSION_MAP
					colorOut.rgb += _EmissionColor * tex2D(_EmissionMap, o.uv.xy).rgb;
				#endif
			#endif

			#if _MK_NOISE
				colorOut.rgb *= Noise(uv.z);
			#endif

			return colorOut;
		}
    ENDCG

	/////////////////////////////////////////////////////////////////////////////////////////////
	// SM 3.0
	/////////////////////////////////////////////////////////////////////////////////////////////
	SubShader 
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "LightMode" = "Always"}
		Cull Off
		ZWrite Off
		ZTest LEqual

		Pass
		{
			Name "XRAYMAIN"
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			
			ENDCG
		}
		Pass
		{
			Name "XRAYSEC"
			Blend One One

			CGPROGRAM
			#pragma target 3.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	/////////////////////////////////////////////////////////////////////////////////////////////
	// SM 2.5
	/////////////////////////////////////////////////////////////////////////////////////////////
	SubShader 
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "LightMode" = "Always"}
		Cull Off
		ZWrite Off
		ZTest LEqual 

		Pass
		{
			Name "XRAYMAIN"
			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma target 2.5
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			ENDCG
		}
		Pass
		{
			Name "XRAYSEC"
			Blend One One

			CGPROGRAM
			#pragma target 2.5
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	/////////////////////////////////////////////////////////////////////////////////////////////
	// SM 2.0
	/////////////////////////////////////////////////////////////////////////////////////////////
	SubShader 
	{
		Tags { "Queue" = "Transparent"  "RenderType" = "Transparent" "LightMode" = "Always"}
		Cull Off
		ZWrite Off
		ZTest LEqual

		Pass
		{
			Name "XRAYMAIN"
			Blend SrcAlpha OneMinusSrcAlpha 

			CGPROGRAM
			#pragma target 2.0
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			ENDCG
		}
		Pass
		{
			Name "XRAYSEC"
			Blend One One 

			CGPROGRAM
			#pragma target 2.0
			#pragma fragmentoption ARB_precision_hint_fastest

			#pragma shader_feature __ _MK_ALBEDO_MAP
			#pragma multi_compile __ _MK_NOISE
			#pragma shader_feature __ _MK_DISSOLVE_DEFAULT
			#pragma shader_feature __ _MK_EMISSION_DEFAULT _MK_EMISSION_MAP

			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			
			ENDCG
		}
	} 
	FallBack "Unlit/Transparent"
	CustomEditor "MK.XRay.MKXRayEditor"
}
