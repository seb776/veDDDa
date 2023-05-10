#version 100
precision highp float;

#define IS_VEDDDA_3000

uniform float TopLeftX;
uniform float TopLeftY;
uniform float TopRightX;
uniform float TopRightY;
uniform float BottomLeftX;
uniform float BottomLeftY;
uniform float BottomRightX;
uniform float BottomRightY;
 // TODO prefix these

// Define the four corners of the desired deformation
vec2 corners1_0;// = vec2(TopRightX, -TopRightY);
vec2 corners1_1;// = vec2(TopLeftX, -TopLeftY);
vec2 corners1_2;// = vec2(BottomLeftX, -BottomLeftY);
vec2 corners1_3;// = vec2(BottomRightX, -BottomRightY);

void setupCorners()
{
	corners1_0 = vec2(TopRightX, -TopRightY);
	corners1_1 = vec2(TopLeftX, -TopLeftY);
	corners1_2 = vec2(BottomLeftX, -BottomLeftY);
	corners1_3 = vec2(BottomRightX, -BottomRightY);
}

// Thanks IQ :)
// distance to a line segment
float sdSegment( in vec2 p, in vec2 a, in vec2 b )
{
    p -= a; b -= a;
	return length( p-b*clamp(dot(p,b)/dot(b,b),0.0,1.0) );
}
float _sqrrr(vec2 uv, vec2 sz)
{
	vec2 l = abs(uv) - sz;
	return max(l.x, l.y);
}
#define sat(a) clamp(a, 0., 1.)
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
vec3 drawScreenLimits(vec2 uv_, vec2 res_, vec3 col)
{
	vec2 a = corners1_0;
	vec2 b = corners1_1;
	vec2 c = corners1_2;
	vec2 d = corners1_3;
	// quad borders
	float h = .3 / res_.y;
	col = mix(col, vec3(1.0, 0.7, 0.2), 1.0 - smoothstep(h, 2.0*h, sdSegment(uv_, a, b)));
	col = mix(col, vec3(1.0, 0.7, 0.2), 1.0 - smoothstep(h, 2.0*h, sdSegment(uv_, b, c)));
	col = mix(col, vec3(1.0, 0.7, 0.2), 1.0 - smoothstep(h, 2.0*h, sdSegment(uv_, c, d)));
	col = mix(col, vec3(1.0, 0.7, 0.2), 1.0 - smoothstep(h, 2.0*h, sdSegment(uv_, d, a)));

	//uv += vec2(EyeDistance*.5*EyePosition, 0.);
	//vec3 col = rdr(uv);
	float screenLim = _sqrrr(uv_, vec2(.5)*res_.xy / res_.xx);
	screenLim = abs(screenLim) - .005;
	col = mix(col, vec3(0., 0., 1.), 1. - sat(screenLim*res_.x));
	return col;
}

vec2 getDeformedUV(vec2 uv_)
{
	setupCorners();
	// Compute the projective transformation matrix
	vec2 deformed_uv = invBilinear(uv_, corners1_0, corners1_1, corners1_2, corners1_3);
	return deformed_uv;
}



uniform float EyeDistance;
uniform float EyePosition; // 1 or -1

// Veda variables // Have to be defined in shader so...
//uniform float time;
//uniform vec2 resolution;

// veDDDa specific
//out vec4 fragColor;


// TODO handle gl_FragCoord & gl_FragColor