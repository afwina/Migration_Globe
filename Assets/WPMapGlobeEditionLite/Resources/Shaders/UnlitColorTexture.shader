Shader "World Political Map/Unlit Color Texture" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white"
    }
    Category {
       ZWrite On
       Cull Back
       Tags {
       	"Queue"="Geometry"
       }
       SubShader {
            Material {
               Emission [_Color]
            }
            Pass {
               SetTexture [_MainTex] {
                      Combine Texture * Primary, Texture * Primary
                }
            }
        } 
    }
}