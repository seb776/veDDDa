#version 330 core
#define sat(a) clamp(a, 0., 1.)

precision mediump float;
uniform float TopLeftX;
uniform float TopLeftY;
uniform float TopRightX;
uniform float TopRightY;
uniform float BottomLeftX;
uniform float BottomLeftY;
uniform float BottomRightX;
uniform float BottomRightY;

uniform float EyeDistance;
uniform float EyePosition; // 1 or -1
uniform float time;
uniform vec2 resolution;
out vec4 fragColor;

// Thanks IQ :)
float cross2d( in vec2 a, in vec2 b ) { return a.x*b.y - a.y*b.x; }
vec2 invBilinear( in vec2 p, in vec2 a, in vec2 b, in vec2 c, in vec2 d )
{
    vec2 res = vec2(-1.0);

    vec2 e = b-a;
    vec2 f = d-a;
    vec2 g = a-b+c-d;
    vec2 h = p-a;
        
    float k2 = cross2d( g, f );
    float k1 = cross2d( e, f ) + cross2d( h, g );
    float k0 = cross2d( h, e );
    
    // if edges are parallel, this is a linear equation
    if( abs(k2)<0.001 )
    {
        res = vec2( (h.x*k1+f.x*k0)/(e.x*k1-g.x*k0), -k0/k1 );
    }
    // otherwise, it's a quadratic
	else
    {
        float w = k1*k1 - 4.0*k0*k2;
        if( w<0.0 ) return vec2(-1.0);
        w = sqrt( w );

        float ik2 = 0.5/k2;
        float v = (-k1 - w)*ik2;
        float u = (h.x - f.x*v)/(e.x + g.x*v);
        
        if( u<0.0 || u>1.0 || v<0.0 || v>1.0 )
        {
           v = (-k1 + w)*ik2;
           u = (h.x - f.x*v)/(e.x + g.x*v);
        }
        res = vec2( u, v );
    }
    
    return res;
}
// Thanks IQ :)
// distance to a line segment
float sdSegment( in vec2 p, in vec2 a, in vec2 b )
{
    p -= a; b -= a;
	return length( p-b*clamp(dot(p,b)/dot(b,b),0.0,1.0) );
}

__REPLACE__

float _sqrrr(vec2 uv, vec2 sz)
{
	vec2 l = abs(uv) - sz;
	return max(l.x, l.y);
}
void main()
{
	vec2 uv = (gl_FragCoord.xy-.5*resolution.xy) / resolution.xx;
// Define the four corners of the desired deformation
vec2 corners1[4] = vec2[4](
    vec2(TopRightX, TopRightY),
    vec2(TopLeftX, TopLeftY),
    vec2(BottomLeftX, BottomLeftY),
    vec2(BottomRightX, BottomRightY)
);

// Compute the projective transformation matrix
vec2 deformed_uv = invBilinear(uv, corners1[0], corners1[1], corners1[2], corners1[3]);

    
	vec3 col = vec3(0.);//rdr(deformed_uv);

    if( max( abs(deformed_uv.x-0.5), abs(deformed_uv.y-0.5))<0.5 )
    {
        col = rdr(deformed_uv-.5+ vec2(EyeDistance*.5*EyePosition, 0.));
    }
    vec2 a = corners1[0];
    vec2 b = corners1[1];
    vec2 c = corners1[2];
    vec2 d = corners1[3];
    // quad borders
    float h = .3/resolution.y;
    col = mix( col, vec3(1.0,0.7,0.2), 1.0-smoothstep(h,2.0*h,sdSegment(uv,a,b)));
    col = mix( col, vec3(1.0,0.7,0.2), 1.0-smoothstep(h,2.0*h,sdSegment(uv,b,c)));
    col = mix( col, vec3(1.0,0.7,0.2), 1.0-smoothstep(h,2.0*h,sdSegment(uv,c,d)));
    col = mix( col, vec3(1.0,0.7,0.2), 1.0-smoothstep(h,2.0*h,sdSegment(uv,d,a)));

	//uv += vec2(EyeDistance*.5*EyePosition, 0.);
	//vec3 col = rdr(uv);
	float screenLim = _sqrrr(uv, vec2(.5)*resolution.xy / resolution.xx);
	screenLim = abs(screenLim) - .005;
	col = mix(col, vec3(0., 0., 1.), 1. - sat(screenLim*resolution.x));
	fragColor = vec4(col, 1.);
}