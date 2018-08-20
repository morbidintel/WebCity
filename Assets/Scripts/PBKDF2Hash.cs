using System;
using System.Security.Cryptography;

public class PBKDF2Hash
{
	private static int iterations = 10000, saltByteCount = 16, hashByteCount = 20;

	/// <summary>
	/// Hash plaintext with random salt
	/// </summary>
	/// <param name="plaintext">text to be hashed</param>
	/// <returns>string of hash</returns>
	public static string Hash(string plaintext)
	{
		byte[] salt;
		new RNGCryptoServiceProvider().GetBytes(salt = new byte[saltByteCount]);
		var pbkdf2 = new Rfc2898DeriveBytes(plaintext, salt, iterations);
		byte[] hash = pbkdf2.GetBytes(hashByteCount);
		byte[] hashBytes = new byte[saltByteCount + hashByteCount];
		Array.Copy(salt, 0, hashBytes, 0, saltByteCount);
		Array.Copy(hash, 0, hashBytes, saltByteCount, hashByteCount);
		return Convert.ToBase64String(hashBytes);
	}

	/// <summary>
	/// Compare a hash with plaintext
	/// </summary>
	/// <param name="hashtext">string of hash to be compared</param>
	/// <param name="plaintext">plaintext to be compared</param>
	/// <returns>true if matches</returns>
	public static bool Compare(string hashtext, string plaintext)
	{
		byte[] hashBytes = Convert.FromBase64String(hashtext);
		byte[] salt = new byte[saltByteCount];
		Array.Copy(hashBytes, 0, salt, 0, saltByteCount);
		var pbkdf2 = new Rfc2898DeriveBytes(plaintext, salt, iterations);
		byte[] hash = pbkdf2.GetBytes(hashByteCount);
		for (int i = 0; i < hashByteCount; i++)
			if (hashBytes[i + saltByteCount] != hash[i]) return false;
		return true;
	}
}
