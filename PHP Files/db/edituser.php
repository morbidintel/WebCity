<?php

	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	

	// escape input for security
	$userid = $mysqli->real_escape_string($_GET['userid']);
	$username = $mysqli->real_escape_string($_GET['username']);
	$pwhash = $mysqli->real_escape_string($_GET['pwhash']);
	$fbid =  $mysqli->real_escape_string($_GET['fbid']);
	$email = $mysqli->real_escape_string($_GET['email']);
	$desc =  $mysqli->real_escape_string($_GET['desc']);
	
	// check for existing username, email, fbid
	$query = "SELECT * FROM users WHERE userid = '{$userid}'";
	$result = $mysqli->query($query);
	if($result->num_rows < 1)
	{
		error("User does not exist");
	}
	else
	{
		// add to database
		$query = "UPDATE users set username='{$username}', pwhash='{$pwhash}', fbid='{$fbid}', email='{$email}', `desc`='{$desc}' where userid='{$userid}'";
		$result = $mysqli->query($query);
		if ($result === TRUE)
		{
			// do query
			$sql = "SELECT userid,username,fbid,email,`desc`,isfbonly,createddate,forgetpwtime FROM users WHERE userid = '{$userid}'";
			//echo $sql;
			$result = $mysqli->query($sql);
			
			if($result->num_rows > 0)
			{
				// get results in a nice key-value array
				$all['edituser'] = $result->fetch_all(MYSQLI_ASSOC);
				// output as json
				echo json_encode($all);
				$result->close();
			}
		}
		else error($mysqli->error);
	}
	
	$mysqli->close();
?>