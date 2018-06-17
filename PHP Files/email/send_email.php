<?php

$to = 'zaihao90@hotmail.com';
$subject = 'WebCity Forget Password';
$headers = "From: webcity.online@webcity.online" . "\r\n";
$headers .= "MIME-Version: 1.0\r\n";
$headers .= "Content-Type: text/html; charset=UTF-8\r\n";
$message = '<p><strong>Click <a href="http://webcity.online">here</a></strong> to reset password</p>';

mail($to, $subject, $message, $headers);
echo "Mail Sent to user";
?>