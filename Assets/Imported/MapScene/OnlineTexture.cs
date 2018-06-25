using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public abstract class OnlineTexture : MonoBehaviour
{
	public bool loadFromFile = false;
	protected Texture2D texture;

	public bool textureLoaded = false;
	protected WWW request_;

	public static int totalrequests = 0; //total tiles requested from server
	public static int totalInMemory = 0; //tiles currently loaded with texture
	const int tilesInMemoryLimit = 256; //when tiles in memory reaches this limit, we need to start purging some tiles

	public void Start()
	{
		// When in edit mode, start downloading a texture preview.
		if (!Application.isPlaying)
		{
			RequestTexturePreview();
		}
	}


	public void RequestTexture(string nodeID)
	{
		textureLoaded = false;
		string url = GenerateRequestURL (nodeID);
		++totalrequests;
		++totalInMemory;
		if (loadFromFile && File.Exists(url))
		{
			texture = new Texture2D(2, 2);
			texture.LoadImage(File.ReadAllBytes(url));
		}
		else
		{
			request_ = new WWW(url);
		}
	}


	public void RequestTexturePreview()
	{
		RequestTexture("0");
	}

#if UNITY_EDITOR
	// Make this update with editor.
	void OnEnable()
	{
		if (!Application.isPlaying)
		{
			EditorApplication.update += Update;
		}
	}


	void OnDisable()
	{
		if (!Application.isPlaying)
		{
			EditorApplication.update -= Update;
		}
	}
#endif

	public void Update()
	{
		if (textureLoaded == false && request_ != null && request_.isDone)
		{
			string errorMessage = "";
			var tempMaterial = new Material(GetComponent<MeshRenderer> ().sharedMaterial);

			if (ValidateDownloadedTexture(out errorMessage))
			{
				textureLoaded = true;
				tempMaterial.mainTexture = request_.texture;
			}
			else
			{
				request_ = null;
				tempMaterial.mainTexture = Texture2D.whiteTexture;
			}
			tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
			GetComponent<MeshRenderer>().material = tempMaterial;
		}
		else if (textureLoaded == false && texture != null)
		{
			var tempMaterial = new Material(GetComponent<MeshRenderer> ().sharedMaterial);
			textureLoaded = true;
			tempMaterial.mainTexture = texture;
			tempMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
			GetComponent<MeshRenderer>().material = tempMaterial;
		}
	}

	protected virtual void OnDestroy()
	{
		--totalInMemory;
	}

	public bool IsDownloading()
	{
		return textureLoaded == false && ((request_ != null && !request_.isDone) || texture == null);
	}

	protected abstract string GenerateRequestURL(string nodeID);
	public void CopyTo(OnlineTexture copy)
	{
		copy.loadFromFile = loadFromFile;
		copy.request_ = request_;
		// This forces inherited component to reload the texture.
		copy.textureLoaded = false;

		InnerCopyTo(copy);
	}

	protected abstract void InnerCopyTo(OnlineTexture copy);


	public virtual bool ValidateDownloadedTexture(out string errorMessage)
	{
		if (request_.error != null)
		{
			errorMessage = request_.error;
			return false;
		}
		else
		{
			errorMessage = "";
			return true;
		}
	}
}
