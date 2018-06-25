//#define PAINT_QUADS

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// This is needed for executing the Start() method and replacing the Unity
// plane with a custom one.
[ExecuteInEditMode]
public class QuadtreeLODPlane : MonoBehaviour
{
	public int vertexResolution = 4;
	private OnlineTexture onlineTexture = null;
	private string nodeID = "0";
	QuadtreeLODPlane[] children_ = null;

	private int depth_ = 0;
	const int MAX_DEPTH = 20;

	//newly created tiles do not activate immediately, so if the user is just rapidly panning around, unneeded tiles won't be created
	//also acts as a form of rate limiting and eases the flood of requests
	float timeToCreateChildActivation;

	//cached value for visibility
	bool isVisible;

	//cached references
	MeshRenderer mRenderer;

	//all tiles will call this function when a cleanup is required.
	//inactive child tiles will self destruct from the bottom
	bool TreeIsInvisible() //recursive function that determines if this branch and children are inactive
	{
		if (!transform.gameObject.activeInHierarchy) { return true; }
		//child node, return self status 
		if (children_ == null)
		{
			return !isVisible;
		}
		else
		{
			if (isVisible) { return false; }

			//non child node, tree is invisible if ALL children are invisible
			for (int i = 0; i < children_.Length; ++i)
			{
				if (children_[i] != null && !children_[i].TreeIsInvisible())
				{
					return false;
				}
			}
			return true;
		}
	}

	//to be called on all planes
	//if all our children are invisible, get rid of all of them
	public void PruneInvisible()
	{
		if (children_ != null)
		{
			bool shouldRemoveChildren = true;
			for (int i = 0; i < children_.Length; ++i)
			{
				if (children_[i] != null && !children_[i].TreeIsInvisible())
				{
					shouldRemoveChildren = false;
					break;
				}
			}
			if (shouldRemoveChildren)
			{
				for (int i = 0; i < children_.Length; ++i)
				{
					if (children_[i] != null)
					{
						Destroy(children_[i].gameObject);
						children_[i] = null;
					}
				}
				children_ = null;
				if (transform.parent.GetComponent<QuadtreeLODPlane>() == null)
				{
					//this is a root node
					SetVisible(true);
				}
			}
		}
	}


	private void Awake()
	{
		timeToCreateChildActivation = UnityEngine.Random.Range(0.1f, 0.15f);
		mRenderer = gameObject.GetComponent<MeshRenderer>();
	}
	public void Start()
	{
		if (depth_ == 0)
		{
			Vector3 meshSize = GetComponent<MeshFilter>().sharedMesh.bounds.size;
			Vector2 mapSize = new Vector2 (meshSize.x, meshSize.z);

			onlineTexture = null;
			if (GetComponent<GoogleMapsTexture>() != null)
			{
				onlineTexture = this.GetComponent<GoogleMapsTexture>();
			}

			nodeID = "0";

			// Create the root mesh.
			gameObject.GetComponent<MeshFilter>().mesh = PlanesFactory.CreateHorizontalPlane(mapSize, vertexResolution);
		}

		if (onlineTexture != null)
		{
			onlineTexture.RequestTexture(nodeID);
		}

		isVisible = mRenderer.enabled;
	}


	public GameObject CreateChild(string nodeID, Color color, Vector3 localPosition)
	{
		// Create the child game object.
		// Initially this was done by duplicating current game object, but this copied
		// children as well and errors arisen.
		GameObject childGameObject = new GameObject();
		childGameObject.name = GenerateNameForChild(gameObject.name, nodeID);
		var childRenderer = childGameObject.AddComponent<MeshRenderer>();
		var childMeshFilter = childGameObject.AddComponent<MeshFilter>();
		var childLOD = childGameObject.AddComponent<QuadtreeLODPlane>();

		// Copy parent's mesh
		Mesh parentMesh = GetComponent<MeshFilter>().mesh;
		Mesh childMesh = new Mesh ();
		childMesh.vertices = parentMesh.vertices;
		childMesh.triangles = parentMesh.triangles;
		childMesh.uv = parentMesh.uv;
		childMesh.RecalculateNormals();
		childMesh.RecalculateBounds();
		childMeshFilter.mesh = childMesh;

		// Make this child transform relative to parent.
		childGameObject.transform.parent = gameObject.transform;

		// Previous assignment alters local transformation, so we reset it.
		childGameObject.transform.localRotation = Quaternion.identity;
		childGameObject.transform.localPosition = localPosition;
		childGameObject.transform.localScale = new Vector3(0.5f, 1.0f, 0.5f);

		// Create material
		Renderer thisRenderer = GetComponent<Renderer>();
		childRenderer.material = new Material(thisRenderer.material.shader);
		childRenderer.material.renderQueue = thisRenderer.material.renderQueue;

#if PAINT_QUADS
		childGameObject.GetComponent<Renderer>().material.color = color;
#endif
		childLOD.depth_ = this.depth_ + 1;
		childLOD.vertexResolution = this.vertexResolution;
		childLOD.nodeID = nodeID;

		if (onlineTexture != null)
		{
			if (GetComponent<GoogleMapsTexture>())
			{
				childLOD.onlineTexture = childGameObject.AddComponent<GoogleMapsTexture>();
			}
			onlineTexture.CopyTo(childLOD.onlineTexture);
		}
		childLOD.SetVisible(false);

		return childGameObject;
	}

