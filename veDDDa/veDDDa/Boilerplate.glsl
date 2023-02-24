#version 330 core
#define sat(a) clamp(a, 0., 1.)

precision mediump float;
uniform float EyeSeparation;
uniform float EyePosition; // 1 or -1
uniform float time;
uniform vec2 resolution;
out vec4 fragColor;

__REPLACE__

void main()
{
	vec2 uv = (gl_FragCoord.xy-vec2(.5)*resolution.xy) / resolution.xx;
	vec3 col = rdr(uv+vec2(EyeSeparation*.5*EyePosition));

	float h = uv.y-sin(time+uv.x*10.)*.2;
float shape = abs(h)-.01;
col = col*sin(time)*.3+.7;
col = mix(col, vec3(0.), 1.-sat(shape*300.));
	fragColor = vec4(col, 1.);
}