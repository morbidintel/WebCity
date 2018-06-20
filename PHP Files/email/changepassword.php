<?php


 echo $_POST['newPassword'];
 echo $_POST['confirmPassword'];
 echo $_POST['userid'];


$passwrd = $_POST['newPassword'];
$confirmpasswrd = $_POST['confirmPassword'];
$userid = $_POST['userid'];

	// Configuration
	$hostname = 'localhost';
	$dbusername = 'unity';
	$dbpassword = '12345';
	$database = 'db';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);


	if($passwrd == $confirmpasswrd)
	{
		$query1 = "UPDATE users SET pwhash ='".$passwrd."' WHERE userid='".$userid."'";
		$result1 = $mysqli->query($query1);
	}
?>