#include "res://Assets/Shaders/KH2Common.gdshaderinc"

uniform sampler2D Sprite0 : hint_default_transparent, source_color;
uniform sampler2D Sprite1 : hint_default_transparent, source_color;
uniform sampler2D Sprite2 : hint_default_transparent, source_color;
uniform sampler2D Sprite3 : hint_default_transparent, source_color;

instance uniform bool UseSprite0 = false;
instance uniform bool UseSprite1 = false;
instance uniform bool UseSprite2 = false;
instance uniform bool UseSprite3 = false;

uniform vec4 TextureOriginalUV0 = vec4(0);
uniform vec4 TextureOriginalUV1 = vec4(0);
uniform vec4 TextureOriginalUV2 = vec4(0);
uniform vec4 TextureOriginalUV3 = vec4(0);

instance uniform vec2 TextureFrame0 = vec2(0);
instance uniform vec2 TextureFrame1 = vec2(0);
instance uniform vec2 TextureFrame2 = vec2(0);
instance uniform vec2 TextureFrame3 = vec2(0);

float lerp(float from, float to, float weight)
{
	return from + (to - from) * weight;
}
float inverselerp(float from, float to, float weight)
{
	return (weight - from) / (to - from);
}
float remap(float value, float inFrom, float inTo, float outFrom, float outTo)
{
	return lerp(outFrom, outTo, inverselerp(inFrom, inTo, value));
}

bool TextureAnimationOverlay(bool use, vec2 uv, vec4 TextureOriginalUV, vec2 TextureFrame, sampler2D Sprite, out vec4 sample)
{
	if (use && uv.x >= TextureOriginalUV.x && uv.x <= TextureOriginalUV.z && uv.y >= TextureOriginalUV.y && uv.y <= TextureOriginalUV.w)
	{
		vec2 newUv = vec2(
			remap(uv.x, TextureOriginalUV.x, TextureOriginalUV.z, 0, 1),
			remap(uv.y, TextureOriginalUV.y, TextureOriginalUV.w, 0, 1)
		);
		float newY = remap(newUv.y, 0, 1, TextureFrame.x, TextureFrame.y);
		sample = texture(Sprite, vec2(newUv.x, newY));
		return true;
	}
	else
	{
		return false;
	}
}