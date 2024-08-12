shader_type spatial;

render_mode unshaded, cull_disabled;

uniform sampler2D paper_tex : source_color,filter_nearest,repeat_enable;
uniform sampler2D sprite_tex : source_color,filter_nearest,repeat_enable;



bool hasContraryNeighbour(vec2 uv, sampler2D base, vec2 searchDistance, vec2 pixelSize) {
	//don't need this because it eliminates this case in fragment()
	//if(texture(base, uv).a > 0.001)return true;
	float startX = max(uv.x - searchDistance.x - 0.0001, 0);
	float endX = min(uv.x + searchDistance.x, 1);
	float startY = max(uv.y - searchDistance.y - 0.0001, 0);
	float endY = min(uv.y + searchDistance.y, 1);
	

	for(float sampleX = startX; sampleX < endX; sampleX += pixelSize.x)
	{
		for(float sampleY = startY; sampleY < endY; sampleY += pixelSize.y)
		{
			if(texture(base, vec2(sampleX, sampleY)).a > 0.0001)
				return true;
		}
	}
	
	
	//didn't find a nontransparent pixel
	return false;
}

void fragment() {
	vec2 base_uv = UV;
	vec4 sprite_tex_pixel = texture(sprite_tex,base_uv);
	vec4 paper_tex_pixel = texture(paper_tex, base_uv);
	
	if(sprite_tex_pixel.a > 0.001)
	{
		ALBEDO = sprite_tex_pixel.rgb * paper_tex_pixel.rgb;
		ALPHA *= sprite_tex_pixel.a;
	}
	else
	{
		ALBEDO = sprite_tex_pixel.rgb;
		ALPHA *= sprite_tex_pixel.a;
	}
	
}