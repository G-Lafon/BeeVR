Shader "Custom/AntiCylinder"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Radius("Radius", Range(0, 1)) = 0
        _Scale("Scale", Range(0, 1))= 1
        _Offset("Offset", Range(-1, 1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        float _Radius;
        float _Offset;
        float _Scale;
        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };


        fixed4 _Color;


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            IN.uv_MainTex.x=_Scale*_Radius*(1-cos((IN.uv_MainTex.x+_Offset )/_Radius));
          //IN.uv_MainTex.x=_Scale*(_Radius*acos((_Radius- IN.uv_MainTex.x)/_Radius));


            // Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
            o.Alpha = c.a;


        }
        ENDCG
    }
    FallBack "Diffuse"
}
