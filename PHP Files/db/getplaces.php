<?php
	include 'error.php';
	include 'paramscheck.php';
	include 'config.php';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);

	// escape input for security
	$itineraryid = $mysqli->real_escape_string($_GET['itineraryid']);
	
	// do query
	$sql = "SELECT * FROM places WHERE itineraryid = '{$itineraryid}'";
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
	else
	{
		error("No Places");
	}
	
	$mysqli->close();
?>