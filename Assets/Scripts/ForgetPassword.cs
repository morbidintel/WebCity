using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Script to handle submission of the Forget Password form.
/// </summary>
public class ForgetPassword : MonoBehaviour
{
	/// <summary>
	/// The InputField for username.
	/// </summary>
	[SerializeField]
	InputField username = null;

	/// <summary>
	/// Text field to show some confirmation text.
	/// </summary>
	[SerializeField]
	Text subtitle = null;

	/// <summary>
	/// URL of the PHP script on server.
	/// </summary>
	readonly string url = "http://webcity.online/live/email/send_emailv2.php?username={0}";

	/// <summary>
	/// Text to show the user.
	/// This is shown regardless whether the email was actually sent or not.
	/// </summary>
	readonly string successText = "<color=green>Email sent successfully. Please check your email and your junk mail.</color>";

	string prevText = "Enter your username";

	void OnDisable()
	{
		// reset the text fields
		subtitle.text = prevText;
		username.text = "";
	}

	public void OnClickSubmit()
	{
		new WWW(string.Format(url, username.text.ToString()));
		subtitle.text = successText;
	}
}
