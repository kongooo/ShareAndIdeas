Shader "Custom/MaskShader" {
	SubShader {
		Tags{"Queue" = "Geometry+10"}
		ColorMask 0
		ZWrite On
		Pass {} 
	}
}
