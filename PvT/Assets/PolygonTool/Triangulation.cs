/*
 * Triangulation code translated from C++ to C# by David Clark
 * Original code by John W. Ratcliff
 * http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
 */

using UnityEngine;
using System.Collections.Generic;

namespace PolygonTool
{
    public class Triangulate
    {
	    static float EPSILON = 0.0000000001f;

	    static public float Area(ref List<Vector2> contour)
	    {
		    int n = contour.Count;

		    float a = 0.0f;

		    for (int p = n - 1, q = 0; q < n; p = q++)
		    {
			    a += contour[p].x * contour[q].y - contour[q].x * contour[p].y;
		    }
		    return a * 0.5f;
	    }

	    /*
	      InsideTriangle decides if a point P is Inside of the triangle
	      defined by A, B, C.
	    */
	    static public bool InsideTriangle(float Ax, float Ay,
						      float Bx, float By,
						      float Cx, float Cy,
						      float Px, float Py)
	    {
		    float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		    float cCROSSap, bCROSScp, aCROSSbp;

		    ax = Cx - Bx; ay = Cy - By;
		    bx = Ax - Cx; by = Ay - Cy;
		    cx = Bx - Ax; cy = By - Ay;
		    apx = Px - Ax; apy = Py - Ay;
		    bpx = Px - Bx; bpy = Py - By;
		    cpx = Px - Cx; cpy = Py - Cy;

		    aCROSSbp = ax * bpy - ay * bpx;
		    cCROSSap = cx * apy - cy * apx;
		    bCROSScp = bx * cpy - by * cpx;

		    return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	    }

	    static public bool Snip(ref List<Vector2> contour, int u, int v, int w, int n, ref List<int> V)
	    {
		    int p;
		    float Ax, Ay, Bx, By, Cx, Cy, Px, Py;

		    Ax = contour[V[u]].x;
		    Ay = contour[V[u]].y;

		    Bx = contour[V[v]].x;
		    By = contour[V[v]].y;

		    Cx = contour[V[w]].x;
		    Cy = contour[V[w]].y;

		    if (EPSILON > (((Bx - Ax) * (Cy - Ay)) - ((By - Ay) * (Cx - Ax)))) return false;

		    for (p = 0; p < n; p++)
		    {
			    if ((p == u) || (p == v) || (p == w)) continue;
			    Px = contour[V[p]].x;
			    Py = contour[V[p]].y;
			    if (InsideTriangle(Ax, Ay, Bx, By, Cx, Cy, Px, Py)) return false;
		    }

		    return true;
	    }

	    static public bool Process(ref List<Vector2> contour, ref List<int> result, out bool counterClockwise)
	    {
		    /* allocate and initialize list of Vertices in polygon */
		    counterClockwise = false;

		    int n = contour.Count;
		    if (n < 3) return false;

		    List<int> V = new List<int>();

		    /* we want a counter-clockwise polygon in V */

		    if (0.0f < Area(ref contour)){
			    counterClockwise = true;
			    for (int v = 0; v < n; v++)
				    V.Add(v);
		    }
		    else
			    for (int v = 0; v < n; v++)
				    V.Add(n - 1 - v);

		    int nv = n;

		    /*  remove nv-2 Vertices, creating 1 triangle every time */
		    int count = 2 * nv;   /* error detection */

		    for (int m = 0, v = nv - 1; nv > 2; )
		    {
			    /* if we loop, it is probably a non-simple polygon */
			    if (0 >= (count--))
			    {
				    //** Triangulate: ERROR - probable bad polygon!
				    return false;
			    }

			    /* three consecutive vertices in current polygon, <u,v,w> */
			    int u = v; if (nv <= u) u = 0;     /* previous */
			    v = u + 1; if (nv <= v) v = 0;     /* new v    */
			    int w = v + 1; if (nv <= w) w = 0;     /* next     */

			    if (Snip(ref contour, u, v, w, nv, ref V))
			    {
				    int a, b, c, s, t;

				    /* true names of the vertices */
				    a = V[u]; b = V[v]; c = V[w];

				    /* output Triangle */
				    result.Add(c);
				    result.Add(b);
				    result.Add(a);
				
				    m++;

				    /* remove v from remaining polygon */
				    for (s = v, t = v + 1; t < nv; s++, t++) V[s] = V[t]; nv--;

				    /* resest error detection counter */
				    count = 2 * nv;
			    }
		    }

		    return true;
	    }

    }
}
