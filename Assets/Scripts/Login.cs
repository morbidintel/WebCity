using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;

public class Login : MonoBehaviour
{
	[SerializeField]
	InputField username = null, password = null;
	[SerializeField]
	Text errorSubtitle = null;

	[SerializeField]
	ChangeScene sceneChanger = null;


	IEnumerator LoginCoroutine()
	{
		errorSubtitle.text = "";
		string encodedpw = Convert.ToBase64String(Encoding.UTF8.GetBytes(password.text));
		string url = string.Format(LoginResult.URL, WWW.EscapeURL(username.text), WWW.EscapeURL(encodedpw));
		WWW www = new WWW(url);
		yield return www;

		if (www.error != null)
		{
			errorSubtitle.text = www.error;
		}
		else
		{
			LoginResult json = JsonUtility.FromJson<LoginResult>(www.text);
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
			StartCoroutine(LoginCoroutine());
	}
}
