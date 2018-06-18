<?php
	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	
	// this allows for exec() calling of this file
	if (PHP_SAPI === 'cli')
	{
		// assert command arguments
		$_GET['username'] = $argv[1];
		$_GET['pwhash'] = $argv[2];
	}
	elseif (($p = check_params('username', 'pwhash')) != null)
	{
		// assert url params
		error('Parameter '.$p.' not set');
	}
	
	// escape input for security
	$username = $mysqli->real_escape_string($_GET['username']);
	$pwhash = $mysqli->real_escape_string($_GET['pwhash']);
	// do query
	$sql = "SELECT * FROM users WHERE username = '{$username}'";
	$result = $mysqli->query($sql);
	
	if($result->num_rows > 0)
	{
		// get results in a nice key-value array
		$all = $result->fetch_all(MYSQLI_ASSOC);
		
		// check if hash in url param matches with hash in db
		if ($all[0]["pwhash"] != base64_decode($pwhash))
			error("Incorrect password");
		else if ($all[0]["forgetpwtime"] != NULL)
		{
			// format is 2018-06-12 09:43:25
			$forgetpwtime = $all[0]["forgetpwtime"];
			if (strtotime($forgetpwtime) - strtotime($currtime) > 600)
			{
				error("User has activated forget password");
			}
			else
			{
				//$sql = "UPDATE users SET forgetpwtime = NULL WHERE username = '{$username}'";
				//$result = $mysqli->query($sql);
			}
		}
		
		// remove the password hash value from the output
		unset($all[0]["pwhash"]);
		$all = $all[0];
		
		// output as json
		echo json_encode($all);
		
		$result->close();
	}
	else
	{
		error("No results");
	}
	
	$mysqli->close();
?>