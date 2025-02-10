shader_type spatial;
render_mode unshaded;

#include "res://Assets/Shaders/KH2Common.gdshaderinc"

void fragment()
{
	if (Scissor)
	{
		ALPHA_SCISSOR_THRESHOLD = 0.5;
	}
	vec4 sample = texture(Texture, UV);
	ALBEDO = sample.rgb;
	if (Scissor || Alpha)
	{
		ALPHA = sample.a;
	}
}