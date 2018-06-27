<?php
	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);

	// escape input for security
	$userid = $mysqli->real_escape_string($_GET['userid']);

	// do query
	$sql = "SELECT * FROM itineraries WHERE userid = '{$userid}'";
	$result = $mysqli->query($sql);
	
	if($result->num_rows > 0)
	{
		// get results in a nice key-value array
		$all = $result->fetch_all(MYSQLI_ASSOC);
		
		// output as json
		echo json_encode($all);
		
		$result->close();
	}
	else
	{
		error("No itineraries");
	}
	
	$mysqli->close();
?>