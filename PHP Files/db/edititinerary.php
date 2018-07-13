<?php

	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	
	// escape input for security
	$itineraryid = $mysqli->real_escape_string($_GET['itineraryid']);
	$name = $mysqli->real_escape_string($_GET['name']);
	$rating = $mysqli->real_escape_string($_GET['rating']);
	$is_public =  $mysqli->real_escape_string($_GET['is_public']);
	$deleted = $mysqli->real_escape_string($_GET['deleted']);
	$colors =  $mysqli->real_escape_string($_GET['colors']);
	
	// check for existing username, email, fbid
	$query = "SELECT * FROM itineraries WHERE name = '{$name}'";
	$result = $mysqli->query($query);
	if($result->num_rows > 0)
	{
		error("Itinerary name already exist");
	}
	else
	{
		// add to database
		$query = "UPDATE itineraries set name='{$name}', rating='{$rating}', is_public='{$is_public}', deleted='{$deleted}', colors='{$colors}' where itineraryid='{$itineraryid}'";
		$result = $mysqli->query($query);
		if ($result === TRUE)
		{
			// do query
			$sql = "SELECT * FROM itineraries WHERE itineraryid = '{$itineraryid}'";
			//echo $sql;
			$result = $mysqli->query($sql);
			
			if($result->num_rows > 0)
			{
				// get results in a nice key-value array
				$all['edititinerary'] = $result->fetch_all(MYSQLI_ASSOC);
				// output as json
				echo json_encode($all);
				$result->close();
			}
		}
		else error($mysqli->error);
	}
	
	$mysqli->close();
?>