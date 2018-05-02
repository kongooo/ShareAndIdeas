Shader "Custom/LowPoly" {
	Properties{
		_Color("Color", Color) = (1, 1, 1, 1)
		_SpecularStrength("SpecularStrength", Float) = 1.0
		_SpecularColor("SpecularColor", Color) = (0.1, 0.1, 1, 1)
	}

	SubShader {
		Tags{"RenderType" = "Opaque"}
		Pass{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag 
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			fixed4 _Color;
			fixed4 _SpecularColor;
			float4 _SpecularStrength;

			struct a2v
			{
				float4 position: POSITION;
				float3 normal: NORMAL;
			};

			struct v2f
			{
				float4 pos: SV_POSITION;
				float3 worldNormal: TEXCOORD0;
			};

			v2f vert(a2v v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.position);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i): SV_Target{
				fixed3 normal = normalize(i.worldNormal);
				fixed3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				fixed3 diffuse = _Color * _LightColor0 * saturate(dot(normal, lightDir));
				fixed3 spe = 1 - saturate(dot(normal, lightDir));
				fixed3 specular = _SpecularColor.rgb * pow(spe, _SpecularStrength);

				return fixed4(diffuse + specular, 1.0);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
