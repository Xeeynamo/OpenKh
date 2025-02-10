varying vec4 vertColor;
varying vec4 minimaxi;
varying vec2 uvAdj;

vec2 wrap(vec2 value, vec2 minimum, vec2 maximum)
{
	vec2 s = maximum - minimum;
    return minimum + mod(mod(value - minimum, s) + s, s);
}
vec4 Compute(vec4 color, vec2 uv, sampler2D tex)
{
	vec2 uvAdjusted = wrap((uv + (uvAdj * TIME)), minimaxi.xy, minimaxi.zw);
	vec2 finalUv = vec2(uvAdj.x != 0.0 ? uvAdjusted.x : uv.x, uvAdj.y != 0.0 ? uvAdjusted.y : uv.y);
    return texture(tex, finalUv) * color;
	//return texture(tex, uv) * color;
}
