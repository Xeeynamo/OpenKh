shader_type spatial;
render_mode unshaded;

#include "res://Assets/Shaders/KH2TextureAnimationCommon.gdshaderinc"

void fragment()
{
	if (Scissor)
	{
		ALPHA_SCISSOR_THRESHOLD = 0.5;
	}
	vec4 sample;
	if (
		TextureAnimationOverlay(UseSprite0, UV, TextureOriginalUV0, TextureFrame0, Sprite0, sample) ||
		TextureAnimationOverlay(UseSprite1, UV, TextureOriginalUV1, TextureFrame1, Sprite1, sample) ||
		TextureAnimationOverlay(UseSprite2, UV, TextureOriginalUV2, TextureFrame2, Sprite2, sample) ||
		TextureAnimationOverlay(UseSprite3, UV, TextureOriginalUV3, TextureFrame3, Sprite3, sample)
		)
	{

	}
	else
	{
		sample = texture(Texture, UV);
	}
	ALBEDO = sample.rgb;
	if (Scissor || Alpha)
	{
		ALPHA = sample.a;
	}
}