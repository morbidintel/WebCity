using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using PhpDB;

public class Register : MonoBehaviour
{
	string registerURL = "http://webcity.online/live/db/register.php?username={0}&pwhash={1}&email={2}";

	[SerializeField]
	InputField email = null,
		username = null,
		password = null,
		confirmPw = null;
	[SerializeField]
	Text errorSubtitle = null;

	[SerializeField]
	ChangeScene sceneChanger = null;

	IEnumerator RegisterCoroutine()
	{
		errorSubtitle.text = "";
		string pwhash = PBKDF2Hash.Hash(password.text);
		string url = string.Format(registerURL,
			WWW.EscapeURL(username.text),
			WWW.EscapeURL(pwhash),
			WWW.EscapeURL(email.text));
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
				PersistentUser.Create(json);
				sceneChanger.LoadScene("Map Scene");
			}
			else
			{
				errorSubtitle.text = json.error;
			}
		}
	}

	public void OnValueChangedConfirmPassword()
	{
		if (password.text != confirmPw.text)
			confirmPw.image.color = Color.HSVToRGB(0f, 0.1f, 1f);
		else
			confirmPw.image.color = Color.HSVToRGB(1f / 3f, 0.1f, 1f);
	}

	public void OnClickRegister()
	{
		// check email format
		bool invalidEmail = false;
		try { new System.Net.Mail.MailAddress(email.text); }
		catch (FormatException) { invalidEmail = true; }

		if (email.text == "")
			errorSubtitle.text = "Email cannot be empty";
		else if (invalidEmail)
			errorSubtitle.text = "Invalid email address";
		else if (username.text == "")
			errorSubtitle.text = "Username cannot be empty";
		else if (password.text == "")
			errorSubtitle.text = "Password cannot be empty";
		else if (confirmPw.text == "")
			errorSubtitle.text = "Please confirm password";
		else if (password.text != confirmPw.text)
			errorSubtitle.text = "Passwords entered are not the same";
		else
			StartCoroutine(RegisterCoroutine());
	}
}
