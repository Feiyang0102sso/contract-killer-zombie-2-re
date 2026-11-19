Shader "Custom/TextureTintColor" {
Properties {
 _Color ("Tint", Color) = (1,1,1,1)
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  Color [_Color]
  SetTexture [_MainTex] { combine texture * primary double }
 }
}
}