<?php

	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	
	// escape input for security
	$userid = $mysqli->real_escape_string($_GET['userid']);
	$name = $mysqli->real_escape_string($_GET['name']);
	$is_public = isset($_GET['is_public']) ? $mysqli->real_escape_string($_GET['is_public']) : 0;
	$colors =  $mysqli->real_escape_string($_GET['colors']);
	
	// check for existing username, email, fbid
	$query = "SELECT * FROM itineraries WHERE name = '{$name}' and userid = '{$userid}'";
	$result = $mysqli->query($query);
	if($result->num_rows > 0)
	{
		error("Itinerary have been created before");
	}
	else
	{
		// add to database
		$query = "INSERT INTO itineraries VALUES (UUID(), '{$userid}', '{$name}', '0', '{$is_public}', '0', '{$colors}', CURRENT_TIMESTAMP)";
		$result = $mysqli->query($query);
		if ($result === TRUE)
		{
			// do query
			$sql = "SELECT * FROM itineraries WHERE name = '{$name}' and userid = '{$userid}'";
			//echo $sql;
			$result = $mysqli->query($sql);
			
			if($result->num_rows > 0)
			{
				// get results in a nice key-value array
				$all['itineraries'] = $result->fetch_all(MYSQLI_ASSOC);
				// output as json
				echo json_encode($all);
				$result->close();
			}
		}
		else error($mysqli->error);
	}
	
	$mysqli->close();
?>