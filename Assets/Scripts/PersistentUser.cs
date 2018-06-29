using UnityEngine;
using PhpDB;

public class PersistentUser : Gamelogic.Extensions.Singleton<PersistentUser>
{
	public LoginResult user = null;
	public static LoginResult User { get { return Instance?.user; } }

	public static void Create(LoginResult user)
	{
		if (Instance == null)
		{
			GameObject perm = new GameObject();
			perm.AddComponent<PersistentUser>();
			perm.name = "Persistent User Object";
			DontDestroyOnLoad(perm);
		}

		Instance.user = user;
	}
}