
precision highp float;

uniform vec2 resolution;
uniform float time;

#define sat(a) clamp(a, 0., 1.)
#define PI 3.14159265
#define TAU (PI*2.0)

float hash11(float seed)
{
    return fract(sin(seed*123.456789)*123.456);
}
float _seed;
float rand()
{
    _seed++;
    return hash11(_seed);
}

mat2 r2d(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

vec3 getCam(vec3 rd, vec2 uv)
{
    float fov =3.;
    vec3 r = normalize(cross(rd, vec3(0.,1.,0.)));
    vec3 u = normalize(cross(rd, r));
    return normalize(rd+fov*(r*uv.x+u*uv.y));
}

vec2 _min(vec2 a, vec2 b)
{
    if (a.x < b.x)
        return a;
    return b;
}

float _cube(vec3 p, vec3 s)
{
  vec3 l = abs(p)-s;
  return max(l.x, max(l.y, l.z));
}

vec2 map(vec3 p)
{
    vec2 acc = vec2(10000.,-1.);
    vec3 op = p;
    p.xy *= r2d(p.z*.1-time*.1);
    vec3 rep = vec3(7.5);
    vec3 id = floor((p+rep*.5)/rep);
    p = mod(p+rep*.5,rep)-rep*.5;
    p.xy *= r2d(time+length(id)+p.z*.5);
    p.xz *= r2d(time);
    float s = 1.+.1*(sin(length(id)+time)*.5+.5);
    float shape = _cube(p, vec3(s*1.2));
    shape = max(shape, -(length(p)-1.7));
    acc = _min(acc, vec2(shape, 0.));

    //acc = _min(acc, vec2(-p.y, 1.));
    return acc;
}

vec3 getNorm(vec3 p, float d)
{
    vec2 e = vec2(0.01, 0.);
    return normalize(vec3(d)-vec3(map(p-e.xyy).x, map(p-e.yxy).x, map(p-e.yyx).x));
}

vec3 accCol;
vec4 trace(vec3 ro, vec3 rd, int steps)
{
  accCol = vec3(0.);
    vec3 p = ro;
    for (int i = 0; i < 128; ++i)
    {
        vec2 res = map(p);
				if (distance(p, ro) > 50.)
					break;
        if (res.x < 0.01)
            return vec4(res.x, distance(p, ro), res.y, i);
        accCol = vec3(.8,.2,.1)*(1.-sat(res.x/.4))*.3;
        p+=rd*res.x*.5;
    }
    return vec4(-1.);
}


vec3 getMat(vec3 p, vec3 n, vec3 rd, vec4 res)
{
  float shape = abs(p.y)-2.;
    return mix(vec3(1.), vec3(1.,0.,0.), 1.-sat(shape*400.));
}


vec3 rdr(vec2 uv)
{
    vec3 col = vec3(0.);

    float d= 3.;
    float t = time*.3;
    vec3 ro = vec3(sin(t)*d,-3.,-1.);
    vec3 ta = vec3(0.,0.,0.);
    vec3 rd = normalize(ta-ro);
uv *= r2d(time*.3);
    rd = getCam(rd, uv);
    vec4 res = trace(ro, rd, 128);
    float depth = 100.;
    if (res.y > 0.)
    {
        depth = res.y;
        vec3 p = ro+rd*res.y;
        vec3 n = getNorm(p, res.x);
        col = n*.5+.5;
        col = getMat(p, n, rd, res);
        col *= pow(1.-sat(res.w/128.),2.);
    }
    col = mix(col, vec3(.1,.2,.4)*.3, 1.-exp(-depth*0.05));
    col += .5*vec3(1.,.2,.2)*(1.-sat(length(uv)));
    col += accCol;
    col *= 1.-sat(length(uv));
    float beat = 1./8.;
    col = mix(col, col.zxy, mod(time, beat)/beat);
    return col;
}


void main()
{
	vec2 uv = (gl_FragCoord.xy - .5*resolution.xy) / resolution.xx; // +.5 to normalize with deforped uv clamping
	vec2 baseuv = uv;
#ifdef IS_VEDDDA_3000
	// This gets proper UV for the 3D setup to work (corners offset), it does not handle eyePosition, it has to be handled in the shader
	uv = getDeformedUV(uv);
#else
	uv += .5;
#endif

	vec3 col = vec3(0.);

	if (max(abs(uv.x - 0.5), abs(uv.y - 0.5)) < 0.5)
	{
		col = rdr(uv -.5
#ifdef IS_VEDDDA_3000
      + vec2(EyeDistance*.5*EyePosition, 0.)
#endif
      );
	}

#ifdef IS_VEDDDA_3000 // This line draw a blue border around the screen to easily overlay the two images
	col = drawScreenLimits(baseuv, resolution, col);
#endif
	gl_FragColor = vec4(col, 1.);
}
