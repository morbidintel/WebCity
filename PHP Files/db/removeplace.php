<?php

	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	

	// escape input for security
	$itineraryid = $mysqli->real_escape_string($_GET['itineraryid']);
	$googleid = $mysqli->real_escape_string($_GET['googleid']);


	
	// check for existing username, email, fbid
	$query = "SELECT * FROM places WHERE itineraryid = '{$itineraryid}' and googleid = '{$googleid}'";
	$result = $mysqli->query($query);
	if($result->num_rows < 1)
	{
		error("Place does not exist");
	}
	else
	{
		// add to database
		$query = "DELETE from places where  itineraryid = '{$itineraryid}' and googleid = '{$googleid}' ";
		$result = $mysqli->query($query);
		if ($result === TRUE)
		{
			echo("OK");
		}
		else error($mysqli->error);
	}
	
	$mysqli->close();
?>