	private string GenerateNameForChild(string parentName, string childNodeID)
	{
		// Strip node ID from parent name (root node doesn't have ID, so we omit 
		// that case.
		if (depth_ > 0)
		{
			parentName = parentName.Substring(0, parentName.IndexOf(" - ["));
		}
		return parentName + " - [" + childNodeID + "]";
	}


	private void FlipUV()
	{
		Vector2[] uv = gameObject.GetComponent<MeshFilter> ().mesh.uv;
		for (int i = 0; i < uv.Length; i++)
		{
			uv[i].x = 1.0f - uv[i].x;
			uv[i].y = 1.0f - uv[i].y;
		}
		gameObject.GetComponent<MeshFilter>().mesh.uv = uv;
	}


	public void SetVisible(bool visible)
	{
		// Set node visibility.
		mRenderer.enabled = visible;
		isVisible = visible;

		// No matter which visibility is applied to this node, children
		// visibility must be set to false.
		if (children_ != null)
		{
			for (int i = 0; i < children_.Length; i++)
			{
				children_[i].SetVisible(false);
			}
		}
	}

	void Update()
	{
		// Don't Update in edit mode.
		if (!Application.isPlaying)
		{
			return;
		}

		bool childrenLoaded = AreChildrenLoaded();

		if (isVisible || childrenLoaded)
		{
			DistanceTestResult distanceTestResult = DoDistanceTest();
			Vector3 meshSize = Vector3.Scale (GetComponent<MeshFilter>().mesh.bounds.size, gameObject.transform.lossyScale);

			// Subdivide the plane if camera is closer than a threshold.
			if (isVisible && distanceTestResult == DistanceTestResult.SUBDIVIDE)
			{
				// Create children if they don't exist.
				if (depth_ < MAX_DEPTH && children_ == null)
				{
					timeToCreateChildActivation -= Time.deltaTime;
					if (timeToCreateChildActivation <= 0)
					{
						CreateChildren(meshSize);
					}
				}

				// Make this node invisible and children visible.
				if (childrenLoaded)
				{
					SetVisible(false);
					for (int i = 0; i < children_.Length; i++)
					{
						children_[i].SetVisible(true);
					}
				}
			}
			else if (!isVisible && childrenLoaded && ParentOfVisibleNodes() && distanceTestResult == DistanceTestResult.JOIN)
			{
				SetVisible(true);
				for (int i = 0; i < children_.Length; i++)
				{
					children_[i].SetVisible(false);
				}
			}
		}
	}


	enum DistanceTestResult
	{
		DO_NOTHING,
		SUBDIVIDE,
		JOIN
	}


	private DistanceTestResult DoDistanceTest()
	{
		const float THRESHOLD_FACTOR = 1.5f;

		Vector3 cameraPos = Camera.main.transform.position;
		float distanceCameraBorder = Vector3.Distance (cameraPos, mRenderer.bounds.ClosestPoint (cameraPos));
		Vector3 boundsSize = mRenderer.bounds.size;
		float radius = (boundsSize.x + boundsSize.y + boundsSize.z) / 3.0f;

		if (distanceCameraBorder < THRESHOLD_FACTOR * radius)
		{
			return DistanceTestResult.SUBDIVIDE;
		}
		else if (distanceCameraBorder >= THRESHOLD_FACTOR * radius)
		{
			return DistanceTestResult.JOIN;
		}

		return DistanceTestResult.DO_NOTHING;
	}


	private void CreateChildren(Vector3 meshSize)
	{
		Vector3 S = new Vector3(
			1.0f / gameObject.transform.lossyScale.x,
			1.0f / gameObject.transform.lossyScale.y,
			1.0f / gameObject.transform.lossyScale.z
		);


		Vector3[] childLocalPosition = new Vector3[]
		{
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( -meshSize.x/4,0,-meshSize.z/4 ), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,meshSize.z/4), S ),
			Vector3.Scale ( new Vector3( meshSize.x/4,0,-meshSize.z/4), S )
		};


		Color[] childrenColors = new Color[]
		{
			new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 1.0f, 0.0f, 1.0f ),
			new Color( 0.0f, 0.0f, 1.0f, 1.0f ),
			new Color( 1.0f, 1.0f, 0.0f, 1.0f )
		};


		string[] childrenIDs = new string[]
		{
			nodeID + "0",
			nodeID + "1",
			nodeID + "2",
			nodeID + "3"
		};

		children_ = new QuadtreeLODPlane[] { null, null, null, null };
		for (int i = 0; i < 4; i++)
		{
			children_[i] = CreateChild(childrenIDs[i], childrenColors[i], childLocalPosition[i]).GetComponent<QuadtreeLODPlane>();
		}
	}


	private bool AreChildrenLoaded()
	{
		if (children_ != null)
		{
			for (int i = 0; i < 4; i++)
			{
				if (children_[i].onlineTexture == null || children_[i].onlineTexture.textureLoaded == false)
				{
					return false;
				}
			}
			return true;
		}
		else
		{
			return false;
		}
	}



	public bool ParentOfVisibleNodes()
	{
		if (children_ == null)
		{
			return false;
		}
		for (int i = 0; i < children_.Length; i++)
		{
			if (children_[i].isVisible == false)
			{
				return false;
			}
		}
		return true;
	}
}
