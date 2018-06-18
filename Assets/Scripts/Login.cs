using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour
{
	string loginURL = "http://webcity.online/live/db/login.php?username={0}&pwhash={1}";

	[SerializeField]
	InputField username = null;
	[SerializeField]
	InputField password = null;
	[SerializeField]
	Text errorSubtitle = null;

	[SerializeField]
	ChangeScene sceneChanger = null;

	[Serializable]
	class User
	{
		public string userid, username, fbid, email, desc, isfbonly, createddate, forgetpwtime;
		public string error;
	}

	IEnumerator GetCredential()
	{
		errorSubtitle.text = "";
		string encodedpw = Convert.ToBase64String(Encoding.UTF8.GetBytes(password.text));
		string url = string.Format(loginURL, WWW.EscapeURL(username.text), WWW.EscapeURL(encodedpw));
		WWW getLogin = new WWW(url);
		yield return getLogin;

		if (getLogin.error != null)
		{
			errorSubtitle.text = getLogin.error;
		}
		else
		{
			User json = JsonUtility.FromJson<User>(getLogin.text);
			if (json.error == null)
			{
				sceneChanger.LoadScene("Map Scene");
			}
			else
			{
				errorSubtitle.text = json.error;
			}
		}
	}

	public void OnClickLoginButton()
	{
		if (username.text == "")
			errorSubtitle.text = "Username cannot be empty";
		else if (password.text == "")
			errorSubtitle.text = "Password cannot be empty";
		else
			StartCoroutine(GetCredential());
	}
}
