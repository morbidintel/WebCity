<?php
// this function exactly matches with our Unity C# class PBKDF2Hash::Hash(string)
function PBKDF2Hash($plaintext)
{
	$salt = openssl_random_pseudo_bytes(16);
	$hash = hash_pbkdf2("sha1", $plaintext, $salt, 10000, 20, TRUE);
	return base64_encode($salt.$hash);
}

// this function exactly matches with our Unity C# class PBKDF2Hash::Compare
function PBKDF2Compare($hashtext, $plaintext)
{
	$hashbytes = base64_decode($hashtext);
	$salt = substr($hashbytes, 0, 16);
	$hash = hash_pbkdf2("sha1", $plaintext, $salt, 10000, 20, TRUE);
	for ($i = 0; $i < 36; $i++)
		if ($hashbytes[$i + 16] != $hash[$i]) return false;
	return true;
}
?>