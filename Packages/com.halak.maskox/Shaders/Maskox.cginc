
#ifndef HALAK_MASKOX_INCLUDED
#define HALAK_MASKOX_INCLUDED

#include "UnityCustomRenderTexture.cginc"

#define MASKOX_SQRT2 (1.41421356237f)

void MaskoxGetVertexAndTexcoord(uint vertexID, out float4 vertex, out float2 texcoord)
{
#if UNITY_UV_STARTS_AT_TOP
    const float2 vertexPositions[6] = { { -1.0f,  1.0f }, { -1.0f, -1.0f }, {  1.0f, -1.0f }, {  1.0f,  1.0f }, { -1.0f,  1.0f }, {  1.0f, -1.0f } };
    const float2 texCoords[6]       = { {  0.0f,  0.0f }, {  0.0f,  1.0f }, {  1.0f,  1.0f }, {  1.0f,  0.0f }, {  0.0f,  0.0f }, {  1.0f,  1.0f } };
#else
    const float2 vertexPositions[6] = { {  1.0f,  1.0f }, { -1.0f, -1.0f }, { -1.0f,  1.0f }, { -1.0f, -1.0f }, {  1.0f,  1.0f }, {  1.0f, -1.0f } };
    const float2 texCoords[6]       = { {  1.0f,  1.0f }, {  0.0f,  0.0f }, {  0.0f,  1.0f }, {  0.0f,  0.0f }, {  1.0f,  1.0f }, {  1.0f,  0.0f } };
#endif

    vertex = float4(vertexPositions[vertexID], 0.0, 1.0);
    texcoord = float2(texCoords[vertexID]);
}

float MaskoxError(float x)
{
    // https://en.wikipedia.org/wiki/Error_function#Approximation_with_elementary_functions
    const float a1 = 0.278393f;
    const float a2 = 0.230389f;
    const float a3 = 0.000972f;
    const float a4 = 0.078108f;

    float sign = x >= 0.0f ? +1.0f : -1.0f;

    x *= sign;

    float xx = x * x;
    float q = 1.0f + (a1 * x) + (a2 * xx) + (a3 * xx * x) + (a4 * xx * xx);
    return (1.0f - (1.0f / (q * q * q * q))) * sign;
}

float MaskoxGaussianCumulative(float x)
{
    return (1.0f + (MaskoxError(x / MASKOX_SQRT2))) / 2.0f;
}

sampler2D _Maskox_MaskTex;
sampler2D _Maskox_ContourTex;
float4 _Maskox_MaskTex_ST;

float MaskoxGetContour(float2 texcoord)
{
    const float ax = dot(float2(tex2D(_Maskox_MaskTex, texcoord).r, 1), _Maskox_MaskTex_ST.zw);
    return tex2D(_Maskox_ContourTex, float2(ax, 0.5f)).r;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

struct v2f_maskox
{
    float4 vertex   : SV_POSITION;
    float2 texcoord : TEXCOORD0;
};

v2f_maskox MaskoxVertexShaderDefault(appdata_customrendertexture IN)
{
    v2f_maskox OUT;
    MaskoxGetVertexAndTexcoord(IN.vertexID % 6, OUT.vertex, OUT.texcoord);
    return OUT;
}

float _Maskox_Angle = 0;

v2f_maskox MaskoxVertexShaderLinearGradient(appdata_customrendertexture IN)
{
    v2f_maskox OUT;
    float2 uv;
    float radian = radians(_Maskox_Angle);
    MaskoxGetVertexAndTexcoord(IN.vertexID % 6, OUT.vertex, uv);

    float scale = 1.0f / (abs(1 * cos(radian)) + abs(1 * sin(radian)));

    OUT.texcoord.x = ((uv.x - 0.5f) * scale * cos(radian)) + ((uv.y - 0.5f) * scale * sin(radian)) + 0.5f;
    OUT.texcoord.y = 0;

    return OUT;
}

float2 _Maskox_Center = float2(0.5, 0.5);

v2f_maskox MaskoxVertexShaderRadial(appdata_customrendertexture IN)
{
    v2f_maskox OUT;
    float2 uv;
    MaskoxGetVertexAndTexcoord(IN.vertexID % 6, OUT.vertex, uv);

    OUT.texcoord.x = (uv.x - _Maskox_Center.x) * MASKOX_SQRT2;
    OUT.texcoord.y = (uv.y - _Maskox_Center.y) * MASKOX_SQRT2;
    
    return OUT;
}

////////////////////////////////////////////////////////////////////////////////////////////////////

float _Maskox_Threshold = 0;

fixed4 MaskoxPixel(fixed value) { return fixed4(value, value, value, 1); }
fixed4 MaskoxFragmentShaderU(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(uv.x); }
fixed4 MaskoxFragmentShaderThresholdedU(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(uv.x < _Maskox_Threshold ? 0 : 1); }
fixed4 MaskoxFragmentShaderGaussianCumulative(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(MaskoxGaussianCumulative((uv.x - 0.5f) * 6.0f)); }
fixed4 MaskoxFragmentShaderUVLength(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(length(uv)); }

#endif