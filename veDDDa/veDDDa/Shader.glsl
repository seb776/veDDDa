float hash11(float seed)
{
    return mod(sin(seed*123.456789)*123.456,1.);
}
float _seed;
float rand()
{
    _seed++;
    return hash11(_seed);
}
#define PI 3.14159265
#define TAU (PI*2.0)

mat2 r2d(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }


vec3 getCam(vec3 rd, vec2 uv)
{
    float fov = 1.;
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

    acc = _min(acc, vec2(length(p)-1., 0.));

    acc = _min(acc, vec2(_cube(p, vec3(.9+.1*sin(time))), 0.));

    return acc;
}

vec3 getNorm(vec3 p, float d)
{
    vec2 e = vec2(0.01, 0.);
    return normalize(vec3(d)-vec3(map(p-e.xyy).x, map(p-e.yxy).x, map(p-e.yyx).x));
}

vec3 trace(vec3 ro, vec3 rd, int steps)
{
    vec3 p = ro;
    for (int i = 0; i < steps && distance(p, ro) < 50.; ++i)
    {
        vec2 res = map(p);
        if (res.x < 0.01)
            return vec3(res.x, distance(p, ro), res.y);
        p+=rd*res.x*.5;
    }
    return vec3(-1.);
}

vec3 getMat(vec3 p, vec3 n, vec3 rd, vec3 res)
{
    return n*.5+.5;
}

vec3 rdr(vec2 uv)
{
    vec3 col = vec3(0.);

    vec3 ro = vec3(0.,0.,-5.);
    vec3 ta = vec3(0.,0.,0.);
    vec3 rd = normalize(ta-ro);

    rd = getCam(rd, uv);
    vec3 res = trace(ro, rd, 128);
    float depth = 100.;
    if (res.y > 0.)
    {
        depth = res.y;
        vec3 p = ro+rd*res.y;
        vec3 n = getNorm(p, res.x);
        col = n*.5+.5;
        col = getMat(p, n, rd, res);
    }
    col = mix(col, vec3(.5), 1.-exp(-depth*0.017));
    return col;
}
