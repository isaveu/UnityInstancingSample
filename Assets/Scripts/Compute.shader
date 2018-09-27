Shader "Custom/Compute" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma multi_compile_instancing
		#pragma instancing_options procedural:setup

		struct Input {
			float2 uv_MainTex;
		};

		#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			StructuredBuffer<float3> PositionBuffer;
			StructuredBuffer<float> AnimationBuffer;
			StructuredBuffer<int> VisibleBuffer;
		#endif

		void setup() {
			#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
				float3 verts = PositionBuffer[unity_InstanceID];
				float x = verts.x;
				float y = verts.y;
				float z = verts.z;
				unity_ObjectToWorld._14_24_34_44 = float4(x, y, z, 1);
			#endif
		}

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void vert(inout appdata_full v) {
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			float animationVal = AnimationBuffer[unity_InstanceID];
			float3 scale = float3(abs(animationVal), abs(animationVal), abs(animationVal));

			float4x4 scaleMat = float4x4(
				scale.x, 0, 0, 0,
				0, scale.y, 0, 0,
				0, 0, scale.z, 0,
				0, 0, 0, 1
			);

			v.vertex.y += animationVal;
			v.vertex.xyz = mul(v.vertex.xyz, scaleMat);
#endif
		}

		void surf(Input IN, inout SurfaceOutputStandard o) {
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
			if (VisibleBuffer[unity_InstanceID] == 0) {
				o.Albedo = float4(1.0,0.0,0.0,1.0);
			}
			else
			{
				o.Albedo = _Color;
			}
#endif
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}