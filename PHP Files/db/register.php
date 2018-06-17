<?php
	//http://webcity.online/db/register.php?username=echo&pwhash=echo1234&fbid=echofbid&email=echo@echo.com&desc=&isfbonly=0
	include 'error.php';
	include 'paramscheck.php';
	
	// Configuration
	$hostname = 'localhost';
	$dbusername = 'unity';
	$dbpassword = 'password';
	$database = 'db';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	
	// assert url params
	if (($p = check_params('username', 'pwhash')) != null)
		error('Parameter '.$p.' not set');
	
	// escape input for security
	$username = $mysqli->real_escape_string($_GET['username']);
	$pwhash = $mysqli->real_escape_string($_GET['pwhash']);
	$fbid = isset($_GET['fbid']) ? $mysqli->real_escape_string($_GET['fbid']) : '';
	$email = isset($_GET['email']) ? $mysqli->real_escape_string($_GET['email']) : '';
	$desc = isset($_GET['desc']) ? $mysqli->real_escape_string($_GET['desc']) : '';
	$isfbonly = isset($_GET['isfbonly']) ? $mysqli->real_escape_string($_GET['isfbonly']) : 0;
	
	// check for existing username, email, fbid
	$query = "SELECT * FROM users WHERE username = '{$username}' OR fbid = '{$fbid}' OR email = '{$email}'";
	$result = $mysqli->query($query);
	if($result->num_rows > 0)
	{
		$result = $result->fetch_all(MYSQLI_ASSOC);
		if (in_array($username, array_column($result, 'username')))
			error('Username exist');
		else if (in_array($fbid, array_column($result, 'fbid')))
			error('Facebook ID already used');
		else if (in_array($email, array_column($result, 'email')))
			error('Email already registered');
		else
			error('User exists');
	}
	
	// add to database
	$query = "INSERT INTO users VALUES (UUID(), '{$username}', '{$pwhash}', '{$fbid}', '{$email}', '{$desc}', {$isfbonly}, CURRENT_TIMESTAMP)";
	$result = $mysqli->query($query);
	if ($result === TRUE)
	{
		// show newly created user
		echo exec("php login.php {$username} {$pwhash}");
	}
	else
	{
		error($result->error());
	}
	
	$mysqli->close();
?>