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
	if($result->num_rows > 0)
	{
		error("Place have been created in current itinerary");
	}
	else
	{
		// add to database
		$query = "INSERT INTO places VALUES (UUID(), '{$itineraryid}', '{$googleid}', CURRENT_TIMESTAMP, NULL)";
		$result = $mysqli->query($query);
		if ($result === TRUE)
		{
			// do query
			$sql = "SELECT * FROM places  WHERE itineraryid = '{$itineraryid}' and googleid = '{$googleid}'";
			//echo $sql;
			$result = $mysqli->query($sql);
			
			if($result->num_rows > 0)
			{
				// get results in a nice key-value array
				$all['places'] = $result->fetch_all(MYSQLI_ASSOC);
				// output as json
				echo json_encode($all);
				$result->close();
			}
		}
		else error($mysqli->error);
	}
	
	$mysqli->close();
?>