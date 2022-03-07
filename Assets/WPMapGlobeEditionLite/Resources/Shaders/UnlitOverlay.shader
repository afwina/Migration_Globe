 
Shader "World Political Map/Unlit Overlay" {
 
Properties
    {
       _MainTex ("Texture", 2D) = ""
    }
 
SubShader
    {
        Tags {
         "Queue" = "Transparent" 
         "RenderType"="Transparent"
        }
        ZWrite Off
//        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            SetTexture[_MainTex] { Combine texture, texture * primary}
        }
    }
}