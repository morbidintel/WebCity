using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Extensions
{
	public static Vector3 GetPos(this MonoBehaviour g)
	{
		return g.transform.position;
	}

	public static void SetPos(this MonoBehaviour g, Vector3 newPos)
	{
		g.transform.position = newPos;
	}

	public static string Repeat(this string s, int count)
	{
		for (int i = 0; i < count; ++i) s += s;
		return s;
	}

	public static T Pop<T>(this LinkedList<T> list)
	{
		T t = list.Last();
		list.RemoveLast();
		return t;
	}

	// Bypasses all elements before a specified element in a sequence and then returns the remaining elements.
	public static LinkedList<T> SkipFrom<T>(this LinkedList<T> source, LinkedListNode<T> node)
	{
		if (node.Next == null) throw new ArgumentOutOfRangeException("Node is last element in list.");

		LinkedList<T> list = new LinkedList<T>();
		list.AddLast(node.Value);
		while (node.Next != null)
		{
			node = node.Next;
			list.AddLast(node.Value);
		}
		return list;
	}

	public static bool HasComponent<T>(this Component m) where T : Component
	{
		return m.GetComponent<T>() != null;
	}

	public static bool HasComponent<T>(this GameObject m)
	{
		return m.GetComponent<T>() != null;
	}

	public static RectTransform RectTransform(this MonoBehaviour m)
	{
		RectTransform rt = (RectTransform)m.transform;
		return rt;
	}

	public static RectTransform RectTransform(this GameObject m)
	{
		RectTransform rt = (RectTransform)m.transform;
		return rt;
	}

	public static int ActiveChildCount(this Transform t)
	{
		int count = 0;
		foreach (Transform tt in t) if (t.gameObject.activeSelf) count++;
		return count;
	}

	// Replaces the " (Clone)" in the name of an Instantiated GameObject to the count of their list
	// "Wall Object (Clone)" => "Wall Object 1";
	public static LinkedListNode<T> AddCountToName<T>(this LinkedListNode<T> node, int padding = 2) where T : MonoBehaviour
	{
		int lastIndex = node.Value.name.LastIndexOf("(Clone)");
		if (lastIndex > 0)
			node.Value.name = node.Value.name.Remove(lastIndex);
		else if (!Regex.IsMatch(node.Value.name, " " + "[0-9]".Repeat(padding - 1)))
			node.Value.name += " " + node.List.Count.ToString("D" + padding);
		return node;
	}

	public static void RenameItemsToCount<T>(this LinkedList<T> list, int padding = 2) where T : MonoBehaviour
	{
		if (list.Count == 0) return;
		LinkedListNode<T> node = list.First;
		for (int i = 0; i < list.Count; ++i, node = node.Next)
		{
			T t = node.Value;
			t.name = (Regex.IsMatch(t.name, " " + "[0-9]".Repeat(padding - 1)) ? t.name.Remove(t.name.Length - (padding + 1)) : t.name);
			t.name += " " + i.ToString("D" + padding);
		}
	}

	public static Toggle GetActiveToggle(this ToggleGroup group)
	{
		return group.ActiveToggles().FirstOrDefault();
	}

	public static Toggle GetOnToggle(this ToggleGroup group)
	{
		return group.ActiveToggles().SingleOrDefault(t => t.isOn);
	}

	private static System.Reflection.FieldInfo _toggleListMember;
	/// <summary>
	/// Gets the list of toggles. Do NOT add to the list, only read from it.
	/// </summary>
	/// <param name="grp"></param>
	/// <returns></returns>
	public static List<Toggle> GetAllToggles(this ToggleGroup grp)
	{
		if (_toggleListMember == null)
		{
			_toggleListMember = typeof(ToggleGroup).GetField("m_Toggles", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
			if (_toggleListMember == null)
				throw new System.Exception("UnityEngine.UI.ToggleGroup source code must have changed in latest version and is no longer compatible with this version of code.");
		}
		return _toggleListMember.GetValue(grp) as List<Toggle>;
	}

	public static float SameSignAs(this float a, float b) { return a * (b * a.Sign()).Sign(); }

	public static float Sign(this float a) { return a < 0 ? -1f : 1f; }
	public static int Sign(this int a) { return a < 0 ? -1 : 1; }
	public static float Abs(this float a) { return Mathf.Abs(a); }
	public static int Abs(this int a) { return Mathf.Abs(a); }

	public static void SetPosX(this Transform t, float x) { t.position = t.position.SetX(x); }
	public static void SetPosY(this Transform t, float y) { t.position = t.position.SetY(y); }
	public static void SetPosZ(this Transform t, float z) { t.position = t.position.SetZ(z); }
	public static void SetLocalPosX(this Transform t, float x) { t.localPosition = t.localPosition.SetX(x); }
	public static void SetLocalPosY(this Transform t, float y) { t.localPosition = t.localPosition.SetY(y); }
	public static void SetLocalPosZ(this Transform t, float z) { t.localPosition = t.localPosition.SetZ(z); }

	public static Vector3 SetX(this Vector3 v, float x) { v.x = x; return v; }
	public static Vector3 SetY(this Vector3 v, float y) { v.y = y; return v; }
	public static Vector3 SetZ(this Vector3 v, float z) { v.z = z; return v; }

	public static Vector2 SetX(this Vector2 v, float x) { v.x = x; return v; }
	public static Vector2 SetY(this Vector2 v, float y) { v.y = y; return v; }

	public static Vector2 Round(this Vector2 v, int decimals)
	{
		v.x = (float)Math.Round(v.x, decimals);
		v.y = (float)Math.Round(v.y, decimals);
		return v;
	}

	public static Vector3 Round(this Vector3 v, int decimals)
	{
		v.x = (float)Math.Round(v.x, decimals);
		v.y = (float)Math.Round(v.y, decimals);
		v.z = (float)Math.Round(v.z, decimals);
		return v;
	}

	public static float Sum(this Vector3 v) { return v.x + v.y + v.z; }
	public static float Average(this Vector3 v) { return v.Sum() / 3; }

	public static bool Approximately(this Vector3 a, Vector3 b)
	{
		return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y) && Mathf.Approximately(a.z, b.z);
	}

	public static bool Approximately(this Vector2 a, Vector2 b)
	{
		return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
	}

	public static Vector3 ClampMagnitude(this Vector3 v, float min, float max)
	{
		float length = v.magnitude;
		if (length < min) return v * (min / length);
		else if (length > max) return v * (max / length);
		else return v;
	}

	public static Vector2 Abs(this Vector2 v) { return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y)); }
	public static Vector3 Abs(this Vector3 v) { return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }

	public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
	{
		// angle in [0,180]
		float angle = Vector3.Angle(a,b);
		float sign = Mathf.Sign(Vector3.Dot(n,Vector3.Cross(a,b)));

		// angle in [-179,180]
		float signed_angle = angle * sign;

		// angle in [0,360] (not used but included here for completeness)
		//float angle360 =  (signed_angle + 180) % 360;

		return signed_angle;
	}

	//Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
	//Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the 
	//same plane, use ClosestPointsOnTwoLines() instead.
	public static bool LineLineIntersection(out Vector3 intersection, Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
	{
		Vector3 lineVec1 = a2 - a1, lineVec2 = b2 - b1;

		Vector3 lineVec3 = b1 - a1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

		//is coplanar, and not parrallel
		if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
		{
			float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
			intersection = a1 + (lineVec1 * s);
			return true;
		}
		else
		{
			intersection = Vector3.zero;
			return false;
		}
	}

	//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
	//to each other. This function finds those two points. If the lines are not parallel, the function 
	//outputs true, otherwise false.
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
	{

		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;

		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);

		float d = a*e - b*b;

		//lines are not parallel
		if (d != 0.0f)
		{
			float recpD = 1.0f / d;
			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);

			float s = (b*f - c*e) * recpD;
			float t = (a*f - c*b) * recpD;

			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;

			return true;
		}

		return false;
	}

	public static Vector3 CenterOfPolygon(List<Vector3> points)
	{
		Vector3 v = Vector3.zero;
		foreach (Vector3 p in points) v = v + p;
		return v / points.Count;
	}

	public static Vector2 CentroidOfPolygon(List<Vector2> points)
	{
		Vector2 v = Vector2.zero;
		float area = 0;

		for (int i = 0; i < points.Count; ++i)
		{
			float x1 = points[i].x, y1 = points[i].y,
				x2 = points[i < points.Count - 1 ? i + 1 : 0].x, y2 = points[i < points.Count - 1 ? i + 1 : 0].y;
			area += x1 * y2 - x2 * y1;
			v.x += (x1 + x2) * (x1 * y2 - x2 * y1);
			v.y += (y1 + y2) * (x1 * y2 - x2 * y1);
		}
		area *= .5f;
		v *= 1f / (6f * area);

		return v;
	}

	public enum BlendMode { Opaque, Cutout, Fade, Transparent }
	public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = -1;
				break;
			case BlendMode.Cutout:
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
				material.SetInt("_ZWrite", 1);
				material.EnableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 2450;
				break;
			case BlendMode.Fade:
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.EnableKeyword("_ALPHABLEND_ON");
				material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
			case BlendMode.Transparent:
				material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
				material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				material.SetInt("_ZWrite", 0);
				material.DisableKeyword("_ALPHATEST_ON");
				material.DisableKeyword("_ALPHABLEND_ON");
				material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				material.renderQueue = 3000;
				break;
		}
	}

	public static Color ColorFromHex(string hex)
	{
		Color color = new Color();

		try
		{
			hex = hex.Replace("#", "");

			color.r = Convert.ToUInt32(hex.Substring(0, 2), 16) / 255f;
			color.g = Convert.ToUInt32(hex.Substring(2, 2), 16) / 255f;
			color.b = Convert.ToUInt32(hex.Substring(4, 2), 16) / 255f;
			color.a = hex.Length > 6 ? Convert.ToUInt32(hex.Substring(6, 2), 16) / 255f : 1;
		}
		catch (Exception)
		{
			Debug.Log("Unable to parse hex into Color");
			color = Color.white;
		}

		return color;
	}

	// Returns intersection Rect between 2 Rects
	public static bool Intersects(this Rect r1, Rect r2, out Rect area)
	{
		area = new Rect();

		if (r2.Overlaps(r1))
		{
			float x1 = Mathf.Min(r1.xMax, r2.xMax);
			float x2 = Mathf.Max(r1.xMin, r2.xMin);
			float y1 = Mathf.Min(r1.yMax, r2.yMax);
			float y2 = Mathf.Max(r1.yMin, r2.yMin);
			area.x = Mathf.Min(x1, x2);
			area.y = Mathf.Min(y1, y2);
			area.width = Mathf.Max(0.0f, x1 - x2);
			area.height = Mathf.Max(0.0f, y1 - y2);

			return true;
		}

		return false;
	}
}

#if UNITY_EDITOR
public class EditorExtensions
{
	[MenuItem("Tools/Extension Methods/Sort Children By Name")]

	public static void SortChildrenByName()
	{
		foreach (GameObject obj in Selection.gameObjects)
		{
			List<Transform> children = new List<Transform>();
			for (int i = obj.transform.childCount - 1; i >= 0; i--)
			{
				Transform child = obj.transform.GetChild(i);
				children.Add(child);
				child.parent = null;
			}
			children.Sort((Transform t1, Transform t2) => { return t1.name.CompareTo(t2.name); });
			foreach (Transform child in children)
			{
				child.parent = obj.transform;
			}
		}
	}
}
#endif
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
	public override float GetPropertyHeight(SerializedProperty property,
											GUIContent label)
	{
		return EditorGUI.GetPropertyHeight(property, label, true);
	}

	public override void OnGUI(Rect position,
							   SerializedProperty property,
							   GUIContent label)
	{
		GUI.enabled = false;
		EditorGUI.PropertyField(position, property, label, true);
		GUI.enabled = true;
	}
}
#endif