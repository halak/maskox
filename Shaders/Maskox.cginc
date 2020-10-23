#ifndef HALAK_MASKOX_INCLUDED
#define HALAK_MASKOX_INCLUDED

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

inline float MaskoxSampleTexture2D(sampler2D samp, float2 uv)
{
#if MASKOX_USE_RED_CHANNEL
    return tex2D(samp, uv).r;
#else
    return tex2D(samp, uv).a;
#endif
}

float MaskoxGetContour(float2 texcoord, float offset)
{
    const float scale = _Maskox_MaskTex_ST.x;
    const float position = _Maskox_MaskTex_ST.z + offset;
    #if MASKOX_INVERT
    const float a = 1.0f - MaskoxSampleTexture2D(_Maskox_MaskTex, texcoord);
    #else
    const float a = MaskoxSampleTexture2D(_Maskox_MaskTex, texcoord);
    #endif
    return MaskoxSampleTexture2D(_Maskox_ContourTex, float2(((a - 1 + position) / scale) + position, 0.5f));
}

////////////////////////////////////////////////////////////////////////////////////////////////////

#ifdef UNITY_CUSTOM_TEXTURE_INCLUDED

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

#endif

////////////////////////////////////////////////////////////////////////////////////////////////////

float _Maskox_Threshold = 0;

fixed4 MaskoxPixel(fixed value) { return fixed4(value, value, value, 1); }
fixed4 MaskoxFragmentShaderU(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(uv.x); }
fixed4 MaskoxFragmentShaderThresholdedU(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(uv.x < _Maskox_Threshold ? 0 : 1); }
fixed4 MaskoxFragmentShaderGaussianCumulative(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(MaskoxGaussianCumulative((uv.x - 0.5f) * 6.0f)); }
fixed4 MaskoxFragmentShaderUVLength(float4 vertex : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target { return MaskoxPixel(length(uv)); }

#endif
