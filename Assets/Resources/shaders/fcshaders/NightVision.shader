Shader "Night Vision" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "RenderType"="Opaque" }
 Pass {
  Tags { "RenderType"="Opaque" }
  SetTexture [_MainTex] { ConstantColor (0,1,0,1) combine texture * constant }
 }
}
SubShader { 
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Blend One OneMinusSrcColor
  SetTexture [_MainTex] { ConstantColor (0,1,0,1) combine texture * constant }
 }
}
}