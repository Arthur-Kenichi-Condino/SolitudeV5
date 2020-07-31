Shader"Custom/TerrainAtlasShader"{
Properties{
_Color("Color",Color)=(1,1,1,1) _MainTex("Albedo (RGB)",2D)="white"{} _Glossiness("Smoothness",Range(0,1))=0.5 _Metallic("Metallic",Range(0,1))=0.0
_TilesResolution("Atlas Tiles Resolution",float)=3 _Scale("Scale",float)=1 _Sharpness("Triplanar Blend Sharpness",float)=1
}
SubShader{Tags{"Queue"="AlphaTest" "RenderType"="Transparent" "IgnoreProjector"="True"}
LOD 200
Pass{
    ZWrite On
    ColorMask 0
    CGPROGRAM
    #pragma   vertex vert
    #pragma fragment frag
    #include "UnityCG.cginc"
    struct v2f{
        float4 pos:SV_POSITION;
    };
    v2f vert(appdata_base v){
        v2f o;
			o.pos=UnityObjectToClipPos(v.vertex);
     return o;
    }
    half4 frag(v2f i):COLOR{
		return half4(0,0,0,0); 
    }
    ENDCG  
}
ZWrite Off
Blend SrcAlpha OneMinusSrcAlpha
CGPROGRAM
//  Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows keepalpha addshadow finalcolor:applyFixedFog vertex:vert
//  Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0
//  Add fog and make it work
#pragma multi_compile_fog
fixed4 _Color;sampler2D _MainTex;half _Glossiness;half _Metallic;
float _TilesResolution;float _Scale;float _Sharpness;
struct Input {
    float3 worldPos:POSITION;
    float3 worldNormal:NORMAL;
    float4 color:COLOR;
    float2 uv_MainTex:TEXCOORD0;
};
//  Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
#pragma instancing_options assumeuniformscaling
UNITY_INSTANCING_BUFFER_START(Props)
//  Put more per-instance properties here
UNITY_INSTANCING_BUFFER_END  (Props)
Input vert(inout appdata_full v){
 Input o;
return o;}
void surf(Input input,inout SurfaceOutputStandard o){
//  Find our UVs for each axis based on world position of the fragment.
half2 xUV=input.worldPos.yz*_Scale;
half2 yUV=input.worldPos.xz*_Scale;
half2 zUV=input.worldPos.xy*_Scale;
//  The worldNormal is the world-space normal of the fragment
//  Get the absolute value of the world normal.
//  Put the blend weights to the power of BlendSharpness, the higher the value, 
// the sharper the transition between the planar maps will be.
half3 blendWeights=pow(abs(input.worldNormal),_Sharpness);
//  Divide our blend mask by the sum of it's components, this will make x + y + z = 1
blendWeights=blendWeights/(blendWeights.x+blendWeights.y+blendWeights.z);
//  Now do texture samples from our diffuse map with each of the 3 UV sets.
float offsetUVSize=(1/_TilesResolution);
fixed4 xAxis;
fixed4 yAxis;
fixed4 zAxis;
float2 offsetUV=input.uv_MainTex;
	   offsetUV=float2(clamp(offsetUV.x,0,1),clamp(offsetUV.y,0,1));
xAxis=input.color.r*tex2D(_MainTex,(frac(xUV)*offsetUVSize+offsetUV));
yAxis=input.color.r*tex2D(_MainTex,(frac(yUV)*offsetUVSize+offsetUV));
zAxis=input.color.r*tex2D(_MainTex,(frac(zUV)*offsetUVSize+offsetUV));
fixed4 xAxis1=0;
fixed4 yAxis1=0;
fixed4 zAxis1=0;
//  Finally, blend together all three samples based on the blend mask.
fixed4 c=(xAxis+xAxis1)*blendWeights.x+(yAxis+yAxis1)*blendWeights.y+(zAxis+zAxis1)*blendWeights.z;
//  Albedo comes from a texture tinted by color
o.Albedo=(c.rgb);float alpha=c.a;




o.Alpha=(alpha);
//  Metallic and smoothness come from slider variables
o.Metallic  =(_Metallic  );
o.Smoothness=(_Glossiness);
}
void applyFixedFog(Input input,SurfaceOutputStandard o,inout fixed4 color){
}
ENDCG
}
FallBack"Diffuse"}