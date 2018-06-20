<?php

if(!is_null($_GET['username']))
{
	// Configuration
	$hostname = 'localhost';
	$dbusername = 'unity';
	$dbpassword = '12345';
	$database = 'db';
	
	// connect to db
	$mysqli = new mysqli($hostname, $dbusername, $dbpassword, $database);
	if ($mysqli->connect_error) error($mysqli->connect_error);
	

	// escape input for security
	$username = $mysqli->real_escape_string($_GET['username']);

	// do query
	$sql = "SELECT * FROM users WHERE username = '$username'";
	$result = $mysqli->query($sql);


	if($result->num_rows>0)
	{

		// get results in a nice key-value array
		$all = $result->fetch_all(MYSQLI_ASSOC);

		$userid = $all[0]['userid'];
		$useremail = $all[0]['email'];
		$url = "http://webcity.online/email/forgotPassword.php?userid=";
		$fullpath = $url.$userid;

		$query1 = "UPDATE users SET forgetpwtime = CURRENT_TIMESTAMP WHERE userid='".$userid."'";
		$result1 = $mysqli->query($query1);

		//prep mail
		$to = $useremail;
		$subject = 'WebCity Forget Password';
		$headers = "From: webcity.online@webcity.online" . "\r\n";
		$headers .= "MIME-Version: 1.0\r\n";
		$headers .= "Content-Type: text/html; charset=UTF-8\r\n";
		$message = '<p><strong>Click <a href="'.$fullpath.'">here</a></strong> to reset password</p>';
		//send email
		mail($to, $subject, $message, $headers);
		echo "Mail Sent to user".$userid.$username.$useremail;

	}
	else
	{
		echo"too bad";
	}
}
?>