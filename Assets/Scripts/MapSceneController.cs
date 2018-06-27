using Gamelogic.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSceneController : Singleton<MapSceneController>
{
	const int pruneTileLimit = 512; //delete invisible tiles when total tiles exceed this limit
	const float pruneTilesTimerMax = 1f;
	[SerializeField]
	ChangeScene sceneChanger;
	[SerializeField]
	Transform [] mapsRoot;

	float pruneTilesTimer = pruneTilesTimerMax;

	// Use this for initialization
	void Start()
	{
	}

	// Update is called once per frame
	void Update()
	{
		if (pruneTilesTimer > 0)
		{
			pruneTilesTimer -= Time.deltaTime;
			if (pruneTilesTimer <= 0)
			{
				pruneTilesTimer = pruneTilesTimerMax;
				PruneMapTiles();
			}
		}
	}

	void PruneMapTiles(bool force = false)
	{
		List<QuadtreeLODPlane> alltiles = new List<QuadtreeLODPlane>();
		foreach (var m in mapsRoot)
		{
			alltiles.AddRange(m.GetComponentsInChildren<QuadtreeLODPlane>(true));
		}

		if (alltiles.Count > pruneTileLimit || force)
		{
			foreach (var v in alltiles)
			{
				if (v) v.PruneInvisible();
			}
			Debug.Log("Pruned because all tiles = " + alltiles.Count);
			Resources.UnloadUnusedAssets();
		}
	}

	public void ChangeScene(string scene)
	{
		PruneMapTiles(true);
		sceneChanger.LoadScene(scene);
	}
}