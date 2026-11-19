Shader "CKZ2/Transparent by Multiply2x" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Geometry+1" "IGNOREPROJECTOR"="True" "RenderType"="Transparent" }
  ZWrite Off
  Cull Off
  Fog {
   Color (1,1,1,1)
  }
  Blend DstColor SrcColor
  ColorMask RGB
  Offset -1, -1
  SetTexture [_MainTex] { ConstantColor (1,1,1,0) combine texture * constant }
 }
}
}