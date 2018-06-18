using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ForgetPassword : MonoBehaviour
{
	[SerializeField]
	InputField username;
	[SerializeField]
	Text subtitle;

	string url = "http://webcity.online/live/email/send_emailv2.php?username={0}";
	string successText = "<color=green>Email sent successfully. Please check your email and your junk mail.</color>";
	string prevText = "";

	void Start()
	{
		prevText = subtitle.text;
	}

	void OnDisable()
	{
		subtitle.text = prevText;
		username.text = "";
	}

	public void OnClickSubmit()
	{
		string valuetxtbox = username.text.ToString();
		url = string.Format(url, valuetxtbox);

		new WWW(url);

		subtitle.text = successText;
	}
}
