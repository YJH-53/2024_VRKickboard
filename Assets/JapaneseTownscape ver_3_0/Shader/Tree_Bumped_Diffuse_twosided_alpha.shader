// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Tree_Bumped_Diffuse_twosided_alpha" {
	Properties{
		_Cutoff("Cutoff", Range(0,1)) = 0.5
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
	}
	SubShader{
		Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
		LOD 200

		// óºñ 
		Cull Off
		CGPROGRAM
		#pragma surface surf Lambert alphatest:_Cutoff addshadow fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG

		// îwñ ÇæÇØâeÇÃçƒï`âÊ
		Cull Front
		CGPROGRAM

		#pragma surface surf Lambert alphatest:_Cutoff fullforwardshadows vertex:vert
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		// ñ@ê¸ÇîΩì]Ç≥ÇπÇƒó†ñ ÇÃâeÇÃï`é Ç™Ç´ÇøÇÒÇ∆çsÇÌÇÍÇÈÇÊÇ§Ç…Ç∑ÇÈ
		void vert(inout appdata_full v) {
			v.normal.xyz = v.normal * -1;
		}

		fixed4 _Color;
		
		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
		}
		ENDCG
	}
	FallBack "Diffuse"
}
