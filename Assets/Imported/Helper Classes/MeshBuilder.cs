using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Gamelogic.Extensions;

public class MeshBuilder
{
	static public Mesh BuildMesh(Vector3[] points, bool useXYinstead = false)
	{
		Mesh mesh = new Mesh();

		BuildMesh(ref mesh, points, useXYinstead);

		return mesh;
	}

    public static List<Vector3> RemoveDuplicateAdjacent(Vector3 [] points)
    {
        List<Vector3> filteredpts = new List<Vector3>();
        for (int i = 0; i < points.Length; ++i)
        {
            if (filteredpts.Count == 0)
            {
                filteredpts.Add(points[i]);
            }
            else
            {
                if (!points[i].Approximately(filteredpts.Last()))
                {
                    filteredpts.Add(points[i]);
                }
            }
        }
        if (filteredpts.Count > 1 && filteredpts[0].Approximately(filteredpts[filteredpts.Count - 1]))
        {
            filteredpts.RemoveAt(filteredpts.Count - 1);
        }
        return filteredpts;
    }

	static public void BuildMesh(ref Mesh mesh, Vector3[] points, bool useXYinstead=false)
	{
        //remove duplicate points
        var filteredpts = RemoveDuplicateAdjacent(points);
        var ptsarr = filteredpts.ToArray();


        mesh.Clear();
		mesh.vertices = null;
		mesh.uv = null;
		mesh.triangles = null;

		mesh.vertices = ptsarr;
		mesh.uv = BuildUVs(ptsarr);
        if (useXYinstead)
        {
            mesh.triangles = Triangulate(ptsarr.Select(v => v.To2DXY()).ToList());
        }
        else
        {
            mesh.triangles = Triangulate(ptsarr.Select(v => v.To2DXZ()).ToList());
        }

		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
	}

	static Vector2[] BuildUVs(Vector3[] vertices)
	{
		float xMin, yMin, xMax, yMax;
		xMin = yMin = Mathf.Infinity;
		xMax = yMax = -Mathf.Infinity;

		foreach (Vector3 v3 in vertices)
		{
			if (v3.x < xMin) xMin = v3.x;
			if (v3.y < yMin) yMin = v3.y;
			if (v3.x > xMax) xMax = v3.x;
			if (v3.y > yMax) yMax = v3.y;
		}

		float xRange = xMax - xMin;
		float yRange = yMax - yMin;

		Vector2[] uvs = new Vector2[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			uvs[i].x = (vertices[i].x - xMin) / xRange;
			uvs[i].y = (vertices[i].y - yMin) / yRange;
		}

		return uvs;
	}

	static public int[] Triangulate(List<Vector2> points)
	{
		List<int> indices = new List<int>();

		int n = points.Count;
		if (n < 3)
			return indices.ToArray();

		List<int> V = new List<int>();
		if (Area(points) > 0)
		{
			for (int v = 0; v < n; v++)
				V.Add(v);
		}
		else
		{
			for (int v = 0; v < n; v++)
				V.Add((n - 1) - v);
		}

		int nv = n;
		int Length = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2;)
		{
			if ((Length--) <= 0)
				return indices.ToArray();

			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;

			if (Snip(points, u, v, w, nv, V))
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					V[s] = V[t];
				nv--;
				Length = 2 * nv;
			}
		}

		indices.Reverse();
		return indices.ToArray();
	}

	static public float Area(List<Vector2> points)
	{
		int n = points.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++)
		{
			Vector2 pval = points[p];
			Vector2 qval = points[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}

	static public bool Snip(List<Vector2> points, int u, int v, int w, int n, List<int> V)
	{
		int p;
		Vector2 A = points[V[u]];
		Vector2 B = points[V[v]];
		Vector2 C = points[V[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++)
		{
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = points[V[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}

	static public bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp > 0.0f) && (bCROSScp > 0.0f) && (cCROSSap > 0.0f));
	}
}