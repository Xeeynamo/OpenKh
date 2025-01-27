shader_type canvas_item;
render_mode unshaded, blend_mix;

#include "KH2InterfaceCommon.gdshaderinc"

void vertex()
{
	vertColor = COLOR;
	minimaxi = CUSTOM0;
	uvAdj = CUSTOM1.rg;
}

void fragment()
{
	COLOR = Compute(vertColor, UV, TEXTURE);
}
