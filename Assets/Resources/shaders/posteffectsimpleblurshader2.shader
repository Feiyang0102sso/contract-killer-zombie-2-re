Shader "PostEffectSimpleBlurShader2" {
Properties {
 _MainTex ("", any) = "" {}
 _Treshold ("Treshold", Float) = 0.2
}
SubShader { 
 Pass {
  ZTest Always
  ZWrite Off
  Cull Off
  Fog { Mode Off }
  SetTexture [_MainTex] { ConstantColor (0,0,0,0.25) combine texture * constant alpha }
  SetTexture [_MainTex] { ConstantColor (0,0,0,0.25) combine texture * constant + previous }
  SetTexture [_MainTex] { ConstantColor (0,0,0,0.25) combine texture * constant + previous }
  SetTexture [_MainTex] { ConstantColor (0,0,0,0.25) combine texture * constant + previous }
 }
}
Fallback Off
}