using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
#if UNITY_EDITOR
public class SaveAssetsThumbnail : EditorWindow
{
	[MenuItem("Assets/Save Thumbnail")]
	public static void Init()
	{
		List<string> saved = new List<string>(), failed = new List<string>();

		string savePath = Application.dataPath + "/Images/Saved Thumbnails/";
		if (!File.Exists(savePath)) Directory.CreateDirectory(savePath);

		foreach (Object o in Selection.objects)
		{
			Texture2D preview = null;
			for (int i = 0; i < 1000 && preview == null; ++i)
				preview = AssetPreview.GetAssetPreview(o);
			if (preview != null)
			{
				File.WriteAllBytes(savePath + o.name + ".png", preview.EncodeToPNG());
				saved.Add(o.name);
			}
			else
			{
				failed.Add(o.name);
			}
		}

		Debug.Log("Saved " + saved.Count + (saved.Count > 0 ? " : " + saved.Aggregate((w, n) => w + ", " + n) : "") +
			(failed.Count > 0 ? "\nFailed " + failed.Count + " : " + failed.Aggregate((w, n) => w + ", " + n) : ""));
	}
}
#endif