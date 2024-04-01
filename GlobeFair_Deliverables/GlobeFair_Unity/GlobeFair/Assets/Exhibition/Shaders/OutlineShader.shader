Shader "Custom/OutlineShader" {
    Properties {
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.05
    }
    SubShader {
        Tags { "Queue"="Overlay" }
        Pass {
            Name "OUTLINE"
            Tags { "LightMode" = "Always" }
            Cull Front
            ZWrite Off
            Offset 1, 1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _OutlineColor;
            float _OutlineWidth;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                float3 inflatedPosition = v.vertex.xyz + v.normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(inflatedPosition);
                return o;
            }

            fixed4 frag () : SV_Target {
                return _OutlineColor;
            }
            ENDCG
        }
    }
}
