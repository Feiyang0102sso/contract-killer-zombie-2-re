Shader "Custom/UnlitWithoutFog" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  Fog { Mode Off }
  SetTexture [_MainTex] { combine texture }
 }
}
}