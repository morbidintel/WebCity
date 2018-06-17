using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
	[SerializeField]
	GameObject transitionPane; //for fades, will enable when scene change

	[SerializeField]
	float delay = 0.0f;

	IEnumerator LoadSceneCorot(string scname, int idx)
	{
		yield return new WaitForEndOfFrame();
		if (transitionPane) transitionPane.SetActive(true);
		yield return new WaitForSeconds(delay);
		Resources.UnloadUnusedAssets();
		if (scname != "") SceneManager.LoadScene(scname);
		else SceneManager.LoadScene(idx);
	}

	public void LoadSceneByIndex(int idx)
	{
		StartCoroutine(LoadSceneCorot("", idx));
	}

	public void LoadScene(string scname)
	{
		StartCoroutine(LoadSceneCorot(scname, 0));
	}
}