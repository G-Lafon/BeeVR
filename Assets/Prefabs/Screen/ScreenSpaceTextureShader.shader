Shader "Custom/ScreenSpaceTextureShader" {
    Properties {
        _MainTex ("Main texture (RGB)", 2D) = "white" {}
    }
    SubShader {
        Tags { "RenderType"="Cutout" }
        LOD 200
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

       sampler2D _MainTex;

      struct Input {
	   float2 uv_MainTex;
	   float4 screenPos;
      };

    void surf (Input IN, inout SurfaceOutputStandard o) {
    // Albedo comes from a texture tinted by color
    half2 screenUV = IN.screenPos.xy / IN.screenPos.w;
    fixed4 c = tex2D (_MainTex, screenUV);
    fixed4 sstc = tex2D(_MainTex, screenUV);
    o.Albedo = sstc.rgb;
    // Metallic and smoothness come from slider variables
    o.Alpha = c.a;
    }
        ENDCG
    }
    FallBack "Diffuse"
}

/**Code from http://www.battlemaze.com/?p=279 */