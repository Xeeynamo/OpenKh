shader_type spatial;
render_mode unshaded, blend_sub;

#include "res://Assets/Shaders/KH2WorldCommon.gdshaderinc"

void fragment()
{
	vec4 sample = Run(UV);
	ALBEDO = sample.rgb * COLOR.rgb;
	ALPHA = sample.a * COLOR.a;
}
