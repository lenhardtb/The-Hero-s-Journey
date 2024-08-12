shader_type spatial;

render_mode unshaded, cull_disabled;

uniform sampler2D texture_albedo : source_color,filter_nearest,repeat_enable;
uniform sampler2D paper_tex : source_color,filter_nearest,repeat_enable;
uniform vec4 outlineColor: source_color;
uniform int outlineWidth : hint_range(0,10) = 1;


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
	vec4 albedo_tex = texture(texture_albedo,base_uv);
	vec4 paper_tex_pixel = texture(paper_tex, base_uv);
	
	
	ivec2 texSize = textureSize(texture_albedo, 0);
	vec2 pixelSize = vec2(1.0/float(texSize.x), 1.0/float(texSize.y));
	vec2 searchDistance = pixelSize * float(outlineWidth);
	
	
	
	if(albedo_tex.a > 0.001)
	{
		ALBEDO = albedo_tex.rgb * paper_tex_pixel.rgb;
		ALPHA *= albedo_tex.a;
	}
	else if(hasContraryNeighbour(base_uv, texture_albedo, searchDistance, pixelSize))
	{
		ALBEDO = outlineColor.rgb;
		ALPHA = outlineColor.a;
	}
	else
	{
		ALBEDO = albedo_tex.rgb * paper_tex_pixel.rgb;
		ALPHA *= albedo_tex.a;
	}
	
}

