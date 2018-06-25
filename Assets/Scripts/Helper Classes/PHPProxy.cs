public class PHPProxy
{
	public static string Escape(string url)
	{
#if !UNITY_EDITOR
		return "http://webcity.online/proxy.php?" + url;
#else
		return url;
#endif
	}
}
