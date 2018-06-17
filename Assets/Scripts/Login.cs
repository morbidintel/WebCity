using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
	string loginURL = "http://webcity.online/db/login.php?username={0}&pwhash={1}";

	[SerializeField]
	InputField user;
	[SerializeField]
	InputField pwd;

	[SerializeField]
	ChangeScene sceneChanger;

	[Serializable]
	public class User
	{
		public string userid, username, fbid, email, desc, isfbonly, createddate, forgetpwtime;
		public string error;
	}

	IEnumerator GetCredential()
	{
		WWW getLogin = new WWW(string.Format(loginURL, user.text, pwd.text));
		yield return getLogin;

		if (getLogin.error != null)
		{
			Debug.Log(getLogin.error);
		}
		else
		{
			Debug.Log(getLogin.text);
			User json = JsonUtility.FromJson<User>(getLogin.text);
			if (json.error == null)
			{
				sceneChanger.LoadScene("Map Scene");
			}
			else
			{
				Debug.Log(json.error);
			}
		}
	}

	public void OnClickLoginButton()
	{
		StartCoroutine(GetCredential());
	}
}
