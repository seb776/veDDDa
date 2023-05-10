// This on is intended for debug purpose

vec3 rdr(vec2 uv)
{
    vec3 col = vec3(0.);

	float shape = length(uv) - .25;
	col = mix(col, vec3(1.), 1. - clamp(shape*500., 0., 1.));
    return col;
}


void main()
{
#ifdef IS_VEDDDA_3000
	// This gets proper UV for the 3D setup to work (corners offset), it does not handle eyePosition, it has to be handled in the shader
	vec2 uv = getDeformedUV(); 
#else
	vec2 uv = (gl_FragCoord.xy - .5*resolution.xy) / resolution.xx;
#endif


	vec3 col = vec3(0.);

	if (max(abs(uv.x - 0.5), abs(uv.y - 0.5)) < 0.5)
	{
		col = rdr(uv - .5 + vec2(EyeDistance*.5*EyePosition, 0.));
		//col = rdr(deformed_uv - .5 + vec2(EyeDistance*.5*EyePosition, 0.));
	}

#ifdef IS_VEDDDA_3000 // This line draw a blue border around the screen to easily overlay the two images
	col = drawScreenLimits(uv, col);
#endif
	fragColor = vec4(col, 1.);
}