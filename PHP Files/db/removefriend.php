<?php

	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	

	// escape input for security
	$userid = $mysqli->real_escape_string($_GET['userid']);
	$friendid = $mysqli->real_escape_string($_GET['friendid']);

	$query1 = "SELECT * FROM users WHERE userid = '{$userid}'";
	$result1 = $mysqli->query($query1);
	if($result1->num_rows < 1)
	{
		$user = 0;
	}
	else
	{
		$user = 1;
	}


	$query2 = "SELECT * FROM users WHERE userid = '{$friendid}'";
	$result2 = $mysqli->query($query2);
	if($result2->num_rows < 1)
	{
		$userfriend = 0;
	}
	else
	{
		$userfriend = 1;
	}

	if($user===0 || $userfriend === 0)
	{
		echo("ID Does not exist");
	}
	
	else
	{
		// check for existing username, email, fbid
		$query = "SELECT * FROM friends WHERE userid = '{$userid}' and friendid = '{$friendid}'";
		$result = $mysqli->query($query);
		if($result->num_rows < 1)
		{
			error("User Never add friend before");
		}
		else
		{
			// add to database
			$query = "DELETE from friends where  userid = '{$userid}' and friendid = '{$friendid}'";
			$result = $mysqli->query($query);
			if ($result === TRUE)
			{
				echo("OK");
			}
			else error($mysqli->error);
		}
	}
	
	$mysqli->close();
?>